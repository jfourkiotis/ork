namespace ork;

public class Environment
{
    private readonly IDictionary<string, object?> store = new Dictionary<string, object?>();
    private readonly Environment? parent;


    public Environment() : this(null)
    {
    }

    public Environment(Environment? parent)
    {
        this.parent = parent;
    }
    
    public bool TryGet(string key, out object? val) =>
        store.TryGetValue(key, out val) || (parent?.TryGet(key, out val) ?? false);
    public void Set(string key, object? val) => store[key] = val;
}