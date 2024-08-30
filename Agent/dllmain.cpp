#include "pch.h"
#include <stdio.h>
#include <fstream>
#include <string>
#include "scanner.h"
#include "hooker.h"

// 
// 
// 
// MatchIsAcceptable(m_match) 取參考此地址的function +0x13 得 call exe + 59da30 的地址
//exe + 59da30(func) 取得以下資料(得基址的基值)
//基址(EXE + 26ec0d0地址(分內值) + 50)跟偏移(EXE + 2B8A60(Func 188) 得出) 算出
// call exe + 59da30 時 rdx 決定 EXE + 2B8A60取的值
//也就是說 call exe + 59da30 時 rdx  會決定 基值得基質的值
// 2B8A60 只有RCX RDX會影響其值
//exe + 5a3a50 拿(基址的基指) + 10 取得基址
//取得基址後 + 24 得案件位置
// 
// 
// No valid case for switch variable 'EState' 取參考此地址的function +0x78 進入call目標地址 + D2 得(59c892 會取的偏移植)
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
//
// 12dce26 準被call finsh ready func
// 掃agent?
//  Gw2-64.exe+2432B0 { 取得"甲" }      
// [ [甲+98]+60]  初始值
// [[[甲+98]+60]+[[[甲+98]+6C]*8] 最後值
//[[甲 + 98] + 60 + 8 * N] 為有值
//[[[甲 + 98] + 60 + 8 * N]]+5CA0 為釣魚地址


struct ADDRESS {
	std::string ImHere = "Im Here";
	bool ready = false;
	uintptr_t langPtr = 0;
	uintptr_t fishPtr = 0;
	std::vector<uintptr_t> characterAry;
	uintptr_t selfCharacterPtr = 0;
};
ADDRESS address;

struct character {

};

void SetLangAddr() {
	uintptr_t setLangFuncPtr = FollowRelativeAddress(FindReadonlyString("ValidateLanguage(language)") + 0x24);
	auto getBase = (uintptr_t(__thiscall*)())FollowRelativeAddress(setLangFuncPtr + 0x9);
	int addrOffset1 = *(char*)(setLangFuncPtr + 0x10);
	int addrOffset2 = *(int*)(setLangFuncPtr + 0x13);
	uintptr_t basePtr = getBase();
	uintptr_t base2Ptr = *(uintptr_t*)(basePtr + addrOffset1);
	address.langPtr = base2Ptr + addrOffset2;

}
void SetFishAddr() {
	auto getBase = (uintptr_t(__thiscall*)())FollowRelativeAddress(FindReadonlyString("ViewAdvanceCharacter") + 0xA);
	uintptr_t baseAddr = *(uintptr_t*)(getBase() + 0x98);
	uintptr_t loopStartAddr = *(uintptr_t*)(baseAddr + 0x60);
	uintptr_t loopEndAddr = loopStartAddr + (*(int*)(baseAddr + 0x6C)) * 8;
	uintptr_t currentLoopAddr = loopStartAddr;
	address.characterAry.clear();
	address.selfCharacterPtr = 0;
	while (currentLoopAddr <= loopEndAddr)
	{
		// addr 裡面是一個ptr ary ,index 1的ptr  call 他帶rcx好像會取得 該charater 狀態 含pos
		uintptr_t* addr = (uintptr_t*)currentLoopAddr;
		if (*addr) {
			address.characterAry.push_back(*addr);
			uintptr_t _addr = (uintptr_t)*addr;  //_addr動態的
			//[[*ADDR + 08]+ 60]  (bool(__thiscall*)(uintptr_t))

			//(*ADDR + 08)
			auto isSelf = (bool(__thiscall*)(uintptr_t))(*((uintptr_t*)(*((uintptr_t*)(_addr + 0x8)) + 0x60)));
			//7FF712C496B0
			if (isSelf(_addr + 0x8)) {
				address.selfCharacterPtr = _addr;

				uintptr_t __addr = *(uintptr_t*)_addr; //__addr 記憶體(可能是func)Ary 固定
				auto getNextPtr = (uintptr_t (__thiscall*)(uintptr_t))(*((uintptr_t*)(__addr + 0x2C0))); 
				auto _base = getNextPtr(_addr);
				if (!_base) continue;
				address.fishPtr = *((uintptr_t*)(_base + 0x5CA0));

				auto a = address.fishPtr + 0x18;
				int b = 1;

			}

		}

		currentLoopAddr += 8;
	}



}

void __fastcall GameLoopCB() {
	if (!address.ready) {
		SetLangAddr();
		address.ready = true;
	}
	SetFishAddr();
}

template <typename T>
T* Wrapper(T& target) {
	T* result = &target;
	return result;
}

Hooker m_hooker;
static DWORD WINAPI SetHook(LPVOID param) {

	uintptr_t funcPtr = FollowRelativeAddress(FindReadonlyString("ViewAdvanceDevice") + 0xa);
	uintptr_t cbPtrSpace = FollowRelativeAddress(funcPtr + 0x3);
	m_hooker.hookVT(*(uintptr_t*)cbPtrSpace, 0, (uintptr_t)GameLoopCB);


	return 0;
}

bool run()
{


	HANDLE hThread = CreateThread(NULL, 0, SetHook, NULL, 0, NULL);
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