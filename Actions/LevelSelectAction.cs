using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using NeuroSdk.Actions;
using NeuroSdk.Json;
using NeuroSdk.Websocket;
using NeuroWordPlay;
using UnityEngine;

public class LevelSelectionAction : NeuroAction<int>
{
    private readonly GameObject _levelSelect;
    private readonly LevelSelectScript _levelSelectScript;
    private readonly List<Tuple<int, string>> playableLevelIds = [];

    public LevelSelectionAction(GameObject levelSelect)
    {
        _levelSelect = levelSelect;

        _levelSelectScript = _levelSelect.GetComponent<LevelSelectScript>();

        for (int i = 0; i < _levelSelectScript.levelNames.Count; i++)
        {
            // pickerTiles is a private field in LevelSelectScript of List<GameObject>
            var pickerTiles = _levelSelectScript.GetType().GetField("pickerTiles", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(_levelSelectScript) as List<GameObject>;
            var pickerTileText = _levelSelectScript.GetType().GetField("pickerTileText", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(_levelSelectScript) as List<GameObject>;
            var lockIcons = _levelSelectScript.GetType().GetField("lockIcons", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(_levelSelectScript) as List<GameObject>;
            if (pickerTileText[i] == null || !pickerTiles[i].activeSelf || (lockIcons[i] != null && lockIcons[i].activeSelf))
            {
                continue;
            }
            var levelName = _levelSelectScript.levelNames[i];
            playableLevelIds.Add(new Tuple<int, string>(i, levelName));
        }
    }

    public override string Name => "levelselect";
    protected override string Description => "Select a level or difficulty to play.";

    protected override JsonSchema Schema => new()
    {
        Type = JsonSchemaType.Object,
        Required = ["level"],
        Properties = new Dictionary<string, JsonSchema>
        {
            ["level"] = QJS.Enum([.. playableLevelIds.Select(tile => tile.Item2)])
        }
    };


    protected override ExecutionResult Validate(ActionJData actionData, out int parsedData)
    {
        var actionDataObject = actionData.Data.ToObject<LevelSelectActionData>();

        var levelId = playableLevelIds.FirstOrDefault(tile => tile.Item2 == actionDataObject.Level);

        if (levelId == null)
        {
            parsedData = -1;
            return ExecutionResult.Failure("Level is invalid");
        }

        parsedData = levelId.Item1;

        return ExecutionResult.Success();
    }

    protected override void Execute(int parsedData)
    {
        if (parsedData == -1)
        {
            Plugin.Logger.LogError("Level is invalid");
            return;
        }

        _levelSelectScript.ChangeDetails(parsedData);
        _levelSelectScript.PlayLevel();
    }
}

public class LevelSelectActionData
{
    public string Level { get; set; }
}
