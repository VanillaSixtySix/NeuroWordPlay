using System.Collections.Generic;
using NeuroSdk.Actions;
using NeuroSdk.Json;
using NeuroSdk.Websocket;

namespace NeuroWordPlay.Actions;

public class SellModifierActionData
{
    public int ModifierId { get; set; }
}

public class SellModifierAction : NeuroAction<int>
{
    private readonly List<BaseBonus> _modifiers;

    public SellModifierAction(List<BaseBonus> modifiers)
    {
        _modifiers = modifiers;
    }

    public override string Name => "sellmodifier";
    protected override string Description => "Sell a modifier";

    protected override JsonSchema Schema => new()
    {
        Type = JsonSchemaType.Object,
        Required = ["modifierId"],
        Properties = new Dictionary<string, JsonSchema>
        {
            ["modifierId"] = new JsonSchema
            {
                Type = JsonSchemaType.Integer,
                Minimum = 0,
                Maximum = _modifiers.Count - 1
            }
        }
    };

    protected override ExecutionResult Validate(ActionJData actionData, out int parsedData)
    {
        var actionDataObject = actionData.Data.ToObject<SellModifierActionData>();
        parsedData = actionDataObject.ModifierId;
        
        if (parsedData < 0 || parsedData >= _modifiers.Count)
        {
            return ExecutionResult.Failure($"Invalid modifier ID: {parsedData}. Must be between 0 and {_modifiers.Count - 1}");
        }

        return ExecutionResult.Success();
    }

    protected override void Execute(int parsedData)
    {
        Plugin.Logger.LogDebug($"Selling modifier {parsedData}: {_modifiers[parsedData].name}");
        
        var modifier = _modifiers[parsedData];
        modifier.myModifierStub.SellStub();

        InteractionLockHelper.AddToQueue(() => Behaviors.Gameplay.CreateGuessAction());
    }
}
