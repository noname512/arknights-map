using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Monsters;

[RegisterMonster]
public class FluffySnowball : AbstractSnowyMountainMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 91, 86);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 105, 100);

    public override MonsterAssetProfile AssetProfile => new(VisualsScenePath: $"res://ArknightsMap/scenes/monsters/{GetType().Name}.tscn");
    private int Dmg1 => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 10, 9);
    private int Dmg2 => 1;
    private int HitCount => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 6, 5);
    public int state = 0;

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
        MoveState summon = new MoveState(
            "SUMMON",
            async targets =>
            {
                await CreatureCmd.Add<Snowchild>(CombatState, "5");
            },
            new SummonIntent()
        );
        MoveState attack2 = new MoveState(
            "ATTACK2",
            async targets =>
            {
                await DamageCmd.Attack(Dmg2).WithHitCount(HitCount).FromMonster(this).Execute(null);
            },
            new MultiAttackIntent(Dmg2, HitCount)
        );

        attack1.FollowUpState = summon;
        summon.FollowUpState = attack2;
        attack2.FollowUpState = attack1;

        list.Add(attack1);
        list.Add(summon);
        list.Add(attack2);

        return new MonsterMoveStateMachine(list, attack1);
    }

    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        AnimState[] idleState = new AnimState[4];
        AnimState[] attackState = new AnimState[4];
        AnimState[] dieState = new AnimState[4];
        for (int i = 0; i < 4; i++)
        {
            idleState[i] = new AnimState("Idle" + (i > 0 ? "_" + (i + 1) : ""), isLooping: true);
            attackState[i] = new AnimState("Attack" + (i > 0 ? "_" + (i + 1) : ""));
            dieState[i] = new AnimState("Die" + (i > 0 ? "_" + (i + 1) : ""));
            attackState[i].NextState = idleState[i];
        }
        AnimState upgradeState = new AnimState("Upgrade");

        CreatureAnimator creatureAnimator = new CreatureAnimator(idleState[0], controller);
        creatureAnimator.AddAnyState("Attack", state <= 3 ? attackState[state] : attackState[3]);
        creatureAnimator.AddAnyState("Die", state <= 3 ? dieState[state] : dieState[3]);
        creatureAnimator.AddAnyState("Upgrade", upgradeState);

        return creatureAnimator;
    }
}
