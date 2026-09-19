using ArknightsMap.Scripts.Monsters;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
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
public class ShieldPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.FromPower<ArtifactPower>()];

    private int? _cachedArtifactNum;

private int GetArtifactNum()
{
    // 已缓存（Owner 已确定）直接返回
    if (_cachedArtifactNum.HasValue)
        return _cachedArtifactNum.Value;

    var num = Owner?.Monster switch
    {
        OpForGun => 2,
        OpCar    => 1,
        _        => 3
    };

    // 关键：模板克隆阶段 Owner 还是 null，此时不能缓存，
    // 否则 OpCar 会永远拿到默认值 3
    if (Owner?.Monster is not null)
        _cachedArtifactNum = num;

    return num;
}

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new IntVar("Cooldown", 2), new IntVar("CurrentCooldown", 0), new IntVar("ArtifactNum", GetArtifactNum())];

    public override int DisplayAmount => DynamicVars["CurrentCooldown"].IntValue;

    public override PowerAssetProfile AssetProfile =>
        new(IconPath: $"res://ArknightsMap/images/powers/{GetType().Name}.png", BigIconPath: $"res://ArknightsMap/images/powers/{GetType().Name}.png");

    public override decimal ModifyDamageMultiplicative(
        Creature? target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource,
        CardPlay? cardPlay
    )
    {
        if (target != Owner)
        {
            return 1m;
        }
        if (Owner.GetPowerAmount<ArtifactPower>() == 0)
        {
            return 1m;
        }
        return 0.5m;
    }

    public override async Task AfterPowerAmountChanged(
        PlayerChoiceContext choiceContext,
        PowerModel power,
        decimal amount,
        Creature? applier,
        CardModel? cardSource
    )
    {
        if (power == Owner.GetPower<ArtifactPower>() && power.Owner == Owner && amount < 0)
        {
            DynamicVars["CurrentCooldown"].BaseValue = DynamicVars["Cooldown"].BaseValue;
            InvokeDisplayAmountChanged();
        }
    }

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side == CombatSide.Enemy && Owner.GetPowerAmount<ArtifactPower>() == 0)
        {
            DynamicVars["CurrentCooldown"].UpgradeValueBy(-1);
            if (DynamicVars["CurrentCooldown"].IntValue <= 0)
            {
                await PowerCmd.Apply<ArtifactPower>(new ThrowingPlayerChoiceContext(), Owner, GetArtifactNum(), Owner, null);
                InvokeDisplayAmountChanged();
            }
        }
    }
}
