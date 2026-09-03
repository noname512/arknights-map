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
public class Snowchild : AbstractSnowyMountainMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 15, 13);
    public override int MaxInitialHp => MinInitialHp;

    public override MonsterAssetProfile AssetProfile => new(VisualsScenePath: $"res://ArknightsMap/scenes/monsters/{GetType().Name}.tscn");
    private int Dmg => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 4, 3);
    private int StrengthApply => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 4, 3);

    public override async Task AfterAddedToRoom()
    {
        await PowerCmd.Apply<MinionPower>(new ThrowingPlayerChoiceContext(), Creature, 1, Creature, null);
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        List<MonsterState> list = new List<MonsterState>();
        MoveState attack = new MoveState(
            "ATTACK",
            async targets =>
            {
                await DamageCmd.Attack(Dmg).FromMonster(this).Execute(null);
            },
            new SingleAttackIntent(Dmg)
        );
        MoveState skill = new MoveState(
            "SKILL",
            async targets =>
            {
                Creature? c = CombatState.Creatures.FirstOrDefault(cr => cr.Monster is FluffySnowball && cr.IsAlive);
                if (c != null)
                {
                    await CreatureCmd.Heal(c, Creature.MaxHp);
                    await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), c, StrengthApply, Creature, null);
                    if (c.Monster is FluffySnowball fluffy)
                    {
                        fluffy.state++;
                        if (fluffy.state == 3)
                        {
                            await CreatureCmd.TriggerAnim(c, "Upgrade", 0f);
                        }
                    }
                    await CreatureCmd.TriggerAnim(Creature, "Die_2", 0f);
                    await CreatureCmd.Kill(Creature);
                }
            },
            new BuffIntent()
        );

        attack.FollowUpState = skill;
        skill.FollowUpState = attack;

        list.Add(attack);
        list.Add(skill);

        return new MonsterMoveStateMachine(list, attack);
    }

    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        AnimState idleState = new AnimState("Idle", isLooping: true);
        AnimState attackState = new AnimState("Attack");
        AnimState dieState = new AnimState("Die");
        AnimState dieState2 = new AnimState("Die_2");

        attackState.NextState = idleState;

        CreatureAnimator creatureAnimator = new CreatureAnimator(idleState, controller);
        creatureAnimator.AddAnyState("Attack", attackState);
        creatureAnimator.AddAnyState("Die", dieState);
        creatureAnimator.AddAnyState("Die_2", dieState2);

        return creatureAnimator;
    }
}
