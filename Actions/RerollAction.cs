using System.Linq;
using NeuroSdk.Actions;
using NeuroSdk.Json;
using NeuroSdk.Websocket;
using NeuroWordPlay.Behaviors;

namespace NeuroWordPlay.Actions;

public class RerollAction : NeuroAction<ShopManager>
{
    private readonly ShopManager _shopManager;

    private readonly int rerollCost = 1;

    public RerollAction(ShopManager shopManager)
    {
        _shopManager = shopManager;

        var rerollCostField = ShopManager.Instance.GetType().GetField("rerollCost");
        if (rerollCostField != null)
        {
            rerollCost = (int)rerollCostField.GetValue(ShopManager.Instance);
        }
        else
        {
            Plugin.Logger.LogWarning("Reroll cost field not found");
        }
    }

    public override string Name => "reroll";
    protected override string Description => $"Reroll the upgrade cards in the shop at the cost of {rerollCost} lives. You can only reroll once per round.";

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

        if (LogicManagerScript.Instance.lives < rerollCost)
        {
            return ExecutionResult.Failure("You do not have enough lives to reroll");
        }

        return ExecutionResult.Success();
    }

    protected override void Execute(ShopManager parsedData)
    {
        parsedData.ReRollShop();
        
        // After rerolling, create a new ActionWindow with only buy and skip actions
        // (no reroll since you can only reroll once per round)
        InteractionLockHelper.AddToQueue(() =>
        {
            // Update the upgrade cards list after reroll
            var upgradeCards = ShopManager.Instance.upgradeCards
                .Where(upgradeCard => upgradeCard.gameObject.activeSelf)
                .Where(upgradeCard => upgradeCard.associatedBonus != null)
                .Where(upgradeCard => upgradeCard.associatedBonus.shopDescription != null)
                .ToList();

            // Update the static property for other components to use
            Behaviors.Behaviors.UpgradeCards = upgradeCards;

            // Create shop context using the static method
            var shopContext = Behaviors.Behaviors.GetShopContext();

            ActionWindow.Create(LogicManagerScript.Instance.gameObject)
                .SetContext(shopContext, false)
                .SetForce(10, "", "", false)
                .AddAction(new BuyUpgradeAction(Behaviors.Behaviors.UpgradeCards))
                .AddAction(new SkipAction(ShopManager.Instance))
                .Register();
        });
    }
}
