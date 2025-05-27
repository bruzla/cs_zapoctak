using System.Collections;

namespace DplyrSharp.Core;

public partial class DataFrame
{
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
