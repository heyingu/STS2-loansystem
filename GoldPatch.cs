using HarmonyLib;
using Godot;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Runs;

namespace LoanSystem;

/// <summary>
/// 金币获得拦截补丁：自动还款
/// </summary>
[HarmonyPatch(typeof(Player), "GainGold")]
public static class GoldGainPatch
{
    [HarmonyPrefix]
    public static bool Prefix(Player __instance, ref int amount, bool triggerRelics = true)
    {
        try
        {
            var loanState = LoanState.Instance;
            if (loanState == null || !loanState.HasDebt)
                return true; // 无债务，正常执行

            // 拦截金币收益，执行自动还款
            int originalAmount = amount;
            amount = loanState.ProcessGoldGain(amount);

            GD.Print($"[{MainFile.ModId}] Gold gain intercepted: {originalAmount} -> {amount}");

            return true; // 继续执行原方法，但金额已被修改
        }
        catch (System.Exception ex)
        {
            GD.PrintErr($"[{MainFile.ModId}] Error in GoldGainPatch: {ex}");
            return true;
        }
    }
}

/// <summary>
/// 新局开始时重置借贷状态
/// </summary>
[HarmonyPatch(typeof(RunManager), "StartNewRun")]
public static class NewRunPatch
{
    [HarmonyPostfix]
    public static void Postfix()
    {
        try
        {
            var loanState = LoanState.Instance;
            if (loanState != null)
            {
                loanState.ResetForNewRun();
                GD.Print($"[{MainFile.ModId}] Loan state reset for new run.");
            }
        }
        catch (System.Exception ex)
        {
            GD.PrintErr($"[{MainFile.ModId}] Error in NewRunPatch: {ex}");
        }
    }
}
