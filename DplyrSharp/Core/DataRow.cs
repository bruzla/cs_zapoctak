using DplyrSharp.IO;

namespace DplyrSharp.Core;

public readonly struct DataRow
{
    private readonly DataFrame _df;
    public readonly int RowIndex { get; init; }

    internal DataRow(DataFrame df, int rowIndex)
    {
        _df = df;
        RowIndex = rowIndex;
    }

    public object? this[string columnName] =>
        _df.Columns[_df.GetColumnIndex(columnName)].GetValue(RowIndex);

    public object? this[int columnIndex] => _df.Columns[columnIndex].GetValue(RowIndex);

    public T Get<T>(string columnName)
    {
        int index = _df.GetColumnIndex(columnName);
        if (_df.Columns[index] is DataColumn<T> col)
            return col[RowIndex];
        throw new InvalidCastException($"Column '{columnName}' is not of type {typeof(T)}");
    }

    public T Get<T>(int columnIndex)
    {
        if (_df.Columns[columnIndex] is DataColumn<T> col)
            return col[RowIndex];
        throw new InvalidCastException($"Column at index {columnIndex} is not of type {typeof(T)}");
    }

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
