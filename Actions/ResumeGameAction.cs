using NeuroSdk.Actions;
using NeuroSdk.Json;
using NeuroSdk.Websocket;

public class ResumeGameAction(TitleScreenManager manager) : NeuroAction<TitleScreenManager>
{
    public override string Name => "resume";
    protected override string Description => "Resume an in-progress game.";

    protected override JsonSchema Schema => new()
    {
        Type = JsonSchemaType.None
    };

    protected override ExecutionResult Validate(ActionJData actionData, out TitleScreenManager parsedData)
    {
        parsedData = manager;

        if (!MetaGameRules.Instance.hasInProgressData)
        {
            parsedData = null;
            return ExecutionResult.Failure("No game in progress");
        }

        return ExecutionResult.Success();
    }

    protected override void Execute(TitleScreenManager _)
    {
        manager.resumeButton.onClick.Invoke();
    }
}
