using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Entities.Players;

namespace LoanSystem;

/// <summary>
/// 游戏API封装，使用Harmony Traverse访问私有成员
/// </summary>
public static class GameAPI
{
    public static Player? GetPlayer()
    {
        var runManager = RunManager.Instance;
        if (runManager == null) return null;

        var runState = Traverse.Create(runManager).Field("runState").GetValue();
        if (runState == null) return null;

        return Traverse.Create(runState).Field("player").GetValue() as Player;
    }

    public static int GetAscension()
    {
        var runManager = RunManager.Instance;
        if (runManager == null) return 0;

        var runState = Traverse.Create(runManager).Field("runState").GetValue();
        if (runState == null) return 0;

        return Traverse.Create(runState).Field("ascension").GetValue<int>();
    }

    public static void GainGold(Player player, int amount, bool triggerRelics = false)
    {
        Traverse.Create(player).Method("GainGold", amount, triggerRelics).GetValue();
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
