namespace LeoDB.Runtime.Actions;

public class PipelineStatus
{
    public bool CanContinue { get; set; } = true;
    public bool HasError { get; set; } = false;
    public string? Message { get; set; } = null;
}