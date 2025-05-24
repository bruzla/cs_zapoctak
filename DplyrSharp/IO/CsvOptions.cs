namespace DplyrSharp.IO;

public sealed class CsvOptions
{
    public char Delimiter { get; set; } = ',';
    public bool HasHeader { get; set; } = true;
    public System.Text.Encoding Encoding { get; set; } = System.Text.Encoding.UTF8;
}