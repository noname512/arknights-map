using ArknightsMap.Scripts.Powers;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Monsters;

[RegisterMonster]
public class IcefieldHunter : AbstractSnowyMountainMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 27, 23);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 31, 27);

    public override MonsterAssetProfile AssetProfile => new(VisualsScenePath: $"res://ArknightsMap/scenes/monsters/{GetType().Name}.tscn");
    private int Dmg1 => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 8, 7);
    private int Dmg2 => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 16, 14);

    public override async Task AfterAddedToRoom()
    {
        await PowerCmd.Apply<PrecisionHunting>(new ThrowingPlayerChoiceContext(), Creature, 1, Creature, null);
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        List<MonsterState> list = new List<MonsterState>();
        MoveState attack1 = new MoveState(
            "ATTACK1",
            async targets =>
            {
                await DamageCmd.Attack(Dmg1).FromMonster(this).Execute(null);
            },
            new SingleAttackIntent(Dmg1)
        );
        MoveState attack2 = new MoveState(
            "ATTACK2",
            async targets =>
            {
                await DamageCmd.Attack(Dmg1).FromMonster(this).Execute(null);
            },
            new SingleAttackIntent(Dmg1)
        );
        MoveState charge = new MoveState("CHARGE", async targets => { }, new UnknownIntent());
        MoveState attack3 = new MoveState(
            "ATTACK3",
            async targets =>
            {
                await DamageCmd.Attack(Dmg2).WithHitCount(2).FromMonster(this).Execute(null);
            },
            new MultiAttackIntent(Dmg2, 2)
        );

        attack1.FollowUpState = attack2;
        attack2.FollowUpState = charge;
        charge.FollowUpState = attack3;
        attack3.FollowUpState = attack1;

        list.Add(attack1);
        list.Add(attack2);
        list.Add(charge);
        list.Add(attack3);

        return new MonsterMoveStateMachine(list, Creature.SlotName == "6" ? attack2 : attack1);
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
