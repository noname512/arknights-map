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
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Monsters;

[RegisterMonster]
public class SanktaPride : AbstractSankta
{
    protected override int BulletMax => 0;
    protected override int InitialBullet => 0;

    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 80, 85);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 85, 90);

    // 怪物场景
    public override MonsterAssetProfile AssetProfile => new(VisualsScenePath: $"res://ArknightsMap/scenes/monsters/{GetType().Name}.tscn");

    public override async Task AfterAddedToRoom()
    {
        await PowerCmd.Apply<PridePower>(new ThrowingPlayerChoiceContext(), Creature, 4, Creature, null);

        await PowerCmd.Apply<SanktaCreaturePower>(new ThrowingPlayerChoiceContext(), Creature, 1, Creature, null);
    }

    private string GetAttackSfx() => "Attack";

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        List<MonsterState> list = new List<MonsterState>();

        MoveState sleep = new MoveState("SLEEP", async targets => { }, new SleepIntent());

        MoveState pray = new MoveState(
            "PRAY",
            async targets =>
            {
                await CreatureCmd.TriggerAnim(Creature, "Skill", 0.8f);
                foreach (Creature c in targets)
                {
                    await PowerCmd.Apply<PrayPower>(new ThrowingPlayerChoiceContext(), c, -3, c, null);
                    await PowerCmd.Apply<LoseEnergyNextTurnPower>(new ThrowingPlayerChoiceContext(), c, 1, c, null);
                }
            },
            new DebuffIntent()
        );

        MoveState skill = new MoveState(
            "SKILL",
            async targets =>
            {
                await CreatureCmd.TriggerAnim(Creature, "Skill", 0.8f);
                foreach (Creature c in CombatState.GetTeammatesOf(Creature))
                {
                    await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), c, 2, c, null);
                }
            },
            new BuffIntent()
        );

        sleep.FollowUpState = pray;
        pray.FollowUpState = skill;
        skill.FollowUpState = sleep;
        list.Add(sleep);
        list.Add(skill);
        list.Add(pray);
        return new MonsterMoveStateMachine(list, sleep);
    }

    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        AnimState idleState = new AnimState("Idle", isLooping: true);
        AnimState skillState = new AnimState("Skill");
        AnimState dieState = new AnimState("Die");

        skillState.NextState = idleState;

        CreatureAnimator creatureAnimator = new CreatureAnimator(idleState, controller);
        creatureAnimator.AddAnyState("Skill", skillState);
        creatureAnimator.AddAnyState("Die", dieState);

        return creatureAnimator;
    }
}
