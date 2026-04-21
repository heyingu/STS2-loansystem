using Godot;
using System;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Entities.Players;

namespace LoanSystem;

public enum LoanTier { Low, Mid, High }

public partial class LoanState : Node
{
    public static LoanState? Instance { get; private set; }

    public bool LoanTakenThisRun { get; set; } = false;
    public LoanTier LoanTier { get; set; } = LoanTier.Mid;
    public int LoanPrincipal { get; set; } = 0;
    public int LoanTotalOwed { get; set; } = 0;
    public int LoanRemaining { get; set; } = 0;
    public bool LoanDefaultChecked { get; set; } = false;
    public bool Turn3DexGrantedThisCombat { get; set; } = false;

    public static (int Principal, int TotalOwed) GetTierValues(LoanTier tier) => tier switch
    {
        LoanTier.Low => (80, 110),
        LoanTier.Mid => (100, 140),
        LoanTier.High => (120, 175),
        _ => (100, 140),
    };

    public static LoanTier GetTierForAscension(int ascension)
    {
        if (ascension <= 2) return LoanTier.Low;
        if (ascension <= 9) return LoanTier.Mid;
        return LoanTier.High;
    }

    private string _lastKnownRunId = "";

    public bool HasDebt => LoanTakenThisRun && LoanRemaining > 0;

    // Called by patches to check if we're in a new run and reset if needed
    public void CheckAndResetForNewRun()
    {
        var runManager = HarmonyLib.Traverse.Create(MegaCrit.Sts2.Core.Runs.RunManager.Instance).Field("runState").GetValue();
        if (runManager == null)
        {
            if (LoanTakenThisRun)
            {
                GD.Print($"[{MainFile.ModId}] No active run, resetting loan state.");
                ResetForNewRun();
            }
            return;
        }

        // Use the runState object's hash as a run identifier
        string currentRunId = runManager.GetHashCode().ToString();
        if (currentRunId != _lastKnownRunId)
        {
            _lastKnownRunId = currentRunId;
            if (LoanTakenThisRun)
            {
                GD.Print($"[{MainFile.ModId}] New run detected, resetting loan state.");
                ResetForNewRun();
            }
        }
    }

    public override void _Ready()
    {
        if (Instance != null)
        {
            QueueFree();
            return;
        }

        Instance = this;
        GD.Print($"[{MainFile.ModId}] LoanState initialized.");
    }

    public void TakeLoan(int ascension, Player player)
    {
        GD.Print($"[{MainFile.ModId}] TakeLoan called, ascension={ascension}");

        if (LoanTakenThisRun)
        {
            GD.PrintErr($"[{MainFile.ModId}] Loan already taken this run!");
            return;
        }

        LoanTier tier = GetTierForAscension(ascension);
        (int principal, int totalOwed) = GetTierValues(tier);

        LoanPrincipal = principal;
        LoanTotalOwed = totalOwed;
        LoanRemaining = totalOwed;
        LoanTakenThisRun = true;

        player.Gold += principal;
        GD.Print($"[{MainFile.ModId}] Loan taken: {principal} gold granted, owe {totalOwed} total. New gold: {player.Gold}");
    }

    public int ProcessGoldGain(int originalAmount)
    {
        if (!HasDebt) return originalAmount;

        int repaymentAmount = (int)(originalAmount * 0.5f);
        int actualRepayment = Math.Min(repaymentAmount, LoanRemaining);

        LoanRemaining -= actualRepayment;
        int playerReceives = originalAmount - actualRepayment;

        GD.Print($"[{MainFile.ModId}] Auto-repayment: {actualRepayment}, remaining debt: {LoanRemaining}");

        if (LoanRemaining <= 0)
        {
            OnDebtCleared();
        }

        return playerReceives;
    }

    private void OnDebtCleared()
    {
        var player = GameAPI.GetPlayer();
        if (player != null)
        {
            PotionHelper.TryGrantCommonPotion(player);
            RelicHelper.RemoveRelic(player, LoanRelicId);
        }

        GD.Print($"[{MainFile.ModId}] Debt cleared! Potion granted, IOU removed.");
    }

    public void ResetForNewRun()
    {
        LoanTakenThisRun = false;
        LoanPrincipal = 0;
        LoanTotalOwed = 0;
        LoanRemaining = 0;
        LoanDefaultChecked = false;
        Turn3DexGrantedThisCombat = false;

        GD.Print($"[{MainFile.ModId}] Loan state reset for new run.");
    }

    public const string LoanRelicId = "iou_note";
}
