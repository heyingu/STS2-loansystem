using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;
using System.Reflection;

namespace LoanSystem;

[ModInitializer(nameof(Initialize))]
public partial class MainFile : Node
{
    public const string ModId = "LoanSystem";
    public static Harmony? HarmonyInstance { get; private set; }

    public static void Initialize()
    {
        GD.Print($"[{ModId}] Initializing Loan System Mod...");

        // 应用Harmony补丁
        HarmonyInstance = new Harmony(ModId);
        HarmonyInstance.PatchAll(Assembly.GetExecutingAssembly());

        // 创建LoanState单例节点
        var loanState = new LoanState();
        loanState.Name = "LoanState";

        // 将LoanState添加到场景树根节点
        var sceneTree = Engine.GetMainLoop() as SceneTree;
        if (sceneTree != null)
        {
            sceneTree.Root.CallDeferred("add_child", loanState);
            GD.Print($"[{ModId}] LoanState node added to scene tree");
        }

        GD.Print($"[{ModId}] Loan System Mod initialized successfully!");
    }
}
