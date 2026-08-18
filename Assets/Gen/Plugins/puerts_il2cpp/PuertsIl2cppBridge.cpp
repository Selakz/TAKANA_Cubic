// Auto Gen

#include "il2cpp-api.h"
#include "il2cpp-class-internals.h"
#include "il2cpp-object-internals.h"
#include "vm/InternalCalls.h"
#include "vm/Object.h"
#include "vm/Array.h"
#include "vm/Runtime.h"
#include "vm/Reflection.h"
#include "vm/MetadataCache.h"
#include "vm/Field.h"
#include "vm/GenericClass.h"
#include "vm/Thread.h"
#include "vm/Method.h"
#include "vm/Parameter.h"
#include "vm/Image.h"
#include "utils/StringUtils.h"
#include "gc/WriteBarrier.h"
#include "pesapi.h"
#include "TDataTrans.h"
#include "PuertsValueType.h"

namespace puerts
{

// Func_Shared_0
static Il2CppObject* b_o(void* target, MethodInfo* method) {
    // PLog("Running b_o");

    auto TIret = GetReturnType(method);

    PObjectRefInfo* delegateInfo = GetPObjectRefInfo(target);
    struct pesapi_ffi* apis = delegateInfo->Apis;
    
    pesapi_env_ref envRef = apis->get_ref_associated_env(delegateInfo->ValueRef);
    AutoValueScope valueScope(apis, envRef);
    auto env = apis->get_env_from_ref(envRef);
    if (!env)
    {
        il2cpp::vm::Exception::Raise(il2cpp::vm::Exception::GetInvalidOperationException("JsEnv had been destroy"));
        return {};
    }
    auto func = apis->get_value_from_ref(env, delegateInfo->ValueRef);
    
    pesapi_value *argv = nullptr;
    auto jsret = apis->call_function(env, func, nullptr, 0, argv);
    
    if (apis->has_caught(valueScope.scope()))
    {
        auto msg = apis->get_exception_as_string(valueScope.scope(), true);
        il2cpp::vm::Exception::Raise(il2cpp::vm::Exception::GetInvalidOperationException(msg));
        return {};
    }
    
    // JSValToCSVal o/O
    Il2CppObject* ret = JsValueToCSRef(apis, TIret, env, jsret);
    return ret;
        
}

static void b_o_Invoker(Il2CppMethodPointer func, const MethodInfo* method, void* thisPtr, void** args, void* il2ppRetVal)
{
    *((Il2CppObject* *)il2ppRetVal) =
    b_o(thisPtr, (MethodInfo*)method);
}


// Func_Shared_1
static Il2CppObject* b_oo(void* target, Il2CppObject* p0, MethodInfo* method) {
    // PLog("Running b_oo");

    auto TIret = GetReturnType(method);
    auto TIp0 = GetParameterType(method, 0);

    PObjectRefInfo* delegateInfo = GetPObjectRefInfo(target);
    struct pesapi_ffi* apis = delegateInfo->Apis;
    
    pesapi_env_ref envRef = apis->get_ref_associated_env(delegateInfo->ValueRef);
    AutoValueScope valueScope(apis, envRef);
    auto env = apis->get_env_from_ref(envRef);
    if (!env)
    {
        il2cpp::vm::Exception::Raise(il2cpp::vm::Exception::GetInvalidOperationException("JsEnv had been destroy"));
        return {};
    }
    auto func = apis->get_value_from_ref(env, delegateInfo->ValueRef);
    
    pesapi_value argv[1]{
        CSRefToJsValue(apis, env, TIp0, p0)
    };
    auto jsret = apis->call_function(env, func, nullptr, 1, argv);
    
    if (apis->has_caught(valueScope.scope()))
    {
        auto msg = apis->get_exception_as_string(valueScope.scope(), true);
        il2cpp::vm::Exception::Raise(il2cpp::vm::Exception::GetInvalidOperationException(msg));
        return {};
    }
    
    // JSValToCSVal o/O
    Il2CppObject* ret = JsValueToCSRef(apis, TIret, env, jsret);
    return ret;
        
}

static void b_oo_Invoker(Il2CppMethodPointer func, const MethodInfo* method, void* thisPtr, void** args, void* il2ppRetVal)
{
    *((Il2CppObject* *)il2ppRetVal) =
    b_oo(thisPtr, (Il2CppObject*)args[0], (MethodInfo*)method);
}


// Func_Shared_2
static Il2CppObject* b_ooo(void* target, Il2CppObject* p0, Il2CppObject* p1, MethodInfo* method) {
    // PLog("Running b_ooo");

    auto TIret = GetReturnType(method);
    auto TIp0 = GetParameterType(method, 0);
    auto TIp1 = GetParameterType(method, 1);

    PObjectRefInfo* delegateInfo = GetPObjectRefInfo(target);
    struct pesapi_ffi* apis = delegateInfo->Apis;
    
    pesapi_env_ref envRef = apis->get_ref_associated_env(delegateInfo->ValueRef);
    AutoValueScope valueScope(apis, envRef);
    auto env = apis->get_env_from_ref(envRef);
    if (!env)
    {
        il2cpp::vm::Exception::Raise(il2cpp::vm::Exception::GetInvalidOperationException("JsEnv had been destroy"));
        return {};
    }
    auto func = apis->get_value_from_ref(env, delegateInfo->ValueRef);
    
    pesapi_value argv[2]{
        CSRefToJsValue(apis, env, TIp0, p0),
        CSRefToJsValue(apis, env, TIp1, p1)
    };
    auto jsret = apis->call_function(env, func, nullptr, 2, argv);
    
    if (apis->has_caught(valueScope.scope()))
    {
        auto msg = apis->get_exception_as_string(valueScope.scope(), true);
        il2cpp::vm::Exception::Raise(il2cpp::vm::Exception::GetInvalidOperationException(msg));
        return {};
    }
    
    // JSValToCSVal o/O
    Il2CppObject* ret = JsValueToCSRef(apis, TIret, env, jsret);
    return ret;
        
}

static void b_ooo_Invoker(Il2CppMethodPointer func, const MethodInfo* method, void* thisPtr, void** args, void* il2ppRetVal)
{
    *((Il2CppObject* *)il2ppRetVal) =
    b_ooo(thisPtr, (Il2CppObject*)args[0], (Il2CppObject*)args[1], (MethodInfo*)method);
}


// Func_Shared_3
static Il2CppObject* b_oooo(void* target, Il2CppObject* p0, Il2CppObject* p1, Il2CppObject* p2, MethodInfo* method) {
    // PLog("Running b_oooo");

    auto TIret = GetReturnType(method);
    auto TIp0 = GetParameterType(method, 0);
    auto TIp1 = GetParameterType(method, 1);
    auto TIp2 = GetParameterType(method, 2);

    PObjectRefInfo* delegateInfo = GetPObjectRefInfo(target);
    struct pesapi_ffi* apis = delegateInfo->Apis;
    
    pesapi_env_ref envRef = apis->get_ref_associated_env(delegateInfo->ValueRef);
    AutoValueScope valueScope(apis, envRef);
    auto env = apis->get_env_from_ref(envRef);
    if (!env)
    {
        il2cpp::vm::Exception::Raise(il2cpp::vm::Exception::GetInvalidOperationException("JsEnv had been destroy"));
        return {};
    }
    auto func = apis->get_value_from_ref(env, delegateInfo->ValueRef);
    
    pesapi_value argv[3]{
        CSRefToJsValue(apis, env, TIp0, p0),
        CSRefToJsValue(apis, env, TIp1, p1),
        CSRefToJsValue(apis, env, TIp2, p2)
    };
    auto jsret = apis->call_function(env, func, nullptr, 3, argv);
    
    if (apis->has_caught(valueScope.scope()))
    {
        auto msg = apis->get_exception_as_string(valueScope.scope(), true);
        il2cpp::vm::Exception::Raise(il2cpp::vm::Exception::GetInvalidOperationException(msg));
        return {};
    }
    
    // JSValToCSVal o/O
    Il2CppObject* ret = JsValueToCSRef(apis, TIret, env, jsret);
    return ret;
        
}

static void b_oooo_Invoker(Il2CppMethodPointer func, const MethodInfo* method, void* thisPtr, void** args, void* il2ppRetVal)
{
    *((Il2CppObject* *)il2ppRetVal) =
    b_oooo(thisPtr, (Il2CppObject*)args[0], (Il2CppObject*)args[1], (Il2CppObject*)args[2], (MethodInfo*)method);
}


// Func_Shared_4
static Il2CppObject* b_ooooo(void* target, Il2CppObject* p0, Il2CppObject* p1, Il2CppObject* p2, Il2CppObject* p3, MethodInfo* method) {
    // PLog("Running b_ooooo");

    auto TIret = GetReturnType(method);
    auto TIp0 = GetParameterType(method, 0);
    auto TIp1 = GetParameterType(method, 1);
    auto TIp2 = GetParameterType(method, 2);
    auto TIp3 = GetParameterType(method, 3);

    PObjectRefInfo* delegateInfo = GetPObjectRefInfo(target);
    struct pesapi_ffi* apis = delegateInfo->Apis;
    
    pesapi_env_ref envRef = apis->get_ref_associated_env(delegateInfo->ValueRef);
    AutoValueScope valueScope(apis, envRef);
    auto env = apis->get_env_from_ref(envRef);
    if (!env)
    {
        il2cpp::vm::Exception::Raise(il2cpp::vm::Exception::GetInvalidOperationException("JsEnv had been destroy"));
        return {};
    }
    auto func = apis->get_value_from_ref(env, delegateInfo->ValueRef);
    
    pesapi_value argv[4]{
        CSRefToJsValue(apis, env, TIp0, p0),
        CSRefToJsValue(apis, env, TIp1, p1),
        CSRefToJsValue(apis, env, TIp2, p2),
        CSRefToJsValue(apis, env, TIp3, p3)
    };
    auto jsret = apis->call_function(env, func, nullptr, 4, argv);
    
    if (apis->has_caught(valueScope.scope()))
    {
        auto msg = apis->get_exception_as_string(valueScope.scope(), true);
        il2cpp::vm::Exception::Raise(il2cpp::vm::Exception::GetInvalidOperationException(msg));
        return {};
    }
    
    // JSValToCSVal o/O
    Il2CppObject* ret = JsValueToCSRef(apis, TIret, env, jsret);
    return ret;
        
}

static void b_ooooo_Invoker(Il2CppMethodPointer func, const MethodInfo* method, void* thisPtr, void** args, void* il2ppRetVal)
{
    *((Il2CppObject* *)il2ppRetVal) =
    b_ooooo(thisPtr, (Il2CppObject*)args[0], (Il2CppObject*)args[1], (Il2CppObject*)args[2], (Il2CppObject*)args[3], (MethodInfo*)method);
}


// Func_Shared_5
static Il2CppObject* b_oooooo(void* target, Il2CppObject* p0, Il2CppObject* p1, Il2CppObject* p2, Il2CppObject* p3, Il2CppObject* p4, MethodInfo* method) {
    // PLog("Running b_oooooo");

    auto TIret = GetReturnType(method);
    auto TIp0 = GetParameterType(method, 0);
    auto TIp1 = GetParameterType(method, 1);
    auto TIp2 = GetParameterType(method, 2);
    auto TIp3 = GetParameterType(method, 3);
    auto TIp4 = GetParameterType(method, 4);

    PObjectRefInfo* delegateInfo = GetPObjectRefInfo(target);
    struct pesapi_ffi* apis = delegateInfo->Apis;
    
    pesapi_env_ref envRef = apis->get_ref_associated_env(delegateInfo->ValueRef);
    AutoValueScope valueScope(apis, envRef);
    auto env = apis->get_env_from_ref(envRef);
    if (!env)
    {
        il2cpp::vm::Exception::Raise(il2cpp::vm::Exception::GetInvalidOperationException("JsEnv had been destroy"));
        return {};
    }
    auto func = apis->get_value_from_ref(env, delegateInfo->ValueRef);
    
    pesapi_value argv[5]{
        CSRefToJsValue(apis, env, TIp0, p0),
        CSRefToJsValue(apis, env, TIp1, p1),
        CSRefToJsValue(apis, env, TIp2, p2),
        CSRefToJsValue(apis, env, TIp3, p3),
        CSRefToJsValue(apis, env, TIp4, p4)
    };
    auto jsret = apis->call_function(env, func, nullptr, 5, argv);
    
    if (apis->has_caught(valueScope.scope()))
    {
        auto msg = apis->get_exception_as_string(valueScope.scope(), true);
        il2cpp::vm::Exception::Raise(il2cpp::vm::Exception::GetInvalidOperationException(msg));
        return {};
    }
    
    // JSValToCSVal o/O
    Il2CppObject* ret = JsValueToCSRef(apis, TIret, env, jsret);
    return ret;
        
}

static void b_oooooo_Invoker(Il2CppMethodPointer func, const MethodInfo* method, void* thisPtr, void** args, void* il2ppRetVal)
{
    *((Il2CppObject* *)il2ppRetVal) =
    b_oooooo(thisPtr, (Il2CppObject*)args[0], (Il2CppObject*)args[1], (Il2CppObject*)args[2], (Il2CppObject*)args[3], (Il2CppObject*)args[4], (MethodInfo*)method);
}


// Func_Shared_6
static Il2CppObject* b_ooooooo(void* target, Il2CppObject* p0, Il2CppObject* p1, Il2CppObject* p2, Il2CppObject* p3, Il2CppObject* p4, Il2CppObject* p5, MethodInfo* method) {
    // PLog("Running b_ooooooo");

    auto TIret = GetReturnType(method);
    auto TIp0 = GetParameterType(method, 0);
    auto TIp1 = GetParameterType(method, 1);
    auto TIp2 = GetParameterType(method, 2);
    auto TIp3 = GetParameterType(method, 3);
    auto TIp4 = GetParameterType(method, 4);
    auto TIp5 = GetParameterType(method, 5);

    PObjectRefInfo* delegateInfo = GetPObjectRefInfo(target);
    struct pesapi_ffi* apis = delegateInfo->Apis;
    
    pesapi_env_ref envRef = apis->get_ref_associated_env(delegateInfo->ValueRef);
    AutoValueScope valueScope(apis, envRef);
    auto env = apis->get_env_from_ref(envRef);
    if (!env)
    {
        il2cpp::vm::Exception::Raise(il2cpp::vm::Exception::GetInvalidOperationException("JsEnv had been destroy"));
        return {};
    }
    auto func = apis->get_value_from_ref(env, delegateInfo->ValueRef);
    
    pesapi_value argv[6]{
        CSRefToJsValue(apis, env, TIp0, p0),
        CSRefToJsValue(apis, env, TIp1, p1),
        CSRefToJsValue(apis, env, TIp2, p2),
        CSRefToJsValue(apis, env, TIp3, p3),
        CSRefToJsValue(apis, env, TIp4, p4),
        CSRefToJsValue(apis, env, TIp5, p5)
    };
    auto jsret = apis->call_function(env, func, nullptr, 6, argv);
    
    if (apis->has_caught(valueScope.scope()))
    {
        auto msg = apis->get_exception_as_string(valueScope.scope(), true);
        il2cpp::vm::Exception::Raise(il2cpp::vm::Exception::GetInvalidOperationException(msg));
        return {};
    }
    
    // JSValToCSVal o/O
    Il2CppObject* ret = JsValueToCSRef(apis, TIret, env, jsret);
    return ret;
        
}

static void b_ooooooo_Invoker(Il2CppMethodPointer func, const MethodInfo* method, void* thisPtr, void** args, void* il2ppRetVal)
{
    *((Il2CppObject* *)il2ppRetVal) =
    b_ooooooo(thisPtr, (Il2CppObject*)args[0], (Il2CppObject*)args[1], (Il2CppObject*)args[2], (Il2CppObject*)args[3], (Il2CppObject*)args[4], (Il2CppObject*)args[5], (MethodInfo*)method);
}


// Func_Shared_7
static Il2CppObject* b_oooooooo(void* target, Il2CppObject* p0, Il2CppObject* p1, Il2CppObject* p2, Il2CppObject* p3, Il2CppObject* p4, Il2CppObject* p5, Il2CppObject* p6, MethodInfo* method) {
    // PLog("Running b_oooooooo");

    auto TIret = GetReturnType(method);
    auto TIp0 = GetParameterType(method, 0);
    auto TIp1 = GetParameterType(method, 1);
    auto TIp2 = GetParameterType(method, 2);
    auto TIp3 = GetParameterType(method, 3);
    auto TIp4 = GetParameterType(method, 4);
    auto TIp5 = GetParameterType(method, 5);
    auto TIp6 = GetParameterType(method, 6);

    PObjectRefInfo* delegateInfo = GetPObjectRefInfo(target);
    struct pesapi_ffi* apis = delegateInfo->Apis;
    
    pesapi_env_ref envRef = apis->get_ref_associated_env(delegateInfo->ValueRef);
    AutoValueScope valueScope(apis, envRef);
    auto env = apis->get_env_from_ref(envRef);
    if (!env)
    {
        il2cpp::vm::Exception::Raise(il2cpp::vm::Exception::GetInvalidOperationException("JsEnv had been destroy"));
        return {};
    }
    auto func = apis->get_value_from_ref(env, delegateInfo->ValueRef);
    
    pesapi_value argv[7]{
        CSRefToJsValue(apis, env, TIp0, p0),
        CSRefToJsValue(apis, env, TIp1, p1),
        CSRefToJsValue(apis, env, TIp2, p2),
        CSRefToJsValue(apis, env, TIp3, p3),
        CSRefToJsValue(apis, env, TIp4, p4),
        CSRefToJsValue(apis, env, TIp5, p5),
        CSRefToJsValue(apis, env, TIp6, p6)
    };
    auto jsret = apis->call_function(env, func, nullptr, 7, argv);
    
    if (apis->has_caught(valueScope.scope()))
    {
        auto msg = apis->get_exception_as_string(valueScope.scope(), true);
        il2cpp::vm::Exception::Raise(il2cpp::vm::Exception::GetInvalidOperationException(msg));
        return {};
    }
    
    // JSValToCSVal o/O
    Il2CppObject* ret = JsValueToCSRef(apis, TIret, env, jsret);
    return ret;
        
}

static void b_oooooooo_Invoker(Il2CppMethodPointer func, const MethodInfo* method, void* thisPtr, void** args, void* il2ppRetVal)
{
    *((Il2CppObject* *)il2ppRetVal) =
    b_oooooooo(thisPtr, (Il2CppObject*)args[0], (Il2CppObject*)args[1], (Il2CppObject*)args[2], (Il2CppObject*)args[3], (Il2CppObject*)args[4], (Il2CppObject*)args[5], (Il2CppObject*)args[6], (MethodInfo*)method);
}


// Func_Shared_8
static Il2CppObject* b_ooooooooo(void* target, Il2CppObject* p0, Il2CppObject* p1, Il2CppObject* p2, Il2CppObject* p3, Il2CppObject* p4, Il2CppObject* p5, Il2CppObject* p6, Il2CppObject* p7, MethodInfo* method) {
    // PLog("Running b_ooooooooo");

    auto TIret = GetReturnType(method);
    auto TIp0 = GetParameterType(method, 0);
    auto TIp1 = GetParameterType(method, 1);
    auto TIp2 = GetParameterType(method, 2);
    auto TIp3 = GetParameterType(method, 3);
    auto TIp4 = GetParameterType(method, 4);
    auto TIp5 = GetParameterType(method, 5);
    auto TIp6 = GetParameterType(method, 6);
    auto TIp7 = GetParameterType(method, 7);

    PObjectRefInfo* delegateInfo = GetPObjectRefInfo(target);
    struct pesapi_ffi* apis = delegateInfo->Apis;
    
    pesapi_env_ref envRef = apis->get_ref_associated_env(delegateInfo->ValueRef);
    AutoValueScope valueScope(apis, envRef);
    auto env = apis->get_env_from_ref(envRef);
    if (!env)
    {
        il2cpp::vm::Exception::Raise(il2cpp::vm::Exception::GetInvalidOperationException("JsEnv had been destroy"));
        return {};
    }
    auto func = apis->get_value_from_ref(env, delegateInfo->ValueRef);
    
    pesapi_value argv[8]{
        CSRefToJsValue(apis, env, TIp0, p0),
        CSRefToJsValue(apis, env, TIp1, p1),
        CSRefToJsValue(apis, env, TIp2, p2),
        CSRefToJsValue(apis, env, TIp3, p3),
        CSRefToJsValue(apis, env, TIp4, p4),
        CSRefToJsValue(apis, env, TIp5, p5),
        CSRefToJsValue(apis, env, TIp6, p6),
        CSRefToJsValue(apis, env, TIp7, p7)
    };
    auto jsret = apis->call_function(env, func, nullptr, 8, argv);
    
    if (apis->has_caught(valueScope.scope()))
    {
        auto msg = apis->get_exception_as_string(valueScope.scope(), true);
        il2cpp::vm::Exception::Raise(il2cpp::vm::Exception::GetInvalidOperationException(msg));
        return {};
    }
    
    // JSValToCSVal o/O
    Il2CppObject* ret = JsValueToCSRef(apis, TIret, env, jsret);
    return ret;
        
}

static void b_ooooooooo_Invoker(Il2CppMethodPointer func, const MethodInfo* method, void* thisPtr, void** args, void* il2ppRetVal)
{
    *((Il2CppObject* *)il2ppRetVal) =
    b_ooooooooo(thisPtr, (Il2CppObject*)args[0], (Il2CppObject*)args[1], (Il2CppObject*)args[2], (Il2CppObject*)args[3], (Il2CppObject*)args[4], (Il2CppObject*)args[5], (Il2CppObject*)args[6], (Il2CppObject*)args[7], (MethodInfo*)method);
}


// Func_Shared_9
static Il2CppObject* b_oooooooooo(void* target, Il2CppObject* p0, Il2CppObject* p1, Il2CppObject* p2, Il2CppObject* p3, Il2CppObject* p4, Il2CppObject* p5, Il2CppObject* p6, Il2CppObject* p7, Il2CppObject* p8, MethodInfo* method) {
    // PLog("Running b_oooooooooo");

    auto TIret = GetReturnType(method);
    auto TIp0 = GetParameterType(method, 0);
    auto TIp1 = GetParameterType(method, 1);
    auto TIp2 = GetParameterType(method, 2);
    auto TIp3 = GetParameterType(method, 3);
    auto TIp4 = GetParameterType(method, 4);
    auto TIp5 = GetParameterType(method, 5);
    auto TIp6 = GetParameterType(method, 6);
    auto TIp7 = GetParameterType(method, 7);
    auto TIp8 = GetParameterType(method, 8);

    PObjectRefInfo* delegateInfo = GetPObjectRefInfo(target);
    struct pesapi_ffi* apis = delegateInfo->Apis;
    
    pesapi_env_ref envRef = apis->get_ref_associated_env(delegateInfo->ValueRef);
    AutoValueScope valueScope(apis, envRef);
    auto env = apis->get_env_from_ref(envRef);
    if (!env)
    {
        il2cpp::vm::Exception::Raise(il2cpp::vm::Exception::GetInvalidOperationException("JsEnv had been destroy"));
        return {};
    }
    auto func = apis->get_value_from_ref(env, delegateInfo->ValueRef);
    
    pesapi_value argv[9]{
        CSRefToJsValue(apis, env, TIp0, p0),
        CSRefToJsValue(apis, env, TIp1, p1),
        CSRefToJsValue(apis, env, TIp2, p2),
        CSRefToJsValue(apis, env, TIp3, p3),
        CSRefToJsValue(apis, env, TIp4, p4),
        CSRefToJsValue(apis, env, TIp5, p5),
        CSRefToJsValue(apis, env, TIp6, p6),
        CSRefToJsValue(apis, env, TIp7, p7),
        CSRefToJsValue(apis, env, TIp8, p8)
    };
    auto jsret = apis->call_function(env, func, nullptr, 9, argv);
    
    if (apis->has_caught(valueScope.scope()))
    {
        auto msg = apis->get_exception_as_string(valueScope.scope(), true);
        il2cpp::vm::Exception::Raise(il2cpp::vm::Exception::GetInvalidOperationException(msg));
        return {};
    }
    
    // JSValToCSVal o/O
    Il2CppObject* ret = JsValueToCSRef(apis, TIret, env, jsret);
    return ret;
        
}

static void b_oooooooooo_Invoker(Il2CppMethodPointer func, const MethodInfo* method, void* thisPtr, void** args, void* il2ppRetVal)
{
    *((Il2CppObject* *)il2ppRetVal) =
    b_oooooooooo(thisPtr, (Il2CppObject*)args[0], (Il2CppObject*)args[1], (Il2CppObject*)args[2], (Il2CppObject*)args[3], (Il2CppObject*)args[4], (Il2CppObject*)args[5], (Il2CppObject*)args[6], (Il2CppObject*)args[7], (Il2CppObject*)args[8], (MethodInfo*)method);
}


// Action_Shared_0
static void b_v(void* target, MethodInfo* method) {
    // PLog("Running b_v");


    PObjectRefInfo* delegateInfo = GetPObjectRefInfo(target);
    struct pesapi_ffi* apis = delegateInfo->Apis;
    
    pesapi_env_ref envRef = apis->get_ref_associated_env(delegateInfo->ValueRef);
    AutoValueScope valueScope(apis, envRef);
    auto env = apis->get_env_from_ref(envRef);
    if (!env)
    {
        il2cpp::vm::Exception::Raise(il2cpp::vm::Exception::GetInvalidOperationException("JsEnv had been destroy"));
    }
    auto func = apis->get_value_from_ref(env, delegateInfo->ValueRef);
    
    pesapi_value *argv = nullptr;
    auto jsret = apis->call_function(env, func, nullptr, 0, argv);
    
    if (apis->has_caught(valueScope.scope()))
    {
        auto msg = apis->get_exception_as_string(valueScope.scope(), true);
        il2cpp::vm::Exception::Raise(il2cpp::vm::Exception::GetInvalidOperationException(msg));
    }
}

static void b_v_Invoker(Il2CppMethodPointer func, const MethodInfo* method, void* thisPtr, void** args, void* il2ppRetVal)
{
    b_v(thisPtr, (MethodInfo*)method);
}


// Action_Shared_1
static void b_vo(void* target, Il2CppObject* p0, MethodInfo* method) {
    // PLog("Running b_vo");

    auto TIp0 = GetParameterType(method, 0);

    PObjectRefInfo* delegateInfo = GetPObjectRefInfo(target);
    struct pesapi_ffi* apis = delegateInfo->Apis;
    
    pesapi_env_ref envRef = apis->get_ref_associated_env(delegateInfo->ValueRef);
    AutoValueScope valueScope(apis, envRef);
    auto env = apis->get_env_from_ref(envRef);
    if (!env)
    {
        il2cpp::vm::Exception::Raise(il2cpp::vm::Exception::GetInvalidOperationException("JsEnv had been destroy"));
    }
    auto func = apis->get_value_from_ref(env, delegateInfo->ValueRef);
    
    pesapi_value argv[1]{
        CSRefToJsValue(apis, env, TIp0, p0)
    };
    auto jsret = apis->call_function(env, func, nullptr, 1, argv);
    
    if (apis->has_caught(valueScope.scope()))
    {
        auto msg = apis->get_exception_as_string(valueScope.scope(), true);
        il2cpp::vm::Exception::Raise(il2cpp::vm::Exception::GetInvalidOperationException(msg));
    }
}

static void b_vo_Invoker(Il2CppMethodPointer func, const MethodInfo* method, void* thisPtr, void** args, void* il2ppRetVal)
{
    b_vo(thisPtr, (Il2CppObject*)args[0], (MethodInfo*)method);
}


// Action_Shared_2
static void b_voo(void* target, Il2CppObject* p0, Il2CppObject* p1, MethodInfo* method) {
    // PLog("Running b_voo");

    auto TIp0 = GetParameterType(method, 0);
    auto TIp1 = GetParameterType(method, 1);

    PObjectRefInfo* delegateInfo = GetPObjectRefInfo(target);
    struct pesapi_ffi* apis = delegateInfo->Apis;
    
    pesapi_env_ref envRef = apis->get_ref_associated_env(delegateInfo->ValueRef);
    AutoValueScope valueScope(apis, envRef);
    auto env = apis->get_env_from_ref(envRef);
    if (!env)
    {
        il2cpp::vm::Exception::Raise(il2cpp::vm::Exception::GetInvalidOperationException("JsEnv had been destroy"));
    }
    auto func = apis->get_value_from_ref(env, delegateInfo->ValueRef);
    
    pesapi_value argv[2]{
        CSRefToJsValue(apis, env, TIp0, p0),
        CSRefToJsValue(apis, env, TIp1, p1)
    };
    auto jsret = apis->call_function(env, func, nullptr, 2, argv);
    
    if (apis->has_caught(valueScope.scope()))
    {
        auto msg = apis->get_exception_as_string(valueScope.scope(), true);
        il2cpp::vm::Exception::Raise(il2cpp::vm::Exception::GetInvalidOperationException(msg));
    }
}

static void b_voo_Invoker(Il2CppMethodPointer func, const MethodInfo* method, void* thisPtr, void** args, void* il2ppRetVal)
{
    b_voo(thisPtr, (Il2CppObject*)args[0], (Il2CppObject*)args[1], (MethodInfo*)method);
}


// Action_Shared_3
static void b_vooo(void* target, Il2CppObject* p0, Il2CppObject* p1, Il2CppObject* p2, MethodInfo* method) {
    // PLog("Running b_vooo");

    auto TIp0 = GetParameterType(method, 0);
    auto TIp1 = GetParameterType(method, 1);
    auto TIp2 = GetParameterType(method, 2);

    PObjectRefInfo* delegateInfo = GetPObjectRefInfo(target);
    struct pesapi_ffi* apis = delegateInfo->Apis;
    
    pesapi_env_ref envRef = apis->get_ref_associated_env(delegateInfo->ValueRef);
    AutoValueScope valueScope(apis, envRef);
    auto env = apis->get_env_from_ref(envRef);
    if (!env)
    {
        il2cpp::vm::Exception::Raise(il2cpp::vm::Exception::GetInvalidOperationException("JsEnv had been destroy"));
    }
    auto func = apis->get_value_from_ref(env, delegateInfo->ValueRef);
    
    pesapi_value argv[3]{
        CSRefToJsValue(apis, env, TIp0, p0),
        CSRefToJsValue(apis, env, TIp1, p1),
        CSRefToJsValue(apis, env, TIp2, p2)
    };
    auto jsret = apis->call_function(env, func, nullptr, 3, argv);
    
    if (apis->has_caught(valueScope.scope()))
    {
        auto msg = apis->get_exception_as_string(valueScope.scope(), true);
        il2cpp::vm::Exception::Raise(il2cpp::vm::Exception::GetInvalidOperationException(msg));
    }
}

static void b_vooo_Invoker(Il2CppMethodPointer func, const MethodInfo* method, void* thisPtr, void** args, void* il2ppRetVal)
{
    b_vooo(thisPtr, (Il2CppObject*)args[0], (Il2CppObject*)args[1], (Il2CppObject*)args[2], (MethodInfo*)method);
}


// Action_Shared_4
static void b_voooo(void* target, Il2CppObject* p0, Il2CppObject* p1, Il2CppObject* p2, Il2CppObject* p3, MethodInfo* method) {
    // PLog("Running b_voooo");

    auto TIp0 = GetParameterType(method, 0);
    auto TIp1 = GetParameterType(method, 1);
    auto TIp2 = GetParameterType(method, 2);
    auto TIp3 = GetParameterType(method, 3);

    PObjectRefInfo* delegateInfo = GetPObjectRefInfo(target);
    struct pesapi_ffi* apis = delegateInfo->Apis;
    
    pesapi_env_ref envRef = apis->get_ref_associated_env(delegateInfo->ValueRef);
    AutoValueScope valueScope(apis, envRef);
    auto env = apis->get_env_from_ref(envRef);
    if (!env)
    {
        il2cpp::vm::Exception::Raise(il2cpp::vm::Exception::GetInvalidOperationException("JsEnv had been destroy"));
    }
    auto func = apis->get_value_from_ref(env, delegateInfo->ValueRef);
    
    pesapi_value argv[4]{
        CSRefToJsValue(apis, env, TIp0, p0),
        CSRefToJsValue(apis, env, TIp1, p1),
        CSRefToJsValue(apis, env, TIp2, p2),
        CSRefToJsValue(apis, env, TIp3, p3)
    };
    auto jsret = apis->call_function(env, func, nullptr, 4, argv);
    
    if (apis->has_caught(valueScope.scope()))
    {
        auto msg = apis->get_exception_as_string(valueScope.scope(), true);
        il2cpp::vm::Exception::Raise(il2cpp::vm::Exception::GetInvalidOperationException(msg));
    }
}

static void b_voooo_Invoker(Il2CppMethodPointer func, const MethodInfo* method, void* thisPtr, void** args, void* il2ppRetVal)
{
    b_voooo(thisPtr, (Il2CppObject*)args[0], (Il2CppObject*)args[1], (Il2CppObject*)args[2], (Il2CppObject*)args[3], (MethodInfo*)method);
}


// Action_Shared_5
static void b_vooooo(void* target, Il2CppObject* p0, Il2CppObject* p1, Il2CppObject* p2, Il2CppObject* p3, Il2CppObject* p4, MethodInfo* method) {
    // PLog("Running b_vooooo");

    auto TIp0 = GetParameterType(method, 0);
    auto TIp1 = GetParameterType(method, 1);
    auto TIp2 = GetParameterType(method, 2);
    auto TIp3 = GetParameterType(method, 3);
    auto TIp4 = GetParameterType(method, 4);

    PObjectRefInfo* delegateInfo = GetPObjectRefInfo(target);
    struct pesapi_ffi* apis = delegateInfo->Apis;
    
    pesapi_env_ref envRef = apis->get_ref_associated_env(delegateInfo->ValueRef);
    AutoValueScope valueScope(apis, envRef);
    auto env = apis->get_env_from_ref(envRef);
    if (!env)
    {
        il2cpp::vm::Exception::Raise(il2cpp::vm::Exception::GetInvalidOperationException("JsEnv had been destroy"));
    }
    auto func = apis->get_value_from_ref(env, delegateInfo->ValueRef);
    
    pesapi_value argv[5]{
        CSRefToJsValue(apis, env, TIp0, p0),
        CSRefToJsValue(apis, env, TIp1, p1),
        CSRefToJsValue(apis, env, TIp2, p2),
        CSRefToJsValue(apis, env, TIp3, p3),
        CSRefToJsValue(apis, env, TIp4, p4)
    };
    auto jsret = apis->call_function(env, func, nullptr, 5, argv);
    
    if (apis->has_caught(valueScope.scope()))
    {
        auto msg = apis->get_exception_as_string(valueScope.scope(), true);
        il2cpp::vm::Exception::Raise(il2cpp::vm::Exception::GetInvalidOperationException(msg));
    }
}

static void b_vooooo_Invoker(Il2CppMethodPointer func, const MethodInfo* method, void* thisPtr, void** args, void* il2ppRetVal)
{
    b_vooooo(thisPtr, (Il2CppObject*)args[0], (Il2CppObject*)args[1], (Il2CppObject*)args[2], (Il2CppObject*)args[3], (Il2CppObject*)args[4], (MethodInfo*)method);
}


// Action_Shared_6
static void b_voooooo(void* target, Il2CppObject* p0, Il2CppObject* p1, Il2CppObject* p2, Il2CppObject* p3, Il2CppObject* p4, Il2CppObject* p5, MethodInfo* method) {
    // PLog("Running b_voooooo");

    auto TIp0 = GetParameterType(method, 0);
    auto TIp1 = GetParameterType(method, 1);
    auto TIp2 = GetParameterType(method, 2);
    auto TIp3 = GetParameterType(method, 3);
    auto TIp4 = GetParameterType(method, 4);
    auto TIp5 = GetParameterType(method, 5);

    PObjectRefInfo* delegateInfo = GetPObjectRefInfo(target);
    struct pesapi_ffi* apis = delegateInfo->Apis;
    
    pesapi_env_ref envRef = apis->get_ref_associated_env(delegateInfo->ValueRef);
    AutoValueScope valueScope(apis, envRef);
    auto env = apis->get_env_from_ref(envRef);
    if (!env)
    {
        il2cpp::vm::Exception::Raise(il2cpp::vm::Exception::GetInvalidOperationException("JsEnv had been destroy"));
    }
    auto func = apis->get_value_from_ref(env, delegateInfo->ValueRef);
    
    pesapi_value argv[6]{
        CSRefToJsValue(apis, env, TIp0, p0),
        CSRefToJsValue(apis, env, TIp1, p1),
        CSRefToJsValue(apis, env, TIp2, p2),
        CSRefToJsValue(apis, env, TIp3, p3),
        CSRefToJsValue(apis, env, TIp4, p4),
        CSRefToJsValue(apis, env, TIp5, p5)
    };
    auto jsret = apis->call_function(env, func, nullptr, 6, argv);
    
    if (apis->has_caught(valueScope.scope()))
    {
        auto msg = apis->get_exception_as_string(valueScope.scope(), true);
        il2cpp::vm::Exception::Raise(il2cpp::vm::Exception::GetInvalidOperationException(msg));
    }
}

static void b_voooooo_Invoker(Il2CppMethodPointer func, const MethodInfo* method, void* thisPtr, void** args, void* il2ppRetVal)
{
    b_voooooo(thisPtr, (Il2CppObject*)args[0], (Il2CppObject*)args[1], (Il2CppObject*)args[2], (Il2CppObject*)args[3], (Il2CppObject*)args[4], (Il2CppObject*)args[5], (MethodInfo*)method);
}


// Action_Shared_7
static void b_vooooooo(void* target, Il2CppObject* p0, Il2CppObject* p1, Il2CppObject* p2, Il2CppObject* p3, Il2CppObject* p4, Il2CppObject* p5, Il2CppObject* p6, MethodInfo* method) {
    // PLog("Running b_vooooooo");

    auto TIp0 = GetParameterType(method, 0);
    auto TIp1 = GetParameterType(method, 1);
    auto TIp2 = GetParameterType(method, 2);
    auto TIp3 = GetParameterType(method, 3);
    auto TIp4 = GetParameterType(method, 4);
    auto TIp5 = GetParameterType(method, 5);
    auto TIp6 = GetParameterType(method, 6);

    PObjectRefInfo* delegateInfo = GetPObjectRefInfo(target);
    struct pesapi_ffi* apis = delegateInfo->Apis;
    
    pesapi_env_ref envRef = apis->get_ref_associated_env(delegateInfo->ValueRef);
    AutoValueScope valueScope(apis, envRef);
    auto env = apis->get_env_from_ref(envRef);
    if (!env)
    {
        il2cpp::vm::Exception::Raise(il2cpp::vm::Exception::GetInvalidOperationException("JsEnv had been destroy"));
    }
    auto func = apis->get_value_from_ref(env, delegateInfo->ValueRef);
    
    pesapi_value argv[7]{
        CSRefToJsValue(apis, env, TIp0, p0),
        CSRefToJsValue(apis, env, TIp1, p1),
        CSRefToJsValue(apis, env, TIp2, p2),
        CSRefToJsValue(apis, env, TIp3, p3),
        CSRefToJsValue(apis, env, TIp4, p4),
        CSRefToJsValue(apis, env, TIp5, p5),
        CSRefToJsValue(apis, env, TIp6, p6)
    };
    auto jsret = apis->call_function(env, func, nullptr, 7, argv);
    
    if (apis->has_caught(valueScope.scope()))
    {
        auto msg = apis->get_exception_as_string(valueScope.scope(), true);
        il2cpp::vm::Exception::Raise(il2cpp::vm::Exception::GetInvalidOperationException(msg));
    }
}

static void b_vooooooo_Invoker(Il2CppMethodPointer func, const MethodInfo* method, void* thisPtr, void** args, void* il2ppRetVal)
{
    b_vooooooo(thisPtr, (Il2CppObject*)args[0], (Il2CppObject*)args[1], (Il2CppObject*)args[2], (Il2CppObject*)args[3], (Il2CppObject*)args[4], (Il2CppObject*)args[5], (Il2CppObject*)args[6], (MethodInfo*)method);
}


// Action_Shared_8
static void b_voooooooo(void* target, Il2CppObject* p0, Il2CppObject* p1, Il2CppObject* p2, Il2CppObject* p3, Il2CppObject* p4, Il2CppObject* p5, Il2CppObject* p6, Il2CppObject* p7, MethodInfo* method) {
    // PLog("Running b_voooooooo");

    auto TIp0 = GetParameterType(method, 0);
    auto TIp1 = GetParameterType(method, 1);
    auto TIp2 = GetParameterType(method, 2);
    auto TIp3 = GetParameterType(method, 3);
    auto TIp4 = GetParameterType(method, 4);
    auto TIp5 = GetParameterType(method, 5);
    auto TIp6 = GetParameterType(method, 6);
    auto TIp7 = GetParameterType(method, 7);

    PObjectRefInfo* delegateInfo = GetPObjectRefInfo(target);
    struct pesapi_ffi* apis = delegateInfo->Apis;
    
    pesapi_env_ref envRef = apis->get_ref_associated_env(delegateInfo->ValueRef);
    AutoValueScope valueScope(apis, envRef);
    auto env = apis->get_env_from_ref(envRef);
    if (!env)
    {
        il2cpp::vm::Exception::Raise(il2cpp::vm::Exception::GetInvalidOperationException("JsEnv had been destroy"));
    }
    auto func = apis->get_value_from_ref(env, delegateInfo->ValueRef);
    
    pesapi_value argv[8]{
        CSRefToJsValue(apis, env, TIp0, p0),
        CSRefToJsValue(apis, env, TIp1, p1),
        CSRefToJsValue(apis, env, TIp2, p2),
        CSRefToJsValue(apis, env, TIp3, p3),
        CSRefToJsValue(apis, env, TIp4, p4),
        CSRefToJsValue(apis, env, TIp5, p5),
        CSRefToJsValue(apis, env, TIp6, p6),
        CSRefToJsValue(apis, env, TIp7, p7)
    };
    auto jsret = apis->call_function(env, func, nullptr, 8, argv);
    
    if (apis->has_caught(valueScope.scope()))
    {
        auto msg = apis->get_exception_as_string(valueScope.scope(), true);
        il2cpp::vm::Exception::Raise(il2cpp::vm::Exception::GetInvalidOperationException(msg));
    }
}

static void b_voooooooo_Invoker(Il2CppMethodPointer func, const MethodInfo* method, void* thisPtr, void** args, void* il2ppRetVal)
{
    b_voooooooo(thisPtr, (Il2CppObject*)args[0], (Il2CppObject*)args[1], (Il2CppObject*)args[2], (Il2CppObject*)args[3], (Il2CppObject*)args[4], (Il2CppObject*)args[5], (Il2CppObject*)args[6], (Il2CppObject*)args[7], (MethodInfo*)method);
}


// Action_Shared_9
static void b_vooooooooo(void* target, Il2CppObject* p0, Il2CppObject* p1, Il2CppObject* p2, Il2CppObject* p3, Il2CppObject* p4, Il2CppObject* p5, Il2CppObject* p6, Il2CppObject* p7, Il2CppObject* p8, MethodInfo* method) {
    // PLog("Running b_vooooooooo");

    auto TIp0 = GetParameterType(method, 0);
    auto TIp1 = GetParameterType(method, 1);
    auto TIp2 = GetParameterType(method, 2);
    auto TIp3 = GetParameterType(method, 3);
    auto TIp4 = GetParameterType(method, 4);
    auto TIp5 = GetParameterType(method, 5);
    auto TIp6 = GetParameterType(method, 6);
    auto TIp7 = GetParameterType(method, 7);
    auto TIp8 = GetParameterType(method, 8);

    PObjectRefInfo* delegateInfo = GetPObjectRefInfo(target);
    struct pesapi_ffi* apis = delegateInfo->Apis;
    
    pesapi_env_ref envRef = apis->get_ref_associated_env(delegateInfo->ValueRef);
    AutoValueScope valueScope(apis, envRef);
    auto env = apis->get_env_from_ref(envRef);
    if (!env)
    {
        il2cpp::vm::Exception::Raise(il2cpp::vm::Exception::GetInvalidOperationException("JsEnv had been destroy"));
    }
    auto func = apis->get_value_from_ref(env, delegateInfo->ValueRef);
    
    pesapi_value argv[9]{
        CSRefToJsValue(apis, env, TIp0, p0),
        CSRefToJsValue(apis, env, TIp1, p1),
        CSRefToJsValue(apis, env, TIp2, p2),
        CSRefToJsValue(apis, env, TIp3, p3),
        CSRefToJsValue(apis, env, TIp4, p4),
        CSRefToJsValue(apis, env, TIp5, p5),
        CSRefToJsValue(apis, env, TIp6, p6),
        CSRefToJsValue(apis, env, TIp7, p7),
        CSRefToJsValue(apis, env, TIp8, p8)
    };
    auto jsret = apis->call_function(env, func, nullptr, 9, argv);
    
    if (apis->has_caught(valueScope.scope()))
    {
        auto msg = apis->get_exception_as_string(valueScope.scope(), true);
        il2cpp::vm::Exception::Raise(il2cpp::vm::Exception::GetInvalidOperationException(msg));
    }
}

static void b_vooooooooo_Invoker(Il2CppMethodPointer func, const MethodInfo* method, void* thisPtr, void** args, void* il2ppRetVal)
{
    b_vooooooooo(thisPtr, (Il2CppObject*)args[0], (Il2CppObject*)args[1], (Il2CppObject*)args[2], (Il2CppObject*)args[3], (Il2CppObject*)args[4], (Il2CppObject*)args[5], (Il2CppObject*)args[6], (Il2CppObject*)args[7], (Il2CppObject*)args[8], (MethodInfo*)method);
}


static BridgeFuncInfo g_bridgeFuncInfos[] = {
    {"o", (Il2CppMethodPointer)b_o, b_o_Invoker},
    {"oo", (Il2CppMethodPointer)b_oo, b_oo_Invoker},
    {"ooo", (Il2CppMethodPointer)b_ooo, b_ooo_Invoker},
    {"oooo", (Il2CppMethodPointer)b_oooo, b_oooo_Invoker},
    {"ooooo", (Il2CppMethodPointer)b_ooooo, b_ooooo_Invoker},
    {"oooooo", (Il2CppMethodPointer)b_oooooo, b_oooooo_Invoker},
    {"ooooooo", (Il2CppMethodPointer)b_ooooooo, b_ooooooo_Invoker},
    {"oooooooo", (Il2CppMethodPointer)b_oooooooo, b_oooooooo_Invoker},
    {"ooooooooo", (Il2CppMethodPointer)b_ooooooooo, b_ooooooooo_Invoker},
    {"oooooooooo", (Il2CppMethodPointer)b_oooooooooo, b_oooooooooo_Invoker},
    {"v", (Il2CppMethodPointer)b_v, b_v_Invoker},
    {"vo", (Il2CppMethodPointer)b_vo, b_vo_Invoker},
    {"voo", (Il2CppMethodPointer)b_voo, b_voo_Invoker},
    {"vooo", (Il2CppMethodPointer)b_vooo, b_vooo_Invoker},
    {"voooo", (Il2CppMethodPointer)b_voooo, b_voooo_Invoker},
    {"vooooo", (Il2CppMethodPointer)b_vooooo, b_vooooo_Invoker},
    {"voooooo", (Il2CppMethodPointer)b_voooooo, b_voooooo_Invoker},
    {"vooooooo", (Il2CppMethodPointer)b_vooooooo, b_vooooooo_Invoker},
    {"voooooooo", (Il2CppMethodPointer)b_voooooooo, b_voooooooo_Invoker},
    {"vooooooooo", (Il2CppMethodPointer)b_vooooooooo, b_vooooooooo_Invoker},
    {nullptr, nullptr, nullptr}
};

BridgeFuncInfo* FindBridgeFunc(const char* signature)
{
    auto begin = &g_bridgeFuncInfos[0];
    auto end = &g_bridgeFuncInfos[sizeof(g_bridgeFuncInfos) / sizeof(BridgeFuncInfo) - 1];
    auto first = std::lower_bound(begin, end, signature, [](const BridgeFuncInfo& x, const char* signature) {return strcmp(x.Signature, signature) < 0;});
    if (first != end && strcmp(first->Signature, signature) == 0) {
        return first;
    }
    return nullptr;
}

}
