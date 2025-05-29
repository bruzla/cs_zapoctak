using System;

namespace DplyrSharp.Core;

public partial class DataFrame
{
    /// <summary>
    /// Performs a left outer join between this <see cref="DataFrame"/> and another, using a row-level predicate to determine matches.
    /// </summary>
    /// <param name="right">The right-hand <see cref="DataFrame"/> to join with.</param>
    /// <param name="predicate">A function that determines whether a pair of rows from the left and right data frames should be joined.</param>
    /// <returns>
    /// A new <see cref="DataFrame"/> containing all rows from the left data frame,
    /// and matching rows from the right. Unmatched right-side rows are excluded.
    /// </returns>
    public DataFrame LeftJoin(DataFrame right, Func<DataRow, DataRow, bool> predicate)
    {
        return JoinByInternal(this, right, predicate, includeLeftUnmatched: true, includeRightUnmatched: false);
    }
}