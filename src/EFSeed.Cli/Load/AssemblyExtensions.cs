using System.Reflection;

namespace EFSeed.Cli;

public static class AssemblyExtensions
{
    public static IEnumerable<TypeInfo> GetLoadableDefinedTypes(
        this Assembly assembly)
    {
        try
        {
            return assembly.DefinedTypes;
        }
        catch (ReflectionTypeLoadException ex)
        {
            return ex.Types.Where(t => t != null).Select(IntrospectionExtensions.GetTypeInfo!);
        }
    }
}
