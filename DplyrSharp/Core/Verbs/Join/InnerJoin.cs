using System.Collections;

namespace DplyrSharp.Core;

public partial class DataFrame
{
    public DataFrame InnerJoin(DataFrame right, Func<DataRow, DataRow, bool> predicate)
    {
        return JoinByInternal(this, right, predicate, includeLeftUnmatched: false, includeRightUnmatched: false);
    }
}