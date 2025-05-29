using System.Collections;

namespace DplyrSharp.Core;

public partial class DataFrame
{
    /// <summary>
    /// Returns a new <see cref="DataFrame"/> containing only the rows for which the specified predicate returns <c>true</c>.
    /// </summary>
    /// <param name="predicate">A function that evaluates each <see cref="DataRow"/> and returns <c>true</c> to keep the row, or <c>false</c> to exclude it.</param>
    /// <returns>A filtered <see cref="DataFrame"/> containing only rows that match the predicate.</returns>
    /// <remarks>
    /// If the predicate throws an exception for a given row, that row is skipped.
    /// </remarks>
    public DataFrame Filter(Func<DataRow, bool> predicate)
    {
        var selected = new List<int>();
        int rowCount = Columns.Count > 0 ? Columns[0].RowCount : 0;
        for (int i = 0; i < rowCount; i++)
        {
            bool keep;
            try
            {
                keep = predicate(new DataRow(this, i));
            }
            catch
            {
                keep = false;
            }
            if (keep)
                selected.Add(i);
        }

        return SliceRows(selected);
    }
}
