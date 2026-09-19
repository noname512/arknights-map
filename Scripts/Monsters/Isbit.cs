using ArknightsMap.Scripts.Powers;
using ArknightsMap.Scripts.Utils;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Monsters;

[RegisterMonster]
public class Isbit : AbstractSnowyMountainMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 1000, 910);
    public override int MaxInitialHp => MinInitialHp;

    public override MonsterAssetProfile AssetProfile => new(VisualsScenePath: $"res://ArknightsMap/scenes/monsters/{GetType().Name}.tscn");
    private int baseDmg => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 7, 6);
    private int attackCounter = 2;

    public override async Task AfterAddedToRoom()
    {
        await PowerCmd.Apply<AvalanchePower>(new ThrowingPlayerChoiceContext(), Creature, 1, Creature, null);
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        List<MonsterState> list = new List<MonsterState>();
        MoveState attack1 = new MoveState(
            "ATTACK1",
            async targets =>
            {
                if (!CreaturePositions.IsBlock(CombatState.PlayerCreatures.FirstOrDefault()!, Creature))
                {
                    await CreatureCmd.TriggerAnim(Creature, "Move", 0.5f);
                    await CreaturePositions.Walk(Creature, -1);
                }
                await CreatureCmd.TriggerAnim(Creature, "Attack", 0.5f);
                await DamageCmd.Attack(baseDmg).FromMonster(this).WithNoAttackerAnim().Execute(null);
                attackCounter++;
            },
            new MoveIntent(),
            new SingleAttackIntent(baseDmg)
        );

        attack1.FollowUpState = attack1;

        list.Add(attack1);

        return new MonsterMoveStateMachine(list, attack1);
    }

    public override decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource, CardPlay? cardPlay)
    {
        if (dealer == Creature && props == ValueProp.Move)
        {
            return baseDmg * attackCounter;
        }
        return 0m;
    }

    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        AnimState idleState = new AnimState("B_Idle", isLooping: true);
        AnimState attackState = new AnimState("B_Attack");
        AnimState moveState = new AnimState("B_Move");
        AnimState dieState = new AnimState("B_Die");

        attackState.NextState = idleState;
        moveState.NextState = idleState;

        CreatureAnimator creatureAnimator = new CreatureAnimator(idleState, controller);
        creatureAnimator.AddAnyState("Attack", attackState);
        creatureAnimator.AddAnyState("Move", moveState);
        creatureAnimator.AddAnyState("Die", dieState);

        return creatureAnimator;
    }
}
