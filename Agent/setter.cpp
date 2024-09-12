#include "pch.h"
#include "main.h"
#include "scanner.h"

extern ADDRESS address;
extern Console console;
extern std::unordered_map<std::string, uintptr_t> staticAddress;

std::vector<uintptr_t> keyBind0;
std::vector<uintptr_t> keyBind1;
std::vector<uintptr_t> avAgentCharacters;
std::vector<uintptr_t> avAgentGadgets;
std::vector<uintptr_t> avAgentAttackGadgets;
std::vector<uintptr_t> avAgentItems;
std::vector<uintptr_t> avAgentUnknown;
std::vector<uintptr_t> avAgentGadgetFishHoles;

// No valid case for switch variable 'EState' ���ѦҦ��a�}��function +0x78 �i�Jcall�ؼЦa�} + D2 �o(59c892 �|����������)
// Gw2-64.exe+59EE67 - lea rcx,[Gw2-64.exe+26EC0D0] ���o �T�w��A
// Gw2-64.exe+59EE6E - call Gw2-64.exe+59C7C0
// Gw2-64.exe+59C877 - lea rdx,[rsp+000000A8] arg1 index��pointer
// Gw2-64.exe+59C87F - lea rcx,[r13+50] arg0 A+50
// Gw2-64.exe+59C883 - call Gw2-64.exe+2B8A60 �o�찾���� �a�Jarg0,arg1 �oresult
// Gw2-64.exe+59C88A - mov rax,[r13+58] //[A+58]
// Gw2-64.exe+59C88E - lea rdx,[rcx+rcx*2]  //result == rcx
// Gw2-64.exe+59C892 - mov rsi,[rax+rdx*8+08]  [rsi+34]�o����code
// Gw2-64.exe+59C8D8 - call qword ptr [rax+20] // call [[rsi]+20] (arg0=rsi,arg1=0|1)���o �O1������ �٬O2������
// Gw2-64.exe+59C8D8 - Gw2-64.exe+5A3A3F - lea rax,[rcx+10]  // arg1 ��0+10
// Gw2-64.exe+59C8D8 - Gw2-64.exe+5A3A36 - lea rax,[rcx+60]  // arg1 ��0+60
// Gw2-64.exe+59C8E5 - Gw2-64.exe+5A2E01 - mov eax,[rcx+24] // Ū������code([rcx+10+24] ��[rcx+60+24])
// Gw2-64.exe+59CA8B - cmp ebp,000000E5  index �W��
void SetKeyBindsAddr() {
	uintptr_t keyBindsStart = staticAddress["No valid case for switch variable 'EState'"];
	// Gw2-64.exe+59EE67 - lea rcx,[Gw2-64.exe+26EC0D0]
	uintptr_t keyBindsBase = FollowRelativeAddress(keyBindsStart);
	// Gw2-64.exe+59EE6E - call Gw2-64.exe+59C7C0
	uintptr_t loopFuncAddr = FollowRelativeAddress(keyBindsStart + 0x5);

	// Gw2-64.exe+59C8D8 - call qword ptr [rax+20] // call [[rsi]+20] (arg0=rsi,arg1=0|1)���o �O1������ �٬O2������
	// Gw2-64.exe+59C8D8 - Gw2-64.exe+5A3A3F - lea rax,[rcx+10]  // arg1 ��0+10
	// Gw2-64.exe+59C8D8 - Gw2-64.exe+5A3A36 - lea rax,[rcx+60]  // arg1 ��0+60
	uintptr_t invalidHint = staticAddress["No valid case for switch variable 'EBind'"];
	int keyBindAddrOffset0 = (int)(*(char*)(invalidHint + 0x22));
	int keyBindAddrOffset1 = (int)(*(char*)(invalidHint + 0x19));
	// Gw2-64.exe+59CA8B - cmp ebp,000000E5  index �W��

	int max = *((int*)(staticAddress["!(primaryEqual && secondaryEqual)"] + 0x55));
	keyBind0.assign(max, 0);
	keyBind1.assign(max, 0);
	for (int i = 0; i < max; i++)
	{

		// Gw2-64.exe+59C877 - lea rdx,[rsp+000000A8] arg1 index��pointer
		// Gw2-64.exe+59C87F - lea rcx,[r13+50] arg0 A+50
		// Gw2-64.exe+59C883 - call Gw2-64.exe+2B8A60 �o�찾���� �a�Jarg0,arg1
		char arg0_offset = *(char*)(loopFuncAddr + 0x96);
		uintptr_t getBase = FollowRelativeAddress(loopFuncAddr + 0x98);
		uintptr_t arg2 = 0;
		int offset = ((int(__thiscall*)(uintptr_t, int*, uintptr_t*))(getBase))(keyBindsBase + arg0_offset, &i, &arg2);
		// Gw2-64.exe+59C88A - mov rax,[r13+58] //[A+58]
		// Gw2-64.exe+59C88E - lea rdx,[rcx+rcx*2]  //result == rcx
		// Gw2-64.exe+59C892 - mov rsi,[rax+rdx*8+08]  [rsi+34]�o����code
		uintptr_t beforeOffset = *(uintptr_t*)(keyBindsBase + 0x58);
		uintptr_t keyBindAddrBase = *(uintptr_t*)(beforeOffset + (offset + offset * 0x2) * 0x8 + 0x8);
		// Gw2-64.exe+59C8D8 - Gw2-64.exe+5A3A3F - lea rax,[rcx+10]  // arg1 ��0+10
		// Gw2-64.exe+59C8D8 - Gw2-64.exe+5A3A36 - lea rax,[rcx+60]  // arg1 ��0+60
		// Gw2-64.exe+59C8E5 - Gw2-64.exe+5A2E01 - mov eax,[rcx+24] // Ū������code([rcx+10+24] ��[rcx+60+24])
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

	//F3 0F 5E 05 ?? ?? ?? ?? F3 0F 5E 05 ?? ?? ?? ?? E8 ?? ?? ?? ?? 33 ED 0F 2F 05 �]�w fish state ready


	//auto getBase = (uintptr_t(__thiscall*)())(staticAddress["ViewAdvanceCharacter"]);
	//uintptr_t baseAddr = *(uintptr_t*)(getBase() + 0x98);
	//uintptr_t loopStartAddr = *(uintptr_t*)(baseAddr + 0x60);
	//uintptr_t loopEndAddr = loopStartAddr + (*(int*)(baseAddr + 0x6C)) * 8;
	//uintptr_t currentLoopAddr = loopStartAddr;
	//address.characterAry.clear();
	//address.selfCharacter = 0;
	//while (currentLoopAddr < loopEndAddr)
	//{
	//	// addr �̭��O�@��ptr ary ,index 1��ptr  call �L�arcx�n���|���o ��charater ���A �tpos
	//	uintptr_t* addr = (uintptr_t*)currentLoopAddr;
	//	if (*addr) {
	//		address.characterAry.push_back(*addr);
	//		uintptr_t _addr = (uintptr_t)*addr;  //_addr�ʺA��
	//		//[[*ADDR + 08]+ 60]  (bool(__thiscall*)(uintptr_t))

	//		//(*ADDR + 08)
	//		auto isPlayer = (bool(__thiscall*)(uintptr_t))(*((uintptr_t*)(*((uintptr_t*)(_addr + 0x8)) + 0x60)));

	//		//7FF712C496B0
	//		if (isPlayer(_addr + 0x8)) {
	//			address.selfCharacter = _addr;

	//			uintptr_t __addr = *(uintptr_t*)_addr; //__addr �O����(�i��Ofunc)Ary �T�w
	//			auto getNextPtr = (uintptr_t(__thiscall*)(uintptr_t))(*((uintptr_t*)(__addr + 0x2C0)));
	//			auto _base = getNextPtr(_addr);
	//			if (!_base) continue;
	//			//[[_base + 05C78 + 28]+18]�|�o�쳨���p��
	//			// _base + 05C78 + 28 ���ȷ|�H�����}�l����
	//			address.fish = *((uintptr_t*)(_base + 0x5CA0));

	//			auto a = address.fish + 0x18;

	//			console.printf("self %p\n", _addr);
	//			console.printf("fish %p\n", a);

	//		}

	//	}

	//	currentLoopAddr += 8;
	//}
}
// Gw2-64.exe+6B7A8F - call Gw2-64.exe+136CA50 // �}�C��T base
// Gw2-64.exe+136C816 - lea rcx,[rsi+68] 
// Gw2-64.exe+136C81F - call Gw2-64.exe+139A950
// Gw2-64.exe+136C81F - Gw2-64.exe+139A95F - mov rbx,[rcx+08]  // ���o��0��item
// Gw2-64.exe+139A965 - mov eax,[rcx+14]  //���o�̤jindex
// Gw2-64.exe+139A9A5 - mov rcx,[rbx] //���Ҧ��S�����e��
// Gw2-64.exe+139A9B2 - mov rax,[rcx] //���oavagent ��0�Ӫ�funcArray 
// Gw2-64.exe+139A9B5 - call qword ptr [rax+00000140] //���otype (arg0=avagent)


// Gw2-64.exe+139A9B5 - Gw2-64.exe+137B638 - add rcx,08 
// Gw2-64.exe+139A9B5 - Gw2-64.exe+137B63C - call qword ptr [rax+00000138] //avagent ��1��funcArray + 138(arg0��avagent+8)
// Gw2-64.exe+139A9B5 - Gw2-64.exe+137B5C0 - mov rcx,[rcx+000000B8] // �����o gadget �A���oagent
// Gw2-64.exe+57E8A5 - call qword ptr [rax+00000100] ///���ogadget type([gadget+200])�Aarg0 �Ogadget
// Gw2-64.exe+57E8AB - cmp eax,13 //�� node resource
// Gw2-64.exe+57E8BA - call qword ptr [rax+00000170] //���oresource node��m�Aarg0 �Ogadget
// Gw2-64.exe+57E8E1 - mov rdx,[rbx] //�����G���� [gadget+4d8]
// Gw2-64.exe+57E8E7 - call qword ptr [rdx] ���oresource type�Aarg0 �Ogadget+4d8�A�o [gadget+4d8+c]
// Gw2-64.exe+57E8E9 - cmp eax,03 //type ��0x3


void SetAvAgent() {
	avAgentCharacters.clear();
	avAgentGadgets.clear();
	avAgentGadgetFishHoles.clear();
	avAgentAttackGadgets.clear();
	avAgentItems.clear();
	avAgentUnknown.clear();

	uintptr_t loopFunc = FollowRelativeAddress(staticAddress["avAgentArray"] + 0x16);
	// Gw2-64.exe+6B7A8F - call Gw2-64.exe+136CA50
	uintptr_t arrayInfo = FollowRelativeAddress(FollowRelativeAddress(staticAddress["ViewAdvanceAgentView"] + 0xA) + 0x3);
	// Gw2-64.exe+136C816 - lea rcx,[rsi+68]
	// Gw2-64.exe+136C81F - call Gw2-64.exe+139A950
	// Gw2-64.exe+136C81F - Gw2-64.exe+139A95F - mov rbx,[rcx+08]  // ���o��0��item
	uintptr_t firstItem = *(uintptr_t*)(arrayInfo + 0x68 + 0x8);
	// Gw2-64.exe+139A965 - mov eax,[rcx+14]  //���o�̤jindex
	int len = *(int*)(arrayInfo + 0x68 + 0x14);
	for (size_t i = 0; i < len; i++)
	{
		// Gw2-64.exe+139A9A5 - mov rcx,[rbx] //���Ҧ��S�����e��
		uintptr_t avAgent = *(uintptr_t*)(firstItem + i * 0x8);
		if (avAgent == 0) continue;
		// Gw2-64.exe+139A9B2 - mov rax,[rcx] //���oavagent ��funcArray 
		uintptr_t avAgentFuncAry = *(uintptr_t*)avAgent;
		// Gw2-64.exe+139A9B5 - call qword ptr [rax+00000140] //���otype (arg0=avagent)
		uintptr_t getAgentType = *(uintptr_t*)(avAgentFuncAry + 0x140);
		int type = ((int(__thiscall*)(uintptr_t))getAgentType)(avAgent);
		if (type == 0x0) {
			avAgentCharacters.push_back(avAgent);
		}
		else if (type == 0xA) {
			avAgentGadgets.push_back(avAgent);
			uintptr_t gadget = *(uintptr_t*)(avAgent + 0xC0);
			int nodeType = *(int*)(gadget + 0x4d8 + 0xc);
			if (nodeType == 0x3) {
				avAgentGadgetFishHoles.push_back(avAgent);
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
		address.avAgent0 = (uintptr_t)avAgentCharacters.data();
		if (address.avAgent0 == 0) return;
		console.printf("avAgent ary0: %p\n", address.avAgent0);
	};
	if (address.avAgentA == 0) {
		address.avAgentA = (uintptr_t)avAgentGadgets.data();
		if (address.avAgentA == 0) return;
		console.printf("avAgent aryA: %p\n", address.avAgentA);
		address.ready = true;
	};
	if (address.avAgentB == 0) {
		address.avAgentB = (uintptr_t)avAgentAttackGadgets.data();
		if (address.avAgentB == 0) return;
		console.printf("avAgent aryB: %p\n", address.avAgentB);
	};
	if (address.avAgentF == 0) {
		address.avAgentF = (uintptr_t)avAgentItems.data();
		if (address.avAgentF == 0) return;
		console.printf("avAgent aryF: %p\n", address.avAgentF);
	};
	if (address.avAgentU == 0) {
		address.avAgentU = (uintptr_t)avAgentUnknown.data();
		if (address.avAgentU == 0) return;
		console.printf("avAgent aryU: %p\n", address.avAgentU);
	};
	if (address.avAgentH == 0) {
		address.avAgentH = (uintptr_t)avAgentGadgetFishHoles.data();
		if (address.avAgentH == 0) return;
		console.printf("avAgent aryH: %p\n", address.avAgentH);
	};
}

void OnMapChange() {
	address.fish = 0;
	address.scanned = false;
}