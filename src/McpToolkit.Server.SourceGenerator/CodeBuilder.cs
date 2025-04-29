using System.Text;

namespace McpToolkit.Server.SourceGenerator;

internal class CodeBuilder(int level)
{
    StringBuilder builder = new StringBuilder();

    public void Indent(int levelIncr = 1)
    {
        level += levelIncr;
    }

    public void Unindent(int levelDecr = 1)
    {
        level -= levelDecr;
    }

    public Scope BeginIndent()
    {
        Indent();
        return new Scope(this);
    }

    public Scope BeginIndent(string code)
    {
        AppendLine(code);
        Indent();
        return new Scope(this);
    }

    public Block BeginBlock()
    {
        AppendLine("{");
        Indent();
        return new Block(this);
    }

    public Block BeginBlock(string code)
    {
        AppendLine(code);
        AppendLine("{");
        Indent();
        return new Block(this);
    }

    public IDisposable Nop => NullDisposable.Instance;

    public void AppendLine()
    {
        builder.AppendLine();
    }

    public void AppendLine(string text)
    {
        if (level != 0)
        {
            builder.Append(' ', level * 4);
        }
        builder.AppendLine(text);
    }

    public override string ToString() => builder.ToString();

    public struct Scope(CodeBuilder parent) : IDisposable
    {
        public void Dispose()
        {
            parent.Unindent();
        }
    }

    public struct Block(CodeBuilder parent) : IDisposable
    {
        public void Dispose()
        {
            parent.Unindent();
            parent.AppendLine("}");
        }
    }

    public CodeBuilder Clone()
    {
        var sb = new CodeBuilder(level);
        sb.builder.Append(builder.ToString());
        return sb;
    }

    class NullDisposable : IDisposable
    {
        public static readonly IDisposable Instance = new NullDisposable();

        public void Dispose()
        {
        }
    }
}