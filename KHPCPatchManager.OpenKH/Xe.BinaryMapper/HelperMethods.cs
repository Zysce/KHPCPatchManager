using System;
using System.Collections;

namespace KHPCPatchManager.OpenKH.Xe.BinaryMapper;

internal static class HelperMethods
{
    public static bool CanEnumerate(this Type type) =>
        type.IsAssignableFrom(typeof(IEnumerable)) || ((IList)type.GetInterfaces())
            .Contains(typeof(IEnumerable));
}
