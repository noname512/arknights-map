using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using ArknightsMap.Scripts.Powers;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace ArknightsMap.Scripts.Monsters;

[RegisterMonster]
public class DublinnCompanionShadowblade : ModMonsterTemplate
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 62, 58);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 62, 58);
    private int Damage1 => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 10, 9);
    private int Damage2 => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 8, 7);
    private int Damage3 => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 12, 11);
    private int UpgradeStrength => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 2, 1);
    private int InitBlock => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 15, 13);
    // 怪物场景
    public override MonsterAssetProfile AssetProfile => new(
        VisualsScenePath: $"res://ArknightsMap/scenes/monsters/{GetType().Name}.tscn"
    );

    public override async Task AfterAddedToRoom()
    {
        await PowerCmd.Apply<CompanionAtkPower>(new ThrowingPlayerChoiceContext(), Creature, 2m, Creature, null);
    }

    public override Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side != CombatSide.Player)
        {
            return Task.CompletedTask;
        }
        if (combatState.RoundNumber > 1)
        {
            return Task.CompletedTask;
        }
        return CreatureCmd.GainBlock(Creature, InitBlock, ValueProp.Unpowered, null);
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        List<MonsterState> list = new List<MonsterState>();
        int repeatCount() => Creature.HasPower<CompanionAtkPower>() ? Creature.GetPower<CompanionAtkPower>()!.Amount : 2;
        MoveState attack1 = new MoveState(
            "ATTACK1",
            async targets => await DamageCmd.Attack(Damage1).WithHitCount(repeatCount()).FromMonster(this).Execute(null),
            new MultiAttackIntent(Damage1, repeatCount)
        );
        MoveState attack2 = new MoveState(
            "ATTACK2",
            async targets => await DamageCmd.Attack(Damage2).WithHitCount(repeatCount()).FromMonster(this).Execute(null),
            new MultiAttackIntent(Damage2, repeatCount)
        );
        MoveState buff = new MoveState(
            "BUFF",
            async targets =>
            {
                await PowerCmd.Apply<CompanionAtkPower>(new ThrowingPlayerChoiceContext(), Creature, 1m, Creature, null);
                await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Creature, UpgradeStrength, Creature, null);
            },
            new BuffIntent()
        );

        MoveState singleAttack = new MoveState(
            "SINGLE_ATTACK",
            async targets => await DamageCmd.Attack(Damage3).FromMonster(this).Execute(null),
            new SingleAttackIntent(Damage3)
        );

        attack1.FollowUpState = attack2;
        attack2.FollowUpState = buff;
        buff.FollowUpState = attack1;
        singleAttack.FollowUpState = singleAttack;

        list.Add(attack1);
        list.Add(attack2);
        list.Add(buff);
        list.Add(singleAttack);

        return new MonsterMoveStateMachine(list, attack1);
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