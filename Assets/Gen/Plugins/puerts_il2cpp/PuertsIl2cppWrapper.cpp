// Auto Gen

#include <memory>
#include "il2cpp-api.h"
#include "il2cpp-class-internals.h"
#include "il2cpp-object-internals.h"
#include "vm/Object.h"
#include "pesapi.h"
#include "TDataTrans.h"

namespace puerts
{



static WrapFuncInfo g_wrapFuncInfos[] = {

    {nullptr, nullptr}
};

WrapFuncPtr FindWrapFunc(const char* signature)
{
    auto begin = &g_wrapFuncInfos[0];
    auto end = &g_wrapFuncInfos[sizeof(g_wrapFuncInfos) / sizeof(WrapFuncInfo) - 1];
    auto first = std::lower_bound(begin, end, signature, [](const WrapFuncInfo& x, const char* signature) {return strcmp(x.Signature, signature) < 0;});
    if (first != end && strcmp(first->Signature, signature) == 0) {
        return first->Method;
    }
    return nullptr;
}

}
