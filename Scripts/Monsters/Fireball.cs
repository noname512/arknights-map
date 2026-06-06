using ArknightsMap.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Monsters;

[RegisterMonster]
public class Fireball : AbstractWildsMonster
{
    public override int MinInitialHp => 99999999;
    public override int MaxInitialHp => MinInitialHp;
    public override MonsterAssetProfile AssetProfile => new(
        VisualsScenePath: $"res://ArknightsMap/scenes/monsters/{GetType().Name}.tscn"
    );

    private int ExplodeDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 25, 23);

    public override async Task AfterAddedToRoom()
    {
        Creature.HpDisplay = HpDisplay.InfiniteWithoutNumbers;
        await PowerCmd.Apply<DealFlamingDamagePower>(new ThrowingPlayerChoiceContext(), Creature, 1, Creature, null);
        await PowerCmd.Apply<DecayPower>(new ThrowingPlayerChoiceContext(), Creature, 1, Creature, null);
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        List<MonsterState> list = new List<MonsterState>();
        MoveState explode = new MoveState(
            "EXPLODE",
            async targets =>
            {
                await DamageCmd.Attack(ExplodeDamage).FromMonster(this)
                    .WithAttackerFx(null, DeathSfx)
                    .Execute(null);
                await CreatureCmd.Kill(Creature);
            },
            new DeathBlowIntent(() => ExplodeDamage)
        );

        explode.FollowUpState = explode;

        list.Add(explode);

        return new MonsterMoveStateMachine(list, explode);
    }

    public override async Task AfterDamageReceivedLate(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target == Creature)
        {
            int remainAttack = ExplodeDamage + Creature.GetPowerAmount<StrengthPower>();
            if (remainAttack <= 0)
            {
                NCombatRoom.Instance?.GetCreatureNode(Creature)?.ScaleTo(0f, 0.35);
                await CreatureCmd.Kill(Creature);
            }

            float percent = 1.0f * (ExplodeDamage - remainAttack) / ExplodeDamage;
            //percent = percent * percent;
            NCombatRoom.Instance?.GetCreatureNode(Creature)?.ScaleTo(1.0f - percent * 0.7f, 0.35);
        }
    }
}