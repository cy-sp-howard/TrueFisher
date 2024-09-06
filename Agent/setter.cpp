#include "pch.h"
#include "main.h"
#include "scanner.h"

extern ADDRESS address;
extern Console console;
extern std::unordered_map<std::string, uintptr_t> staticAddress;

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

	auto getBase = (uintptr_t(__thiscall*)())(staticAddress["ViewAdvanceCharacter"]);
	uintptr_t baseAddr = *(uintptr_t*)(getBase() + 0x98);
	uintptr_t loopStartAddr = *(uintptr_t*)(baseAddr + 0x60);
	uintptr_t loopEndAddr = loopStartAddr + (*(int*)(baseAddr + 0x6C)) * 8;
	uintptr_t currentLoopAddr = loopStartAddr;
	address.characterAry.clear();
	address.selfCharacter = 0;
	while (currentLoopAddr < loopEndAddr)
	{
		// addr �̭��O�@��ptr ary ,index 1��ptr  call �L�arcx�n���|���o ��charater ���A �tpos
		uintptr_t* addr = (uintptr_t*)currentLoopAddr;
		if (*addr) {
			address.characterAry.push_back(*addr);
			uintptr_t _addr = (uintptr_t)*addr;  //_addr�ʺA��
			//[[*ADDR + 08]+ 60]  (bool(__thiscall*)(uintptr_t))

			//(*ADDR + 08)
			auto isPlayer = (bool(__thiscall*)(uintptr_t))(*((uintptr_t*)(*((uintptr_t*)(_addr + 0x8)) + 0x60)));

			//7FF712C496B0
			if (isPlayer(_addr + 0x8)) {
				address.selfCharacter = _addr;

				uintptr_t __addr = *(uintptr_t*)_addr; //__addr �O����(�i��Ofunc)Ary �T�w
				auto getNextPtr = (uintptr_t(__thiscall*)(uintptr_t))(*((uintptr_t*)(__addr + 0x2C0)));
				auto _base = getNextPtr(_addr);
				if (!_base) continue;
				//[[_base + 05C78 + 28]+18]�|�o�쳨���p��
				// _base + 05C78 + 28 ���ȷ|�H�����}�l����
				address.fish = *((uintptr_t*)(_base + 0x5CA0));

				auto a = address.fish + 0x18;

				console.printf("self %p\n", _addr);
				console.printf("fish %p\n", a);

			}

		}

		currentLoopAddr += 8;
	}



}
