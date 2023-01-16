namespace ork.builtins;

using System.Collections.Immutable;
using static Object;
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
                string s => (long)s.Length,
                ImmutableArray<object?> l => (long)l.Length,
                _ => throw new OrkRuntimeException($"argument to `len` not supported, got {TypeName(args[0])}"),
            };
        } },
        { "first", args =>
        {
            if (args.Length != 1)
                throw new OrkRuntimeException($"wrong number of arguments, got={args.Length}, want=1");
            return args[0] switch
            {
                ImmutableArray<object?> l => l.Length > 0 ? l[0] : null,
                _ => throw new OrkRuntimeException($"argument to `first` must be ARRAY, got {TypeName(args[0])}"),
            };
        } },
        { "last", args =>
        {
            if (args.Length != 1)
                throw new OrkRuntimeException($"wrong number of arguments, got={args.Length}, want=1");
            return args[0] switch
            {
                ImmutableArray<object?> l => l.Length > 0 ? l[^1] : null,
                _ => throw new OrkRuntimeException($"argument to `last` must be ARRAY, got {TypeName(args[0])}"),
            };
        } },
        { "rest", args =>
        {
            if (args.Length != 1)
                throw new OrkRuntimeException($"wrong number of arguments, got={args.Length}, want=1");
            return args[0] switch
            {
                ImmutableArray<object?> l => l[1..],
                _ => throw new OrkRuntimeException($"argument to `last` must be ARRAY, got {TypeName(args[0])}"),
            };
        } },
        { "puts", args =>
        {
            foreach (var a in args)
            {
                Console.WriteLine(Object.ToString(a));
            }
            return null;
        } },
    };
}