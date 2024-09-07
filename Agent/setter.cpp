#include "pch.h"
#include "main.h"
#include "scanner.h"

extern ADDRESS address;
extern Console console;
extern std::unordered_map<std::string, uintptr_t> staticAddress;

void SetMapStateAddr() {

	uintptr_t getMapStatePtr = staticAddress["ViewAdvanceUi"]; //504a90
	uintptr_t baseAddr = ((uintptr_t(__thiscall*)())getMapStatePtr)();
	uintptr_t func = *((uintptr_t*)(*(uintptr_t*)baseAddr + 0x8));
	char offset = *(char*)(func + 0x8);
	address.mapState = baseAddr + offset;

	console.printf("map state: %p\n", address.mapState);

}
void SetLangAddr() {

	uintptr_t setLangFuncPtr = staticAddress["ValidateLanguage(language)"]; //504a90
	//Gw2-64.exe+504A98 - call Gw2-64.exe+2432B0	
	auto getBase = (uintptr_t(__thiscall*)())FollowRelativeAddress(setLangFuncPtr + 0x9);
	//Gw2-64.exe+504A9D - mov rdx,[rax+50]
	int addrOffset1 = *(char*)(setLangFuncPtr + 0x10);
	//Gw2-64.exe+504AA1 - mov [rdx+00000334],ebx
	int addrOffset2 = *(int*)(setLangFuncPtr + 0x13);
	uintptr_t basePtr = getBase();
	uintptr_t base2Ptr = *(uintptr_t*)(basePtr + addrOffset1);
	address.lang = base2Ptr + addrOffset2;
	
	console.printf("lang: %p\n", address.lang);

}
void SetFishAddr() {

	uintptr_t baseAddr = ((uintptr_t(__thiscall*)())staticAddress["!m_state.TestBits(FLAG_ENTER_GAME)"])();
	uintptr_t selfCharacter = *(uintptr_t*)(baseAddr + 0xD0);
	uintptr_t chararcterFuncAry1 = *(uintptr_t*)selfCharacter;
	// Gw2-64.exe+128424D - call qword ptr [rax+000002C0]
	uintptr_t getFishBase = *(uintptr_t*)(chararcterFuncAry1 + 0x2c0);
	uintptr_t base = ((uintptr_t(__thiscall*)(uintptr_t))(getFishBase))(selfCharacter);
	// Gw2-64.exe+12B2187 - lea rcx,[rsi+00005C78]
	// Gw2-64.exe+12DCE18 - mov rcx,[rbx+28]
	address.fish = base + 0x5c78 + 0x28;
	console.printf("fish: %p\n", address.fish);


	//auto getBase = (uintptr_t(__thiscall*)())(staticAddress["ViewAdvanceCharacter"]);
	//uintptr_t baseAddr = *(uintptr_t*)(getBase() + 0x98);
	//uintptr_t loopStartAddr = *(uintptr_t*)(baseAddr + 0x60);
	//uintptr_t loopEndAddr = loopStartAddr + (*(int*)(baseAddr + 0x6C)) * 8;
	//uintptr_t currentLoopAddr = loopStartAddr;
	//address.characterAry.clear();
	//address.selfCharacter = 0;
	//while (currentLoopAddr < loopEndAddr)
	//{
	//	// addr 裡面是一個ptr ary ,index 1的ptr  call 他帶rcx好像會取得 該charater 狀態 含pos
	//	uintptr_t* addr = (uintptr_t*)currentLoopAddr;
	//	if (*addr) {
	//		address.characterAry.push_back(*addr);
	//		uintptr_t _addr = (uintptr_t)*addr;  //_addr動態的
	//		//[[*ADDR + 08]+ 60]  (bool(__thiscall*)(uintptr_t))

	//		//(*ADDR + 08)
	//		auto isPlayer = (bool(__thiscall*)(uintptr_t))(*((uintptr_t*)(*((uintptr_t*)(_addr + 0x8)) + 0x60)));

	//		//7FF712C496B0
	//		if (isPlayer(_addr + 0x8)) {
	//			address.selfCharacter = _addr;

	//			uintptr_t __addr = *(uintptr_t*)_addr; //__addr 記憶體(可能是func)Ary 固定
	//			auto getNextPtr = (uintptr_t(__thiscall*)(uintptr_t))(*((uintptr_t*)(__addr + 0x2C0)));
	//			auto _base = getNextPtr(_addr);
	//			if (!_base) continue;
	//			//[[_base + 05C78 + 28]+18]會得到釣魚計算
	//			// _base + 05C78 + 28 內值會隨釣魚開始改變
	//			address.fish = *((uintptr_t*)(_base + 0x5CA0));

	//			auto a = address.fish + 0x18;

	//			console.printf("self %p\n", _addr);
	//			console.printf("fish %p\n", a);

	//		}

	//	}

	//	currentLoopAddr += 8;
	//}
}

void resetInstanceImpactedAddress() {
	address.fish = 0;
}