using HarmonyLib;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;

namespace LoanSystem;

[HarmonyPatch(typeof(PlayerCmd), "GainGold")]
public static class GoldGainPatch
{
    [HarmonyPrefix]
    public static bool Prefix(ref decimal amount, Player player, bool wasStolenBack = true)
    {
        try
        {
            var loanState = LoanState.Instance;
            if (loanState == null || !loanState.HasDebt || player == null)
                return true;

            int originalAmount = (int)amount;
            int newAmount = loanState.ProcessGoldGain(originalAmount);
            amount = newAmount;

            GD.Print($"[{MainFile.ModId}] Gold gain intercepted: {originalAmount} -> {newAmount}");
            return true;
        }
        catch (System.Exception ex)
        {
            GD.PrintErr($"[{MainFile.ModId}] Error in GoldGainPatch: {ex}");
            return true;
        }
    }
}

// 暂时注释掉，等确认正确的方法名后再启用
// [HarmonyPatch(typeof(RunManager), "StartNewRun")]
// public static class NewRunPatch { ... }
