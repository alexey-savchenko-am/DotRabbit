namespace DotRabbit.Core.Messaging.Entities;

internal sealed class EmptyHeaders : IReadOnlyDictionary<string, string?>
{
    public static readonly EmptyHeaders Instance = new();

    private EmptyHeaders() { }

    public string? this[string key] => throw new KeyNotFoundException();
    public IEnumerable<string> Keys => [];
    public IEnumerable<string?> Values => [];
    public int Count => 0;

    public bool ContainsKey(string key) => false;
    public IEnumerator<KeyValuePair<string, string?>> GetEnumerator()
        => Enumerable.Empty<KeyValuePair<string, string?>>().GetEnumerator();
    public bool TryGetValue(string key, out string? value)
    {
        value = null;
        return false;
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        => GetEnumerator();
}
