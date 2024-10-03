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
	uintptr_t selfCharacter = 0;
	bool scanned = false;
	uintptr_t avAgent0 = 0;
	uintptr_t avAgentA = 0;
	uintptr_t avAgentB = 0;
	uintptr_t avAgentF = 0;
	uintptr_t avAgentU = 0;
	uintptr_t avAgentH = 0;
	uintptr_t avAgentR = 0;
	uintptr_t avModels = 0;
};


struct character {

};



void SetLangAddr();
void SetFishAddr();
void SetMapStateAddr();
void SetKeyBindsAddr();
void SetAvAgent();
void SetAvModel();
void OnMapChange();
uintptr_t GetPtr(uintptr_t);