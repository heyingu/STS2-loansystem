using Godot;
using MegaCrit.Sts2.Core.Entities.Players;

namespace LoanSystem;

public static class PotionHelper
{
    public static void TryGrantCommonPotion(Player player)
    {
        GD.Print($"[{MainFile.ModId}] Potion grant not yet implemented");
    }
}

public static class RelicHelper
{
    public static void RemoveRelic(Player player, string relicId)
    {
        GameAPI.RemoveRelic(player, relicId);
    }

    public static void AddRelic(Player player, string relicId)
    {
        GameAPI.AddRelic(player, relicId);
    }

    public static bool HasRelic(Player player, string relicId)
    {
        return GameAPI.HasRelic(player, relicId);
    }
}

public static class CurseHelper
{
    private static readonly string[] CommonCurses =
    [
        "Clumsy", "Decay", "Doubt", "Injury", "Normality",
        "Pain", "Parasite", "Regret", "Shame", "Writhe"
    ];

    public static void AddRandomCurse(Player player)
    {
        var randomCurse = CommonCurses[GD.RandRange(0, CommonCurses.Length - 1)];
        GameAPI.AddCard(player, randomCurse);
        GD.Print($"[{MainFile.ModId}] Added random curse: {randomCurse}");
    }
}
