namespace LeoDB.Engine;

public partial class LeoEngine
{
    private void SysIndexes(string name)
    {
        _settings.Database.GetCollection<SysIndex>(name);
    }

}

internal class SysIndex
{
    public string collection { get; set; }
    public string field { get; set; }
}