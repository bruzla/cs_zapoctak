using System.Collections;
using System.ComponentModel;
using DplyrSharp.Core;

namespace DplyrSharp.IO;

/// <summary>
/// Provides functionality for reading CSV files into <see cref="DataFrame"/> objects.
/// </summary>
public static class CsvReader
{
    /// <summary>
    /// Reads a CSV file and converts it into a <see cref="DataFrame"/>.
    /// </summary>
    /// <param name="path">The path to the CSV file.</param>
    /// <param name="options">Optional CSV options such as delimiter, encoding, and header presence.</param>
    /// <returns>A <see cref="DataFrame"/> containing the parsed data.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the CSV is empty or rows are malformed.</exception>
    public static DataFrame ReadCsv(string path, CsvOptions? options = null)
    {
        options ??= new CsvOptions();
        using var reader = new StreamReader(path, options.Encoding);

        string[] headers = GetHeaders(reader, options);
        List<string[]> rows = GetAllRows(reader, options, headers.Length);
        List<IDataColumn> columns = GetColumns(headers, rows);

        return new DataFrame(columns);
    }

    /// <summary>
    /// Extracts and trims header names from the CSV file.
    /// If headers are not present, generates default column names.
    /// </summary>
    /// <param name="reader">The stream reader instance.</param>
    /// <param name="options">CSV reading options.</param>
    /// <returns>An array of header names.</returns>
    private static string[] GetHeaders(StreamReader reader, CsvOptions options)
    {
        string[] headers;

        if (options.HasHeader)
        {
            var headerLine =
                reader.ReadLine() ?? throw new InvalidOperationException("CSV is empty");
            headers = headerLine.Split(options.Delimiter);
            headers = TrimWhitespace(headers);
        }
        else
        {
            var firstLine =
                reader.ReadLine() ?? throw new InvalidOperationException("CSV is empty");
            var count = firstLine.Split(options.Delimiter).Length;
            headers = Enumerable.Range(0, count).Select(i => $"Column_{i}").ToArray();
            reader.BaseStream.Seek(0, SeekOrigin.Begin);
            reader.DiscardBufferedData();
        }

        return headers;
    }

    /// <summary>
    /// Trims whitespace from all strings in the given array.
    /// </summary>
    /// <param name="values">The string array to trim.</param>
    /// <returns>A new array with trimmed values.</returns>
    private static string[] TrimWhitespace(string[] values)
    {
        for (int i = 0; i < values.Length; ++i)
        {
            values[i] = values[i].Trim();
        }

        return values;
    }

    /// <summary>
    /// Reads all non-empty data rows from the CSV and splits them into arrays of column values.
    /// </summary>
    /// <param name="reader">The stream reader instance.</param>
    /// <param name="options">CSV reading options.</param>
    /// <param name="numberOfColumns">The expected number of columns per row.</param>
    /// <returns>A list of string arrays, one per row.</returns>
    /// <exception cref="InvalidOperationException">Thrown when a row has an unexpected number of columns.</exception>
    private static List<string[]> GetAllRows(
        StreamReader reader,
        CsvOptions options,
        int numberOfColumns
    )
    {
        var rows = new List<string[]>();
        while (!reader.EndOfStream)
        {
            var line = reader.ReadLine()!;
            if (String.IsNullOrEmpty(line))
                continue;

            var lineItems = line.Split(options.Delimiter);
            if (lineItems.Length != numberOfColumns)
                throw new InvalidOperationException(
                    $"Number of items in a row does not match the number of columns. Faulty row: \"{line}\""
                );

            rows.Add(lineItems);
        }

        return rows;
    }

    /// <summary>
    /// Constructs <see cref="IDataColumn"/> instances from raw CSV rows and headers.
    /// </summary>
    /// <param name="headers">The column headers.</param>
    /// <param name="rows">The raw row data.</param>
    /// <returns>A list of typed <see cref="IDataColumn"/> objects.</returns>
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

    /// <summary>
    /// Attempts to build a typed <see cref="IDataColumn"/> by inferring the data type of the column.
    /// Falls back to string column if type inference or conversion fails.
    /// </summary>
    /// <param name="rawValues">The raw column values as strings.</param>
    /// <param name="header">The column header name.</param>
    /// <param name="numberOfRows">The number of rows in the column.</param>
    /// <returns>A typed <see cref="IDataColumn"/> instance.</returns>
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

    /// <summary>
    /// Builds a string-based <see cref="DataColumn{String}"/> with a blank null bitmap.
    /// </summary>
    /// <param name="rawValues">The raw string values.</param>
    /// <param name="header">The column header.</param>
    /// <param name="numberOfRows">The number of rows.</param>
    /// <returns>A string-based <see cref="IDataColumn"/>.</returns>
    private static IDataColumn BuildStringColumn(IEnumerable<string> rawValues, string header, int numberOfRows)
    {
        var data = rawValues.ToArray();
        var nullBitmap = new BitArray(numberOfRows);

        return new DataColumn<string>(header, data, nullBitmap);
    }


    /// <summary>
    /// Casts string values to a specified type and constructs the corresponding data array and null bitmap.
    /// </summary>
    /// <param name="rawValues">The raw string values.</param>
    /// <param name="type">The target CLR type.</param>
    /// <param name="numberOfRows">The number of values to cast.</param>
    /// <returns>A tuple containing the typed array and null bitmap.</returns>
    /// <exception cref="NotSupportedException">Thrown when a value cannot be converted to the specified type.</exception>
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

            var converted = converter.ConvertFromString(value); // if not able to parse, throws NotSupportedException
            data.SetValue(converted, index);
            index++;
        }

        return (data, nullBitmap);
    }
}
