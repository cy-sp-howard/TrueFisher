#include "pch.h"
#include <stdio.h>
#include <fstream>
#include "scanner.h"

static DWORD WINAPI ThreadFunc(LPVOID param)
{
	auto a = FindPatternByModule("CC 33 C0 4C 8D 0D ?? ?? ?? ?? 4C 8B DA");
	printf("%zu\n", a);
	return 0;
}
bool run()
{

	HANDLE hThread = CreateThread(NULL, 0, ThreadFunc, NULL, 0, NULL);
	if (hThread == NULL)
	{
		throw std::runtime_error(std::string("CreateThread failed with code ") + std::to_string(GetLastError()));
	}
	else
	{
		// Thread will be exited by suiciding with FreeLibraryAndExitThread.
		CloseHandle(hThread);
	}
	printf("next");
	return true;
}

bool IsRun = run();