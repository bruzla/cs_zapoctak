using System.Collections;
using System.ComponentModel;
using DplyrSharp.Core;

namespace DplyrSharp.IO;

public static class CsvReader
{
    public static DataFrame ReadCsv(string path, CsvOptions? options = null)
    {
        options ??= new CsvOptions();
        using var reader = new StreamReader(path, options.Encoding);

        string[] headers = GetHeaders(reader, options);
        List<string[]> rows = GetAllRows(reader, options, headers.Length);
        List<IDataColumn> columns = GetColumns(headers, rows);

        return new DataFrame(columns);
    }

    private static string[] GetHeaders(StreamReader reader, CsvOptions options)
    {
        string[] headers;

        if (options.HasHeader)
        {
            var headerLine = reader.ReadLine() ?? throw new InvalidOperationException("CSV is empty");
            headers = headerLine.Split(options.Delimiter);
            headers = TrimWhitespace(headers);
        }
        else
        {
            var firstLine = reader.ReadLine() ?? throw new InvalidOperationException("CSV is empty");
            var count = firstLine.Split(options.Delimiter).Length;
            headers = Enumerable.Range(0, count).Select(i => $"Column_{i}").ToArray();
            reader.BaseStream.Seek(0, SeekOrigin.Begin);
            reader.DiscardBufferedData();
        }

        return headers;
    }

    private static string[] TrimWhitespace(string[] values)
    {
        for (int i = 0; i < values.Length; ++i)
        {
            values[i] = values[i].Trim();
        }

        return values;
    }

    private static List<string[]> GetAllRows(StreamReader reader, CsvOptions options, int numberOfColumns)
    {
        var rows = new List<string[]>();
        while (!reader.EndOfStream)
        {
            var line = reader.ReadLine()!;
            if (String.IsNullOrEmpty(line)) continue;

            var lineItems = line.Split(options.Delimiter);
            if (lineItems.Length != numberOfColumns) throw new InvalidOperationException($"Number of items in a row does not match the number of columns. Faulty row: \"{line}\"");

            rows.Add(lineItems);
        }

        return rows;
    }

    private static List<IDataColumn> GetColumns(string[] headers, List<string[]> rows)
    {
        var columns = new List<IDataColumn>(headers.Length);

        for (int i = 0; i < headers.Length; ++i)
        {
            var rawValues = rows.Select(r => r[i]);
            var column = BuildColumn(rawValues, headers[i], rows.Count);
            columns.Add(column);
        }

        return columns;
    }

    private static IDataColumn BuildColumn(IEnumerable<string> rawValues, string header, int numberOfRows)
    {
        var dataType = TypeInferer.InferColumnType(rawValues);

        if (dataType == typeof(string))
        {
            return BuildStringColumn(rawValues, header, numberOfRows);
        }

        try
        {
            var (dataArray, nullBitmap) = CastColumnValues(rawValues, dataType, numberOfRows);
            var colType = typeof(DataColumn<>).MakeGenericType(dataType);
            var column = (IDataColumn)Activator.CreateInstance(colType, header, dataArray, nullBitmap)!;

            return column;
        }
        catch (NotSupportedException)
        {
            return BuildStringColumn(rawValues, header, numberOfRows);
        }
    }

    private static IDataColumn BuildStringColumn(IEnumerable<string> rawValues, string header, int numberOfRows)
    {
        var data = rawValues.ToArray();
        var nullBitmap = new BitArray(numberOfRows);

        return new DataColumn<string>(header, data, nullBitmap);
    }

    private static (Array data, BitArray nullBitmap) CastColumnValues(IEnumerable<string> rawValues, Type type, int numberOfRows)
    {
        var data = Array.CreateInstance(type, numberOfRows);
        var nullBitmap = new BitArray(numberOfRows);
        var converter = TypeDescriptor.GetConverter(type);

        int index = 0;
        foreach (var value in rawValues)
        {
            if (string.IsNullOrEmpty(value))
            {
                nullBitmap[index] = true;
                index++;
                continue;
            }
            
            var converted = converter.ConvertFromString(value);         // if not able to parse, throws NotSupportedException
            data.SetValue(converted, index);
            index++;
        }
            
        return (data, nullBitmap);
    }
}
