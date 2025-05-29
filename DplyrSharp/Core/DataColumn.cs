using System.Collections;

namespace DplyrSharp.Core;

/// <summary>
/// Represents a column in a <see cref="DataFrame"/> with a name, type, and value access.
/// </summary>
public interface IDataColumn
{
    /// <summary>
    /// Gets the name of the column.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the CLR type of the values stored in the column.
    /// </summary>
    Type ClrType { get; }

    /// <summary>
    /// Gets the number of rows in the column.
    /// </summary>
    int RowCount { get; }

    /// <summary>
    /// Gets the bitmap representing which rows have null values.
    /// </summary>
    BitArray NullBitmap { get; }


    /// <summary>
    /// Gets the value at the specified row index.
    /// Returns <c>null</c> if the value is marked as null.
    /// </summary>
    /// <param name="rowIndex">The zero-based row index.</param>
    /// <returns>The value at the specified row index.</returns>
    object? GetValue(int rowIndex);
}

/// <summary>
/// Represents a strongly typed column in a <see cref="DataFrame"/>.
/// </summary>
/// <typeparam name="T">The type of values stored in the column.</typeparam>
public sealed class DataColumn<T> : IDataColumn, IReadOnlyList<T>
{
    private readonly T[] _data;
    private readonly BitArray _nullBitmap;

    /// <summary>
    /// Gets the name of the column.
    /// </summary>
    public string Name { get; init; }

    /// <summary>
    /// Gets the CLR type of the column, which is <typeparamref name="T"/>.
    /// </summary>
    public Type ClrType => typeof(T);

    /// <summary>
    /// Gets the number of rows in the column.
    /// </summary>
    public int RowCount => _data.Length;

    /// <summary>
    /// Gets the bitmap representing null values in the column.
    /// </summary>
    public BitArray NullBitmap => _nullBitmap;

    /// <summary>
    /// Initializes a new instance of the <see cref="DataColumn{T}"/> class.
    /// </summary>
    /// <param name="name">The name of the column.</param>
    /// <param name="data">The array of values.</param>
    /// <param name="nullBitmap">The bitmap indicating null entries.</param>
    public DataColumn(string name, T[] data, BitArray nullBitmap)
    {
        Name = name;
        _data = data;
        _nullBitmap = nullBitmap;
    }

    /// <summary>
    /// Gets the value at the specified index.
    /// </summary>
    /// <param name="index">The index of the value.</param>
    public T this[int index] => _data[index];


    /// <inheritdoc/>
    public IEnumerator<T> GetEnumerator() => ((IEnumerable<T>)_data).GetEnumerator();

    /// <inheritdoc/>
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() =>
        _data.GetEnumerator();


    /// <inheritdoc/>
    int IReadOnlyCollection<T>.Count => _data.Length;

    /// <inheritdoc/>
    public object? GetValue(int rowIndex)
    {
        if (_nullBitmap[rowIndex])
            return null;
        return _data[rowIndex]!;
    }
}

/// <summary>
/// Contains metadata about a column in a <see cref="Schema"/>.
/// </summary>
public readonly struct ColumnInfo
{
    /// <summary>
    /// Gets the name of the column.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the CLR type of the column.
    /// </summary>
    public Type ClrType { get; }


    /// <summary>
    /// Initializes a new instance of the <see cref="ColumnInfo"/> struct.
    /// </summary>
    /// <param name="name">The name of the column.</param>
    /// <param name="clrType">The CLR type of the column.</param>
    public ColumnInfo(string name, Type clrType)
    {
        Name = name;
        ClrType = clrType;
    }
}

/// <summary>
/// Represents the schema of a <see cref="DataFrame"/>, including column names and types.
/// </summary>
public sealed class Schema
{
    /// <summary>
    /// Gets the list of column metadata entries.
    /// </summary>
    public IReadOnlyList<ColumnInfo> Columns { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Schema"/> class from a list of data columns.
    /// </summary>
    /// <param name="columns">The data columns used to build the schema.</param>
    public Schema(IEnumerable<IDataColumn> columns)
    {
        Columns = columns.Select(c => new ColumnInfo(c.Name, c.ClrType)).ToList();
    }
}
