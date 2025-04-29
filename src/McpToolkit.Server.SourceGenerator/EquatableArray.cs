namespace McpToolkit.Server.SourceGenerator;

public readonly struct EquatableArray<T>(T[] array) : IEquatable<EquatableArray<T>>
{
    public ref T this[int index] => ref Array[index];

    public T[] Array { get; } = array;
    public int Length => Array.Length;

    public bool Equals(EquatableArray<T> other)
    {
        return Array.SequenceEqual(other.Array);
    }

    public static implicit operator EquatableArray<T>(T[] array)
    {
        return new(array);
    }
}
