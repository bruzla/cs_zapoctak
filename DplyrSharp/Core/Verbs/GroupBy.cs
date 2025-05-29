using System.Collections;

namespace DplyrSharp.Core;

public partial class DataFrame
{
    /// <summary>
    /// Groups the rows of the current <see cref="DataFrame"/> by one or more specified columns.
    /// </summary>
    /// <param name="groupColumns">An array of column names to group by.</param>
    /// <returns>A <see cref="GroupedDataFrame"/> where rows are grouped according to the specified columns.</returns>
    public GroupedDataFrame GroupBy(params string[] groupColumns)
    {
        return new GroupedDataFrame(this, groupColumns);
    }
}

