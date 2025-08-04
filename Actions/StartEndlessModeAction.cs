using NeuroSdk.Actions;
using NeuroSdk.Json;
using NeuroSdk.Websocket;

public class StartEndlessModeAction(LogicManagerScript manager) : NeuroAction<LogicManagerScript>
{
    public override string Name => "startendlessmode";
    protected override string Description => "Start endless mode.";

    protected override JsonSchema Schema => new()
    {
        Type = JsonSchemaType.None
    };


    protected override ExecutionResult Validate(ActionJData actionData, out LogicManagerScript parsedData)
    {
        parsedData = manager;
        return ExecutionResult.Success();
    }

    protected override void Execute(LogicManagerScript _)
    {
        LogicManagerScript.Instance.PlayEndlessMode();
    }
}
