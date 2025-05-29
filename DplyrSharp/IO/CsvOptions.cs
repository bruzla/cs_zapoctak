namespace DplyrSharp.IO;

/// <summary>
/// Provides options for reading from or writing to CSV files.
/// </summary>
public sealed class CsvOptions
{
    /// <summary>
    /// Gets the default CSV options:
    /// comma as the delimiter, UTF-8 encoding, and header enabled.
    /// </summary>
    public static readonly CsvOptions Default = new CsvOptions();


    /// <summary>
    /// Gets or sets the character used to separate values in the CSV file.
    /// Default is a comma (<c>,</c>).
    /// </summary>
    public char Delimiter { get; set; } = ',';

    /// <summary>
    /// Gets or sets a value indicating whether the CSV file includes a header row.
    /// Default is <c>true</c>.
    /// </summary>
    public bool HasHeader { get; set; } = true;

    /// <summary>
    /// Gets or sets the text encoding used when reading or writing CSV files.
    /// Default is UTF-8.
    /// </summary>
    public System.Text.Encoding Encoding { get; set; } = System.Text.Encoding.UTF8;
}
