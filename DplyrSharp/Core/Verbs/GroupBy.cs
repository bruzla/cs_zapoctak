using System.Collections;

namespace DplyrSharp.Core;

public partial class DataFrame
{
    public GroupedDataFrame GroupBy(params string[] groupColumns)
    {
        return new GroupedDataFrame(this, groupColumns);
    }
}

