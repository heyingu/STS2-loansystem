using HarmonyLib;
using Godot;
using MegaCrit.Sts2.Core.Combat;

namespace LoanSystem;

[HarmonyPatch(typeof(CombatManager), "StartCombatInternal")]
public static class CombatStartPatch
{
    [HarmonyPostfix]
    public static void Postfix()
    {
        try
        {
            var loanState = LoanState.Instance;
            if (loanState != null)
            {
                loanState.Turn3DexGrantedThisCombat = false;
                GD.Print($"[{MainFile.ModId}] Combat started, reset turn 3 dex flag.");
            }
        }
        catch (System.Exception ex)
        {
            GD.PrintErr($"[{MainFile.ModId}] Error in CombatStartPatch: {ex}");
        }
    }
}

[HarmonyPatch(typeof(CombatManager), "StartTurn")]
public static class TurnStartPatch
{
    [HarmonyPostfix]
    public static void Postfix(CombatManager __instance)
    {
        try
        {
            var loanState = LoanState.Instance;
            if (loanState == null || !loanState.HasDebt)
                return;

            int currentTurn = Traverse.Create(__instance).Field("turnNumber").GetValue<int>();
            if (currentTurn != 3)
                return;

            if (loanState.Turn3DexGrantedThisCombat)
                return;

            var player = GameAPI.GetPlayer();
            if (player != null)
            {
                Traverse.Create(player).Method("GainDexterity", 1).GetValue();
                loanState.Turn3DexGrantedThisCombat = true;

                GD.Print($"[{MainFile.ModId}] Turn 3: Granted 1 Dexterity.");
            }
        }
        catch (System.Exception ex)
        {
            GD.PrintErr($"[{MainFile.ModId}] Error in TurnStartPatch: {ex}");
        }
    }
}
