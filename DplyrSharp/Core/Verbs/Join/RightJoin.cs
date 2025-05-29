using System;

namespace DplyrSharp.Core;

public partial class DataFrame
{
    /// <summary>
    /// Performs a right outer join between this <see cref="DataFrame"/> and another, using a row-level predicate to determine matches.
    /// </summary>
    /// <param name="right">The right-hand <see cref="DataFrame"/> to join with.</param>
    /// <param name="predicate">A function that determines whether a pair of rows from the left and right data frames should be joined.</param>
    /// <returns>
    /// A new <see cref="DataFrame"/> containing all rows from the right data frame,
    /// and matching rows from the left. Unmatched left-side rows are excluded.
    /// </returns>
    public DataFrame RightJoin(DataFrame right, Func<DataRow, DataRow, bool> predicate)
    {
        return JoinByInternal(this, right, predicate, includeLeftUnmatched: false, includeRightUnmatched: true);
    }
}