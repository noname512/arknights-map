using HarmonyLib;
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

[RegisterPower]
public sealed class SetTrapPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar("Upgrade", 0), new PowerVar<VulnerablePower>(1)];
    protected override IEnumerable<IHoverTip> AdditionalHoverTips
    {
        get
        {
            IEnumerable<IHoverTip> tips = [HoverTipFactory.Static(StaticHoverTip.Stun)];
            if (DynamicVars["Upgrade"].IntValue == 1)
            {
                tips.AddItem(HoverTipFactory.FromPower<VulnerablePower>());
            }
            return tips;
        }
    }

    public override PowerAssetProfile AssetProfile =>
        new(IconPath: $"res://ArknightsMap/images/powers/{GetType().Name}.png", BigIconPath: $"res://ArknightsMap/images/powers/{GetType().Name}.png");

    private HashSet<Creature> targets = [];

    public void SetUpgrade()
    {
        DynamicVars["Upgrade"].BaseValue = 1;
    }

    public override Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource
    )
    {
        if (target == Owner && dealer != null && props == ValueProp.Move && result.UnblockedDamage > 0)
        {
            targets.Add(dealer);
        }
        return Task.CompletedTask;
    }

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        foreach (Creature c in targets)
        {
            await CreatureCmd.Stun(c);
        }
        if (DynamicVars["Upgrade"].IntValue == 1)
        {
            await PowerCmd.Apply<WeakPower>(new ThrowingPlayerChoiceContext(), targets, DynamicVars.Vulnerable.IntValue, Owner, null);
        }
    }
}
