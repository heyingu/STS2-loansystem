using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Entities.Players;
using System.Linq;
using System.Collections.Generic;

namespace LoanSystem;

public static class GameAPI
{
    public static Player? GetPlayer()
    {
        GD.Print($"[{MainFile.ModId}] GetPlayer v2 called");
        var runManager = RunManager.Instance;
        if (runManager == null) return null;

        var traverse = Traverse.Create(runManager);

        // 先试 _players 字段
        var players = traverse.Field("_players").GetValue<IEnumerable<Player>>();
        if (players != null)
            return players.FirstOrDefault();

        // 再试通过 _runState 字段
        var runState = traverse.Field("_runState").GetValue();
        if (runState == null) runState = traverse.Field("runState").GetValue();
        if (runState == null) return null;

        var statePlayers = Traverse.Create(runState).Field("_players").GetValue<IEnumerable<Player>>();
        if (statePlayers != null)
            return statePlayers.FirstOrDefault();

        return Traverse.Create(runState).Property("Players").GetValue<IEnumerable<Player>>()?.FirstOrDefault();
    }

    public static int GetAscension()
    {
        var runManager = RunManager.Instance;
        if (runManager == null) return 0;

        var traverse = Traverse.Create(runManager);
        var ascension = traverse.Field("_ascension").GetValue<int>();
        if (ascension > 0) return ascension;

        var runState = traverse.Field("_runState").GetValue();
        if (runState == null) runState = traverse.Field("runState").GetValue();
        if (runState == null) return 0;

        return Traverse.Create(runState).Field("ascension").GetValue<int>();
    }

    public static void AddRelic(Player player, string relicId)
    {
        var relicManager = Traverse.Create(player).Field("relicManager").GetValue();
        if (relicManager != null)
        {
            Traverse.Create(relicManager).Method("AddRelic", relicId).GetValue();
            GD.Print($"[{MainFile.ModId}] Added relic: {relicId}");
        }
    }

    public static void RemoveRelic(Player player, string relicId)
    {
        var relicManager = Traverse.Create(player).Field("relicManager").GetValue();
        if (relicManager != null)
        {
            Traverse.Create(relicManager).Method("RemoveRelic", relicId).GetValue();
            GD.Print($"[{MainFile.ModId}] Removed relic: {relicId}");
        }
    }

    public static bool HasRelic(Player player, string relicId)
    {
        var relicManager = Traverse.Create(player).Field("relicManager").GetValue();
        if (relicManager == null) return false;

        return Traverse.Create(relicManager).Method("HasRelic", relicId).GetValue<bool>();
    }

    public static void AddCard(Player player, string cardId)
    {
        var deck = Traverse.Create(player).Field("deck").GetValue();
        if (deck != null)
        {
            Traverse.Create(deck).Method("AddCard", cardId).GetValue();
        }
    }
}
