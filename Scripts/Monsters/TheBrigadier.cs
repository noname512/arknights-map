using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Entities.Creatures;

namespace ArknightsMap.Scripts.Monsters;

[RegisterMonster]
public class TheBrigadier : AbstractWildsMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 200, 180);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 200, 180);
    private int Damage1 => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 16, 14);
    private int Damage2 => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 17, 15);
    private int Damage3 => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 18, 16);
    private int Damage4 => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 0, 0);
    private int StrengthGain => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 2, 2);
    public override MonsterAssetProfile AssetProfile => new(
        VisualsScenePath: $"res://ArknightsMap/scenes/monsters/{GetType().Name}.tscn"
    );
    private int SpecialMoveCounter = 0;

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        List<MonsterState> list = new List<MonsterState>();
        MoveState attack1 = new MoveState(
            "ATTACK1",
            async targets => await DamageCmd
                .Attack(Damage1)
                .FromMonster(this)
                .WithAttackerAnim("Attack", 0.5f)
                .WithHitFx(sfx: "event:/ArknightsMap/sfx/DublinnSpecOps/attack_fire")
                .Execute(null),
            new SingleAttackIntent(Damage1)
        );
        MoveState attack2 = new MoveState(
            "ATTACK2",
            async targets => await DamageCmd
                .Attack(Damage2)
                .FromMonster(this)
                .WithAttackerAnim("Attack", 0.5f)
                .WithHitFx(sfx: "event:/ArknightsMap/sfx/DublinnSpecOps/attack_fire")
                .Execute(null),
            new SingleAttackIntent(Damage2)
        );
        MoveState attack3 = new MoveState(
            "ATTACK3",
            async targets => await DamageCmd
                .Attack(Damage3)
                .FromMonster(this)
                .WithAttackerAnim("Attack", 0.5f)
                .WithHitFx(sfx: "event:/ArknightsMap/sfx/DublinnSpecOps/attack_fire")
                .Execute(null),
            new SingleAttackIntent(Damage3)
        );
        MoveState attack4 = GenerateSpecialMoveState();

        attack1.FollowUpState = attack2;
        attack2.FollowUpState = attack3;
        attack3.FollowUpState = attack4;
        attack4.FollowUpState = attack1;

        list.Add(attack1);
        list.Add(attack2);
        list.Add(attack3);
        list.Add(attack4);

        return new MonsterMoveStateMachine(list, attack4);
    }

    public MoveState GenerateSpecialMoveState() => new MoveState(
        "SPECIAL_MOVE" + SpecialMoveCounter++,
        SpecialMove,
        new MultiAttackIntent(Damage4, 5),
        new BuffIntent(),
        new IgniteIntent()
    );

    public async Task SpecialMove(IEnumerable<Creature> targets)
    {
        await DamageCmd
            .Attack(Damage4)
            .WithHitCount(5)
            .FromMonster(this)
            .WithAttackerAnim("Skill", 0.5f)
            .WithHitFx(sfx: "event:/ArknightsMap/sfx/DublinnSpecOps/attack")
            .OnlyPlayAnimOnce()
            .Execute(null);
        await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Creature, StrengthGain, Creature, null);
        await Entry.reedBed.SetBurningDurningCombat(true, CombatState);
    }

    public override async Task OnReedBedStatusChange(bool burning)
    {
        if (!NextMove.Id.StartsWith("SPECIAL_MOVE"))
        {
            MoveState newState = GenerateSpecialMoveState();
            foreach (var (k, v) in MoveStateMachine!.States)
            {
                if (v is not MoveState) continue;
                MoveState moveState = (MoveState)v;
                if (moveState.FollowUpState!.Id == NextMove.Id)
                {
                    moveState.FollowUpState = newState;
                }
            }
            newState.FollowUpState = NextMove.FollowUpState;
            newState.RegisterStates(MoveStateMachine.States);
            SetMoveImmediate(newState);
        }
    }

    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        AnimState idleState = new AnimState("Idle", isLooping: true);
        AnimState attackState = new AnimState("Attack");
        AnimState dieState = new AnimState("Die");
        AnimState skillState = new AnimState("Skill");
        attackState.NextState = idleState;
        CreatureAnimator creatureAnimator = new CreatureAnimator(idleState, controller);
        creatureAnimator.AddAnyState("Attack", attackState);
        creatureAnimator.AddAnyState("Dead", dieState);
        creatureAnimator.AddAnyState("Skill", skillState);
        return creatureAnimator;
    }
}