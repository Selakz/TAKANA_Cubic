using System;
using System.Reflection;
using UnityEngine.Scripting;

namespace Puerts
{
public static class ExtensionMethodInfos_Gen
{
    [Preserve]
    public static MethodInfo[] TryLoadExtensionMethod(string assemblyQualifiedName)
    {
        if (false) {}
        else if (typeof(int[]).AssemblyQualifiedName == assemblyQualifiedName)
        {
            return ExtensionMethodInfo.GetExtensionMethods(typeof(int[]), typeof(ArrayExtension));
        }
        else if (typeof(float[]).AssemblyQualifiedName == assemblyQualifiedName)
        {
            return ExtensionMethodInfo.GetExtensionMethods(typeof(float[]), typeof(ArrayExtension));
        }
        else if (typeof(double[]).AssemblyQualifiedName == assemblyQualifiedName)
        {
            return ExtensionMethodInfo.GetExtensionMethods(typeof(double[]), typeof(ArrayExtension));
        }
        else if (typeof(bool[]).AssemblyQualifiedName == assemblyQualifiedName)
        {
            return ExtensionMethodInfo.GetExtensionMethods(typeof(bool[]), typeof(ArrayExtension));
        }
        else if (typeof(long[]).AssemblyQualifiedName == assemblyQualifiedName)
        {
            return ExtensionMethodInfo.GetExtensionMethods(typeof(long[]), typeof(ArrayExtension));
        }
        else if (typeof(ulong[]).AssemblyQualifiedName == assemblyQualifiedName)
        {
            return ExtensionMethodInfo.GetExtensionMethods(typeof(ulong[]), typeof(ArrayExtension));
        }
        else if (typeof(sbyte[]).AssemblyQualifiedName == assemblyQualifiedName)
        {
            return ExtensionMethodInfo.GetExtensionMethods(typeof(sbyte[]), typeof(ArrayExtension));
        }
        else if (typeof(byte[]).AssemblyQualifiedName == assemblyQualifiedName)
        {
            return ExtensionMethodInfo.GetExtensionMethods(typeof(byte[]), typeof(ArrayExtension));
        }
        else if (typeof(ushort[]).AssemblyQualifiedName == assemblyQualifiedName)
        {
            return ExtensionMethodInfo.GetExtensionMethods(typeof(ushort[]), typeof(ArrayExtension));
        }
        else if (typeof(short[]).AssemblyQualifiedName == assemblyQualifiedName)
        {
            return ExtensionMethodInfo.GetExtensionMethods(typeof(short[]), typeof(ArrayExtension));
        }
        else if (typeof(Char[]).AssemblyQualifiedName == assemblyQualifiedName)
        {
            return ExtensionMethodInfo.GetExtensionMethods(typeof(Char[]), typeof(ArrayExtension));
        }
        else if (typeof(uint[]).AssemblyQualifiedName == assemblyQualifiedName)
        {
            return ExtensionMethodInfo.GetExtensionMethods(typeof(uint[]), typeof(ArrayExtension));
        }
        else if (typeof(string[]).AssemblyQualifiedName == assemblyQualifiedName)
        {
            return ExtensionMethodInfo.GetExtensionMethods(typeof(string[]), typeof(ArrayExtension));
        }
        else if (typeof(Array).AssemblyQualifiedName == assemblyQualifiedName)
        {
            return ExtensionMethodInfo.GetExtensionMethods(typeof(Array), typeof(ArrayExtension));
        }
        return null;
    }
}
}