#pragma once
#include <cstdint>
#include <memory>
#include <vector>
#include "scanner.h"


void PageFree(void* p, size_t n = 0);




template <typename T>
using data_page_vector = std::vector<T>;

typedef std::vector<unsigned char> code_page_vector;

struct CpuContext_x86_64
{
	uintptr_t RIP;
	uintptr_t RFLAGS;
	uintptr_t R15;
	uintptr_t R14;
	uintptr_t R13;
	uintptr_t R12;
	uintptr_t R11;
	uintptr_t R10;
	uintptr_t R9;
	uintptr_t R8;
	uintptr_t RDI;
	uintptr_t RSI;
	uintptr_t RBP;
	uintptr_t RBX;
	uintptr_t RDX;
	uintptr_t RCX;
	uintptr_t RAX;
	uintptr_t RSP;
};
typedef CpuContext_x86_64 CpuContext;
class IHook
{
public:
	virtual ~IHook() {}
	/// Returns the memory address that was hooked by this hook.
	virtual uintptr_t getLocation() const = 0;
};

class Hooker
{
public:
	typedef void (*HookCallback_t)(CpuContext*);

	/// Hook by replacing an object instances virtual table pointer.
	/// This method can only target virtual functions. It should always
	/// be preferred if possible as it is almost impossible to detect.
	/// No read-only target memory is modified.
	/// \param classInstance: The class instance to modify.
	/// \param functionIndex: Zero based ordinal number of the targeted virtual function.
	/// \param cbHook: The hook target location.
	/// \param vtBackupSize: Amount of memory to use for backing up the original virtual table.
	const IHook* hookVT(uintptr_t classInstance, int functionIndex, uintptr_t cbHook, int vtBackupSize = 1024);


	/// Removes the hook represented by the given IHook object and releases all associated resources.
	void unhook(const IHook* pHook);


	/// \overload
	template <typename T, typename C>
	const IHook* hookVT(T* classInstance, int functionIndex, C cbHook, int vtBackupSize = 1024)
	{
		return hookVT((uintptr_t)classInstance, functionIndex, (uintptr_t)cbHook, vtBackupSize);
	}



private:
	std::vector<std::unique_ptr<IHook>> m_hooks;
};

