using System.Collections;

namespace DplyrSharp.Core;

public partial class DataFrame
{
    /// <summary>
    /// Returns a new <see cref="DataFrame"/> that includes only the specified columns by name.
    /// </summary>
    /// <param name="columnNames">An array of column names to include in the result.</param>
    /// <returns>A new <see cref="DataFrame"/> containing only the selected columns.</returns>
    /// <exception cref="ArgumentException">Thrown if any specified column name does not exist in the data frame.</exception>
    public DataFrame Select(params string[] columnNames)
    {
        List<IDataColumn> columns = new List<IDataColumn>(columnNames.Length);

        foreach (string columnName in columnNames)
        {
            columns.Add(this.Columns[this.GetColumnIndex(columnName)]);
        }

        return new DataFrame(columns);
    }
}
