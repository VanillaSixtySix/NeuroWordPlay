using NeuroSdk.Actions;
using NeuroSdk.Json;
using NeuroSdk.Websocket;

public class GoToLevelSelectAction(TitleScreenManager manager) : NeuroAction<TitleScreenManager>
{
    public override string Name => "levelselect";
    protected override string Description => "Go to the level selection screen.";

    protected override JsonSchema Schema => new()
    {
        Type = JsonSchemaType.None
    };


    protected override ExecutionResult Validate(ActionJData actionData, out TitleScreenManager parsedData)
    {
        parsedData = manager;
        return ExecutionResult.Success();
    }

    protected override void Execute(TitleScreenManager _)
    {
        manager.OpenLevelSelect();
    }
}
