using ArknightsMap.Scripts.Cards;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Monsters;

[RegisterMonster]
public class SupersweetieDeliveryDrone : AbstractSankta
{
    protected override int BulletMax => 0;
    protected override int InitialBullet => 0;

    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 50, 50);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 50, 50);
    private int Damage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 5, 5);
    public int Time = 1;

    // 怪物场景
    public override MonsterAssetProfile AssetProfile => new(VisualsScenePath: $"res://ArknightsMap/scenes/monsters/{GetType().Name}.tscn");

    private string GetAttackSfx() => "Attack";

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        List<MonsterState> list = new List<MonsterState>();

        MoveState attack = new MoveState(
            "ATTACK",
            async targets =>
            {
                await UseBullet(1);
                await DamageCmd.Attack(Damage).FromMonster(this).WithAttackerAnim("Attack", 0.8f).WithHitFx(sfx: GetAttackSfx()).Execute(null);
                foreach (Creature c in targets)
                {
                    await CardPileCmd.AddToCombatAndPreview<Milk>(c, PileType.Draw, 1, null, CardPilePosition.Random);
                }
            },
            new SingleAttackIntent(Damage)
        );

        attack.FollowUpState = attack;

        list.Add(attack);

        return new MonsterMoveStateMachine(list, attack);
    }

    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        AnimState idleState = new AnimState("Idle", isLooping: true);
        AnimState attackState = new AnimState("Start");
        AnimState dieState = new AnimState("Die");

        attackState.NextState = idleState;

        CreatureAnimator creatureAnimator = new CreatureAnimator(idleState, controller);
        creatureAnimator.AddAnyState("Start", attackState);
        creatureAnimator.AddAnyState("Die", dieState);
        attackState.NextState = idleState;

        return creatureAnimator;
    }
}
