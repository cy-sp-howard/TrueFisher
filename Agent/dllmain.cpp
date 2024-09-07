#include "pch.h"
#include <string>
#include "main.h"
#include "scanner.h"

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
// /[[rax+5c78]+28] call character第一個func array+2c0
// Gw2-64.exe+128424D - call qword ptr [rax+000002C0]
//Gw2 - 64.exe + 12B2187 - lea rcx, [rsi + 00005C78]
//Gw2-64.exe+12DCE18 - mov rcx,[rbx+28]
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
//
// 
// [rcx+18]判斷是否load地圖 0e load  ,0f loaded
// Gw2-64.exe+672763 - call Gw2-64.exe+6E15F0 得到rcx
// Gw2-64.exe+6D8886 - F6 41 18 01           - test byte ptr [rcx+18],01
// ViewAdvanceUi的參考 +0xA call實際位置  得到 result 
// [result] 去此func 的實際位置+0x8 取得偏移
// result+偏移 此位置可判斷load地圖


EXTERN_C IMAGE_DOS_HEADER __ImageBase;
ADDRESS address;
Console console;
std::unordered_map<std::string, uintptr_t> staticAddress;

struct {
	uintptr_t* target = 0;
	uintptr_t replaced = 0;
} hook;
std::vector<uintptr_t> wrapper;

uintptr_t GetPtr(uintptr_t addr) {
	wrapper.push_back(addr);

	long long index = wrapper.size() - 1;
	return 	(uintptr_t)(&(wrapper.data()[index]));
}
void __fastcall GameLoopCB(uintptr_t ptr, int time, uintptr_t zero) {
	uintptr_t replacedCB = *((uintptr_t*)(hook.replaced));
	((uintptr_t(__thiscall*)(uintptr_t, int, uintptr_t))replacedCB)(ptr, time, zero);
	if (!address.ready) {
		// Gw2-64.exe+5C252D - call Gw2-64.exe+504A90
		uintptr_t setLangFuncPtr = FollowRelativeAddress(FindReadonlyStringRef("ValidateLanguage(language)") + 0x24);
		staticAddress["ValidateLanguage(language)"] = setLangFuncPtr;
		uintptr_t getCharacterBasePtr = FollowRelativeAddress(FindReadonlyStringRef("ViewAdvanceCharacter") + 0xA);
		staticAddress["ViewAdvanceCharacter"] = getCharacterBasePtr;
		uintptr_t getSelfCharacterBasePtr = FollowRelativeAddress(FindReadonlyStringRef("!m_state.TestBits(FLAG_ENTER_GAME)") + 0x55);
		staticAddress["!m_state.TestBits(FLAG_ENTER_GAME)"] = getSelfCharacterBasePtr;
		uintptr_t getMapStateBasePtr = FollowRelativeAddress(FindReadonlyStringRef("ViewAdvanceUi") + 0xA);
		staticAddress["ViewAdvanceUi"] = getMapStateBasePtr;


		SetMapStateAddr();
		SetLangAddr();
		address.ready = true;
		console.printf("ready\n");

	}

	if (address.mapState) {
		char mapState = *(char*)address.mapState;
		if (mapState == 0xF) {
			if (address.fish == 0) SetFishAddr();
		}
		else {
			resetInstanceImpactedAddress();
		}
	}

}
static DWORD WINAPI SetHook(LPVOID param) {
	console.create("Debug");
	console.printf("dll base:%p\n", &__ImageBase);
	console.printf("address offset:%X\n", (long long)(&address) - (long long)(&__ImageBase));
	//Gw2-64.exe+671A3D - call Gw2-64.exe+1381DB0
	uintptr_t funcPtr = FollowRelativeAddress(FindReadonlyStringRef("ViewAdvanceDevice") + 0xa);
	uintptr_t resultPtr = FollowRelativeAddress(funcPtr + 0x3);
	uintptr_t* cbPtrPtr = *(uintptr_t**)resultPtr;
	if (cbPtrPtr == 0) return -1;
	hook.target = cbPtrPtr;
	hook.replaced = *cbPtrPtr;
	*cbPtrPtr = GetPtr((uintptr_t)GameLoopCB);
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
	if (hook.replaced == 0) return;
	*(hook.target) = hook.replaced;
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