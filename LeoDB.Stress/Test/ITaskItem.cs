using System;

namespace LeoDB.Stress
{
    public interface ITestItem
    {
        string Name { get; }
        int TaskCount { get; }
        TimeSpan Sleep { get; }
        BsonValue Execute(LeoDatabase db);
    }
}
