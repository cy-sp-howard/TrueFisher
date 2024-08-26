#include "pch.h"
#include <stdio.h>
#include <fstream>
#include <string>
#include "scanner.h"

// 
// 00 00 00 0? 00 00 00 55 36 11 00 00 00 00 00 (+3) LANG
// 設定LANG EXE+504a90 (ValidateLanguage(language)第一個參考 偏移+0x24 )
//尋找基址 EXE + 2432B0 得ax->AX + 50 取值設在DX  設定DX + 334 內值，DX + 334 內值為語言ID
//基址 + 50取內值 ，其值 + 334  得LANG地址
// 
//  
//exe + 59da30(func) 取得以下資料(得基址的基值)
//基址(EXE + 26ec0d0地址(分內值) + 50)跟偏移(EXE + 2B8A60(Func 188) 得出) 算出
// call exe + 59da30 時 rdx 決定 EXE + 2B8A60取的值
//也就是說 call exe + 59da30 時 rdx  會決定 基值得基質的值
// 2B8A60 只有RCX RDX會影響其值
//exe + 5a3a50 拿(基址的基指) + 10 取得基址
//取得基址後 + 24 得案件位置
// 
// 
// 片例所有案件
// exe 59c883開頭  遍例會取經過他好幾次取得計算值
//59c892 會取的偏移植
// 59c8d8 會取得(案件地址-24)
// 98c8E5  比較案件地址內容內容是否為目標案件
//若值找到 59c8ec 會跳轉到執行位置
//59c95c 會再次取得地址
//改地址 + 24 得到 按鈕地址
// 推測用來遍例取得
// 
// 
// 85 C0 75 F7 (gw2 module) while的根
static DWORD WINAPI ThreadFunc(LPVOID param)
{
	uintptr_t abc = 0x7FF6E040C0D0;
	auto keybindbase = (int(__thiscall*)(int,int,int, uintptr_t))FindPatternByModule("E8 E3 AA FF FF");
	int a = keybindbase(0, 0, 0, abc);
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