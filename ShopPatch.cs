using HarmonyLib;
using Godot;
using MegaCrit.Sts2.Core.Nodes.Screens.Shops;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using System.Linq;
using System.Collections.Generic;

namespace LoanSystem;

[HarmonyPatch(typeof(NMerchantInventory), "_Ready")]
public static class ShopPatch
{
    [HarmonyPostfix]
    public static void Postfix(NMerchantInventory __instance)
    {
        // 使用 GetTree().CreateTimer 延迟执行，确保子节点初始化完毕
        var timer = __instance.GetTree().CreateTimer(0.1);
        timer.Timeout += () => AddLoanButtonDeferred(__instance);
    }

    private static void AddLoanButtonDeferred(NMerchantInventory __instance)
    {
        try
        {
            // 打印所有子节点，帮助找到正确容器
            GD.Print($"[{MainFile.ModId}] === Shop node children ===");
            PrintChildren(__instance, 0);

            var loanState = LoanState.Instance;
            if (loanState == null)
            {
                GD.PrintErr($"[{MainFile.ModId}] LoanState not found");
                return;
            }

            // Check if we're in a new run and reset if needed
            loanState.CheckAndResetForNewRun();

            if (loanState.LoanTakenThisRun)
            {
                GD.Print($"[{MainFile.ModId}] Loan already taken, skipping button");
                return;
            }

            int ascension = GameAPI.GetAscension();

            // 直接把按钮加到 SlotsContainer 下
            var slotsContainer = __instance.GetNodeOrNull<Control>("SlotsContainer");
            if (slotsContainer == null)
            {
                GD.PrintErr($"[{MainFile.ModId}] SlotsContainer not found");
                return;
            }

            // 防止按钮重复添加
            if (slotsContainer.HasNode("LoanButton"))
            {
                GD.Print($"[{MainFile.ModId}] LoanButton already exists, skipping");
                return;
            }

            var loanButton = new Button
            {
                Name = "LoanButton",
                Text = LocString("shop_ui", "LOANSYSTEM.shop.loan_button"),
                CustomMinimumSize = new Vector2(200, 50),
                Position = new Vector2(20, 500)
            };

            loanButton.Pressed += () => OnLoanButtonPressed(__instance, ascension);
            slotsContainer.AddChild(loanButton);

            GD.Print($"[{MainFile.ModId}] Loan button added to SlotsContainer successfully");
        }
        catch (System.Exception ex)
        {
            GD.PrintErr($"[{MainFile.ModId}] Error in AddLoanButtonDeferred: {ex}");
        }
    }

    private static void PrintChildren(Node node, int depth)
    {
        foreach (var child in node.GetChildren())
        {
            if (child is Node childNode)
            {
                string indent = new string(' ', depth * 2);
                GD.Print($"[{MainFile.ModId}] {indent}{childNode.Name} ({childNode.GetType().Name})");
                if (depth < 3) // 只打印3层深度
                    PrintChildren(childNode, depth + 1);
            }
        }
    }

    private static void OnLoanButtonPressed(NMerchantInventory shop, int ascension)
    {
        var loanState = LoanState.Instance;
        if (loanState == null || loanState.LoanTakenThisRun) return;

        // 暂时跳过确认弹窗，直接借款
        ExecuteLoan(shop, ascension);
    }

    private static void ShowLoanConfirmation(NMerchantInventory shop, int ascension)
    {
        var tier = LoanState.GetTierForAscension(ascension);
        var (principal, totalOwed) = LoanState.GetTierValues(tier);

        var dialog = new AcceptDialog
        {
            Title = LocString("shop_ui", "LOANSYSTEM.shop.confirm_title"),
            DialogText = string.Format(
                LocString("shop_ui", "LOANSYSTEM.shop.confirm_text"),
                principal, totalOwed, totalOwed - principal
            ),
            OkButtonText = LocString("shop_ui", "LOANSYSTEM.shop.confirm_accept")
        };

        dialog.Confirmed += () =>
        {
            ExecuteLoan(shop, ascension);
            dialog.QueueFree();
        };

        shop.AddChild(dialog);
        dialog.PopupCentered();
    }

    private static void ExecuteLoan(NMerchantInventory shop, int ascension)
    {
        var loanState = LoanState.Instance;
        if (loanState == null) return;

        // 直接从商店的 Inventory 拿 Players
        var inventory = shop.Inventory;
        if (inventory == null)
        {
            GD.PrintErr($"[{MainFile.ModId}] Inventory is null");
            return;
        }

        var players = Traverse.Create(inventory).Property("Players").GetValue<IEnumerable<Player>>()
                      ?? Traverse.Create(inventory).Field("_players").GetValue<IEnumerable<Player>>();

        var player = players?.FirstOrDefault();
        if (player == null)
        {
            GD.PrintErr($"[{MainFile.ModId}] Player is null from Inventory");
            return;
        }

        GD.Print($"[{MainFile.ModId}] Got player, gold before: {player.Gold}");
        loanState.TakeLoan(ascension, player);
        GD.Print($"[{MainFile.ModId}] Gold after loan: {player.Gold}");

        shop.QueueRedraw();
    }

    private static string LocString(string category, string key) => key switch
    {
        "LOANSYSTEM.shop.loan_button" => "向商人借款",
        "LOANSYSTEM.shop.confirm_title" => "借款确认",
        "LOANSYSTEM.shop.confirm_text" => "立即获得 {0} 金币，需偿还 {1} 金币（利息 {2} 金币）。\n每次获得金币时50%自动还款。\n持有期间每场战斗第三回合获得1点敏捷。\n第三层Boss前未还清则获得随机诅咒。",
        "LOANSYSTEM.shop.confirm_accept" => "接受借款",
        "LOANSYSTEM.shop.confirm_cancel" => "取消",
        _ => key
    };
}
