using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using static LeoDB.Constants;

namespace LeoDB.Engine
{
    internal enum TransactionState
    {
        Active,
        Committed,
        Aborted,
        Disposed
    }
}