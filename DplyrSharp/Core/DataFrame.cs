using System.Collections;
using System.Text;

namespace DplyrSharp.Core;

public partial class DataFrame
{
    private readonly List<IDataColumn> _columns;
    private readonly Dictionary<string, int> _nameToIndex;

    public int RowCount { get; init; }
    public int ColCount { get; init; }
    public Schema Schema { get; }

    public DataFrame(IEnumerable<IDataColumn> columns)
    {
        _columns = columns.ToList(); // tady deep copy!!!!!!

        _nameToIndex = _columns
            .Select((c, i) => (c.Name, i))
            .ToDictionary(x => x.Name, x => x.i, StringComparer.Ordinal);

        Schema = new Schema(_columns);
        ColCount = _columns.Count;
        RowCount = ColCount == 0 ? 0 : _columns[0].RowCount;
    }

    public IReadOnlyList<IDataColumn> Columns => _columns;

    public IEnumerable<DataRow> Rows
    {
        get
        {
            for (int i = 0; i < RowCount; i++)
                yield return new DataRow(this, i);
        }
    }

    internal int GetColumnIndex(string name)
    {
        if (!_nameToIndex.TryGetValue(name, out var index))
            throw new ArgumentException($"Column '{name}' not found.");
        return index;
    }

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

    public void WriteCsv(string path, IO.CsvOptions? options = null)
    {
        IO.CsvWriter.WriteCsv(this, path, options);
    }

    public override string ToString()
    {
        var sb = new StringBuilder();

        // names
        var colNames = _columns.Select(x => x.Name);
        sb.AppendLine(string.Join(IO.CsvOptions.Default.Delimiter, colNames));

        // values
        foreach (var row in Rows)
        {
            sb.AppendLine(row.ToString());
        }

        return sb.ToString();
    }

    public void Print()
    {
        Console.WriteLine(this.ToString());
    }
}
