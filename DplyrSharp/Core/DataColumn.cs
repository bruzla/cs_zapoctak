using System.Collections;

namespace DplyrSharp.Core;

public interface IDataColumn
{
    string Name { get; }
    Type ClrType { get; }
    int RowCount { get; }

    BitArray NullBitmap { get; }

    object? GetValue(int rowIndex);
}

public sealed class DataColumn<T> : IDataColumn, IReadOnlyList<T>
{
    private readonly T[] _data;
    private readonly BitArray _nullBitmap;
    public string Name { get; init; }
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

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() =>
        _data.GetEnumerator();

    int IReadOnlyCollection<T>.Count => _data.Length;

    public object? GetValue(int rowIndex)
    {
        if (_nullBitmap[rowIndex])
            return null;
        return _data[rowIndex]!;
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

public sealed class Schema
{
    public IReadOnlyList<ColumnInfo> Columns { get; }

    public Schema(IEnumerable<IDataColumn> columns)
    {
        Columns = columns.Select(c => new ColumnInfo(c.Name, c.ClrType)).ToList();
    }
}
