using System.Collections;

namespace DplyrSharp.Core;

/// <summary>
/// Provides extension methods for comparing data columns within a <see cref="DataFrame"/>.
/// </summary>
public static class ColumnExtensions
{
    /// <summary>
    /// Determines whether two <see cref="IDataColumn"/> instances contain the same sequence of values.
    /// </summary>
    /// <param name="a">The first column to compare.</param>
    /// <param name="b">The second column to compare.</param>
    /// <returns><c>true</c> if the columns have the same type, length, and cell values; otherwise, <c>false</c>.</returns>
    public static bool SequenceEqual(this IDataColumn a, IDataColumn b)
    {
        // reference or identity check
        if (ReferenceEquals(a, b))
            return true;

        // type and row-count must match
        if (a.ClrType != b.ClrType || a.RowCount != b.RowCount)
            return false;

        // compare each cell
        for (int i = 0; i < a.RowCount; i++)
        {
            var va = a.GetValue(i);
            var vb = b.GetValue(i);

            // both null?
            if (va == null && vb == null)
                continue;

            // one null, one non-null
            if (va == null || vb == null)
                return false;

            // value comparison
            if (!va.Equals(vb))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Determines whether two <see cref="DataColumn{T}"/> instances contain the same sequence of values, with optional custom equality comparison.
    /// </summary>
    /// <typeparam name="T">The type of values stored in the columns.</typeparam>
    /// <param name="a">The first typed column to compare.</param>
    /// <param name="b">The second typed column to compare.</param>
    /// <param name="comparer">An optional equality comparer for values of type <typeparamref name="T"/>.</param>
    /// <returns><c>true</c> if the columns have the same values and null bitmap; otherwise, <c>false</c>.</returns>
    public static bool SequenceEqual<T>(
        this DataColumn<T> a,
        DataColumn<T> b,
        IEqualityComparer<T>? comparer = null
    )
    {
        if (ReferenceEquals(a, b))
            return true;

        if (a.RowCount != b.RowCount)
            return false;

        comparer ??= EqualityComparer<T>.Default;

        // compare null bitmaps first
        var na = a.NullBitmap;
        var nb = b.NullBitmap;
        if (na != null || nb != null)
        {
            // normalize to non-null bit arrays
            var ba = na ?? new BitArray(a.RowCount, false);
            var bb = nb ?? new BitArray(b.RowCount, false);
            for (int i = 0; i < a.RowCount; i++)
            {
                if (ba[i] != bb[i])
                    return false;
            }
        }

        // compare values
        for (int i = 0; i < a.RowCount; i++)
        {
            if (a.NullBitmap != null && a.NullBitmap[i])
                continue;

            if (!comparer.Equals(a[i], b[i]))
                return false;
        }

        return true;
    }
}
