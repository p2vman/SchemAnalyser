using NbtToolkit;

namespace SchemAnalyser;

public class Mapper
{
    private Dictionary<string, Func<TagCompound, object>> _Map { get; } =
        new Dictionary<string, Func<TagCompound, object>>();

    public Mapper Put(string key, Func<TagCompound, object> func)
    {
        _Map[key] = func;
        return this;
    }

    public T Map<T>(string id, TagCompound source)
    {
        if (source is null || !_Map.TryGetValue(id, out var func))
            return default;

        var result = func(source);
        if (result is T value)
            return value;

        throw new InvalidCastException($"Cannot cast mapping result for '{id}' to {typeof(T)}");
    }
    
    public T Map<T>(TagCompound source)
    {
        if (source is null || !source.ContainsKey("id") || !_Map.TryGetValue(source["id"].AsString(), out var func))
            return default;

        var result = func(source);
        if (result is T value)
            return value;

        throw new InvalidCastException($"Cannot cast mapping {typeof(T)}");
    }
}
