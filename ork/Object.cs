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
        _ => throw new NotImplementedException(),
    };
}