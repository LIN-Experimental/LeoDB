﻿using System.Diagnostics;

namespace LeoDB.Utils
{
    internal class TryCatch
    {
        public readonly List<Exception> Exceptions = new List<Exception>();

        public TryCatch()
        {
        }

        public TryCatch(Exception initial)
        {
            this.Exceptions.Add(initial);
        }

        public bool InvalidDatafileState => this.Exceptions.Any(ex =>
            ex is LeoException liteEx &&
            liteEx.ErrorCode == LeoException.INVALID_DATAFILE_STATE);

        [DebuggerHidden]
        public void Catch(Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                this.Exceptions.Add(ex);
            }
        }
    }
}
