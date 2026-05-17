using ArknightsMap.Scripts.Monsters;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Powers;

[RegisterPower]
public class CaughtOutPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
    protected override IEnumerable<DynamicVar> CanonicalVars => [];
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => HoverTipFactory.FromPowerWithPowerHoverTips<FlamingDamagePower>();

    // 自定义图标路径。1:1即可。原版游戏大图256x256，小图64x64。
    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://Test/images/powers/test_power.png",
        BigIconPath: "res://Test/images/powers/test_power.png"
    );

    public override async Task AfterDeath(PlayerChoiceContext choiceContext, Creature target, bool wasRemovalPrevented, float deathAnimLength)
    {
        if (wasRemovalPrevented || target != base.Owner) return;
        await SummonSeed();
    }

    public override async Task BeforeTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (side != base.Owner.Side) return;
        if (base.Owner.CombatState.RoundNumber >= AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 6, 5))
        {
            await SummonSeed();
        }
    }

    public async Task SummonSeed()
    {
        GD.Print("[CaughtOutPower] SummonSeed started");
        try
        {
            for (int i = 0; i < 3; i++)
            {
                GD.Print($"[CaughtOutPower] Creating seed {i + 1}");
                await CreatureCmd.Add<CabbageSeedling>(base.CombatState, $"seed{i + 1}");
                GD.Print($"[CaughtOutPower] Created seed {i + 1}");
            }
            GD.Print("[CaughtOutPower] All seeds created, killing owner");
            if (base.Owner.IsAlive) await CreatureCmd.Kill(base.Owner);
            GD.Print("[CaughtOutPower] Owner killed");
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[CaughtOutPower] Error in SummonSeed: {ex}");
        }
    }
}