using System.Collections.Immutable;

namespace ork;

public static class Object
{
    public static string TypeName(object? o) => o switch
    {
        long => "INTEGER",
        bool => "BOOLEAN",
        null => "NIL",
        string => "STRING",
        Func<object?[], object?> => "BUILTIN",
        ImmutableArray<object?> => "ARRAY",
        ImmutableDictionary<object, object?> => "HASH",
        _ => throw new NotImplementedException(),
    };

    public static string ToString(object? o) => o switch
    {
        long l => l.ToString(),
        bool a => a.ToString(),
        null => "nil",
        string s => s,
        Func<object?[], object?> => "builtin",
        ImmutableArray<object?> l => "[" + String.Join(", ", l) + "]",
        ImmutableDictionary<object, object?> d => "{" + String.Join(", ", d.Select(kv => ToString(kv.Key) + " : " + ToString(kv.Value)).ToList()) + "}",
        _ => throw new NotImplementedException(),
    };
}