using System;
using System.Collections.Generic;
using System.Linq;

namespace LeoDB.Shell
{
    internal interface IShellCommand
    {
        bool IsCommand(StringScanner s);

        void Execute(StringScanner s, Env env);
    }
}