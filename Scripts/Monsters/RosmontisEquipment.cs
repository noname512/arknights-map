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
public class RosmontisEquipment : ModMonsterTemplate
{
    

    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 50, 50);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 50, 50);
    

    private int Block => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 10, 10);

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

        MoveState knock = new MoveState(
            "KNOCK",
            async targets =>
            {
                foreach (Creature c in targets)
                {
                    await PowerCmd.Apply<RingingPower>(new ThrowingPlayerChoiceContext(), c, 1, c, null);
                    await PowerCmd.Apply<ChargePower>(new ThrowingPlayerChoiceContext(), Creature, 5, Creature, null);
                }
                
            },
            [new DebuffIntent()]
        );

        MoveState block = new MoveState(
            "BLOCK",
            async targets =>
            {
                await CreatureCmd.GainBlock(Creature, Block, ValueProp.Unpowered, null);
                if (Creature.CombatState!.ContainsMonster<Rosmontis>())
                {
                    foreach (Creature c in Creature.CombatState.HittableEnemies)
                    {
                        if (c.Monster is Rosmontis)
                        {
                            await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Creature, 2, Creature, null);
                        }
                    }
                }
                
            },
            [new DefendIntent(), new BuffIntent()]
        );

        

        knock.FollowUpState = block;
        block.FollowUpState = knock;
        

        list.Add(knock);
        list.Add(block);
        
        return new MonsterMoveStateMachine(list, knock);
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
