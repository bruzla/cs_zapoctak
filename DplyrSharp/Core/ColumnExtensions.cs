using System;
using System.Collections;

namespace DplyrSharp.Core;

public static class ColumnExtensions
{
    public static bool SequenceEqual(this IDataColumn a, IDataColumn b)
    {
        // Reference or identity check
        if (ReferenceEquals(a, b))
            return true;

        // Type and row-count must match
        if (a.ClrType != b.ClrType || a.RowCount != b.RowCount)
            return false;

        // Compare each cell
        for (int i = 0; i < a.RowCount; i++)
        {
            var va = a.GetValue(i);
            var vb = b.GetValue(i);

            // Both null?
            if (va == null && vb == null)
                continue;

            // One null, one non-null
            if (va == null || vb == null)
                return false;

            // Value comparison
            if (!va.Equals(vb))
                return false;
        }

        return true;
    }

    public static bool SequenceEqual<T>(this DataColumn<T> a, DataColumn<T> b, IEqualityComparer<T>? comparer = null)
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
            // Normalize to non-null bit arrays
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
