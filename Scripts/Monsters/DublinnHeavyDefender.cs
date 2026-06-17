using ArknightsMap.Scripts.Powers;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Monsters;

[RegisterMonster]
public class DublinnHeavyDefender : AbstractWildsMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 50, 46);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 50, 46);

    public int StrengthGain = 2;

    // 怪物场景
    public override MonsterAssetProfile AssetProfile => new(VisualsScenePath: $"res://ArknightsMap/scenes/monsters/{GetType().Name}.tscn");

    public override async Task AfterAddedToRoom()
    {
        foreach (Creature c in CombatState.GetOpponentsOf(Creature))
        {
            PerseverePower p = (PerseverePower)ModelDb.Power<PerseverePower>().ToMutable();
            p.Target = c;
            await PowerCmd.Apply(new ThrowingPlayerChoiceContext(), p, Creature, 1m, Creature, null);
        }
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        List<MonsterState> list = new List<MonsterState>();
        int[] Damage = { 7, 8, 9, 10, 11, 12, 13 };
        MoveState[] attacks = new MoveState[7];
        Func<int, MoveState> f = i =>
        {
            return new MoveState(
                "ATTACK" + i,
                async targets =>
                {
                    await DamageCmd
                        .Attack(Damage[i])
                        .FromMonster(this)
                        .WithAttackerAnim("Attack", 0.5f)
                        .WithHitFx(sfx: $"event:/ArknightsMap/sfx/{GetType().Name}")
                        .Execute(null);
                    await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Creature, StrengthGain, Creature, null);
                },
                new SingleAttackIntent(Damage[i]),
                new BuffIntent()
            );
        };
        for (int i = 0; i < 7; i++)
        {
            attacks[i] = f(i);
            list.Add(attacks[i]);
        }

        RandomBranchState[] randomStates = new RandomBranchState[7];
        for (int i = 0; i < 7; i++)
        {
            randomStates[i] = new RandomBranchState("RANDOM" + i);
            for (int j = i; j < 7; j++)
                randomStates[i].AddBranch(attacks[j], MoveRepeatType.CanRepeatForever);
            list.Add(randomStates[i]);
        }

        if (RunManager.Instance.HasAscension(AscensionLevel.ToughEnemies))
        {
            for (int i = 0; i < 5; i++)
                attacks[i].FollowUpState = randomStates[i + 1];
            for (int i = 5; i < 7; i++)
                attacks[i].FollowUpState = randomStates[0];
        }
        else
        {
            for (int i = 0; i < 7; i++)
                attacks[i].FollowUpState = randomStates[0];
        }

        return new MonsterMoveStateMachine(list, randomStates[0]);
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
