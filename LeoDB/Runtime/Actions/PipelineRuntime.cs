namespace LeoDB.Runtime.Actions;

public class PipelineRuntime
{

    /// <summary>
    /// Lista de acciones que se ejecutan antes de insertar un documento.
    /// </summary>
    private List<Action<BsonDocument, PipelineStatus>> OnInsert { get; set; } = [];

    /// <summary>
    /// Lista de acciones que se ejecutan antes de actualizar un documento.
    /// </summary>
    private List<Action<BsonDocument, PipelineStatus>> OnUpdate { get; set; } = [];

    public void AddOnInsert(Action<BsonDocument, PipelineStatus> action) => OnInsert.Add(action);
    public void AddOnUpdate(Action<BsonDocument, PipelineStatus> action) => OnUpdate.Add(action);

    public PipelineStatus ExecuteOnInsert(BsonDocument doc)
    {
        PipelineStatus pipelineStatus = new();

        foreach (var action in OnInsert)
        {
            action(doc, pipelineStatus);

            if (pipelineStatus.HasError)
                throw new LeoException(0, pipelineStatus.Message);

            if (!pipelineStatus.CanContinue)
                break;
        }

        return pipelineStatus;
    }

    public PipelineStatus ExecuteOnUpdate(BsonDocument doc)
    {
        PipelineStatus pipelineStatus = new();

        foreach (var action in OnUpdate)
        {
            action(doc, pipelineStatus);

            if (pipelineStatus.HasError)
                throw new LeoException(0, pipelineStatus.Message);

            if (!pipelineStatus.CanContinue)
                break;
        }

        return pipelineStatus;
    }

}