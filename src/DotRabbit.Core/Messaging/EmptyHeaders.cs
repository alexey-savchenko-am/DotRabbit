namespace DotRabbit.Core.Messaging;

internal sealed class EmptyHeaders : IReadOnlyDictionary<string, object?>
{
    public static readonly EmptyHeaders Instance = new();

    private EmptyHeaders() { }

    public object? this[string key] => throw new KeyNotFoundException();
    public IEnumerable<string> Keys => [];
    public IEnumerable<object?> Values => [];
    public int Count => 0;

    public bool ContainsKey(string key) => false;
    public IEnumerator<KeyValuePair<string, object?>> GetEnumerator()
        => Enumerable.Empty<KeyValuePair<string, object?>>().GetEnumerator();
    public bool TryGetValue(string key, out object? value)
    {
        value = null;
        return false;
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        => GetEnumerator();
}
