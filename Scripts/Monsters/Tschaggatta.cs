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
public class Tschaggatta : AbstractSnowyMountainMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 65, 60);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 65, 60);
    private int Damage1 => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 10, 9);
    private int Damage2 => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 17, 15);
    private int Block1 => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 10, 10);

    public override MonsterAssetProfile AssetProfile => new(VisualsScenePath: $"res://ArknightsMap/scenes/monsters/{GetType().Name}.tscn");

    public override async Task AfterAddedToRoom() { }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        List<MonsterState> list = new List<MonsterState>();
        MoveState move1 = new MoveState(
            "ATTACK",
            async targets =>
                await DamageCmd
                    .Attack(Damage1)
                    .FromMonster(this)
                    .WithAttackerAnim("Attack", 0.5f)
                    // .WithHitFx(sfx: $"event:/ArknightsMap/sfx/{GetType().Name}")
                    .Execute(null),
            new SingleAttackIntent(Damage1)
        );
        MoveState move2 = new MoveState(
            "DEBUFF",
            async targets => await PowerCmd.Apply<DexterityPower>(new ThrowingPlayerChoiceContext(), targets, -1, Creature, null),
            new DebuffIntent()
        );
        MoveState move3 = new MoveState(
            "ATTACK_DEBUFF",
            async targets =>
            {
                await DamageCmd
                    .Attack(Damage2)
                    .FromMonster(this)
                    .WithAttackerAnim("Attack", 0.5f)
                    // .WithHitFx(sfx: $"event:/ArknightsMap/sfx/{GetType().Name}")
                    .Execute(null);
                await PowerCmd.Apply<FrailPower>(new ThrowingPlayerChoiceContext(), targets, 2, Creature, null);
            },
            new SingleAttackIntent(Damage2),
            new DebuffIntent()
        );
        MoveState move4 = new MoveState(
            "BLOCK",
            async targets =>
            {
                foreach (Creature m in CombatState.Enemies)
                {
                    await CreatureCmd.GainBlock(m, Block1, ValueProp.Move, null);
                }
            },
            new DefendIntent()
        );

        ConditionalBranchState conditionalBranchState = new ConditionalBranchState("INIT");
        conditionalBranchState.AddState(move1, () => Creature.SlotName == "first");
        conditionalBranchState.AddState(move2, () => Creature.SlotName == "second");
        conditionalBranchState.AddState(move3, () => Creature.SlotName == "third");
        conditionalBranchState.AddState(move4, () => Creature.SlotName == "fourth");
        move1.FollowUpState = move2;
        move2.FollowUpState = move3;
        move3.FollowUpState = move4;
        move4.FollowUpState = move1;
        list.Add(move1);
        list.Add(move2);
        list.Add(move3);
        list.Add(move4);
        list.Add(conditionalBranchState);

        return new MonsterMoveStateMachine(list, conditionalBranchState);
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
