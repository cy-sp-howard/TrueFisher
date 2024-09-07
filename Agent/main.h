#pragma once
#include <vector>
#include <unordered_map>
#include "console.h"

struct ADDRESS {
	bool ready = false;
	uintptr_t mapState = 0;
	uintptr_t lang = 0;
	uintptr_t fish = 0;
	uintptr_t keyBind0 = 0;
	uintptr_t keyBind1 = 0;
	std::vector<uintptr_t> characterAry;
	uintptr_t selfCharacter = 0;
};


struct character {

};



void SetLangAddr();
void SetFishAddr();
void SetMapStateAddr();
void SetKeyBindsAddr();
void resetInstanceImpactedAddress();
uintptr_t GetPtr(uintptr_t);