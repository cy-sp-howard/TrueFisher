#include "pch.h"
#include "scanner.h"
#include "exeFile.h"

#define NO_OF_CHARS 256

std::vector<MemoryRegion> GetMemoryMap(int pid = 0);
std::vector<MemoryRegion> memoryMap = GetMemoryMap();
std::unordered_map<std::string, HINSTANCE> moduleMap;
std::unordered_map<std::string, std::unique_ptr<ExeFile>> exeFileMap;
std::unordered_map<std::string, bool> verifyRelocsMap;

HINSTANCE GetModuleByName(const std::string& name)
{
	if (name == "")
	{
		return GetModuleHandleA(NULL);
	}
	else
	{
		return GetModuleHandleA(name.c_str());
	}
}


Protection FromWindowsProt(DWORD windowsProt)
{
	Protection protection = PROTECTION_NOACCESS;

	if (windowsProt & PAGE_GUARD)
	{
		protection |= PROTECTION_GUARD;
	}

	// Strip modifiers.
	windowsProt &= ~(PAGE_GUARD | PAGE_NOCACHE | PAGE_WRITECOMBINE);

	switch (windowsProt)
	{
	case PAGE_NOACCESS:
		break;
	case PAGE_READONLY:
		protection |= PROTECTION_READ;
		break;
	case PAGE_READWRITE:
	case PAGE_WRITECOPY:
		protection |= PROTECTION_READ_WRITE;
		break;
	case PAGE_EXECUTE:
		protection |= PROTECTION_EXECUTE;
		break;
	case PAGE_EXECUTE_READ:
		protection |= PROTECTION_READ_EXECUTE;
		break;
	case PAGE_EXECUTE_READWRITE:
	case PAGE_EXECUTE_WRITECOPY:
		protection |= PROTECTION_READ_WRITE_EXECUTE;
		break;
	default:
		throw std::runtime_error("unknown windows protection");
	}

	return protection;
}


HINSTANCE GetModuleByAddress(uintptr_t adr)
{
	HINSTANCE hModule = NULL;

	if (GetModuleHandleEx(GET_MODULE_HANDLE_EX_FLAG_FROM_ADDRESS | GET_MODULE_HANDLE_EX_FLAG_UNCHANGED_REFCOUNT,
		(LPCTSTR)adr, &hModule) == 0)
	{
		if (GetLastError() != ERROR_MOD_NOT_FOUND)
		{
			throw std::runtime_error("GetModuleHandleEx failed");
		}
	}

	return hModule;
}

std::string GetModulePath(HINSTANCE hModule)
{
	char path[MAX_PATH];

	if (GetModuleFileNameA(hModule, path, MAX_PATH) == 0)
	{
		throw std::runtime_error("GetModuleFileName failed");
	}

	return path;
}

MemoryRegion GetMemoryByAddress(uintptr_t adr, int pid)
{
	MEMORY_BASIC_INFORMATION mbi = {};
	MemoryRegion region;

	if (pid)
	{
		auto hProc = OpenProcess(PROCESS_QUERY_INFORMATION, FALSE, pid);
		if (!hProc)
		{
			throw std::runtime_error("OpenProcess failed");
		}
		if (VirtualQueryEx(hProc, (LPCVOID)adr, &mbi, sizeof(mbi)) != sizeof(mbi))
		{
			CloseHandle(hProc);
			return region;
		}
		CloseHandle(hProc);
	}
	else
	{
		if (VirtualQuery((LPCVOID)adr, &mbi, sizeof(mbi)) != sizeof(mbi))
		{
			return region;
		}
	}

	region.base = (uintptr_t)mbi.BaseAddress;
	region.size = mbi.RegionSize;

	if (mbi.State != MEM_COMMIT)
	{
		region.status = MemoryRegion::Status::Free;
		return region;
	}

	region.status = MemoryRegion::Status::Valid;
	region.protection = FromWindowsProt(mbi.Protect);

	if (mbi.Type == MEM_IMAGE && pid == 0)
	{
		region.hModule = GetModuleByAddress(adr);
		region.name = GetModulePath(region.hModule);
	}

	return region;
}


std::vector<MemoryRegion> GetMemoryMap(int pid)
{
	std::vector<MemoryRegion> regions;

	uintptr_t adr = 0;
	auto region = GetMemoryByAddress(adr, pid);
	while (region.status != MemoryRegion::Status::Invalid)
	{
		if (region.status == MemoryRegion::Status::Valid)
		{
			regions.push_back(region);
		}

		adr += region.size;
		region = GetMemoryByAddress(adr, pid);
	}

	return regions;
}

const std::vector<MemoryRegion>& GetCodeRegions(const std::string& moduleName)
{
	static std::unordered_map<std::string, std::vector<MemoryRegion>> lut;

	auto it = lut.find(moduleName);
	if (it != lut.end())
	{
		return it->second;
	}

	auto hModule = GetModuleByName(moduleName);
	if (!hModule)
	{
		throw std::runtime_error("no such module");
	}

	std::copy_if(memoryMap.begin(), memoryMap.end(), std::back_inserter(lut[moduleName]),
		[hModule](const MemoryRegion& r) {
			return r.hModule == hModule && r.protection == PROTECTION_READ_EXECUTE;
		});
	if (lut[moduleName].empty())
	{
		throw std::runtime_error("no code sections found");
	}

	return lut[moduleName];
}

std::vector<uintptr_t> FindReadonlyStringByAry(const std::vector<std::string>& strings, const std::string& moduleName)
{
	std::vector<uintptr_t> results(strings.size());
	int stringsFound = 0;

	int i = 0;
	for (const auto& str : strings)
	{
		uintptr_t found = 0;
		try
		{
			found = FindReadonlyString(str, moduleName);
		}
		catch (...)
		{
		}

		if (found)
		{
			results[i] = found;
			stringsFound++;
			if (stringsFound == strings.size())
				break;
		}
		i++;
	}

	if (stringsFound != strings.size())
		throw std::runtime_error("one or more patterns not found");

	return results;
}

std::map<std::string, uintptr_t> FindReadonlyStringByAry2(const std::vector<std::string>& strings, const std::string& moduleName)
{
	auto results = FindReadonlyStringByAry(strings, moduleName);

	std::map<std::string, uintptr_t> resultMap;
	for (size_t i = 0; i < results.size(); i++)
	{
		resultMap[strings[i]] = results[i];
	}
	return resultMap;
}

static bool MatchMaskedPattern(uintptr_t address, const char* byteMask, const char* checkMask)
{
	for (; *checkMask; ++checkMask, ++address, ++byteMask)
		if (*checkMask == 'x' && *(char*)address != *byteMask)
			return false;
	return *checkMask == 0;
}

uintptr_t FindPatternMask(const char* byteMask, const char* checkMask, uintptr_t address, size_t len, int instance)
{
	uintptr_t end = address + len - strlen(checkMask) + 1;
	for (uintptr_t i = address; i < end; i++)
	{
		if (MatchMaskedPattern(i, byteMask, checkMask))
		{
			if (!instance--)
				return i;
		}
	}
	return 0;
}

uintptr_t FindPattern(const std::string& pattern, uintptr_t address, size_t len, int instance)
{
	std::vector<char> byteMask;
	std::vector<char> checkMask;

	std::string lowPattern = pattern;
	std::transform(lowPattern.begin(), lowPattern.end(), lowPattern.begin(), ::tolower);
	lowPattern += " ";

	for (size_t i = 0; i < lowPattern.size() / 3; i++)
	{
		if (lowPattern[3 * i + 2] == ' ' && lowPattern[3 * i] == '?' && lowPattern[3 * i + 1] == '?')
		{
			byteMask.push_back(0);
			checkMask.push_back('?');
		}
		else if (lowPattern[3 * i + 2] == ' ' &&
			((lowPattern[3 * i] >= '0' && lowPattern[3 * i] <= '9') ||
				(lowPattern[3 * i] >= 'a' && lowPattern[3 * i] <= 'f')) &&
			((lowPattern[3 * i + 1] >= '0' && lowPattern[3 * i + 1] <= '9') ||
				(lowPattern[3 * i + 1] >= 'a' && lowPattern[3 * i + 1] <= 'f')))

		{
			auto value = strtol(lowPattern.data() + 3 * i, nullptr, 16);
			byteMask.push_back((char)value);
			checkMask.push_back('x');
		}
		else
		{
			throw std::runtime_error("invalid format of pattern string");
		}
	}

	// Terminate mask string, because it is used to determine length.
	checkMask.push_back('\0');

	return FindPatternMask(byteMask.data(), checkMask.data(), address, len, instance);
}

uintptr_t FindPatternByModule(const std::string& pattern, const std::string& moduleName, int instance)
{
	uintptr_t result = 0;
	for (const auto& region : GetCodeRegions(moduleName))
	{
		result = FindPattern(pattern, region.base, region.size, instance);
		if (result)
			break;
	}
	return result;
}

uintptr_t FollowRelativeAddress(uintptr_t adr, int trail)
{
	// Hardcoded 32-bit dereference to make it work with 64-bit code.
	return *(int32_t*)adr + adr + 4 + trail;
}

// The preprocessing function for Boyer Moore's bad character heuristic
static void badCharHeuristic(const uint8_t* str, size_t size, int badchar[NO_OF_CHARS])
{
	size_t i;

	// Initialize all occurrences as -1
	for (i = 0; i < NO_OF_CHARS; i++)
		badchar[i] = -1;

	// Fill the actual value of last occurrence of a character
	for (i = 0; i < size; i++)
		badchar[(int)str[i]] = (int)i;
}

/* A pattern searching function that uses Bad Character Heuristic of
Boyer Moore Algorithm */
static const uint8_t* boyermoore(const uint8_t* txt, const size_t n, const uint8_t* pat, const size_t m)
{
	if (m > n || m < 1)
		return nullptr;

	int badchar[NO_OF_CHARS];

	/* Fill the bad character array by calling the preprocessing
	function badCharHeuristic() for given pattern */
	badCharHeuristic(pat, m, badchar);

	int s = 0; // s is shift of the pattern with respect to text
	int end = (int)(n - m);
	while (s <= end)
	{
		int j = (int)m - 1;

		/* Keep reducing index j of pattern while characters of
		pattern and text are matching at this shift s */
		while (j >= 0 && pat[j] == txt[s + j])
			j--;

		/* If the pattern is present at current shift, then index j
		will become -1 after the above loop */
		if (j < 0)
		{
			// HACKLIB EDIT BEGIN
			// We only want the first occurence of the pattern, so return immediatly.
			return txt + s;

			// printf("\n pattern occurs at shift = %d", s);

			/* Shift the pattern so that the next character in text
			aligns with the last occurrence of it in pattern.
			The condition s+m < n is necessary for the case when
			pattern occurs at the end of text */
			// s += (s + m < n) ? m-badchar[txt[s + m]] : 1;
			// HACKLIB EDIT END
		}
		else
		{
			/* Shift the pattern so that the bad character in text
			aligns with the last occurrence of it in pattern. The
			max function is used to make sure that we get a positive
			shift. We may get a negative shift if the last occurrence
			of bad character in pattern is on the right side of the
			current character. */
			s += (std::max)(1, j - badchar[txt[s + j]]);
		}
	}

	return nullptr;
}


uintptr_t FindReadonlyString(const std::string& str, const std::string& moduleName, int instance)
{
	if (!moduleMap.count(moduleName))
		moduleMap[moduleName] = GetModuleByName(moduleName);
	auto hModule = moduleMap[moduleName];

	uintptr_t addr = 0;

	// Search all readonly sections for the string.
	for (const auto& region : memoryMap)
	{
		if (region.hModule == hModule && region.protection == PROTECTION_READ)
		{
			const uint8_t* found =
				boyermoore((const uint8_t*)region.base, region.size, (const uint8_t*)str.data(), str.size() + 1);

			if (found)
			{
				addr = (uintptr_t)found;
				break;
			}
		}
	}

	if (!addr)
		throw std::runtime_error("pattern not found");

	if (!exeFileMap.count(moduleName))
		exeFileMap[moduleName] = std::make_unique<ExeFile>();
	ExeFile& exeFile = *exeFileMap[moduleName].get();

	if (!verifyRelocsMap.count(moduleName))
		verifyRelocsMap[moduleName] = exeFile.loadFromMem((uintptr_t)hModule) && exeFile.hasRelocs();
	bool verifyWithRelocs = verifyRelocsMap[moduleName];

	uintptr_t ret = 0;

	// Search all code sections for references to the string.
	for (const auto& region : memoryMap)
	{
		if (region.hModule == hModule && region.protection == PROTECTION_READ_EXECUTE)
		{
			const uint8_t* baseAdr = (const uint8_t*)region.base;
			size_t regionSize = region.size;


			do
			{
				auto found = boyermoore(baseAdr, regionSize, (const uint8_t*)&addr, sizeof(uintptr_t));
				if (found)
				{
					// Prevent false positives by checking if the reference is relocated.
					if ((verifyWithRelocs && !exeFile.isReloc((uintptr_t)found - (uintptr_t)hModule)) || instance-- > 0)
					{
						// continue searching
						baseAdr = found + 1;
						regionSize -= (size_t)(found - baseAdr + 1);
						continue;
					}

					ret = (uintptr_t)found;
				}
			} while (false);

			if (ret)
				break;
		}
	}

	return ret;
}

// unused
uintptr_t FindPatternMaskByModule(const char* byteMask, const char* checkMask, const std::string& moduleName, int instance)
{
	uintptr_t result = 0;
	for (const auto& region : GetCodeRegions(moduleName))
	{
		result = FindPatternMask(byteMask, checkMask, region.base, region.size, instance);
		if (result)
			break;
	}
	return result;
}