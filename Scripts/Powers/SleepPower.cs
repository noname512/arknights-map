using ArknightsMap.Scripts.Powers;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Scaffolding.Content;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class SleepPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource
    )
    {
        if (target == Owner && result.UnblockedDamage != 0)
        {
            if (Owner.HasPower<DamageOutPower>())
            {
                await PowerCmd.Remove(Owner.GetPower<DamageOutPower>());
            }
            if (Owner.HasPower<PlatingPower>())
            {
                await PowerCmd.Remove(Owner.GetPower<PlatingPower>());
            }
            await CreatureCmd.TriggerAnim(Owner, "Awake", 0.6f);
            await CreatureCmd.Stun(Owner, "ATTACK1");
            await PowerCmd.Remove(this);
        }
    }

    public override async Task BeforeSideTurnEndVeryEarly(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (participants.Contains(Owner) && Amount <= 1 && Owner.HasPower<PlatingPower>())
        {
            await PowerCmd.Remove(Owner.GetPower<PlatingPower>());
        }
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (participants.Contains(Owner))
        {
            await PowerCmd.Decrement(this);
            if (Amount <= 0)
            {
                if (Owner.HasPower<DamageOutPower>())
                {
                    await PowerCmd.Remove(Owner.GetPower<DamageOutPower>());
                }
                await CreatureCmd.TriggerAnim(Owner, "Awake", 0.6f);
            }
        }
    }
}
