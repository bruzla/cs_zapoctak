using System.Collections;

namespace DplyrSharp.Core;

public partial class DataFrame
{
    public DataFrame Arrange<TKey>(Func<DataRow, TKey> keySelector, bool descending = false)
        where TKey : notnull
    {
        var keys = new List<(int index, TKey key)>(RowCount);
        for (int i = 0; i < RowCount; i++)
            keys.Add((i, keySelector(new DataRow(this, i))));
        var ordered = descending ? keys.OrderByDescending(k => k.key) : keys.OrderBy(k => k.key);
        var order = ordered.Select(k => k.index).ToList();
        return SliceRows(order);
    }

    // ------------------------------------------------------------------------------------------ overloads for multiple arrange keys

    public DataFrame Arrange<TKey1, TKey2>(
        Func<DataRow, TKey1> keySelector1,
        Func<DataRow, TKey2> keySelector2,
        bool descending = false
    )
        where TKey1 : IComparable
        where TKey2 : IComparable
    {
        return Arrange(row => (keySelector1(row), keySelector2(row)), descending);
    }

    public DataFrame Arrange<TKey1, TKey2, TKey3>(
        Func<DataRow, TKey1> keySelector1,
        Func<DataRow, TKey2> keySelector2,
        Func<DataRow, TKey3> keySelector3,
        bool descending = false
    )
        where TKey1 : IComparable
        where TKey2 : IComparable
        where TKey3 : IComparable
    {
        return Arrange(
            row => (keySelector1(row), keySelector2(row), keySelector3(row)),
            descending
        );
    }

    public DataFrame Arrange<TKey1, TKey2, TKey3, TKey4>(
        Func<DataRow, TKey1> keySelector1,
        Func<DataRow, TKey2> keySelector2,
        Func<DataRow, TKey3> keySelector3,
        Func<DataRow, TKey4> keySelector4,
        bool descending = false
    )
        where TKey1 : IComparable
        where TKey2 : IComparable
        where TKey3 : IComparable
        where TKey4 : IComparable
    {
        return Arrange(
            row => (keySelector1(row), keySelector2(row), keySelector3(row), keySelector4(row)),
            descending
        );
    }

    public DataFrame Arrange<TKey1, TKey2, TKey3, TKey4, TKey5>(
        Func<DataRow, TKey1> keySelector1,
        Func<DataRow, TKey2> keySelector2,
        Func<DataRow, TKey3> keySelector3,
        Func<DataRow, TKey4> keySelector4,
        Func<DataRow, TKey5> keySelector5,
        bool descending = false
    )
        where TKey1 : IComparable
        where TKey2 : IComparable
        where TKey3 : IComparable
        where TKey4 : IComparable
        where TKey5 : IComparable
    {
        return Arrange(
            row =>
                (
                    keySelector1(row),
                    keySelector2(row),
                    keySelector3(row),
                    keySelector4(row),
                    keySelector5(row)
                ),
            descending
        );
    }
}
