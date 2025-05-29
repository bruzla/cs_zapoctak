using System;

namespace DplyrSharp.Core;

public partial class DataFrame
{
    /// <summary>
    /// Performs a full outer join between this <see cref="DataFrame"/> and another, using a row-level predicate to determine matches.
    /// </summary>
    /// <param name="right">The right-hand <see cref="DataFrame"/> to join with.</param>
    /// <param name="predicate">A function that determines whether a pair of rows from the left and right data frames should be joined.</param>
    /// <returns>
    /// A new <see cref="DataFrame"/> containing rows from both data frames:
    /// matched pairs, unmatched left rows with <c>null</c> right values, and unmatched right rows with <c>null</c> left values.
    /// </returns>
    public DataFrame FullJoin(DataFrame right, Func<DataRow, DataRow, bool> predicate)
    {
        return JoinByInternal(this, right, predicate, includeLeftUnmatched: true, includeRightUnmatched: true);
    }
}