# Slay the Spire 2 - Loan System Mod

一个为《杀戮尖塔 2》添加单局借贷服务的 Mod。

## 功能

- 在商店界面添加"向商人借款"按钮
- 按 Ascension 等级分三档借款额度
- 借款后获得特殊遗物"欠条"
- 每次获得金币时，50% 自动用于还债
- 持有欠条期间，每场战斗第三回合获得 1 点敏捷
- 还清债务后移除欠条，获得 1 瓶普通药水
- 第三层 Boss 前休息点若未还清，获得 1 张随机诅咒

## 借款档位

| 难度 | Ascension | 借款 | 应还 | 利息 |
|------|-----------|------|------|------|
| 低   | A0–A2     | 80   | 110  | 30   |
| 中   | A3–A9     | 100  | 140  | 40   |
| 高   | A10+      | 120  | 175  | 55   |

## 构建

### 前置条件

- .NET 9 SDK
- Godot 4.5.1（含 .NET 支持）
- Steam 版《杀戮尖塔 2》已安装

### 构建步骤

```bash
# 编译并自动部署到游戏 mods 目录
dotnet build

# 构建完整 .pck（含本地化资源）
dotnet publish
```

构建后会自动将 `LoanSystem.dll` 和 `LoanSystem.json` 复制到游戏的 `mods/LoanSystem/` 目录。

## 项目结构

```
sts2-loan-system/
├── MainFile.cs          # Mod 入口，Harmony 初始化
├── LoanState.cs         # 全局借贷状态管理
├── Helpers.cs           # 药水、遗物、诅咒辅助方法
├── ShopPatch.cs         # 商店界面补丁（借款按钮）
├── GoldPatch.cs         # 金币拦截补丁（自动还款）
├── CombatPatch.cs       # 战斗补丁（第三回合敏捷）
├── DefaultPatch.cs      # 休息点违约检查补丁
├── LoanSystem.csproj    # 项目文件
├── LoanSystem.json      # Mod 元数据
├── project.godot        # Godot 项目配置
└── LoanSystem/
    └── localization/
        ├── eng/shop_ui.json   # 英文本地化
        └── zhs/shop_ui.json   # 简体中文本地化
```

## 注意事项

由于 STS2 尚处于早期访问阶段，游戏内部 API（类名、方法名）可能随版本更新而变化。
若构建失败，请参考 [quickRestart2](https://github.com/erasels/StS2-Quick-Restart) 等社区 Mod 确认当前正确的 API 名称。
