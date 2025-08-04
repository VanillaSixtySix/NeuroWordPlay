using NeuroSdk.Actions;
using NeuroSdk.Json;
using NeuroSdk.Websocket;

public class RefreshTilesAction : NeuroAction<bool>
{
    public override string Name => "refreshtiles";
    protected override string Description => "Refresh the tiles in play.";

    protected override JsonSchema Schema => new()
    {
        Type = JsonSchemaType.None
    };


    protected override ExecutionResult Validate(ActionJData actionData, out bool parsedData)
    {
        parsedData = true;

        if (LogicManagerScript.Instance.numberOfRefreshes == 0)
        {
            parsedData = false;
            return ExecutionResult.Failure("No refreshes left");
        }

        return ExecutionResult.Success();
    }

    protected override void Execute(bool parsedData)
    {
        ToolsScript.Instance.refresh.PressedRefreshButton();
    }
}
