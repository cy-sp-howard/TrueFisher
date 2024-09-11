#include "pch.h"
#include <string>
#include "main.h"
#include "scanner.h"



// 
// 
// 85 C0 75 F7 (gw2 module) while的根
//
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
// 寫入 語言的 
// Gw2-64.exe+50AB3A - mov [rax+rcx*2],si


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
void mapLoadedCall() {

	if (address.mapState) {
		char mapState = *(char*)address.mapState;
		if (mapState == 0xF) {
			if (address.fish == 0) SetFishAddr();
			SetAvAgent();
		}
		else {
			OnMapChange();
		}
	}
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
		staticAddress["!m_state.TestBits(FLAG_ENTER_GAME)"] = FindReadonlyStringRef("!m_state.TestBits(FLAG_ENTER_GAME)");
		uintptr_t getMapStateBasePtr = FollowRelativeAddress(FindReadonlyStringRef("ViewAdvanceUi") + 0xA);
		staticAddress["ViewAdvanceUi"] = getMapStateBasePtr;
		uintptr_t keyBindLoopStart = FindReadonlyStringRef("No valid case for switch variable 'EState'") + 0x71;
		staticAddress["No valid case for switch variable 'EState'"] = keyBindLoopStart;
		staticAddress["No valid case for switch variable 'EBind'"] = FindReadonlyStringRef("No valid case for switch variable 'EBind'");
		staticAddress["progressToCheck"] = FindReadonlyStringRef("progressToCheck");
		staticAddress["!(primaryEqual && secondaryEqual)"] = FindReadonlyStringRef("!(primaryEqual && secondaryEqual)");
		staticAddress["avAgentArray"] = FindReadonlyStringRef("avAgentArray");
		staticAddress["ViewAdvanceAgentView"] = FindReadonlyStringRef("ViewAdvanceAgentView");
		


		SetMapStateAddr();
		SetLangAddr();
		SetKeyBindsAddr();
		address.ready = true;
		console.printf("ready\n");

	}
	mapLoadedCall();


}
static DWORD WINAPI SetHook(LPVOID param) {

	//Gw2-64.exe+671A3D - call Gw2-64.exe+1381DB0
	uintptr_t funcPtr = FollowRelativeAddress(FindReadonlyStringRef("ViewAdvanceDevice") + 0xa);
	uintptr_t resultPtr = FollowRelativeAddress(funcPtr + 0x3);

	while (true)
	{
		uintptr_t* cbPtrPtr = *(uintptr_t**)resultPtr;
		if (cbPtrPtr == 0) {
			Sleep(2000);
			continue;
		};
		hook.target = cbPtrPtr;
		hook.replaced = *cbPtrPtr;
		*cbPtrPtr = GetPtr((uintptr_t)GameLoopCB);
		break;
	}

	console.create("Debug");
	console.printf("dll base:%p\n", &__ImageBase);
	console.printf("address offset:%X\n", (long long)(&address) - (long long)(&__ImageBase));
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