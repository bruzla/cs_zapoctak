using System;

namespace DplyrSharp.Core;

public partial class DataFrame
{
    public DataFrame RightJoin(DataFrame right, Func<DataRow, DataRow, bool> predicate)
    {
        return JoinByInternal(this, right, predicate, includeLeftUnmatched: false, includeRightUnmatched: true);
    }
}