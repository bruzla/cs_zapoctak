using System.Collections;

namespace DplyrSharp.Core;

public partial class DataFrame
{
    /// <summary>
    /// Returns a new <see cref="DataFrame"/> with a new or updated column based on a selector function.
    /// </summary>
    /// <typeparam name="T">The type of values produced by the selector.</typeparam>
    /// <param name="name">The name of the new or replaced column.</param>
    /// <param name="selector">A function that computes a value for each row.</param>
    /// <returns>A new <see cref="DataFrame"/> with the added or updated column.</returns>
    /// <exception cref="ArgumentException">Thrown when the column name is null, empty, or whitespace.</exception>
    public DataFrame Mutate<T>(string name, Func<DataRow, T> selector)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Column name cannot be null or whitespace.", nameof(name));

        var data = new T[RowCount];
        var nulls = new BitArray(RowCount);

        for (int i = 0; i < RowCount; i++)
        {
            try { data[i] = selector(new DataRow(this, i))!; } catch { nulls[i] = true; }
        }

        var newColumns = new List<IDataColumn>(Columns);
        var newCol = new DataColumn<T>(name, data, nulls);

        int index = newColumns.FindIndex(c => c.Name == name);
        if (index >= 0) newColumns[index] = newCol; else newColumns.Add(newCol);

        return new DataFrame(newColumns);
    }

    /// <summary>
    /// Replaces an existing column or adds a new one with the specified name and data.
    /// </summary>
    /// <typeparam name="T">The type of values in the column.</typeparam>
    /// <param name="columns">The list of columns to modify.</param>
    /// <param name="name">The column name to replace or add.</param>
    /// <param name="data">The data array.</param>
    /// <param name="nulls">The bitmap indicating null values.</param>
    private void ReplaceOrAddColumn<T>(List<IDataColumn> columns, string name, T[] data, BitArray nulls)
    {
        var col = new DataColumn<T>(name, data, nulls);
        int index = columns.FindIndex(c => c.Name == name);
        if (index >= 0) columns[index] = col;
        else columns.Add(col);
    }

    // ------------------------------------------------------------------------------------------ overloads for multiple new columns

    public DataFrame Mutate<T1, T2>(
        (string name, Func<DataRow, T1> selector) col1,
        (string name, Func<DataRow, T2> selector) col2)
    {
        if (string.IsNullOrWhiteSpace(col1.name)) throw new ArgumentException("Column name cannot be null or whitespace.", nameof(col1.name));
        if (string.IsNullOrWhiteSpace(col2.name)) throw new ArgumentException("Column name cannot be null or whitespace.", nameof(col2.name));

        var newColumns = new List<IDataColumn>(Columns);
        var data1 = new T1[RowCount];
        var nulls1 = new BitArray(RowCount);
        var data2 = new T2[RowCount];
        var nulls2 = new BitArray(RowCount);

        for (int i = 0; i < RowCount; i++)
        {
            var row = new DataRow(this, i);
            try { data1[i] = col1.selector(row)!; } catch { nulls1[i] = true; }
            try { data2[i] = col2.selector(row)!; } catch { nulls2[i] = true; }
        }

        ReplaceOrAddColumn(newColumns, col1.name, data1, nulls1);
        ReplaceOrAddColumn(newColumns, col2.name, data2, nulls2);

        return new DataFrame(newColumns);
    }

    public DataFrame Mutate<T1, T2, T3>(
        (string name, Func<DataRow, T1> selector) col1,
        (string name, Func<DataRow, T2> selector) col2,
        (string name, Func<DataRow, T3> selector) col3)
    {
        if (string.IsNullOrWhiteSpace(col1.name)) throw new ArgumentException("Column name cannot be null or whitespace.", nameof(col1.name));
        if (string.IsNullOrWhiteSpace(col2.name)) throw new ArgumentException("Column name cannot be null or whitespace.", nameof(col2.name));
        if (string.IsNullOrWhiteSpace(col3.name)) throw new ArgumentException("Column name cannot be null or whitespace.", nameof(col3.name));

        var newColumns = new List<IDataColumn>(Columns);
        var data1 = new T1[RowCount]; var nulls1 = new BitArray(RowCount);
        var data2 = new T2[RowCount]; var nulls2 = new BitArray(RowCount);
        var data3 = new T3[RowCount]; var nulls3 = new BitArray(RowCount);

        for (int i = 0; i < RowCount; i++)
        {
            var row = new DataRow(this, i);
            try { data1[i] = col1.selector(row)!; } catch { nulls1[i] = true; }
            try { data2[i] = col2.selector(row)!; } catch { nulls2[i] = true; }
            try { data3[i] = col3.selector(row)!; } catch { nulls3[i] = true; }
        }

        ReplaceOrAddColumn(newColumns, col1.name, data1, nulls1);
        ReplaceOrAddColumn(newColumns, col2.name, data2, nulls2);
        ReplaceOrAddColumn(newColumns, col3.name, data3, nulls3);

        return new DataFrame(newColumns);
    }

    public DataFrame Mutate<T1, T2, T3, T4>(
        (string name, Func<DataRow, T1> selector) col1,
        (string name, Func<DataRow, T2> selector) col2,
        (string name, Func<DataRow, T3> selector) col3,
        (string name, Func<DataRow, T4> selector) col4)
    {
        if (string.IsNullOrWhiteSpace(col1.name)) throw new ArgumentException("Column name cannot be null or whitespace.", nameof(col1.name));
        if (string.IsNullOrWhiteSpace(col2.name)) throw new ArgumentException("Column name cannot be null or whitespace.", nameof(col2.name));
        if (string.IsNullOrWhiteSpace(col3.name)) throw new ArgumentException("Column name cannot be null or whitespace.", nameof(col3.name));
        if (string.IsNullOrWhiteSpace(col4.name)) throw new ArgumentException("Column name cannot be null or whitespace.", nameof(col4.name));

        var newColumns = new List<IDataColumn>(Columns);
        var data1 = new T1[RowCount]; var nulls1 = new BitArray(RowCount);
        var data2 = new T2[RowCount]; var nulls2 = new BitArray(RowCount);
        var data3 = new T3[RowCount]; var nulls3 = new BitArray(RowCount);
        var data4 = new T4[RowCount]; var nulls4 = new BitArray(RowCount);

        for (int i = 0; i < RowCount; i++)
        {
            var row = new DataRow(this, i);
            try { data1[i] = col1.selector(row)!; } catch { nulls1[i] = true; }
            try { data2[i] = col2.selector(row)!; } catch { nulls2[i] = true; }
            try { data3[i] = col3.selector(row)!; } catch { nulls3[i] = true; }
            try { data4[i] = col4.selector(row)!; } catch { nulls4[i] = true; }
        }

        ReplaceOrAddColumn(newColumns, col1.name, data1, nulls1);
        ReplaceOrAddColumn(newColumns, col2.name, data2, nulls2);
        ReplaceOrAddColumn(newColumns, col3.name, data3, nulls3);
        ReplaceOrAddColumn(newColumns, col4.name, data4, nulls4);

        return new DataFrame(newColumns);
    }

    public DataFrame Mutate<T1, T2, T3, T4, T5>(
        (string name, Func<DataRow, T1> selector) col1,
        (string name, Func<DataRow, T2> selector) col2,
        (string name, Func<DataRow, T3> selector) col3,
        (string name, Func<DataRow, T4> selector) col4,
        (string name, Func<DataRow, T5> selector) col5)
    {
        if (string.IsNullOrWhiteSpace(col1.name)) throw new ArgumentException("Column name cannot be null or whitespace.", nameof(col1.name));
        if (string.IsNullOrWhiteSpace(col2.name)) throw new ArgumentException("Column name cannot be null or whitespace.", nameof(col2.name));
        if (string.IsNullOrWhiteSpace(col3.name)) throw new ArgumentException("Column name cannot be null or whitespace.", nameof(col3.name));
        if (string.IsNullOrWhiteSpace(col4.name)) throw new ArgumentException("Column name cannot be null or whitespace.", nameof(col4.name));
        if (string.IsNullOrWhiteSpace(col5.name)) throw new ArgumentException("Column name cannot be null or whitespace.", nameof(col5.name));

        var newColumns = new List<IDataColumn>(Columns);
        var data1 = new T1[RowCount]; var nulls1 = new BitArray(RowCount);
        var data2 = new T2[RowCount]; var nulls2 = new BitArray(RowCount);
        var data3 = new T3[RowCount]; var nulls3 = new BitArray(RowCount);
        var data4 = new T4[RowCount]; var nulls4 = new BitArray(RowCount);
        var data5 = new T5[RowCount]; var nulls5 = new BitArray(RowCount);

        for (int i = 0; i < RowCount; i++)
        {
            var row = new DataRow(this, i);
            try { data1[i] = col1.selector(row)!; } catch { nulls1[i] = true; }
            try { data2[i] = col2.selector(row)!; } catch { nulls2[i] = true; }
            try { data3[i] = col3.selector(row)!; } catch { nulls3[i] = true; }
            try { data4[i] = col4.selector(row)!; } catch { nulls4[i] = true; }
            try { data5[i] = col5.selector(row)!; } catch { nulls5[i] = true; }
        }

        ReplaceOrAddColumn(newColumns, col1.name, data1, nulls1);
        ReplaceOrAddColumn(newColumns, col2.name, data2, nulls2);
        ReplaceOrAddColumn(newColumns, col3.name, data3, nulls3);
        ReplaceOrAddColumn(newColumns, col4.name, data4, nulls4);
        ReplaceOrAddColumn(newColumns, col5.name, data5, nulls5);

        return new DataFrame(newColumns);
    }
}
