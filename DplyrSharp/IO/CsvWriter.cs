using System;
using System.IO;
using System.Linq;
using DplyrSharp.Core;

namespace DplyrSharp.IO;

public static class CsvWriter
{
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