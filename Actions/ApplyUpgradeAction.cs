using System;
using System.Collections.Generic;
using NeuroSdk.Actions;
using NeuroSdk.Json;
using NeuroSdk.Websocket;

namespace NeuroWordPlay.Actions;

public class ApplyUpgradeActionData
{
    public int UpgradeId { get; set; }
    public int TileId { get; set; }
}

public class ApplyUpgradeAction : NeuroAction<ApplyUpgradeActionData>
{
    private readonly List<BaseBonus> _ownedUpgrades;
    private readonly List<TileScript> _tiles;

    public ApplyUpgradeAction(List<BaseBonus> ownedUpgrades, List<TileScript> tiles)
    {
        _ownedUpgrades = ownedUpgrades;
        _tiles = tiles;
    }

    public override string Name => "applyupgrade";
    protected override string Description => "Apply an upgrade to a given tile";

    protected override JsonSchema Schema => new()
    {
        Type = JsonSchemaType.Object,
        Required = ["upgradeId", "tileId"],
        Properties = new Dictionary<string, JsonSchema>
        {
            ["upgradeId"] = new JsonSchema
            {
                Type = JsonSchemaType.Integer,
                Minimum = 0,
                Maximum = _ownedUpgrades.Count - 1
            },
            ["tileId"] = new JsonSchema
            {
                Type = JsonSchemaType.Integer,
                Minimum = 0,
                Maximum = _tiles.Count - 1
            }
        }
    };

    protected override ExecutionResult Validate(ActionJData actionData, out ApplyUpgradeActionData parsedData)
    {
        parsedData = actionData.Data.ToObject<ApplyUpgradeActionData>();
        
        if (parsedData.UpgradeId < 0 || parsedData.UpgradeId >= _ownedUpgrades.Count)
        {
            return ExecutionResult.Failure($"Invalid upgrade ID: {parsedData.UpgradeId}. Must be between 0 and {_ownedUpgrades.Count - 1}");
        }

        if (parsedData.TileId < 0 || parsedData.TileId >= _tiles.Count)
        {
            return ExecutionResult.Failure($"Invalid tile ID: {parsedData.TileId}. Must be between 0 and {_tiles.Count - 1}");
        }

        return ExecutionResult.Success();
    }

    protected override void Execute(ApplyUpgradeActionData parsedData)
    {
        var targetTile = _tiles[parsedData.TileId];
        Plugin.Logger.LogDebug($"Applying upgrade {parsedData.UpgradeId}: {_ownedUpgrades[parsedData.UpgradeId].name ?? "Unknown"}");
    
        var targetUpgrade = _ownedUpgrades[parsedData.UpgradeId];
        targetUpgrade.myUpgradeStub.UpgradeTile(targetTile);
    }
}
