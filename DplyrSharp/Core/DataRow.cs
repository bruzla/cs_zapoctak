using DplyrSharp.IO;

namespace DplyrSharp.Core;

/// <summary>
/// Represents a single row of data within a <see cref="DataFrame"/>.
/// Provides indexed and typed access to column values.
/// </summary>
public readonly struct DataRow
{
    /// <summary>
    /// The parent <see cref="DataFrame"/> that this row belongs to.
    /// </summary>
    private readonly DataFrame _df;

    /// <summary>
    /// Gets the zero-based index of the row in the parent <see cref="DataFrame"/>.
    /// </summary>
    public readonly int RowIndex { get; init; }


    /// <summary>
    /// Initializes a new instance of the <see cref="DataRow"/> struct.
    /// </summary>
    /// <param name="df">The parent <see cref="DataFrame"/>.</param>
    /// <param name="rowIndex">The index of the row within the data frame.</param>
    internal DataRow(DataFrame df, int rowIndex)
    {
        _df = df;
        RowIndex = rowIndex;
    }

    /// <summary>
    /// Gets the value of the specified column by name.
    /// </summary>
    /// <param name="columnName">The name of the column.</param>
    /// <returns>The value of the column at this row, or <c>null</c> if missing.</returns>
    public object? this[string columnName] =>
        _df.Columns[_df.GetColumnIndex(columnName)].GetValue(RowIndex);

    /// <summary>
    /// Gets the value of the specified column by index.
    /// </summary>
    /// <param name="columnIndex">The index of the column.</param>
    /// <returns>The value of the column at this row, or <c>null</c> if missing.</returns>
    public object? this[int columnIndex] => _df.Columns[columnIndex].GetValue(RowIndex);

    /// <summary>
    /// Retrieves the value of the specified column by name, strongly typed.
    /// </summary>
    /// <typeparam name="T">The expected type of the column value.</typeparam>
    /// <param name="columnName">The name of the column.</param>
    /// <returns>The value of type <typeparamref name="T"/>.</returns>
    /// <exception cref="InvalidCastException">Thrown if the column is not of type <typeparamref name="T"/>.</exception>
    public T Get<T>(string columnName)
    {
        int index = _df.GetColumnIndex(columnName);
        if (_df.Columns[index] is DataColumn<T> col)
            return col[RowIndex];
        throw new InvalidCastException($"Column '{columnName}' is not of type {typeof(T)}");
    }

    /// <summary>
    /// Retrieves the value of the specified column by index, strongly typed.
    /// </summary>
    /// <typeparam name="T">The expected type of the column value.</typeparam>
    /// <param name="columnIndex">The zero-based column index.</param>
    /// <returns>The value of type <typeparamref name="T"/>.</returns>
    /// <exception cref="InvalidCastException">Thrown if the column is not of type <typeparamref name="T"/>.</exception>
    public T Get<T>(int columnIndex)
    {
        if (_df.Columns[columnIndex] is DataColumn<T> col)
            return col[RowIndex];
        throw new InvalidCastException($"Column at index {columnIndex} is not of type {typeof(T)}");
    }

    /// <summary>
    /// Returns a comma-separated string representation of the row values.
    /// Null values are represented as empty strings.
    /// </summary>
    /// <returns>A string representation of the row.</returns>
    public override string ToString()
    {
        var rowValuesAsString = new List<string>(_df.ColCount);

        for (int i = 0; i < _df.ColCount; ++i)
        {
            object? value = _df.Columns[i].GetValue(RowIndex);

            if (value != null)
                rowValuesAsString.Add(value.ToString() ?? "");
            else
                rowValuesAsString.Add("");
        }

        return string.Join(CsvOptions.Default.Delimiter, rowValuesAsString);
    }
}
