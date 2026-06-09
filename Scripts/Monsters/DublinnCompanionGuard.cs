using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using ArknightsMap.Scripts.Powers;

namespace ArknightsMap.Scripts.Monsters;

[RegisterMonster]
public class DublinnCompanionGuard : AbstractWildsMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 62, 58);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 62, 58);
    private int Damage1 => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 18, 16);
    private int Damage2 => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 12, 11);
    private int Block1 => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 15, 13);
    private int Block2 => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 30, 27);
    private int Block3 => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 12, 11);
    // 怪物场景
    public override MonsterAssetProfile AssetProfile => new(
        VisualsScenePath: $"res://ArknightsMap/scenes/monsters/{GetType().Name}.tscn"
    );

    public override async Task AfterAddedToRoom()
    {
        await PowerCmd.Apply<CompanionDefPower>(new ThrowingPlayerChoiceContext(), Creature, AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 6, 5), Creature, null);
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        List<MonsterState> list = new List<MonsterState>();
        MoveState defend1 = new MoveState(
            "DEFEND1",
            async targets =>
            {
                foreach (var monster in CombatState.Enemies.Where(m => m.IsAlive))
                {
                    await CreatureCmd.GainBlock(monster, Block1, ValueProp.Move, null);
                }
            },
            new DefendIntent()
        );
        MoveState defend2 = new MoveState(
            "DEFEND2",
            async targets =>
            {
                foreach (var monster in CombatState.Enemies.Where(m => m.IsAlive && m.Monster is DublinnCompanionShadowblade))
                {
                    await CreatureCmd.GainBlock(monster, Block2, ValueProp.Move, null);
                }
            },
            new DefendIntent()
        );
        MoveState attack = new MoveState(
            "ATTACK",
            async targets =>
            {
                await DamageCmd
                    .Attack(Damage1)
                    .FromMonster(this)
                    .WithAttackerAnim("Attack", 0.6f)
                    .WithHitFx(sfx: $"event:/ArknightsMap/sfx/{GetType().Name}")
                    .Execute(null);
                await CreatureCmd.GainBlock(Creature, Block3, ValueProp.Move, null);
            },
            new SingleAttackIntent(Damage1),
            new DefendIntent()
        );

        MoveState singleAttack = new MoveState(
            "SINGLE_ATTACK",
            async targets => await DamageCmd
                .Attack(Damage2)
                .FromMonster(this)
                .WithAttackerAnim("Attack", 0.6f)
                .WithHitFx(sfx: $"event:/ArknightsMap/sfx/{GetType().Name}")
                .Execute(null),
            new SingleAttackIntent(Damage2)
        );

        defend1.FollowUpState = defend2;
        defend2.FollowUpState = attack;
        attack.FollowUpState = defend1;
        singleAttack.FollowUpState = singleAttack;

        list.Add(defend1);
        list.Add(defend2);
        list.Add(attack);
        list.Add(singleAttack);

        return new MonsterMoveStateMachine(list, defend1);
    }

    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        AnimState idleState = new AnimState("Idle", isLooping: true);
        AnimState attackState = new AnimState("Attack");
        AnimState dieState = new AnimState("Die");
        attackState.NextState = idleState;
        CreatureAnimator creatureAnimator = new CreatureAnimator(idleState, controller);
        creatureAnimator.AddAnyState("Attack", attackState);
        creatureAnimator.AddAnyState("Dead", dieState);
        return creatureAnimator;
    }
}