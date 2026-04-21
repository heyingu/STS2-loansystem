using HarmonyLib;
using Godot;
using MegaCrit.Sts2.Core.Nodes.Screens.Shops;
using MegaCrit.Sts2.Core.Localization;

namespace LoanSystem;

[HarmonyPatch(typeof(NMerchantInventory), "_Ready")]
public static class ShopPatch
{
    [HarmonyPostfix]
    public static void Postfix(NMerchantInventory __instance)
    {
        try
        {
            var loanState = LoanState.Instance;
            if (loanState == null)
            {
                GD.PrintErr($"[{MainFile.ModId}] LoanState not found");
                return;
            }

            if (loanState.LoanTakenThisRun)
            {
                GD.Print($"[{MainFile.ModId}] Loan already taken, skipping button");
                return;
            }

            int ascension = GameAPI.GetAscension();

            // 尝试多种方式找到按钮容器
            Container? buttonContainer = null;

            // 方法1: 通过唯一名称查找
            buttonContainer = __instance.GetNodeOrNull<Container>("%ButtonContainer");

            // 方法2: 尝试查找任何Container子节点
            if (buttonContainer == null)
            {
                GD.Print($"[{MainFile.ModId}] ButtonContainer not found, searching for any Container");
                foreach (var child in __instance.GetChildren())
                {
                    if (child is Container container)
                    {
                        buttonContainer = container;
                        break;
                    }
                }
            }

            if (buttonContainer == null)
            {
                GD.PrintErr($"[{MainFile.ModId}] Cannot find suitable container for loan button");
                return;
            }

            var loanButton = new Button
            {
                Text = LocString("shop_ui", "LOANSYSTEM.shop.loan_button"),
                CustomMinimumSize = new Vector2(200, 50)
            };

            loanButton.Pressed += () => OnLoanButtonPressed(__instance, ascension);

            buttonContainer.AddChild(loanButton);

            GD.Print($"[{MainFile.ModId}] Loan button added to shop successfully");
        }
        catch (System.Exception ex)
        {
            GD.PrintErr($"[{MainFile.ModId}] Error in ShopPatch: {ex}");
        }
    }

    private static void OnLoanButtonPressed(NMerchantInventory shop, int ascension)
    {
        var loanState = LoanState.Instance;
        if (loanState == null || loanState.LoanTakenThisRun) return;

        ShowLoanConfirmation(shop, ascension);
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

        loanState.TakeLoan(ascension);

        var player = GameAPI.GetPlayer();
        if (player != null)
        {
            RelicHelper.AddRelic(player, LoanState.LoanRelicId);
        }

        shop.QueueRedraw();
    }

    private static string LocString(string category, string key)
    {
        return new LocString(category, key).GetFormattedText();
    }
}
