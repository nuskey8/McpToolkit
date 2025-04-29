namespace McpToolkit.Server.SourceGenerator;

public readonly struct IgnoreEquality<T>(T value) : IEquatable<IgnoreEquality<T>>
{
    public T Value => value;

    public bool Equals(IgnoreEquality<T> other)
    {
        return true;
    }

    public static implicit operator IgnoreEquality<T>(T value)
    {
        return new(value);
    }
}