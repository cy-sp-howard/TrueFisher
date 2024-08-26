#include "pch.h"
#include <stdio.h>
#include <fstream>
#include <string>
#include "scanner.h"

// 00 00 00 0? 00 00 00 55 36 11 00 00 00 00 00 (+3) LANG
// 設定LANG EXE+504a90
//尋找基址 EXE + 2432B0 得ax->AX + 50 取值設在DX  設定DX + 334 內值，DX + 334 內值為基址
//基址 + 50取內值 ，其值 + 334  的LANG地址
// 
// 
// exe + 59DA58 得RAX => RAX+24  取得案件地址
// 
// 85 C0 75 F7 (gw2 module) while的根
static DWORD WINAPI ThreadFunc(LPVOID param)
{
	auto a = FindPatternByModule("CC 33 C0 4C 8D 0D ?? ?? ?? ?? 4C 8B DA");
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
	return true;
}

bool IsRun = run();