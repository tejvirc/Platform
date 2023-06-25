namespace Aristocrat.Fluxor.Extensions;

using System;

public interface IJsonSerialization
{
    object? Deserialize(string json, Type type);
    string Serialize(object source, Type type);
}

public static class JsonSerializationExtensions
{
    public static T? Deserialize<T>(this IJsonSerialization instance, string json) =>
        (T?)instance.Deserialize(json, typeof(T));
}
