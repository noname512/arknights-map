using ArknightsMap.Scripts.Powers;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Monsters;

[RegisterMonster]
public class DelegationShieldbearer : AbstractSnowyMountainMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 208, 198);
    public override int MaxInitialHp => MinInitialHp;

    public override MonsterAssetProfile AssetProfile => new(VisualsScenePath: $"res://ArknightsMap/scenes/monsters/{GetType().Name}.tscn");
    private int Dmg1 => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 37, 26);
    private int PowerAmt => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 3, 2);

    public override async Task AfterAddedToRoom()
    {
        foreach (Creature item in Creature.CombatState!.GetOpponentsOf(Creature))
        {
            CounterpartFormPower power = (CounterpartFormPower)ModelDb.Power<CounterpartFormPower>().ToMutable();
            power.Target = item;
            await PowerCmd.Apply(new ThrowingPlayerChoiceContext(), power, item, PowerAmt, Creature, null);
        }
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        List<MonsterState> list = new List<MonsterState>();
        MoveState attack = new MoveState(
            "ATTACK",
            async targets =>
            {
                await DamageCmd.Attack(Dmg1).FromMonster(this).WithAttackerAnim("Attack", 0.5f).Execute(null);
            },
            new SingleAttackIntent(Dmg1)
        );

        attack.FollowUpState = attack;

        list.Add(attack);

        return new MonsterMoveStateMachine(list, attack);
    }

    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        AnimState idleState = new AnimState("Idle", isLooping: true);
        AnimState attackState = new AnimState("Attack");
        AnimState dieState = new AnimState("Die");

        attackState.NextState = idleState;

        CreatureAnimator creatureAnimator = new CreatureAnimator(idleState, controller);
        creatureAnimator.AddAnyState("Attack", attackState);
        creatureAnimator.AddAnyState("Die", dieState);

        return creatureAnimator;
    }
}
