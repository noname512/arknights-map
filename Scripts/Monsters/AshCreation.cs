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
public class AshCreation : AbstractWildsMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 40, 35);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 45, 40);
    private int Damage1 => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 15, 13);

    public override MonsterAssetProfile AssetProfile => new(VisualsScenePath: $"res://ArknightsMap/scenes/monsters/{GetType().Name}.tscn");

    public override async Task AfterAddedToRoom()
    {
        if (CombatState.ContainsMonster<TreeShield>())
        {
            await CreatureCmd.TriggerAnim(Creature, "Idle", 0);
            SetMoveImmediate((MoveState)MoveStateMachine!.States["ATTACK"]);
        }
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        List<MonsterState> list = new List<MonsterState>();
        MoveState attack = new MoveState(
            "ATTACK",
            async targets =>
                await DamageCmd
                    .Attack(Damage1)
                    .FromMonster(this)
                    .WithHitCount(2)
                    .WithAttackerAnim("Attack", 0.5f)
                    .WithHitFx(sfx: $"event:/ArknightsMap/sfx/{GetType().Name}")
                    .OnlyPlayAnimOnce()
                    .Execute(null),
            new MultiAttackIntent(Damage1, 2)
        );
        MoveState stun = new MoveState("STUN", async targets => { }, new StunIntent());
        MoveState wait = new MoveState("WAIT", async targets => { }, new UnknownIntent());

        attack.FollowUpState = attack;
        stun.FollowUpState = stun;
        wait.FollowUpState = attack;

        list.Add(attack);
        list.Add(stun);
        list.Add(wait);

        return new MonsterMoveStateMachine(list, stun);
    }

    public override async Task AfterDeath(PlayerChoiceContext choiceContext, Creature target, bool wasRemovalPrevented, float deathAnimLength)
    {
        if (target.IsMonster && target.Monster is TreeShield)
        {
            if (CombatState.Enemies.Count(e => e.IsAlive && e.Monster is TreeShield) == 0)
            {
                await CreatureCmd.TriggerAnim(Creature, "Idle2", 0);
                SetMoveImmediate((MoveState)MoveStateMachine!.States["STUN"]);
            }
        }
    }

    public override async Task AfterCreatureAddedToCombat(Creature creature)
    {
        if (creature.Monster is TreeShield)
        {
            await CreatureCmd.TriggerAnim(Creature, "Idle", 0);
            SetMoveImmediate((MoveState)MoveStateMachine!.States["WAIT"]);
        }
    }

    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        AnimState idleState = new AnimState("Idle", isLooping: true);
        AnimState attackState = new AnimState("Attack");
        AnimState dieState = new AnimState("Die");
        AnimState idleState2 = new AnimState("A_Idle", isLooping: true);
        AnimState dieState2 = new AnimState("A_Die");
        attackState.NextState = idleState;
        CreatureAnimator creatureAnimator = new CreatureAnimator(idleState2, controller);
        creatureAnimator.AddAnyState("Attack", attackState);
        creatureAnimator.AddAnyState("Dead", dieState, () => NextMove.Id == "ATTACK");
        creatureAnimator.AddAnyState("Dead", dieState2, () => NextMove.Id == "STUN");
        creatureAnimator.AddAnyState("Idle", idleState);
        creatureAnimator.AddAnyState("Idle2", idleState2);
        return creatureAnimator;
    }
}
