#include "pch.h"
#include "main.h"
#include "scanner.h"

extern ADDRESS address;
extern Console console;
extern std::unordered_map<std::string, uintptr_t> staticAddress;

std::vector<uintptr_t> keyBind0;
std::vector<uintptr_t> keyBind1;

// No valid case for switch variable 'EState' 取參考此地址的function +0x78 進入call目標地址 + D2 得(59c892 會取的偏移植)
// Gw2-64.exe+59EE67 - lea rcx,[Gw2-64.exe+26EC0D0] 取得 固定值A
// Gw2-64.exe+59EE6E - call Gw2-64.exe+59C7C0
// Gw2-64.exe+59C877 - lea rdx,[rsp+000000A8] arg1 index的pointer
// Gw2-64.exe+59C87F - lea rcx,[r13+50] arg0 A+50
// Gw2-64.exe+59C883 - call Gw2-64.exe+2B8A60 得到偏移值 帶入arg0,arg1 得result
// Gw2-64.exe+59C88A - mov rax,[r13+58] //[A+58]
// Gw2-64.exe+59C88E - lea rdx,[rcx+rcx*2]  //result == rcx
// Gw2-64.exe+59C892 - mov rsi,[rax+rdx*8+08]  [rsi+34]得按鍵code
// Gw2-64.exe+59C8D8 - call qword ptr [rax+20] // call [[rsi]+20] (arg0=rsi,arg1=0|1)取得 是1號按鍵 還是2號按鍵
// Gw2-64.exe+59C8D8 - Gw2-64.exe+5A3A3F - lea rax,[rcx+10]  // arg1 為0+10
// Gw2-64.exe+59C8D8 - Gw2-64.exe+5A3A36 - lea rax,[rcx+60]  // arg1 為0+60
// Gw2-64.exe+59C8E5 - Gw2-64.exe+5A2E01 - mov eax,[rcx+24] // 讀取按鍵code([rcx+10+24] 或[rcx+60+24])
// Gw2-64.exe+59CA8B - cmp ebp,000000E5  index 上限
void SetKeyBindsAddr() {
	uintptr_t keyBindsStart = staticAddress["No valid case for switch variable 'EState'"];
	// Gw2-64.exe+59EE67 - lea rcx,[Gw2-64.exe+26EC0D0]
	uintptr_t keyBindsBase = FollowRelativeAddress(keyBindsStart);
	// Gw2-64.exe+59EE6E - call Gw2-64.exe+59C7C0
	uintptr_t loopFuncAddr = FollowRelativeAddress(keyBindsStart + 0x5);

	// Gw2-64.exe+59C8D8 - call qword ptr [rax+20] // call [[rsi]+20] (arg0=rsi,arg1=0|1)取得 是1號按鍵 還是2號按鍵
	// Gw2-64.exe+59C8D8 - Gw2-64.exe+5A3A3F - lea rax,[rcx+10]  // arg1 為0+10
	// Gw2-64.exe+59C8D8 - Gw2-64.exe+5A3A36 - lea rax,[rcx+60]  // arg1 為0+60
	uintptr_t invalidHint = staticAddress["No valid case for switch variable 'EBind'"];
	int keyBindAddrOffset0 = (int)(*(char*)(invalidHint + 0x22));
	int keyBindAddrOffset1 = (int)(*(char*)(invalidHint + 0x19));
	// Gw2-64.exe+59CA8B - cmp ebp,000000E5  index 上限
	int max = *((int*)(loopFuncAddr + 0x2CD));
	keyBind0.assign(max, 0);
	keyBind1.assign(max, 0);
	for (int i = 0; i < max; i++)
	{

		// Gw2-64.exe+59C877 - lea rdx,[rsp+000000A8] arg1 index的pointer
		// Gw2-64.exe+59C87F - lea rcx,[r13+50] arg0 A+50
		// Gw2-64.exe+59C883 - call Gw2-64.exe+2B8A60 得到偏移值 帶入arg0,arg1
		uintptr_t getBase = FollowRelativeAddress(loopFuncAddr + 0xC4);
		uintptr_t arg2 = 0;
		int offset = ((int(__thiscall*)(uintptr_t, int*, uintptr_t*))(getBase))(keyBindsBase + 0x50, &i, &arg2);
		// Gw2-64.exe+59C88A - mov rax,[r13+58] //[A+58]
		// Gw2-64.exe+59C88E - lea rdx,[rcx+rcx*2]  //result == rcx
		// Gw2-64.exe+59C892 - mov rsi,[rax+rdx*8+08]  [rsi+34]得按鍵code
		uintptr_t beforeOffset = *(uintptr_t*)(keyBindsBase + 0x58);
		uintptr_t keyBindAddrBase = *(uintptr_t*)(beforeOffset + (offset + offset * 0x2) * 0x8 + 0x8);
		// Gw2-64.exe+59C8D8 - Gw2-64.exe+5A3A3F - lea rax,[rcx+10]  // arg1 為0+10
		// Gw2-64.exe+59C8D8 - Gw2-64.exe+5A3A36 - lea rax,[rcx+60]  // arg1 為0+60
		// Gw2-64.exe+59C8E5 - Gw2-64.exe+5A2E01 - mov eax,[rcx+24] // 讀取按鍵code([rcx+10+24] 或[rcx+60+24])
		keyBind0[i] = keyBindAddrBase + keyBindAddrOffset0 + 0x24;
		keyBind1[i] = keyBindAddrBase + keyBindAddrOffset1 + 0x24;

	}
	address.keyBind0 = (uintptr_t)keyBind0.data();
	address.keyBind1 = (uintptr_t)keyBind1.data();
	console.printf("keyBind ary0: %p\n", address.keyBind0);
	console.printf("keyBind ary1: %p\n", address.keyBind1);

}
void SetMapStateAddr() {

	uintptr_t getMapStatePtr = staticAddress["ViewAdvanceUi"];
	// Gw2-64.exe+672763 - call Gw2-64.exe+6E15F0
	uintptr_t baseAddr = ((uintptr_t(__thiscall*)())getMapStatePtr)();
	uintptr_t func = *((uintptr_t*)(*(uintptr_t*)baseAddr + 0x8));
	// Gw2-64.exe+6D8886 - test byte ptr [rcx+18],01
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
	uintptr_t getBaseAddr = FollowRelativeAddress(staticAddress["!m_state.TestBits(FLAG_ENTER_GAME)"] + 0x55);
	uintptr_t baseAddr = ((uintptr_t(__thiscall*)())getBaseAddr)();

	// Gw2-64.exe+6D88FE - call qword ptr [rdx+40]
	int callOffset = (int)(*(char*)(staticAddress["!m_state.TestBits(FLAG_ENTER_GAME)"] + 0x61));
	uintptr_t callAddr = *(uintptr_t*)((*(uintptr_t*)baseAddr) + callOffset);
	// Gw2-64.exe+7557BE - mov rcx,[rbx+000000D0]
	int chararcterOffset = *(int*)(callAddr + 0x61);
	uintptr_t selfCharacter = *(uintptr_t*)(baseAddr + chararcterOffset);
	uintptr_t chararcterFuncAry0 = *(uintptr_t*)selfCharacter;

	
	// Gw2-64.exe+128424D - call qword ptr [rax+000002C0]
	int chararcterFuncAry0_offset = *(int*)(staticAddress["progressToCheck"] + 0x2C);
	uintptr_t getFishBase = *(uintptr_t*)(chararcterFuncAry0 + chararcterFuncAry0_offset);
	uintptr_t base = ((uintptr_t(__thiscall*)(uintptr_t))(getFishBase))(selfCharacter);

	// Gw2-64.exe+12B2187 - lea rcx,[rsi+00005C78]
	// Gw2-64.exe+12DCE18 - mov rcx,[rbx+28]
	int fishOffset = 0x5c78 + 0x28;
	address.fish = base + fishOffset;
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