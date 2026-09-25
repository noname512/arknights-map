using ArknightsMap.Scripts.Cards;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Monsters;

[RegisterMonster]
public class DelegationCenturion : AbstractSnowyMountainMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 114, 106);
    public override int MaxInitialHp => MinInitialHp;

    public override MonsterAssetProfile AssetProfile => new(VisualsScenePath: $"res://ArknightsMap/scenes/monsters/{GetType().Name}.tscn");
    private int Dmg1 => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 56, 42);
    private int HeaterCount => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 4, 3);

    public override async Task AfterAddedToRoom() { }

    public override async Task BeforeSideTurnStart(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState
    )
    {
        if (side == CombatSide.Player && combatState.Players.FirstOrDefault()!.PlayerCombatState!.TurnNumber <= 1)
        {
            await CardPileCmd.AddToCombatAndPreview<EmergencyHeater>(combatState.PlayerCreatures, PileType.Hand, HeaterCount, null);
        }
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        List<MonsterState> list = new List<MonsterState>();
        MoveState charge = new MoveState("CHARGE", async targets => { }, new UnknownIntent());
        MoveState attack = new MoveState(
            "ATTACK",
            async targets =>
            {
                await DamageCmd.Attack(Dmg1).FromMonster(this).WithAttackerAnim("Attack", 0.5f).Execute(null);
                foreach (Creature c in targets)
                {
                    if (c.IsPlayer)
                    {
                        Player p = c.Player!;
                        var hand = p.PlayerCombatState!.Hand.Cards;
                        List<CardModel> cards = hand.Where(c => c is EmergencyHeater).ToList();
                        if (cards.Any())
                        {
                            await CardCmd.Exhaust(new ThrowingPlayerChoiceContext(), cards.First());
                        }
                    }
                }
            },
            new SingleAttackIntent(Dmg1),
            new DebuffIntent()
        );

        attack.FollowUpState = charge;
        charge.FollowUpState = attack;

        list.Add(attack);
        list.Add(charge);

        return new MonsterMoveStateMachine(list, charge);
    }

    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        AnimState idleState = new AnimState("Idle", isLooping: true);
        AnimState attackState = new AnimState("Attack");
        AnimState dieState = new AnimState("Die");

        attackState.NextState = idleState;

        CreatureAnimator creatureAnimator = new CreatureAnimator(idleState, controller);
        creatureAnimator.AddAnyState("Attack", attackState);
        creatureAnimator.AddAnyState("Die", dieState);

        return creatureAnimator;
    }
}
