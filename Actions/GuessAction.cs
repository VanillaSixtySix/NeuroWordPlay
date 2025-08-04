using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using NeuroSdk.Actions;
using NeuroSdk.Json;
using NeuroSdk.Websocket;
using NeuroWordPlay.Behaviors;

namespace NeuroWordPlay.Actions;

public class GuessActionData
{
    public string Guess { get; set; }
}

public class GuessAction : NeuroAction<string>
{
    private readonly List<char> _lettersInPlay;

    public GuessAction(List<char> lettersInPlay)
    {
        _lettersInPlay = lettersInPlay;
    }

    public override string Name => "guess";
    protected override string Description => "Guess the word";

    protected override JsonSchema Schema => new()
    {
        Type = JsonSchemaType.Object,
        Required = ["guess"],
        Properties = new Dictionary<string, JsonSchema>
        {
            ["guess"] = QJS.Type(JsonSchemaType.String)
        }
    };

    protected override ExecutionResult Validate(ActionJData actionData, out string parsedData)
    {
        var actionDataObject = actionData.Data.ToObject<GuessActionData>();
        parsedData = actionDataObject.Guess;
        if (parsedData.Length == 0)
        {
            return ExecutionResult.Failure("No guess provided.");
        }

        if (parsedData.Length > Behaviors.Behaviors.AllowedLetterCount)
        {
            return ExecutionResult.Failure("Word is too long, you can only use (at most) " + Behaviors.Behaviors.AllowedLetterCount + " letters.");
        }

        if (parsedData.Length < 4)
        {
            return ExecutionResult.Failure("Word is too short, you must use at least 4 letters.");
        }

        foreach (char c in parsedData)
        {
            var letter = char.ToUpperInvariant(c);
            if (!_lettersInPlay.Contains(letter))
            {
                return ExecutionResult.Failure("Letter not in play: " + letter);
            }
        }
        return ExecutionResult.Success();
    }

    protected override void Execute(string parsedData)
    {
        Plugin.Logger.LogDebug("Inputting word: " + parsedData);
        Behaviors.Behaviors.LastWord = parsedData;

        foreach (char c in parsedData)
        {
            KeyboardControl.Instance.GetType().GetMethod("OnTextInput", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(KeyboardControl.Instance, [c]);
        }

        Timer timer = new(state =>
        {
            SubmissionScript.Instance.PressedSubmitButton();
        }, null, 1000, 0);
    }
}
