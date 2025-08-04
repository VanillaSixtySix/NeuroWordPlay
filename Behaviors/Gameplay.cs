using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using HarmonyLib;
using NeuroSdk.Actions;
using NeuroWordPlay.Actions;
using UnityEngine;

namespace NeuroWordPlay.Behaviors;

public static class Behaviors
{
    public static string LastWord { get; set; }

    public static int AllowedLetterCount { get; set; }

    public static List<char> LettersInPlay { get; set; }

    public static List<BonusCardScript> UpgradeCards { get; set; }

    public static List<BaseBonus> OwnedUpgrades { get; set; }

    public static List<BaseBonus> OwnedModifiers { get; set; }

    public static string GetGuessContext()
    {
        SpecialRoundManager specialRound = null;
        SpecialRoundManager nastyRound = null;
        var isNastyRound = false;

        var specialRoundContext = "";
        var specialRoundType = 0;

        if (LogicManagerScript.Instance.weAreInASpecialRound)
        {
            specialRoundType = LogicManagerScript.Instance.specialRoundTypes[LogicManagerScript.Instance.specialRoundWeAreOn];
            specialRound = LogicManagerScript.Instance.specialRounds[specialRoundType];
            isNastyRound = LogicManagerScript.Instance.onRunawayStopper;
            nastyRound = LogicManagerScript.Instance.nastyRounds[specialRoundType];

            // Normal special rounds
            //  0. Stuck Tile
            //  1. Require 6 Tiles
            //  2. Shuffle After Words
            //  3. Mandatory Tile
            //  4. Lock Top 4
            //  5. Length Increases
            //  6. Must be Vowel

            // Nasty special rounds
            //  0. Words Cost 2 Plays
            //  1. Vowels Don't Score
            //  2. Special Tiles Nullified
            //  3. Words Cost 2 Plays
            //  4. Vowels Don't Score
            //  5. Special Tiles Nullified
            //  6. Words Cost 2 Plays

            Plugin.Logger.LogDebug("Special round type: " + specialRoundType);
            Plugin.Logger.LogDebug("Special round: " + specialRound);
            Plugin.Logger.LogDebug("Nasty round: " + nastyRound);
            Plugin.Logger.LogDebug("Is nasty round: " + isNastyRound);

            if (isNastyRound)
            {
                specialRoundContext = specialRoundType switch
                {
                    0 => "Words cost 2 plays.",
                    1 => "Vowels don't score.",
                    2 => "Special tiles nullified.",
                    3 => "Words cost 2 plays.",
                    4 => "Vowels don't score.",
                    5 => "Special tiles nullified.",
                    6 => "Words cost 2 plays.",
                    _ => "No extra context.",
                };
            } else {
                switch (specialRoundType)
                {
                    case 0:
                        var component = Traverse.Create(LogicManagerScript.Instance).Field("stuckTile").GetValue() as GameObject;
                        var stuckTile = component.GetComponent<TileScript>();
                        specialRoundContext = $"You MUST start the word with the letter '{stuckTile.myLetter}'.";
                        break;
                    case 1:
                        specialRoundContext = "TODO";
                        break;
                    case 2:
                        // we don't need to provide context.. letters are shuffled after each word
                        break;
                    case 3:
                        var mandatoryTileSlot = LogicManagerScript.Instance.mandatoryTileSlot;
                        var mandatoryTile = GridManagerScript.Instance.everyTileInPlay[mandatoryTileSlot];
                        var letterMustBeUsed = mandatoryTile.myLetter;
                        specialRoundContext = $"The following letter is REQUIRED to be used in the word: {letterMustBeUsed}";
                        break;
                    case 4:
                        // we don't need to provide context.. we're already modifying the returned letters to make it simpler for context
                        break;
                    case 5:
                        specialRoundContext = "After each valid word, the maximum length will increase.";
                        break;
                    case 6:
                        var letterMustBeVowel = LogicManagerScript.Instance.letterMustBeVowel;
                        specialRoundContext = $"The letter in (zero-indexed) position {letterMustBeVowel} must be a vowel.";
                        break;
                    default:
                        specialRoundContext = "No extra context.";
                        break;
                    }
            }
        }

        var scoreMap = new Dictionary<int, List<char>>();
        int i = 0;
        foreach (var letter in LettersInPlay)
        {
            if (LogicManagerScript.Instance.weAreInASpecialRound && specialRoundType == 4 && i < 4)
            {
                // only for Lock Top 4 special round, remove first four letters 
                i++;
                continue;
            }

            var score = ScoreManager.Instance.GetTileScore(letter.ToString().ToUpper());
            if (!scoreMap.ContainsKey(score))
            {
                scoreMap[score] = [];
            }
            scoreMap[score].Add(letter);
        }

        var scoreMapString = string.Join("\n", scoreMap.Select(pair => $"{pair.Key} points: {string.Join(", ", pair.Value)}"));

        var ownedUpgradesString = string.Join("\n", OwnedUpgrades.Select((upgrade, index) => $"Upgrade {index}: {upgrade.shopDescription}"));

        var ownedModifiersString = string.Join("\n", OwnedModifiers.Select((modifier, index) => $"Modifier {index}: {modifier.shopDescription}"));

        var barScript = Traverse.Create(LogicManagerScript.Instance).Field("scoreBarScript").GetValue() as BarScript;
        var scoreToGo = (float)barScript.GetType().GetField("scoreToGo", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(barScript);
        var scoreToGoInt = (int)scoreToGo;

        var normalContext = $"I have the following letters and their associated points. Each letter can only be used once. I need to form the longest English word possible to earn more score, based on the points of the letters in the word. Proper nouns are not allowed. The word must be at least 4 letters long but not exceed {AllowedLetterCount} letters. Try using higher-scoring letters first.\n\n{scoreMapString}\n\n";

        if (OwnedUpgrades.Count > 0)
        {
            normalContext += $" I have the following upgrades available:\n\n{ownedUpgradesString}\n\n";
        }

        if (OwnedModifiers.Count > 0)
        {
            normalContext += $"I have the following modifiers available, each of which can be sold:\n\n{ownedModifiersString}\n\n";
        }

        normalContext += $"I only have {LogicManagerScript.Instance.lives} plays left.";

        if (scoreToGoInt > 0)
        {
            normalContext += $"I need to earn {scoreToGoInt} more points to win the round.";
        }

        if (specialRound != null)
        {
            if (!isNastyRound && (specialRoundType == 4 || specialRoundType == 2))
            {
                return normalContext;
            } else {
                var roundText = isNastyRound ? nastyRound.textToDisplay : specialRound.textToDisplay;
                return $"{normalContext}\n\nWe are in a special round, so we must obey this rule: {roundText}\n\n{specialRoundContext}";
            }
        }
        return normalContext;
    }

    public static string GetShopContext()
    {
        var cardDescriptions = UpgradeCards.Select((card, index) => $"Card {index}: {card.associatedBonus.shopDescription}").ToList();
        return $"The shop is open with the following upgrade cards available:\n----------\n{string.Join("\n\n", cardDescriptions)}\n----------\nChoose which card to buy by specifying the card ID (0-{UpgradeCards.Count - 1}).\n\nYou can also reroll the upgrade cards at a cost of lives, or skip the shop and go to the next round without buying any upgrade cards, but you get two more refreshes of the letters in play.";
    }
}

[HarmonyPatch(typeof(LogicManagerScript))]
public class Gameplay
{
    public static void CreateGuessAction()
    {
        // unregister shop actions
        NeuroActionHandler.UnregisterActions(["reroll", "skip", "buyupgrade", "sellmodifier"]);
        // unregister apply upgrade action
        NeuroActionHandler.UnregisterActions(["applyupgrade"]);

        Behaviors.AllowedLetterCount = LogicManagerScript.Instance.allowedNumberOfLetters;
        Behaviors.LettersInPlay = [.. LetterBagManager.Instance.LettersInPlay.Select(code => code[0])];
        Behaviors.OwnedUpgrades = BonusInventory.Instance.ownedUpgrades;
        Behaviors.OwnedModifiers = BonusInventory.Instance.ownedModifiers;
        var tiles = GridManagerScript.Instance.everyTileInPlay.Where(tile => tile.isActiveAndEnabled).ToList();

        List<INeuroAction> actions = [new GuessAction([.. Behaviors.LettersInPlay])];

        if (Behaviors.OwnedUpgrades.Count > 0)
        {
            actions.Add(new ApplyUpgradeAction(Behaviors.OwnedUpgrades, tiles));
        }

        if (LogicManagerScript.Instance.numberOfRefreshes > 0)
        {
            actions.Add(new RefreshTilesAction());
        }

        if (Behaviors.OwnedModifiers.Count > 0)
        {
            actions.Add(new SellModifierAction(Behaviors.OwnedModifiers));
        }

        var window = ActionWindow.Create(LogicManagerScript.Instance.gameObject)
            .SetContext(Behaviors.GetGuessContext(), false)
            .SetForce(10, "", "", false);

        foreach (var action in actions)
        {
            window.AddAction(action);
        }

        window.Register();
    }

    [HarmonyPostfix]
    [HarmonyPatch("Start")]
    static void Start(LogicManagerScript __instance)
    {
        Plugin.Logger.LogDebug("Gameplay started, asking for a new word");
        InteractionLockHelper.AddToQueue(() => CreateGuessAction());
        NeuroActionHandler.RegisterActions([new SaveAndQuitAction(__instance)]);
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(LogicManagerScript.FinishRound))]
    static void FinishRound(LogicManagerScript __instance)
    {
        Plugin.Logger.LogDebug("Round finished");

        NeuroActionHandler.UnregisterActions(["guess", "refreshtiles", "sellmodifier"]);
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(LogicManagerScript.GoToNextRound))]
    static void GoToNextRound(LogicManagerScript __instance)
    {
        Plugin.Logger.LogDebug("Gameplay going to next round");
        
        // wait 1 second
        Timer timer = new(state =>
        {
            InteractionLockHelper.AddToQueue(() => CreateGuessAction());
        }, null, 1000, 0);
    }

    // GameComplete
    [HarmonyPostfix]
    [HarmonyPatch(nameof(LogicManagerScript.GameComplete))]
    static void GameComplete(LogicManagerScript __instance)
    {
        Plugin.Logger.LogDebug("Game complete");
        NeuroActionHandler.UnregisterActions(["guess", "refreshtiles", "sellmodifier"]);
        InteractionLockHelper.AddToQueue(() =>
        {
            ActionWindow.Create(__instance.gameObject)
                .SetContext("Game over, you won! You can go back to the title screen or start endless mode.", false)
                .SetForce(10, "", "", false)
                .AddAction(new StartEndlessModeAction(__instance))
                .AddAction(new GoToTitleScreenAction(__instance))
                .Register();
        });
    }

    // GameOver
    [HarmonyPostfix]
    [HarmonyPatch(nameof(LogicManagerScript.GameOver))]
    static void GameOver(LogicManagerScript __instance)
    {
        Plugin.Logger.LogDebug("Game over");
        NeuroActionHandler.UnregisterActions(["guess", "refreshtiles", "sellmodifier"]);
        InteractionLockHelper.AddToQueue(() =>
        {
            ActionWindow.Create(__instance.gameObject)
                .SetContext("Game over, you lost. You can go back to the title screen or play again on the same difficulty.", false)
                .SetForce(10, "", "", false)
                .AddAction(new PlayAgainAction(__instance))
                .AddAction(new GoToTitleScreenAction(__instance))
                .Register();
        });
    }
}

[HarmonyPatch(typeof(ShopManager))]
public class ShopManagerHandler
{
    [HarmonyPostfix]
    [HarmonyPatch("ShowShop")]
    static void ShowShop(ShopManager __instance)
    {
        Plugin.Logger.LogDebug("Showing shop");

        InteractionLockHelper.AddToQueue(() =>
        {
            // Possible actions on a normal shop screen:
            // - Buy 1 of N upgrade cards
            // - Re-roll upgrade cards
            // - Skip the shop

            Behaviors.UpgradeCards = [.. ShopManager.Instance.upgradeCards
                .Where(upgradeCard => upgradeCard.gameObject.activeSelf)
                .Where(upgradeCard => upgradeCard.associatedBonus != null)
                .Where(upgradeCard => upgradeCard.associatedBonus.shopDescription != null)];

            List<INeuroAction> actions = [new RerollAction(ShopManager.Instance), new SkipAction(ShopManager.Instance)];

            if (Behaviors.OwnedModifiers.Count < BonusInventory.Instance.maximumModifiers)
            {
                actions.Add(new BuyUpgradeAction(Behaviors.UpgradeCards));
            }

            if (Behaviors.OwnedUpgrades.Count > 0)
            {
                actions.Add(new SellModifierAction(Behaviors.OwnedModifiers));
            }

            var window = ActionWindow.Create(__instance.gameObject)
                .SetContext(Behaviors.GetShopContext(), false)
                .SetForce(10, "", "", false);

            foreach (var action in actions)
            {
                window.AddAction(action);
            }

            window.Register();
        });
    }
}

[HarmonyPatch(typeof(SubmitWord))]
public class SubmitWordHandler
{
    [HarmonyPostfix]
    [HarmonyPatch("WordIsIncorrect")]
    static IEnumerator WordIsIncorrect(IEnumerator result, LastWordArea ___lastWord)
    {
        while (result.MoveNext())
        {
            yield return result.Current;
        }
        Plugin.Logger.LogDebug("Word is incorrect, removing tiles and asking for a new word");

        SubmissionScript.Instance.RemoveAllTilesFromSubmissionList();

        NeuroActionHandler.UnregisterActions(["guess"]);

        InteractionLockHelper.AddToQueue(Gameplay.CreateGuessAction);
    }

    [HarmonyPostfix]
    [HarmonyPatch("ReturnToGame")]
    static void ReturnToGame()
    {
        Plugin.Logger.LogDebug("Returned to game, asking for a new word");
        InteractionLockHelper.AddToQueue(Gameplay.CreateGuessAction);
    }
}

[HarmonyPatch(typeof(RefreshTool))]
public class RefreshToolHandler
{
    [HarmonyPostfix]
    [HarmonyPatch("RefreshTiles")]
    static IEnumerator RefreshTiles(IEnumerator result, bool resumeAfterwards)
    {
        while (result.MoveNext())
        {
            yield return result.Current;
        }
        InteractionLockHelper.AddToQueue(Gameplay.CreateGuessAction);
    }
}

[HarmonyPatch(typeof(UpgradeStubScript))]
public class UpgradeStubScriptHandler
{
    [HarmonyPrefix]
    [HarmonyPatch("ResetStub")]
    static bool ResetStub(UpgradeStubScript __instance)
    {
        Behaviors.OwnedUpgrades.Remove(__instance.associatedBonus);
        InteractionLockHelper.AddToQueue(Gameplay.CreateGuessAction);
        return true;
    }
}
