#pragma once
#include <windows.h>
#include <algorithm>
#include <unordered_map>
#include <iterator>
#include <cstring>
#include <stdexcept>
#include <climits>
#include <cstdint>
#include <memory>
#include <map>

typedef int Protection;

static const Protection PROTECTION_NOACCESS = 0x0;
static const Protection PROTECTION_READ = 0x1;
static const Protection PROTECTION_WRITE = 0x2;
static const Protection PROTECTION_EXECUTE = 0x4;
static const Protection PROTECTION_GUARD = 0x8; // Only supported on Windows.
static const Protection PROTECTION_READ_WRITE = PROTECTION_READ | PROTECTION_WRITE;
static const Protection PROTECTION_READ_EXECUTE = PROTECTION_READ | PROTECTION_EXECUTE;
static const Protection PROTECTION_READ_WRITE_EXECUTE = PROTECTION_READ_WRITE | PROTECTION_EXECUTE;

struct MemoryRegion
{
	enum class Status
	{
		// A valid committed memory region. All fields are valid.
		Valid,
		// A free memory region. Only the base and size fields are valid.
		Free,
		// An invalid memory region. This is usually kernel address space. No fields are valid.
		Invalid
	};

	Status status = Status::Invalid;
	uintptr_t base = 0;
	size_t size = 0;
	Protection protection = PROTECTION_NOACCESS;
	HINSTANCE hModule = 0;
	std::string name;
};

uintptr_t FindPatternByModule(const std::string& pattern, const std::string& moduleName = "", int instance = 0);
uintptr_t FollowRelativeAddress(uintptr_t adr, int trail = 0);
uintptr_t FindReadonlyStringRef(const std::string& str, const std::string& moduleName = "", int instance = 0);
std::vector<uintptr_t> FindReadonlyStringRefByAry(const std::vector<std::string>& strings, const std::string& moduleName = "");
std::map<std::string, uintptr_t> FindReadonlyStringRefByAry2(const std::vector<std::string>& strings, const std::string& moduleName = "");