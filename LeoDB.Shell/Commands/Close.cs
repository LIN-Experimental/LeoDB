﻿namespace LeoDB.Shell.Commands
{
    [Help(
        Name = "close",
        Syntax = "close",
        Description = "Close current datafile"
    )]
    internal class Close : IShellCommand
    {
        public bool IsCommand(StringScanner s)
        {
            return s.Scan(@"close$").Length > 0;
        }

        public void Execute(StringScanner s, Env env)
        {
            if (env.Database != null)
            {
                env.Database.Dispose();
                env.Database = null;
            }
        }
    }
}