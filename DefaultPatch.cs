using HarmonyLib;
using Godot;
using MegaCrit.Sts2.Core.Nodes.RestSite;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Localization;

namespace LoanSystem;

[HarmonyPatch(typeof(NRestSiteCharacter), "_Ready")]
public static class RestSitePatch
{
    [HarmonyPostfix]
    public static void Postfix(NRestSiteCharacter __instance)
    {
        try
        {
            var loanState = LoanState.Instance;
            if (loanState == null || !loanState.HasDebt)
                return;

            if (loanState.LoanDefaultChecked)
                return;

            var runManager = RunManager.Instance;
            if (runManager == null) return;

            var runState = Traverse.Create(runManager).Field("runState").GetValue();
            if (runState == null) return;

            int currentAct = Traverse.Create(runState).Field("currentAct").GetValue<int>();
            if (currentAct != 3)
                return;

            int currentFloor = Traverse.Create(runState).Field("currentFloor").GetValue<int>();
            int bossFloor = Traverse.Create(runState).Field("bossFloor").GetValue<int>();

            if (currentFloor >= bossFloor - 1)
            {
                ExecuteDefaultCheck(__instance);
            }
        }
        catch (System.Exception ex)
        {
            GD.PrintErr($"[{MainFile.ModId}] Error in RestSitePatch: {ex}");
        }
    }

    private static void ExecuteDefaultCheck(NRestSiteCharacter restSite)
    {
        var loanState = LoanState.Instance;
        if (loanState == null) return;

        loanState.LoanDefaultChecked = true;

        if (loanState.LoanRemaining > 0)
        {
            GD.Print($"[{MainFile.ModId}] Loan default! Remaining debt: {loanState.LoanRemaining}");

            ShowDefaultMessage(restSite);

            var player = GameAPI.GetPlayer();
            if (player != null)
            {
                CurseHelper.AddRandomCurse(player);
            }
        }
        else
        {
            GD.Print($"[{MainFile.ModId}] Loan cleared before Act 3 boss, no default.");
        }
    }

    private static void ShowDefaultMessage(NRestSiteCharacter restSite)
    {
        var dialog = new AcceptDialog
        {
            Title = new LocString("rest_ui", "LOANSYSTEM.default.title").GetFormattedText(),
            DialogText = new LocString("rest_ui", "LOANSYSTEM.default.message").GetFormattedText(),
            OkButtonText = new LocString("rest_ui", "LOANSYSTEM.default.ok").GetFormattedText()
        };

        dialog.Confirmed += () => dialog.QueueFree();

        restSite.AddChild(dialog);
        dialog.PopupCentered();
    }
}
