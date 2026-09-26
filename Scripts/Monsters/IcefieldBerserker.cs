using ArknightsMap.Scripts.Cards;
using ArknightsMap.Scripts.Powers;
using ArknightsMap.Scripts.Utils;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Monsters;

[RegisterMonster]
public class IcefieldBerserker : AbstractSnowyMountainMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 230, 217);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 250, 237);

    public override MonsterAssetProfile AssetProfile => new(VisualsScenePath: $"res://ArknightsMap/scenes/monsters/{GetType().Name}.tscn");
    private int Dmg1 => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 7, 6);
    private int Dmg2 => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 12, 10);
    private int Dmg3 => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 17, 14);

    public override async Task AfterAddedToRoom()
    {
        await PowerCmd.Apply<CloseQuartersCombatPower>(new ThrowingPlayerChoiceContext(), Creature, 12, Creature, null);
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        List<MonsterState> list = new List<MonsterState>();
        MoveState attack1 = new MoveState(
            "ATTACK1",
            async targets =>
            {
                await CreatureCmd.TriggerAnim(Creature, "Attack", 0.5f);
                await Cmd.Wait(1.0f);
                await DamageCmd.Attack(Dmg1).FromMonster(this).WithNoAttackerAnim().Execute(null);
                await CardPileCmd.AddToCombatAndPreview<Cold>(targets, PileType.Hand, 1, null);
            },
            new SingleAttackIntent(Dmg1), new StatusIntent(1)
        );
        MoveState attack2 = new MoveState(
            "ATTACK2",
            async targets =>
            {
                await CreatureCmd.TriggerAnim(Creature, "Attack", 0.5f);
                await Cmd.Wait(1.0f);
                await DamageCmd.Attack(Dmg2).FromMonster(this).WithNoAttackerAnim().Execute(null);
                await CardPileCmd.AddToCombatAndPreview<Cold>(targets, PileType.Hand, 1, null);
            },
            new SingleAttackIntent(Dmg2), new StatusIntent(1)
        );
        MoveState attack3 = new MoveState(
            "ATTACK3UNBLOCK",
            async targets =>
            {
                await CreatureCmd.TriggerAnim(Creature, "Attack", 0.5f);
                await Cmd.Wait(1.0f);
                await DamageCmd.Attack(Dmg3).FromMonster(this).WithNoAttackerAnim().Execute(null);
                await CreatureCmd.TriggerAnim(Creature, "Move", 0);
                await CreaturePositions.Walk(Creature, -1);
            },
            new SingleAttackIntent(Dmg3),
            new MoveIntent()
        );
        MoveState attack3block = new MoveState(
            "ATTACK3BLOCK",
            async targets =>
            {
                await CreatureCmd.TriggerAnim(Creature, "Attack", 0.5f);
                await Cmd.Wait(1.0f);
                await DamageCmd.Attack(Dmg3).FromMonster(this).WithNoAttackerAnim().Execute(null);
                await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Creature, 8, Creature, null);
            },
            new SingleAttackIntent(Dmg3),
            new BuffIntent()
        );

        ConditionalBranchState conditionalBranchState = new ConditionalBranchState("ATTACK3");
        conditionalBranchState.AddState(attack3, () => !CreaturePositions.IsBlock(Creature, CombatState.PlayerCreatures.First()));
        conditionalBranchState.AddState(attack3block, () => true);

        attack1.FollowUpState = attack2;
        attack2.FollowUpState = conditionalBranchState;
        attack3.FollowUpState = attack1;
        attack3block.FollowUpState = attack1;

        list.Add(attack1);
        list.Add(attack2);
        list.Add(attack3);
        list.Add(attack3block);
        list.Add(conditionalBranchState);

        return new MonsterMoveStateMachine(list, attack1);
    }

    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        AnimState idleState = new AnimState("Idle", isLooping: true);
        AnimState attackState = new AnimState("Attack");
        AnimState dieState = new AnimState("Die");
        AnimState moveState = new AnimState("Move");

        attackState.NextState = idleState;

        CreatureAnimator creatureAnimator = new CreatureAnimator(idleState, controller);
        creatureAnimator.AddAnyState("Attack", attackState);
        creatureAnimator.AddAnyState("Die", dieState);
        creatureAnimator.AddAnyState("Move", moveState);

        return creatureAnimator;
    }
}
