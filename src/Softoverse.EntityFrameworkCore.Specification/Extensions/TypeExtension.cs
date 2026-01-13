namespace Softoverse.EntityFrameworkCore.Specification.Extensions;

internal static class TypeExtension
{
    public static bool IsNumeric(this Type type)
    {
        return type == typeof(int) || type == typeof(long) || type == typeof(float) ||
               type == typeof(double) || type == typeof(decimal) || type == typeof(short) ||
               type == typeof(byte) || type == typeof(uint) || type == typeof(ulong) ||
               type == typeof(ushort) || type == typeof(sbyte);
    }

    public static bool IsComparable(this Type type)
    {
        return typeof(IComparable).IsAssignableFrom(type) || type.IsPrimitive || type == typeof(string);
    }
}