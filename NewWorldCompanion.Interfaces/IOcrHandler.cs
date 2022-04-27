namespace NewWorldCompanion.Interfaces
{
    public interface IOcrHandler
    {
        string OcrText { get; }
        string OcrTextCount { get; }
        string OcrTextCountRaw { get; }
    }
}
