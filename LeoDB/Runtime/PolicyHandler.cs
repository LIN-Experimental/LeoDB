using LeoDB.Runtime.Policies;

namespace LeoDB;

public class PolicyHandler
{

    public List<PolicyRule> Policies { get; } = [];

    public bool CanRead(string collection)
    {
        var rule = Policies.FirstOrDefault(p => p is PolicyTableRule r && r.Table == collection);

        if (rule is null)
            return true; // default allow

        return rule.CanRead;
    }

    public bool CanInsert(string collection)
    {
        var rule = Policies.FirstOrDefault(p => p is PolicyTableRule r && r.Table == collection);

        if (rule is null)
            return true; // default allow

        return rule.CanInsert;
    }

    public bool CanUpdate(string collection)
    {
        var rule = Policies.FirstOrDefault(p => p is PolicyTableRule r && r.Table == collection);

        if (rule is null)
            return true; // default allow

        return rule.CanUpdate;
    }

    public bool CanDelete(string collection)
    {
        var rule = Policies.FirstOrDefault(p => p is PolicyTableRule r && r.Table == collection);

        if (rule is null)
            return true; // default allow

        return rule.CanDelete;
    }


    public static PolicyHandler CreateSuper()
    {
        return new PolicyHandler();
    }

}