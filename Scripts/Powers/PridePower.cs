using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

[RegisterPower]
public sealed class PridePower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile =>
        new(IconPath: $"res://ArknightsMap/images/powers/{GetType().Name}.png", BigIconPath: $"res://ArknightsMap/images/powers/{GetType().Name}.png");


    public override bool ShouldScaleInMultiplayer => true;

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
        if (!props.IsPoweredAttack())
        {
            return 1m;
        }
        return 50m / 100m;
    }

    public override async Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource
    )
    {
        if (target == Owner && result.UnblockedDamage != 0 && props.IsPoweredAttack())
        {
            await PowerCmd.Decrement(this);
            if (Amount <= 0)
            {
                await CreatureCmd.TriggerAnim(Owner, "StunTrigger", 0.6f);
                string nextState = Owner.Monster!.MoveStateMachine!.StateLog.Last().GetNextState(Owner, Owner.Monster.RunRng.MonsterAi);
                await CreatureCmd.Stun(Owner, StunnedMove, nextState);
                Flash();
                await Cmd.Wait(0.25f);
            }
        }
    }

    private Task StunnedMove(IReadOnlyList<Creature> targets)
    {
        return Task.CompletedTask;
    }
}
