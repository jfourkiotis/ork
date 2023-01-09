namespace ork;

public class Environment
{
    private IDictionary<string, object?> store = new Dictionary<string, object?>();

    public Environment()
    {
    }

    public bool TryGet(string key, out object? val) => store.TryGetValue(key, out val);
    public void Set(string key, object? val) => store[key] = val;
}