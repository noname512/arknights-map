using ArknightsMap.Scripts.Powers;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Monsters;

[RegisterMonster]
public class Rosmontis : ModMonsterTemplate
{
    

    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 120, 120);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 120, 120);
    private int Damage1 => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 15, 18);

    private int Damage2 => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 10, 12);

    private int Damage3 => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 10, 10);

    public override async Task AfterAddedToRoom()
    {
        
    }

    public int MoveInt = 0;

    // 怪物场景
    public override MonsterAssetProfile AssetProfile => new(VisualsScenePath: $"res://ArknightsMap/scenes/monsters/{GetType().Name}.tscn");

    private string GetAttackSfx() => "Attack";

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        List<MonsterState> list = new List<MonsterState>();

        MoveState attack_summon = new MoveState(
            "ATTACK_SUMMON",
            async targets =>
            {
                await DamageCmd.Attack(Damage1).FromMonster(this).WithAttackerAnim("Attack", 0.8f).WithHitFx(sfx: GetAttackSfx()).Execute(null);
                
            },
            [new SingleAttackIntent(Damage1), new SummonIntent()]
        );

        MoveState attack_charge = new MoveState(
            "ATTACK_CHARGE",
            async targets =>
            {
                await DamageCmd.Attack(Damage2).FromMonster(this).WithAttackerAnim("Attack", 0.8f).WithHitFx(sfx: GetAttackSfx()).Execute(null);
                
            },
            [new SingleAttackIntent(Damage2), new DefendIntent()]
        );

        MoveState attack_debuff = new MoveState(
            "ATTACK_DEBUFF",
            async targets =>
            {
                await DamageCmd.Attack(Damage3).WithHitCount(3).FromMonster(this).WithAttackerAnim("Attack", 0.8f).WithHitFx(sfx: GetAttackSfx()).Execute(null);
                await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), Creature, 10, ValueProp.Unpowered, Creature);
            },
            [new MultiAttackIntent(Damage3, 3)]
        );

        attack_summon.FollowUpState = attack_charge;
        attack_charge.FollowUpState = attack_debuff;
        attack_debuff.FollowUpState = attack_summon;

        list.Add(attack_summon);
        list.Add(attack_charge);
        list.Add(attack_debuff);
        return new MonsterMoveStateMachine(list, attack_summon);
    }

    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        AnimState startState = new AnimState("Start");
        AnimState idleState = new AnimState("Idle", isLooping: true);
        AnimState attackState = new AnimState("Attack");
        AnimState skillState = new AnimState("Skill");

        AnimState dieState = new AnimState("Die");

        attackState.NextState = idleState;
        skillState.NextState = idleState;
        startState.NextState = idleState;

        CreatureAnimator creatureAnimator = new CreatureAnimator(startState, controller);
        creatureAnimator.AddAnyState("Attack", attackState);
        creatureAnimator.AddAnyState("Skill", skillState);
        creatureAnimator.AddAnyState("Start", startState);
        creatureAnimator.AddAnyState("Die", dieState);

        return creatureAnimator;
    }
}
