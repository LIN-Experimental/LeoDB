namespace LeoDB.Runtime.Policies;

public class PolicyRule
{
    public bool CanInsert { get; set; } = true;
    public bool CanRead { get; set; } = true;
    public bool CanUpdate { get; set; } = true;
    public bool CanDelete { get; set; } = true;
}


public class PolicyTableRule : PolicyRule
{
    public string Table { get; set; } = string.Empty;
}