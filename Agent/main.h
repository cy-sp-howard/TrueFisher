#pragma once
#include <vector>
#include <unordered_map>
#include "console.h"

struct ADDRESS {
	std::string ImHere;
	bool ready = false;
	uintptr_t lang = 0;
	uintptr_t fish = 0;
	std::vector<uintptr_t> characterAry;
	uintptr_t selfCharacter = 0;
};


struct character {

};



void SetLangAddr();
void SetFishAddr();
uintptr_t GetPtr(uintptr_t);