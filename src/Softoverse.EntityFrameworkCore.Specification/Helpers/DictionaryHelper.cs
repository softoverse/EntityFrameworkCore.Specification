using System.Text.Json;

namespace Softoverse.EntityFrameworkCore.Specification.Helpers;

public static class DictionaryHelper
{
    public static IDictionary<string, object> FlattenDictionary(this IDictionary<string, object> dictionary, string parentKey = "")
    {
        var flattened = new Dictionary<string, object>();

        foreach (var kvp in dictionary)
        {
            string key = string.IsNullOrEmpty(parentKey) ? kvp.Key : $"{parentKey}.{kvp.Key}";

            object value = kvp.Value;

            // 🔹 Convert JsonElement to object if needed
            if (value is JsonElement jsonElement)
            {
                value = ConvertJsonElement(jsonElement);
            }

            if (value is IDictionary<string, object> nestedDict)
            {
                // Recursively process nested dictionaries
                foreach (var nestedKvp in FlattenDictionary(nestedDict, key))
                {
                    flattened[nestedKvp.Key] = nestedKvp.Value;
                }
            }
            else
            {
                // Add normal key-value pairs
                flattened[key] = value;
            }
        }

        return flattened;
    }

    public static object ConvertJsonElement(this JsonElement jsonElement)
    {
        object? value;
        switch (jsonElement.ValueKind)
        {
            case JsonValueKind.String:
                {
                    var str = jsonElement.GetString();
                    value = str is
                    {
                        Length: 1
                    } ? str[0] : str;
                    break;
                }
            case JsonValueKind.Number:
                value = jsonElement.TryGetInt64(out long longValue)
                    ? longValue
                    : jsonElement.TryGetDouble(out double doubleValue)
                        ? doubleValue
                        : jsonElement.TryGetDecimal(out decimal decimalValue)
                            ? decimalValue
                            : jsonElement.TryGetSingle(out float floatValue)
                                ? floatValue
                                : jsonElement.TryGetByte(out byte byteValue)
                                    ? byteValue
                                    : jsonElement.TryGetSByte(out sbyte sbyteValue)
                                        ? sbyteValue
                                        : jsonElement.TryGetUInt16(out ushort ushortValue)
                                            ? ushortValue
                                            : jsonElement.TryGetInt16(out short shortValue)
                                                ? shortValue
                                                : jsonElement.TryGetUInt32(out uint uintValue)
                                                    ? uintValue
                                                    : jsonElement.TryGetInt32(out int intValue)
                                                        ? intValue
                                                        : jsonElement.GetDouble();
                break;
            case JsonValueKind.True:
                value = true;
                break;
            case JsonValueKind.False:
                value = false;
                break;
            case JsonValueKind.Null or JsonValueKind.Undefined:
                value = null;
                break;
            case JsonValueKind.Array:
                value = jsonElement.EnumerateArray().Select(ConvertJsonElement).ToArray();
                break;
            case JsonValueKind.Object:
                value = jsonElement.EnumerateObject().ToDictionary(x => x.Name, x => ConvertJsonElement(x.Value));
                break;
            default:
                value = jsonElement.ToString();
                break;
        }

        return value!;
    }
}