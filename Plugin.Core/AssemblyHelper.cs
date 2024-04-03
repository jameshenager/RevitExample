using System;
using System.Linq;

namespace Plugin.Core;

public class AssemblyHelper
{
    public static bool IsRunningInRevit() =>
        AppDomain
            .CurrentDomain
            .GetAssemblies()
            .Any(assembly => assembly.FullName.StartsWith("RevitAPI", StringComparison.OrdinalIgnoreCase));
}