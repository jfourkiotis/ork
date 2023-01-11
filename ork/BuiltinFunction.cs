namespace ork.builtins;

using static ork.Object;
using BuiltinFunction = Func<object?[], object?>;

public static class Builtin
{
    public static readonly Dictionary<string, BuiltinFunction> Functions = new()
    {
        { "len", args =>
        {
            if (args.Length != 1)
                throw new OrkRuntimeException($"wrong number of arguments, got={args.Length}, want=1");
            return args[0] switch
            {
                string s => (Int64)s.Length,
                _ => throw new OrkRuntimeException($"argument to `len` not supported, got {TypeName(args[0])}"),
            };
        } },
    };
}