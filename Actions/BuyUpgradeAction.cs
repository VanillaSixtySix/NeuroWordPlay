using System.Collections.Generic;
using NeuroSdk.Actions;
using NeuroSdk.Json;
using NeuroSdk.Websocket;

namespace NeuroWordPlay.Actions;

public class BuyUpgradeActionData
{
    public int CardId { get; set; }
}

public class BuyUpgradeAction(List<BonusCardScript> upgradeCards) : NeuroAction<int>
{
    private readonly List<BonusCardScript> _upgradeCards = upgradeCards;

    public override string Name => "buyupgrade";
    protected override string Description => "Buy an upgrade card from the shop";

    protected override JsonSchema Schema => new()
    {
        Type = JsonSchemaType.Object,
        Required = ["cardId"],
        Properties = new Dictionary<string, JsonSchema>
        {
            ["cardId"] = new JsonSchema
            {
                Type = JsonSchemaType.Integer,
                Minimum = 0,
                Maximum = _upgradeCards.Count - 1
            }
        }
    };

    protected override ExecutionResult Validate(ActionJData actionData, out int parsedData)
    {
        var actionDataObject = actionData.Data.ToObject<BuyUpgradeActionData>();
        parsedData = actionDataObject.CardId;
        
        if (parsedData < 0 || parsedData >= _upgradeCards.Count)
        {
            return ExecutionResult.Failure($"Invalid card ID: {parsedData}. Must be between 0 and {_upgradeCards.Count - 1}");
        }

        // Check if the shop is currently showing
        if (!ShopManager.Instance.shopIsShowing)
        {
            return ExecutionResult.Failure("Shop is not currently open");
        }

        // Check if the card is available and enabled
        var targetCard = _upgradeCards[parsedData];
        if (targetCard.associatedBonus == null)
        {
            return ExecutionResult.Failure($"Card {parsedData} has no associated upgrade");
        }

        return ExecutionResult.Success();
    }

    protected override void Execute(int parsedData)
    {
        Plugin.Logger.LogDebug($"Buying upgrade card {parsedData}: {_upgradeCards[parsedData].associatedBonus?.name ?? "Unknown"}");
        
        ShopManager.Instance.BuyCard(parsedData);
    }
}
