using ArknightsMap.Scripts.Enchantments;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Combat.HealthBars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Powers;

[RegisterPower]
public class FlamingDamagePower : ModPowerTemplate, IHealthBarForecastSource
{
    // 类型，Buff或Debuff
    public override PowerType Type => PowerType.Debuff;

    // 叠加类型，Counter表示可叠加，Single表示不可叠加
    public override PowerStackType StackType => PowerStackType.Counter;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar("Bound", 20), new IntVar("ExtraDamage", 8)];
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => HoverTipFactory.FromPowerWithPowerHoverTips<VulnerablePower>();

    // 自定义图标路径。1:1即可。原版游戏大图256x256，小图64x64。
    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"res://ArknightsMap/images/powers/{GetType().Name}.png",
        BigIconPath: $"res://ArknightsMap/images/powers/{GetType().Name}.png"
    );

    public override async Task AfterTurnEndLate(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if ((Amount >= DynamicVars["Bound"].IntValue) && (side != Owner.Side))
        {
            Flash();
            await CreatureCmd.Damage(choiceContext, Owner, new DamageVar(DynamicVars["ExtraDamage"].IntValue, ValueProp.Unpowered), Owner);
            if (Owner.IsAlive)
            {
                if ((side == CombatSide.Player) && (!Owner.HasPower<VulnerablePower>()))
                {
                    await PowerCmd.Apply<VulnerablePower>(choiceContext, Owner, 2, Owner, null, false);
                }
                else
                {
                    await PowerCmd.Apply<VulnerablePower>(choiceContext, Owner, 1, Owner, null, false);
                }
                await PowerCmd.Remove(this);
            }
        }
    }
    public IEnumerable<HealthBarForecastSegment> GetHealthBarForecastSegments(HealthBarForecastContext context)
    {
        if (Amount < DynamicVars["Bound"].IntValue)
        {
            return HealthBarForecasts.Single(
                0, // 展示的数量（例如如果你的能力有2倍效果可以乘2）
                new Color(0.4f, 0.1f, 0.1f), // 颜色
                HealthBarForecastGrowthDirection.FromRight // 从左边开始延伸还是右边开始
                                                           // 0, // 顺序，越大越远离血条边缘，默认0
                                                           // PreloadManager.Cache.GetMaterial("res://xxx.tres") // 如果需要自定义材质
            );
        }

        int value = Math.Max(0, DynamicVars["ExtraDamage"].IntValue - Owner.Block);
        return HealthBarForecasts.Single(
            value, // 展示的数量（例如如果你的能力有2倍效果可以乘2）
            new Color(0.4f, 0.1f, 0.1f), // 颜色
            HealthBarForecastGrowthDirection.FromRight // 从左边开始延伸还是右边开始
                                                       // 0, // 顺序，越大越远离血条边缘，默认0
                                                       // PreloadManager.Cache.GetMaterial("res://xxx.tres") // 如果需要自定义材质
        );
    }
}