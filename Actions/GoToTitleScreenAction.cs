using System.Reflection;
using NeuroSdk.Actions;
using NeuroSdk.Json;
using NeuroSdk.Websocket;

public class GoToTitleScreenAction(LogicManagerScript manager) : NeuroAction<LogicManagerScript>
{
    public override string Name => "gototitlescreen";
    protected override string Description => "Go to the title screen.";

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
        var gOScreen = LogicManagerScript.Instance.GetType().GetField("gOScreen", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(LogicManagerScript.Instance);
        gOScreen.GetType().GetMethod("GoToTitleScreen").Invoke(gOScreen, null);
    }
}
