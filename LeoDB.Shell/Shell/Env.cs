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