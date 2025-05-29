using System;
using System.IO;
using System.Linq;
using DplyrSharp.Core;

namespace DplyrSharp.IO;

/// <summary>
/// Provides functionality for writing <see cref="DataFrame"/> objects to CSV files.
/// </summary>
public static class CsvWriter
{
    /// <summary>
    /// Writes the contents of a <see cref="DataFrame"/> to a CSV file at the specified path.
    /// </summary>
    /// <param name="df">The data frame to write.</param>
    /// <param name="path">The path to the output CSV file.</param>
    /// <param name="options">Optional settings such as delimiter, encoding, and whether to write a header row.</param>
    public static void WriteCsv(DataFrame df, string path, CsvOptions? options = null)
    {
        options ??= new CsvOptions();
        using var writer = new StreamWriter(path);

        if (options.HasHeader)
            writer.WriteLine(string.Join(options.Delimiter, df.Columns.Select(c => c.Name)));

        int rowCount = df.Columns.Count > 0 ? df.Columns[0].RowCount : 0;
        for (int i = 0; i < rowCount; i++)
        {
            var fields = df.Columns.Select(c =>
            {
                var val = c.GetValue(i);
                return val?.ToString() ?? string.Empty;
            });

            writer.WriteLine(string.Join(options.Delimiter, fields));
        }
    }
}
