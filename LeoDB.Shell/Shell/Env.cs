using System;
using System.Collections.Generic;
using System.IO;
using LeoDB;

namespace LeoDB.Shell
{
    internal class Env
    {
        public Display Display { get; set; }
        public InputCommand Input { get; set; }
        public ILeoDatabase Database { get; set; }
        public bool Running { get; set; } = false;
    }
}