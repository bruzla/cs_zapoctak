using System;

namespace DplyrSharp.Core;

public partial class DataFrame
{
    public DataFrame FullJoin(DataFrame right, Func<DataRow, DataRow, bool> predicate)
    {
        return JoinByInternal(this, right, predicate, includeLeftUnmatched: true, includeRightUnmatched: true);
    }
}