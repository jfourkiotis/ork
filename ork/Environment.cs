namespace ork;

public class Environment
{
    private IDictionary<string, object?> store = new Dictionary<string, object?>();


    public Environment() : this(null)
    {
    }

    public Environment(Environment? parent)
    {
        ParentEnvironment = parent;
    }

    public Environment? ParentEnvironment { get; }

    public bool TryGet(string key, out object? val) => store.TryGetValue(key, out val) || (ParentEnvironment?.TryGet(key, out val) ?? false);
    public void Set(string key, object? val) => store[key] = val;
}