#include "pch.h"
#include <stdio.h>
#include <fstream>
#include <string>
#include <unordered_map>
#include <vector>
#include "scanner.h"
#include "hooker.h"
#include "console.h"

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
//
//
// 滑鼠座標轉世界座標(英寸)
// ViewAdvanceAgentSelect的參考+0xA 取得位置+0x3 follow 得位置再+300( "Gw2-64.exe"+279FD90)
//
//
// ViewAdvanceModel
//[exe + 2750658 + 8 + 8] 取得rdi初始位置 exe+Ea6e63
//[rdi + 30 + 8] 為下一個 rdi
//迴圈得值 rdi == base
//
//[base + 1b0] == r9   //exe+ef023c
//[[base + 1b0]+ 8] == model base
//
//[[base + 1b0]+ 8] + 104 就是 pos 計算後的值
//[r9 + 28] 為model pos的位置
//
//
// 掃描計算過物件位置與距離
//(player 是頭的座標進來
//[exe + 2750658 + 8 + 8] 取得rdi初始位置
//[rdi + 30 + 8] 為下一個 rdi
//迴圈得值 rdi == base
//
//[base + 1b0] == r9   //exe+ef023c
//[[base + 1b0]+ 8] == model base
//
//[[base + 1b0]+ 8] + 104 就是 pos 計算後的值
//[[base + 1b0]+ 8] + b4 為與本人的距離
//[[base + 1b0]+ 28] 為原始 pos(character + 480)的位置
//
//
//exe + DC89E0這是英尺轉公尺的func
//
//Gw2-64.exe+46F310  去得名字 (invalid character name)



struct ADDRESS {
	std::string ImHere;
	bool ready = false;
	uintptr_t* hookTarget = 0;
	uintptr_t replacedCBPtr = 0;
	uintptr_t langPtr = 0;
	uintptr_t fishPtr = 0;
	std::vector<uintptr_t> characterAry;
	uintptr_t selfCharacterPtr = 0;
};

struct character {

};

std::unordered_map<std::string, uintptr_t> staticAddrees;
std::vector<uintptr_t> wrapper;
ADDRESS address;
Console m_con;


void SetLangAddr() {

	uintptr_t setLangFuncPtr = staticAddrees["ValidateLanguage(language)"]; //504a90
	//Gw2-64.exe+504A98 - call Gw2-64.exe+2432B0	
	auto getBase = (uintptr_t(__thiscall*)())FollowRelativeAddress(setLangFuncPtr + 0x9);
	//Gw2-64.exe+504A9D - mov rdx,[rax+50]
	int addrOffset1 = *(char*)(setLangFuncPtr + 0x10);
	//Gw2-64.exe+504AA1 - mov [rdx+00000334],ebx
	int addrOffset2 = *(int*)(setLangFuncPtr + 0x13);
	uintptr_t basePtr = getBase();
	uintptr_t base2Ptr = *(uintptr_t*)(basePtr + addrOffset1);
	address.langPtr = base2Ptr + addrOffset2;

}
void SetFishAddr() {

	auto getBase = (uintptr_t(__thiscall*)())(staticAddrees["ViewAdvanceCharacter"]);
	uintptr_t baseAddr = *(uintptr_t*)(getBase() + 0x98);
	uintptr_t loopStartAddr = *(uintptr_t*)(baseAddr + 0x60);
	uintptr_t loopEndAddr = loopStartAddr + (*(int*)(baseAddr + 0x6C)) * 8;
	uintptr_t currentLoopAddr = loopStartAddr;
	address.characterAry.clear();
	address.selfCharacterPtr = 0;
	while (currentLoopAddr < loopEndAddr)
	{
		// addr 裡面是一個ptr ary ,index 1的ptr  call 他帶rcx好像會取得 該charater 狀態 含pos
		uintptr_t* addr = (uintptr_t*)currentLoopAddr;
		if (*addr) {
			address.characterAry.push_back(*addr);
			uintptr_t _addr = (uintptr_t)*addr;  //_addr動態的
			//[[*ADDR + 08]+ 60]  (bool(__thiscall*)(uintptr_t))

			//(*ADDR + 08)
			auto isPlayer = (bool(__thiscall*)(uintptr_t))(*((uintptr_t*)(*((uintptr_t*)(_addr + 0x8)) + 0x60)));

			//7FF712C496B0
			if (isPlayer(_addr + 0x8)) {
				address.selfCharacterPtr = _addr;

				uintptr_t __addr = *(uintptr_t*)_addr; //__addr 記憶體(可能是func)Ary 固定
				auto getNextPtr = (uintptr_t(__thiscall*)(uintptr_t))(*((uintptr_t*)(__addr + 0x2C0)));
				auto _base = getNextPtr(_addr);
				if (!_base) continue;
				//[[_base + 05C78 + 28]+18]會得到釣魚計算
				// _base + 05C78 + 28 內值會隨釣魚開始改變
				address.fishPtr = *((uintptr_t*)(_base + 0x5CA0));

				auto a = address.fishPtr + 0x18;

				m_con.printf("self %p\n", _addr);
				m_con.printf("fish %p\n", a);

			}

		}

		currentLoopAddr += 8;
	}



}

uintptr_t getPtr(uintptr_t addr) {
	wrapper.push_back(addr);

	long long index = wrapper.size() - 1;
	return 	(uintptr_t)(&(wrapper.data()[index]));
}
void __fastcall GameLoopCB(uintptr_t ptr, int time, uintptr_t zero) {
	uintptr_t replacedCB = *((uintptr_t*)(address.replacedCBPtr));
	((uintptr_t(__thiscall*)(uintptr_t, int, uintptr_t))replacedCB)(ptr, time, zero);
	if (!address.ready) {
		// Gw2-64.exe+5C252D - call Gw2-64.exe+504A90
		uintptr_t setLangFuncPtr = FollowRelativeAddress(FindReadonlyStringRef("ValidateLanguage(language)") + 0x24);
		staticAddrees["ValidateLanguage(language)"] = setLangFuncPtr;
		uintptr_t getCharacterBasePtr = FollowRelativeAddress(FindReadonlyStringRef("ViewAdvanceCharacter") + 0xA);
		staticAddrees["ViewAdvanceCharacter"] = getCharacterBasePtr;

		SetLangAddr();
		m_con.create("debug");
		address.ready = true;
		m_con.printf("ready\n");

	}
	SetFishAddr();
}

static DWORD WINAPI SetHook(LPVOID param) {
	address.ImHere = "HERE";
	//Gw2-64.exe+671A3D - call Gw2-64.exe+1381DB0
	uintptr_t funcPtr = FollowRelativeAddress(FindReadonlyStringRef("ViewAdvanceDevice") + 0xa);
	uintptr_t resultPtr = FollowRelativeAddress(funcPtr + 0x3);
	uintptr_t* cbPtrPtr = *(uintptr_t**)resultPtr;
	if (cbPtrPtr == 0) return -1;
	address.hookTarget = cbPtrPtr;
	address.replacedCBPtr = *cbPtrPtr;
	*cbPtrPtr = getPtr((uintptr_t)GameLoopCB);


	return 0;
}

void mount()
{


	HANDLE hThread = CreateThread(NULL, 0, SetHook, NULL, 0, NULL);
	if (hThread == NULL)
	{
		throw std::runtime_error(std::string("CreateThread failed with code ") + std::to_string(GetLastError()));
	}
	else
	{
		CloseHandle(hThread);
	}
}
void unmount() {
	if (address.replacedCBPtr == 0) return;
	*(address.hookTarget) = address.replacedCBPtr;
}


BOOL APIENTRY DllMain(HMODULE hModule,
	DWORD  ul_reason_for_call,
	LPVOID lpReserved
)
{
	switch (ul_reason_for_call)
	{
	case DLL_PROCESS_ATTACH:
		mount();
		break;
	case DLL_THREAD_ATTACH:
		break;
	case DLL_THREAD_DETACH:
		break;
	case DLL_PROCESS_DETACH:
		unmount();
		break;
	}
	return TRUE;
}