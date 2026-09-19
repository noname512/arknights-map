using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Powers;

[RegisterPower]
public class MyVigorPower : ModPowerTemplate
{
    private class Data
    {
        public AttackCommand? commandToModify;

        public int amountWhenAttackStarted;
    }

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

    public override PowerAssetProfile AssetProfile =>
        new(IconPath: $"res://ArknightsMap/images/powers/{GetType().Name}.png", BigIconPath: $"res://ArknightsMap/images/powers/{GetType().Name}.png");

    public override Task BeforeAttack(AttackCommand command)
    {
        if (command.Attacker != Owner)
        {
            return Task.CompletedTask;
        }
        if (!command.DamageProps.IsPoweredAttack())
        {
            return Task.CompletedTask;
        }
        Data internalData = GetInternalData<Data>();
        if (internalData.commandToModify != null)
        {
            return Task.CompletedTask;
        }
        if (command.ModelSource != null && !(command.ModelSource is CardModel))
        {
            return Task.CompletedTask;
        }
        internalData.commandToModify = command;
        internalData.amountWhenAttackStarted = Amount;
        return Task.CompletedTask;
    }

    public override decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource, CardPlay? cardPlay)
    {
        if (Owner != dealer || Target != target)
        {
            return 0m;
        }
        if (!props.IsPoweredAttack())
        {
            return 0m;
        }
        Data internalData = GetInternalData<Data>();
        if (internalData.commandToModify != null && cardSource != null && cardSource != internalData.commandToModify.ModelSource)
        {
            return 0m;
        }
        if (internalData.commandToModify != null && internalData.commandToModify.Attacker != dealer)
        {
            return 0m;
        }
        return Amount;
    }

    public override async Task AfterAttack(PlayerChoiceContext choiceContext, AttackCommand command)
    {
        Data internalData = GetInternalData<Data>();
        if (command == internalData.commandToModify)
        {
            await PowerCmd.ModifyAmount(choiceContext, this, -internalData.amountWhenAttackStarted, null, null);
        }
    }
}
