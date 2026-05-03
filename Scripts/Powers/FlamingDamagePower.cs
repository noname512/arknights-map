using ArknightsMap.Scripts.Enchantments;
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
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Powers;

[RegisterPower]
public class FlamingDamagePower : ModPowerTemplate
{
    // 类型，Buff或Debuff
    public override PowerType Type => PowerType.Debuff;
    // 叠加类型，Counter表示可叠加，Single表示不可叠加
    public override PowerStackType StackType => PowerStackType.Counter;
    protected override IEnumerable<DynamicVar> CanonicalVars => []; // 20 和 8 不应该是写死的数字
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => HoverTipFactory.FromPowerWithPowerHoverTips<VulnerablePower>();

    // 自定义图标路径。1:1即可。原版游戏大图256x256，小图64x64。
    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://Test/images/powers/test_power.png",
        BigIconPath: "res://Test/images/powers/test_power.png"
    );
    
    public override Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, CombatState combatState)
    {
        if (Amount >= 20)
        {
            Flash();
            CreatureCmd.Damage(choiceContext, Owner, new DamageVar(8, ValueProp.Unpowered), Owner);
            PowerCmd.Apply<VulnerablePower>(Owner, 1, Owner, null, false);
            PowerCmd.Remove(this);
        }
        return Task.CompletedTask;
    }
}