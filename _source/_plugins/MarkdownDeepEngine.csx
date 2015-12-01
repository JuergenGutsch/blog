#r MarkdownDeep.dll

using MarkdownDeep;

[Export(typeof(ILightweightMarkupEngine))]
public sealed class MarkdownDeepEngine : ILightweightMarkupEngine
{
    private readonly Markdown _markdownDeep; 

    public MarkdownDeepEngine()
    {
        _markdownDeep = new MarkdownDeep.Markdown();
        _markdownDeep.ExtraMode = true;
    }

    public string Convert(string markdownContent)
    {
        return _markdownDeep.Transform(markdownContent);
    }
}