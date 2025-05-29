using System.Collections;
using System.Text;

namespace DplyrSharp.Core;

/// <summary>
/// Represents a tabular data structure consisting of named columns of potentially different types.
/// </summary>
public partial class DataFrame
{
    /// <summary>
    /// The internal list of data columns in the data frame.
    /// </summary>
    private readonly List<IDataColumn> _columns;

    /// <summary>
    /// A dictionary mapping column names to their corresponding index positions.
    /// </summary>
    private readonly Dictionary<string, int> _nameToIndex;

    /// <summary>
    /// Gets the number of rows in the data frame.
    /// </summary>
    public int RowCount { get; init; }
    /// <summary>
    /// Gets the number of columns in the data frame.
    /// </summary>
    public int ColCount { get; init; }
    /// <summary>
    /// Gets the schema of the data frame, describing the structure of the columns.
    /// </summary>
    public Schema Schema { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DataFrame"/> class with the specified columns.
    /// </summary>
    /// <param name="columns">The sequence of columns to include in the data frame.</param>
    public DataFrame(IEnumerable<IDataColumn> columns)
    {
        _columns = columns.ToList();

        _nameToIndex = _columns
            .Select((c, i) => (c.Name, i))
            .ToDictionary(x => x.Name, x => x.i, StringComparer.Ordinal);

        Schema = new Schema(_columns);
        ColCount = _columns.Count;
        RowCount = ColCount == 0 ? 0 : _columns[0].RowCount;
    }

    /// <summary>
    /// Gets a read-only list of columns in the data frame.
    /// </summary>
    public IReadOnlyList<IDataColumn> Columns => _columns;

    /// <summary>
    /// Gets an enumerable collection of <see cref="DataRow"/> objects representing each row of the data frame.
    /// </summary>
    public IEnumerable<DataRow> Rows
    {
        get
        {
            for (int i = 0; i < RowCount; i++)
                yield return new DataRow(this, i);
        }
    }

    /// <summary>
    /// Retrieves the index of a column by its name.
    /// </summary>
    /// <param name="name">The name of the column.</param>
    /// <returns>The zero-based index of the column.</returns>
    /// <exception cref="ArgumentException">Thrown if the specified column name does not exist.</exception>
    internal int GetColumnIndex(string name)
    {
        if (!_nameToIndex.TryGetValue(name, out var index))
            throw new ArgumentException($"Column '{name}' not found.");
        return index;
    }

    /// <summary>
    /// Creates a new <see cref="DataFrame"/> that includes only the specified rows.
    /// </summary>
    /// <param name="rows">The list of row indices to include.</param>
    /// <returns>A new <see cref="DataFrame"/> containing only the selected rows.</returns>
    private DataFrame SliceRows(IList<int> rows)
    {
        var newCols = new List<IDataColumn>(Columns.Count);
        foreach (var col in Columns)
        {
            var type = col.ClrType;
            var data = Array.CreateInstance(type, rows.Count);
            var nulls = new BitArray(rows.Count);
            for (int i = 0; i < rows.Count; i++)
            {
                var v = col.GetValue(rows[i]);
                if (v == null)
                    nulls[i] = true;
                data.SetValue(v, i);
            }
            var ctor = typeof(DataColumn<>)
                .MakeGenericType(type)
                .GetConstructor(new[] { typeof(string), data.GetType(), typeof(BitArray) });
            var newCol = (IDataColumn)ctor!.Invoke(new object[] { col.Name, data, nulls });
            newCols.Add(newCol);
        }
        return new DataFrame(newCols);
    }


    /// <summary>
    /// Writes the contents of the data frame to a CSV file at the specified path.
    /// </summary>
    /// <param name="path">The file path to write the CSV to.</param>
    /// <param name="options">Optional CSV writing options.</param>
    public void WriteCsv(string path, IO.CsvOptions? options = null)
    {
        IO.CsvWriter.WriteCsv(this, path, options);
    }

    /// <summary>
    /// Returns a string representation of the data frame, displaying up to the specified number of rows.
    /// </summary>
    /// <param name="nrow">The number of rows to display.</param>
    /// <returns>A string representation of the data frame.</returns>
    public string ToString(int nrow)
    {
        var sb = new StringBuilder();

        // names
        var colNames = _columns.Select(x => x.Name);
        sb.AppendLine(string.Join(IO.CsvOptions.Default.Delimiter, colNames));

        // values
        int i = 0;
        foreach (var row in Rows)
        {
            if (++i > nrow)
                break;

            sb.AppendLine(row.ToString());
        }

        return sb.ToString();
    }

    /// <summary>
    /// Returns a string representation of the full data frame.
    /// </summary>
    /// <returns>A string representation of the full data frame.</returns>
    public override string ToString()
    {
        return ToString(RowCount);
    }


    /// <summary>
    /// Prints a string representation of the data frame to the console, showing up to the specified number of rows.
    /// </summary>
    /// <param name="nrow">The number of rows to display.</param>
    public void Print(int nrow)
    {
        Console.WriteLine(this.ToString(nrow));
    }

    /// <summary>
    /// Prints the entire data frame to the console.
    /// </summary>
    public void Print()
    {
        Console.WriteLine(this.ToString());
    }
}
