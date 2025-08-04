using NeuroSdk.Actions;
using NeuroSdk.Json;
using NeuroSdk.Websocket;

namespace NeuroWordPlay.Actions;

public class SkipAction : NeuroAction<ShopManager>
{
    private readonly ShopManager _shopManager;

    public SkipAction(ShopManager shopManager)
    {
        _shopManager = shopManager;
    }

    public override string Name => "skip";
    protected override string Description => "Skip the shop and go to the next round without buying any upgrade cards. You get two more refreshes of the letters in play.";

    protected override JsonSchema Schema => new()
    {
        Type = JsonSchemaType.None
    };

    protected override ExecutionResult Validate(ActionJData actionData, out ShopManager parsedData)
    {
        parsedData = _shopManager;
        // Check if the shop is currently showing
        if (!ShopManager.Instance.shopIsShowing)
        {
            return ExecutionResult.Failure("Shop is not currently open");
        }

        return ExecutionResult.Success();
    }

    protected override void Execute(ShopManager parsedData)
    {
        parsedData.SkipShop();
    }
}
