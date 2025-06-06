using System.Collections;
using System.Reflection;

namespace DplyrSharp.Core;

public partial class GroupedDataFrame
{
    /// <summary>
    /// Applies an aggregation function to each group and returns a new <see cref="DataFrame"/> with one summarised column and all group keys.
    /// </summary>
    /// <typeparam name="T">The return type of the aggregation function.</typeparam>
    /// <param name="name">The name of the summarised column.</param>
    /// <param name="aggregator">A function that computes an aggregated value for each group.</param>
    /// <returns>A new <see cref="DataFrame"/> containing the group keys and the summarised column.</returns>
    /// <exception cref="ArgumentException">Thrown when the column name is null, empty, or whitespace.</exception>
    public DataFrame Summarise<T>(string name, Func<IEnumerable<DataRow>, T> aggregator)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Column name cannot be null or whitespace.", nameof(name));

        var resultCols = new List<IDataColumn>();
        AddKeyColumns(resultCols);

        var resultCol = BuildAggregationColumn(name, aggregator);
        ReplaceOrAddColumn(resultCols, resultCol, name);

        return new DataFrame(resultCols);
    }

    /// <summary>
    /// Adds group key columns to the summarised result using their original types.
    /// </summary>
    /// <param name="resultCols">The list of result columns to populate with group key data.</param>
    private void AddKeyColumns(List<IDataColumn> resultCols)
    {
        int n = GroupedRows.Count;

        for (int colIndex = 0; colIndex < GroupColumns.Length; ++colIndex)
        {
            // find the original column type
            var colName = GroupColumns[colIndex];
            var origCol = DataFrame.Columns[DataFrame.GetColumnIndex(colName)];
            var dataType = origCol.ClrType;
            var colType = typeof(DataColumn<>).MakeGenericType(dataType);

            // create typed array for keys
            var keyArray = Array.CreateInstance(dataType, n);
            var nulls = new BitArray(n);

            for (int i = 0; i < n; i++)
            {
                var val = GroupedRows[i].Key[colIndex];
                if (val == null) nulls[i] = true;
                keyArray.SetValue(val, i);
            }

            var dcKey = (IDataColumn)Activator.CreateInstance(colType, colName, keyArray, nulls)!;

            resultCols.Add(dcKey);
        }
    }

    /// <summary>
    /// Builds a typed column from aggregation results across all groups.
    /// </summary>
    /// <typeparam name="T">The type of the values produced by the aggregator.</typeparam>
    /// <param name="name">The name of the new column.</param>
    /// <param name="aggregator">The function to aggregate rows in a group.</param>
    /// <returns>An <see cref="IDataColumn"/> representing the summarised data.</returns>
    private IDataColumn BuildAggregationColumn<T>(string name, Func<IEnumerable<DataRow>, T> aggregator)
    {
        int n = GroupedRows.Count;
        var data = new T[n];
        var nulls = new BitArray(n);
        for (int i = 0; i < n; i++)
        {
            T? val;
            try { val = aggregator(GroupedRows[i].Items); }
            catch { val = default; }

            if (val == null || (val is ValueType && val.Equals(default(T))))
            {
                nulls[i] = true;
                // leave data[i] as default
            }
            else
            {
                data[i] = val!;
            }
        }

        return new DataColumn<T>(name, data, nulls);
    }

    /// <summary>
    /// Replaces an existing column in the result list or adds a new one if it doesn't exist.
    /// </summary>
    /// <param name="columns">The list of columns to update.</param>
    /// <param name="col">The new column to insert.</param>
    /// <param name="name">The name to match for replacement.</param>
    private void ReplaceOrAddColumn(List<IDataColumn> columns, IDataColumn col, string name)
    {
        int index = columns.FindIndex(c => c.Name == name);
        if (index >= 0) columns[index] = col;
        else columns.Add(col);
    }

    // ------------------------------------------------------------------------------------------ overloads for multiple aggregation columns

    public DataFrame Summarise<T1, T2>(
        (string name, Func<IEnumerable<DataRow>, T1> aggregator) col1,
        (string name, Func<IEnumerable<DataRow>, T2> aggregator) col2
        )
    {
        if (string.IsNullOrWhiteSpace(col1.name)) throw new ArgumentException("Column name cannot be null or whitespace.", nameof(col1.name));
        if (string.IsNullOrWhiteSpace(col2.name)) throw new ArgumentException("Column name cannot be null or whitespace.", nameof(col2.name));

        var resultCols = new List<IDataColumn>();
        AddKeyColumns(resultCols);

        ReplaceOrAddColumn(resultCols, BuildAggregationColumn(col1.name, col1.aggregator), col1.name);
        ReplaceOrAddColumn(resultCols, BuildAggregationColumn(col2.name, col2.aggregator), col2.name);

        return new DataFrame(resultCols);
    }

    public DataFrame Summarise<T1, T2, T3>(
    (string name, Func<IEnumerable<DataRow>, T1> aggregator) col1,
    (string name, Func<IEnumerable<DataRow>, T2> aggregator) col2,
    (string name, Func<IEnumerable<DataRow>, T3> aggregator) col3)
    {
        if (string.IsNullOrWhiteSpace(col1.name)) throw new ArgumentException("Column name cannot be null or whitespace.", nameof(col1.name));
        if (string.IsNullOrWhiteSpace(col2.name)) throw new ArgumentException("Column name cannot be null or whitespace.", nameof(col2.name));
        if (string.IsNullOrWhiteSpace(col3.name)) throw new ArgumentException("Column name cannot be null or whitespace.", nameof(col3.name));

        var resultCols = new List<IDataColumn>();
        AddKeyColumns(resultCols);

        ReplaceOrAddColumn(resultCols, BuildAggregationColumn(col1.name, col1.aggregator), col1.name);
        ReplaceOrAddColumn(resultCols, BuildAggregationColumn(col2.name, col2.aggregator), col2.name);
        ReplaceOrAddColumn(resultCols, BuildAggregationColumn(col3.name, col3.aggregator), col3.name);

        return new DataFrame(resultCols);
    }

    public DataFrame Summarise<T1, T2, T3, T4>(
        (string name, Func<IEnumerable<DataRow>, T1> aggregator) col1,
        (string name, Func<IEnumerable<DataRow>, T2> aggregator) col2,
        (string name, Func<IEnumerable<DataRow>, T3> aggregator) col3,
        (string name, Func<IEnumerable<DataRow>, T4> aggregator) col4)
    {
        if (string.IsNullOrWhiteSpace(col1.name)) throw new ArgumentException("Column name cannot be null or whitespace.", nameof(col1.name));
        if (string.IsNullOrWhiteSpace(col2.name)) throw new ArgumentException("Column name cannot be null or whitespace.", nameof(col2.name));
        if (string.IsNullOrWhiteSpace(col3.name)) throw new ArgumentException("Column name cannot be null or whitespace.", nameof(col3.name));
        if (string.IsNullOrWhiteSpace(col4.name)) throw new ArgumentException("Column name cannot be null or whitespace.", nameof(col4.name));

        var resultCols = new List<IDataColumn>();
        AddKeyColumns(resultCols);

        ReplaceOrAddColumn(resultCols, BuildAggregationColumn(col1.name, col1.aggregator), col1.name);
        ReplaceOrAddColumn(resultCols, BuildAggregationColumn(col2.name, col2.aggregator), col2.name);
        ReplaceOrAddColumn(resultCols, BuildAggregationColumn(col3.name, col3.aggregator), col3.name);
        ReplaceOrAddColumn(resultCols, BuildAggregationColumn(col4.name, col4.aggregator), col4.name);

        return new DataFrame(resultCols);
    }

    public DataFrame Summarise<T1, T2, T3, T4, T5>(
        (string name, Func<IEnumerable<DataRow>, T1> aggregator) col1,
        (string name, Func<IEnumerable<DataRow>, T2> aggregator) col2,
        (string name, Func<IEnumerable<DataRow>, T3> aggregator) col3,
        (string name, Func<IEnumerable<DataRow>, T4> aggregator) col4,
        (string name, Func<IEnumerable<DataRow>, T5> aggregator) col5)
    {
        if (string.IsNullOrWhiteSpace(col1.name)) throw new ArgumentException("Column name cannot be null or whitespace.", nameof(col1.name));
        if (string.IsNullOrWhiteSpace(col2.name)) throw new ArgumentException("Column name cannot be null or whitespace.", nameof(col2.name));
        if (string.IsNullOrWhiteSpace(col3.name)) throw new ArgumentException("Column name cannot be null or whitespace.", nameof(col3.name));
        if (string.IsNullOrWhiteSpace(col4.name)) throw new ArgumentException("Column name cannot be null or whitespace.", nameof(col4.name));
        if (string.IsNullOrWhiteSpace(col5.name)) throw new ArgumentException("Column name cannot be null or whitespace.", nameof(col5.name));

        var resultCols = new List<IDataColumn>();
        AddKeyColumns(resultCols);

        ReplaceOrAddColumn(resultCols, BuildAggregationColumn(col1.name, col1.aggregator), col1.name);
        ReplaceOrAddColumn(resultCols, BuildAggregationColumn(col2.name, col2.aggregator), col2.name);
        ReplaceOrAddColumn(resultCols, BuildAggregationColumn(col3.name, col3.aggregator), col3.name);
        ReplaceOrAddColumn(resultCols, BuildAggregationColumn(col4.name, col4.aggregator), col4.name);
        ReplaceOrAddColumn(resultCols, BuildAggregationColumn(col5.name, col5.aggregator), col5.name);

        return new DataFrame(resultCols);
    }

}