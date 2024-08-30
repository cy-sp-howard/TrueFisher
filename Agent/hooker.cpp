#include "pch.h"
#include "hooker.h"
#include <algorithm>
#include <cstring>
#include <mutex>
#include <unordered_map>
#include <algorithm>
#include <fstream>
#include <stdexcept>

static const int JMPHOOKSIZE = 14;

struct FakeVT
{
    FakeVT(uintptr_t** instance, int vtBackupSize) : m_data(vtBackupSize), m_orgVT(*instance), m_refs(1)
    {
        // Copy original VT.
        for (int i = 0; i < vtBackupSize; i++)
        {
            m_data[i] = m_orgVT[i];
        }
    }
    data_page_vector<uintptr_t> m_data;
    uintptr_t* m_orgVT;
    int m_refs;
};


class VTHookManager
{
public:
    uintptr_t getOrgFunc(uintptr_t** instance, int functionIndex)
    {
        return m_fakeVTs[instance]->m_orgVT[functionIndex];
    }
    void addHook(uintptr_t** instance, int functionIndex, uintptr_t cbHook, int vtBackupSize)
    {
        auto& fakeVT = m_fakeVTs[instance];
        if (fakeVT)
        {
     

            fakeVT->m_refs++;
        }
        else
        {
            // Create new fake VT (mirroring the original one).
            fakeVT = std::make_unique<FakeVT>(instance, vtBackupSize);

            // Replace the VT pointer in the object instance.
            *instance = fakeVT->m_data.data();
        }

        // Overwrite the hooked function in VT. This applies the hook.
        fakeVT->m_data[functionIndex] = cbHook;

        // Make the fake VT read-only like a real VT would be.
    }
    void removeHook(uintptr_t** instance, int functionIndex)
    {
        auto& fakeVT = m_fakeVTs[instance];
        if (fakeVT)
        {
            if (fakeVT->m_refs == 1)
            {
                // Last reference. Restore pointer to original VT.
                *instance = fakeVT->m_orgVT;

                m_fakeVTs.erase(instance);
            }
            else
            {
    

                fakeVT->m_refs--;
            }
        }
    }

private:
    std::unordered_map<uintptr_t**, std::unique_ptr<FakeVT>> m_fakeVTs;
};


static VTHookManager g_vtHookManager;


class VTHook : public IHook
{
public:
    VTHook(uintptr_t classInstance, int functionIndex, uintptr_t cbHook, int vtBackupSize)
        : instance((uintptr_t**)classInstance), functionIndex(functionIndex)
    {
        g_vtHookManager.addHook(instance, functionIndex, cbHook, vtBackupSize);
    }
    ~VTHook() override { g_vtHookManager.removeHook(instance, functionIndex); }
    uintptr_t getLocation() const override { return g_vtHookManager.getOrgFunc(instance, functionIndex); }

    uintptr_t** instance;
    int functionIndex;
};
;

const IHook* Hooker::hookVT(uintptr_t classInstance, int functionIndex, uintptr_t cbHook, int vtBackupSize)
{
    // Check for invalid parameters.
    if (!classInstance || functionIndex < 0 || functionIndex >= vtBackupSize || !cbHook)
        return nullptr;

    auto pHook = std::make_unique<VTHook>(classInstance, functionIndex, cbHook, vtBackupSize);

    auto result = pHook.get();
    m_hooks.push_back(std::move(pHook));
    return result;
}


void Hooker::unhook(const IHook* pHook)
{
    auto cond = [pHook](const auto& uptr) { return uptr.get() == pHook; };

    m_hooks.erase(std::remove_if(m_hooks.begin(), m_hooks.end(), cond), m_hooks.end());
}

void PageFree(void* p, size_t n)
{
    VirtualFree(p, 0, MEM_RELEASE);
}