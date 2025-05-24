using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DplyrSharp.Core;

public sealed class DataFrame
{
    private readonly List<IDataColumn> _columns;
    private readonly Dictionary<string, int> _nameToIndex;

    public IReadOnlyList<IDataColumn> Columns => _columns;
    public Schema Schema { get; }

    public DataFrame(IEnumerable<IDataColumn> columns)
    {
        _columns = new List<IDataColumn>(columns);
        _nameToIndex = _columns
            .Select((c, i) => (c.Name, i))
            .ToDictionary(x => x.Name, x => x.i, StringComparer.Ordinal);
        Schema = new Schema(_columns);
    }
    public DataFrame Materialise() => this;

    public void WriteCsv(string path, IO.CsvOptions? options = null)
        => IO.CsvWriter.WriteCsv(this, path, options);

    public IEnumerable<DataRow> Rows
    {
        get
        {
            int rowCount = _columns.Count > 0 ? _columns[0].RowCount : 0;
            for (int i = 0; i < rowCount; i++)
                yield return new DataRow(this, i);
        }
    }

    internal int GetColumnIndex(string name)
    {
        if (!_nameToIndex.TryGetValue(name, out var idx))
            throw new ArgumentException($"Column '{name}' not found.");
        return idx;
    }
}

public interface IDataColumn
{
    string Name { get; }
    Type ClrType { get; }
    int RowCount { get; }

    BitArray? NullBitmap { get; }

    object? GetValue(int rowIndex);
}

public sealed class DataColumn<T> : IDataColumn, IReadOnlyList<T>
{
    private readonly T[] _data;
    private readonly BitArray _nullBitmap;

    public string Name { get; }
    public Type ClrType => typeof(T);
    public int RowCount => _data.Length;
    public BitArray NullBitmap => _nullBitmap;

    public DataColumn(string name, T[] data, BitArray nullBitmap)
    {
        Name = name;
        _data = data;
        _nullBitmap = nullBitmap;
    }

    public T this[int index] => _data[index];

    public IEnumerator<T> GetEnumerator() => ((IEnumerable<T>)_data).GetEnumerator();
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => _data.GetEnumerator();
    int IReadOnlyCollection<T>.Count => _data.Length;

    public object? GetValue(int rowIndex)
    {
        if (_nullBitmap[rowIndex])
            return null;
        return _data[rowIndex]!;
    }
}

public readonly struct DataRow
{
    private readonly DataFrame _df;
    private readonly int _rowIndex;

    internal DataRow(DataFrame df, int rowIndex)
    {
        _df = df;
        _rowIndex = rowIndex;
    }

    public object? this[string columnName] => _df.Columns[_df.GetColumnIndex(columnName)].GetValue(_rowIndex);

    public object? this[int columnIndex] => _df.Columns[columnIndex].GetValue(_rowIndex);
}

public sealed class Schema
{
    public IReadOnlyList<ColumnInfo> Columns { get; }

    public Schema(IEnumerable<IDataColumn> columns)
    {
        Columns = columns.Select(c => new ColumnInfo(c.Name, c.ClrType)).ToList();
    }
}

public readonly struct ColumnInfo
{
    public string Name { get; }
    public Type ClrType { get; }

    public ColumnInfo(string name, Type clrType)
    {
        Name = name;
        ClrType = clrType;
    }
}