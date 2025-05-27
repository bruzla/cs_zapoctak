using System.Collections;

namespace DplyrSharp.Core;

public partial class DataFrame
{
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
