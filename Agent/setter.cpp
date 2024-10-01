#include "pch.h"
#include "main.h"
#include "scanner.h"

extern ADDRESS address;
extern Console console;
extern std::unordered_map<std::string, uintptr_t> refStringAddress;

std::vector<uintptr_t> keyBind0;
std::vector<uintptr_t> keyBind1;
std::vector<uintptr_t> avAgentCharacters;
std::vector<uintptr_t> avAgentGadgets;
std::vector<uintptr_t> avAgentAttackGadgets;
std::vector<uintptr_t> avAgentItems;
std::vector<uintptr_t> avAgentUnknown;
std::vector<uintptr_t> avAgentGadgetFishHoles;
std::vector<uintptr_t> avAgentGadgetResourceNodes;

void printOffset(const char* str, uintptr_t* addr) {
	long long offset = (long long)addr - ((long long)&address);
	console.printf("%s: 0x%X, %p\n", str, offset, *addr);
}
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
	uintptr_t keyBindsStart = refStringAddress["No valid case for switch variable 'EState'"] + 0x71;
	// Gw2-64.exe+59EE67 - lea rcx,[Gw2-64.exe+26EC0D0]
	uintptr_t keyBindsBase = FollowRelativeAddress(keyBindsStart);
	// Gw2-64.exe+59EE6E - call Gw2-64.exe+59C7C0
	uintptr_t loopFuncAddr = FollowRelativeAddress(keyBindsStart + 0x5);

	// Gw2-64.exe+59C8D8 - call qword ptr [rax+20] // call [[rsi]+20] (arg0=rsi,arg1=0|1)取得 是1號按鍵 還是2號按鍵
	// Gw2-64.exe+59C8D8 - Gw2-64.exe+5A3A3F - lea rax,[rcx+10]  // arg1 為0+10
	// Gw2-64.exe+59C8D8 - Gw2-64.exe+5A3A36 - lea rax,[rcx+60]  // arg1 為0+60
	uintptr_t invalidHint = refStringAddress["No valid case for switch variable 'EBind'"];
	int keyBindAddrOffset0 = (int)(*(char*)(invalidHint + 0x22));
	int keyBindAddrOffset1 = (int)(*(char*)(invalidHint + 0x19));
	// Gw2-64.exe+59CA8B - cmp ebp,000000E5  index 上限

	int max = *((int*)(refStringAddress["!(primaryEqual && secondaryEqual)"] + 0x55));
	keyBind0.assign(max, 0);
	keyBind1.assign(max, 0);
	for (int i = 0; i < max; i++)
	{

		// Gw2-64.exe+59C877 - lea rdx,[rsp+000000A8] arg1 index的pointer
		// Gw2-64.exe+59C87F - lea rcx,[r13+50] arg0 A+50
		// Gw2-64.exe+59C883 - call Gw2-64.exe+2B8A60 得到偏移值 帶入arg0,arg1
		char arg0_offset = *(char*)(loopFuncAddr + 0x96);
		uintptr_t getBase = FollowRelativeAddress(loopFuncAddr + 0x98);
		uintptr_t arg2 = 0;
		int offset = ((int(__thiscall*)(uintptr_t, int*, uintptr_t*))(getBase))(keyBindsBase + arg0_offset, &i, &arg2);
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

	printOffset("keyBind ary0", &address.keyBind0);
	printOffset("keyBind ary1", &address.keyBind1);

}
void SetMapStateAddr() {

	uintptr_t getMapStatePtr = FollowRelativeAddress(refStringAddress["ViewAdvanceUi"] + 0xA);
	// Gw2-64.exe+672763 - call Gw2-64.exe+6E15F0
	uintptr_t baseAddr = ((uintptr_t(__thiscall*)())getMapStatePtr)();
	uintptr_t func = *((uintptr_t*)(*(uintptr_t*)baseAddr + 0x8));
	// Gw2-64.exe+6D8886 - test byte ptr [rcx+18],01
	char offset = *(char*)(func + 0x8);
	address.mapState = baseAddr + offset;

	printOffset("map state", &address.mapState);

}
void SetLangAddr() {
	uintptr_t setLangFuncPtr = FollowRelativeAddress(refStringAddress["ValidateLanguage(language)"] + 0x24); //504a90
	//Gw2-64.exe+504A98 - call Gw2-64.exe+2432B0	
	auto getBase = (uintptr_t(__thiscall*)())FollowRelativeAddress(setLangFuncPtr + 0x9);
	//Gw2-64.exe+504A9D - mov rdx,[rax+50]
	int addrOffset1 = *(char*)(setLangFuncPtr + 0x10);
	//Gw2-64.exe+504AA1 - mov [rdx+00000334],ebx
	int addrOffset2 = *(int*)(setLangFuncPtr + 0x13);
	uintptr_t basePtr = getBase();
	uintptr_t base2Ptr = *(uintptr_t*)(basePtr + addrOffset1);
	address.lang = base2Ptr + addrOffset2;

	printOffset("lang", &address.lang);

}
void SetFishAddr() {
	uintptr_t getBaseAddr = FollowRelativeAddress(refStringAddress["!m_state.TestBits(FLAG_ENTER_GAME)"] + 0x55);
	uintptr_t baseAddr = ((uintptr_t(__thiscall*)())getBaseAddr)();

	// Gw2-64.exe+6D88FE - call qword ptr [rdx+40]
	int callOffset = (int)(*(char*)(refStringAddress["!m_state.TestBits(FLAG_ENTER_GAME)"] + 0x61));
	uintptr_t callAddr = *(uintptr_t*)((*(uintptr_t*)baseAddr) + callOffset);
	// Gw2-64.exe+7557BE - mov rcx,[rbx+000000D0]
	int chararcterOffset = *(int*)(callAddr + 0x61);
	uintptr_t selfCharacter = *(uintptr_t*)(baseAddr + chararcterOffset);
	uintptr_t chararcterFuncAry0 = *(uintptr_t*)selfCharacter;


	// Gw2-64.exe+128424D - call qword ptr [rax+000002C0]
	int chararcterFuncAry0_offset = *(int*)(refStringAddress["progressToCheck"] + 0x2C);
	uintptr_t getFishBase = *(uintptr_t*)(chararcterFuncAry0 + chararcterFuncAry0_offset);
	uintptr_t base = ((uintptr_t(__thiscall*)(uintptr_t))(getFishBase))(selfCharacter);

	// Gw2-64.exe+12B2187 - lea rcx,[rsi+00005C78]
	// Gw2-64.exe+12DCE18 - mov rcx,[rbx+28]
	int fishOffset = 0x5c78 + 0x28;
	address.fish = base + fishOffset;

	printOffset("fish", &address.fish);

	//F3 0F 5E 05 ?? ?? ?? ?? F3 0F 5E 05 ?? ?? ?? ?? E8 ?? ?? ?? ?? 33 ED 0F 2F 05 設定 fish state ready


	//auto getBase = (uintptr_t(__thiscall*)())(FollowRelativeAddress(refStringAddress["ViewAdvanceCharacter"] + 0xA));
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
// Gw2-64.exe+6B7A8F - call Gw2-64.exe+136CA50 // 陣列資訊 base
// Gw2-64.exe+136C816 - lea rcx,[rsi+68] 
// Gw2-64.exe+136C81F - call Gw2-64.exe+139A950
// Gw2-64.exe+136C81F - Gw2-64.exe+139A95F - mov rbx,[rcx+08]  // 取得第0個item
// Gw2-64.exe+139A965 - mov eax,[rcx+14]  //取得最大index
// Gw2-64.exe+139A9A5 - mov rcx,[rbx] //驗證有沒有內容物
// Gw2-64.exe+139A9B2 - mov rax,[rcx] //取得avagent 第0個的funcArray 
// Gw2-64.exe+139A9B5 - call qword ptr [rax+00000140] //取得type (arg0=avagent)


// Gw2-64.exe+139A9B5 - Gw2-64.exe+137B638 - add rcx,08 
// Gw2-64.exe+139A9B5 - Gw2-64.exe+137B63C - call qword ptr [rax+00000138] //avagent 第1個funcArray + 138(arg0為avagent+8)
// Gw2-64.exe+139A9B5 - Gw2-64.exe+137B5C0 - mov rcx,[rcx+000000B8] // 先取得 gadget 再取得agent
// Gw2-64.exe+57E8A5 - call qword ptr [rax+00000100] ///取得gadget type([gadget+200])，arg0 是gadget
// Gw2-64.exe+57E8AB - cmp eax,13 //為 node resource
// Gw2-64.exe+57E8BA - call qword ptr [rax+00000170] //取得resource node位置，arg0 是gadget
// Gw2-64.exe+57E8E1 - mov rdx,[rbx] //取結果內值 [gadget+4d8]
// Gw2-64.exe+57E8E7 - call qword ptr [rdx] 取得resource type，arg0 是gadget+4d8，得 [gadget+4d8+c]
// Gw2-64.exe+57E8E9 - cmp eax,03 //type 為0x3


void SetAvAgent() {
	avAgentCharacters.clear();
	avAgentGadgets.clear();
	avAgentGadgetFishHoles.clear();
	avAgentAttackGadgets.clear();
	avAgentItems.clear();
	avAgentUnknown.clear();
	avAgentGadgetResourceNodes.clear();

	uintptr_t loopFunc = FollowRelativeAddress(refStringAddress["avAgentArray"] + 0x16);
	// Gw2-64.exe+6B7A8F - call Gw2-64.exe+136CA50
	uintptr_t arrayInfo = FollowRelativeAddress(FollowRelativeAddress(refStringAddress["ViewAdvanceAgentView"] + 0xA) + 0x3);
	// Gw2-64.exe+136C816 - lea rcx,[rsi+68]
	// Gw2-64.exe+136C81F - call Gw2-64.exe+139A950
	// Gw2-64.exe+136C81F - Gw2-64.exe+139A95F - mov rbx,[rcx+08]  // 取得第0個item
	uintptr_t firstItem = *(uintptr_t*)(arrayInfo + 0x68 + 0x8);
	// Gw2-64.exe+139A965 - mov eax,[rcx+14]  //取得最大index
	int len = *(int*)(arrayInfo + 0x68 + 0x14);
	for (size_t i = 0; i < len; i++)
	{
		// Gw2-64.exe+139A9A5 - mov rcx,[rbx] //驗證有沒有內容物
		uintptr_t avAgent = *(uintptr_t*)(firstItem + i * 0x8);
		if (avAgent == 0) continue;
		// Gw2-64.exe+139A9B2 - mov rax,[rcx] //取得avagent 的funcArray 
		uintptr_t avAgentFuncAry = *(uintptr_t*)avAgent;
		// Gw2-64.exe+139A9B5 - call qword ptr [rax+00000140] //取得type (arg0=avagent)
		// D:\Perforce\Live\NAEU\v2\Code\Gw2\Game\AgentView\AvManager.cpp 第三個參考 +0x29
		// characterArray參考 +13 該func addr +65 ->call qword ptr [rax+00000140] 
		uintptr_t getAgentType = *(uintptr_t*)(avAgentFuncAry + 0x140);
		int type = ((int(__thiscall*)(uintptr_t))getAgentType)(avAgent);
		if (type == 0x0) {
			avAgentCharacters.push_back(avAgent);
		}
		else if (type == 0xA) {
			avAgentGadgets.push_back(avAgent);
			uintptr_t gadget = *(uintptr_t*)(avAgent + 0xC0);
			// resourceNode->GetGatherType() < Content::GATHERING_TYPES || markerFileId 參考 - 0x55 call 取得 resource (gadget+4d8) 
			// resourceNode->GetGatherType() < Content::GATHERING_TYPES || markerFileId 參考 - 0x1c call 取得 GatherType (resource+c) 
			int nodeType = *(int*)(gadget + 0x4d8 + 0xc);
			int nodeFlags = *(int*)(gadget + 0x4d8 + 0x10);
			int aliveFlag = 1 << 1;
			if ((aliveFlag & nodeFlags) > 0) {
				avAgentGadgetResourceNodes.push_back(avAgent);
				if (nodeType == 0x3) {
					avAgentGadgetFishHoles.push_back(avAgent);
				}
			}

		}
		else if (type == 0xB) {
			avAgentAttackGadgets.push_back(avAgent);
		}
		else if (type == 0xF) {
			avAgentItems.push_back(avAgent);
		}
		else {
			avAgentUnknown.push_back(avAgent);
		}

	}

	address.scanned = true;
	console.printf("avAgent scanned\n");
	if (address.avAgent0 == 0) {
		address.avAgent0 = (uintptr_t)&avAgentCharacters;
		address.avAgentA = (uintptr_t)&avAgentGadgets;
		address.avAgentB = (uintptr_t)&avAgentAttackGadgets;
		address.avAgentF = (uintptr_t)&avAgentItems;
		address.avAgentU = (uintptr_t)&avAgentUnknown;
		address.avAgentH = (uintptr_t)&avAgentGadgetFishHoles;
		address.avAgentR = (uintptr_t)&avAgentGadgetResourceNodes;

		printOffset("avAgent ary0", &address.avAgent0);
		printOffset("avAgent aryA", &address.avAgentA);
		printOffset("avAgent aryH", &address.avAgentH);
		printOffset("avAgent aryR", &address.avAgentR);
		printOffset("avAgent aryB", &address.avAgentB);
		printOffset("avAgent aryF", &address.avAgentF);
		printOffset("avAgent aryU", &address.avAgentU);
	}
}

void OnMapChange() {
	address.fish = 0;
	address.scanned = false;
}