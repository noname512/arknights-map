using System.Reflection;
using ArknightsMap.Scripts.Cards;
using ArknightsMap.Scripts.Powers;
using ArknightsMap.Scripts.Utils;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Audio;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Monsters;

[RegisterMonster]
public class AllFlamesReturned : AbstractWildsMonster
{
    public override int MinInitialHp => AscensionHelper.IsDread() ? 51 : AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 72, 66);
    public override int MaxInitialHp => MinInitialHp;
    public override MonsterAssetProfile AssetProfile => new(VisualsScenePath: $"res://ArknightsMap/scenes/monsters/{GetType().Name}.tscn");

    private int P1AttackDamage => AscensionHelper.IsDread() ? 11 : 8;
    private int P1PurpleFlame => AscensionHelper.IsDread() ? 2 : 1;
    private int StrengthGain => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 2, 1);
    private int RevivePurpleFlame => AscensionHelper.IsDread() ? 6 : 3;
    private int P2AttackDamage => AscensionHelper.IsDread() ? 15 : AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 26, 23);
    private int P2PurpleFlame => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 6, 4);

    private MoveState? p1Attack;
    private MoveState? DeadState;
    private MoveState? DeadState3;
    private MoveState? p2Attack;
    private bool _isStage2 = false;
    public override bool ShouldDisappearFromDoom => _isStage2;

    public override async Task AfterAddedToRoom()
    {
        await PowerCmd.Apply<RebornPower>(new ThrowingPlayerChoiceContext(), Creature, 1, Creature, null);
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        List<MonsterState> list = new List<MonsterState>();

        p1Attack = new MoveState(
            "ATTACK_P1",
            async targets =>
            {
                await DamageCmd
                    .Attack(P1AttackDamage)
                    .FromMonster(this)
                    .WithAttackerAnim("Attack", 0.8f)
                    .WithHitFx(sfx: "event:/ArknightsMap/sfx/AllFlamesReturned/attack_1")
                    .Execute(null);
                await CardPileCmd.AddToCombatAndPreview<PurpleFlame>(targets, PileType.Hand, P1PurpleFlame, null);
                await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Creature, StrengthGain, Creature, null);
            },
            new SingleAttackIntent(P1AttackDamage),
            new CardDebuffIntent(),
            new BuffIntent()
        );

        DeadState = new MoveState(
            "RESPAWN_MOVE1",
            async _ =>
            {
                await CreatureCmd.Heal(Creature, Creature.MaxHp / 3);
            },
            new StunIntent()
        )
        {
            MustPerformOnceBeforeTransitioning = true,
        };

        MoveState DeadState2 = new MoveState(
            "RESPAWN_MOVE2",
            async _ =>
            {
                await CreatureCmd.Heal(Creature, Creature.MaxHp / 3);
            },
            new StunIntent()
        );

        DeadState3 = new MoveState("RESPAWN_MOVE3", RespawnMove, new HealIntent(), new BuffIntent(), new IgniteIntent());

        p2Attack = new MoveState(
            "ATTACK_P2",
            async targets =>
            {
                await DamageCmd
                    .Attack(P2AttackDamage)
                    .FromMonster(this)
                    .WithAttackerAnim("Attack", 0.8f)
                    .WithHitFx(sfx: "event:/ArknightsMap/sfx/AllFlamesReturned/attack_2")
                    .Execute(null);
                await ModelDb.RunModel<ReedBed>().SetBurningDurningCombat(true, CombatState);
            },
            new SingleAttackIntent(P2AttackDamage),
            new IgniteIntent()
        );
        MoveState breeth = new MoveState(
            "BREETH",
            async targets =>
            {
                await CreatureCmd.TriggerAnim(Creature, "Skill1", 0);
                SfxCmd.Play("event:/ArknightsMap/sfx/AllFlamesReturned/skill");
                await CardPileCmd.AddToCombatAndPreview<PurpleFlame>(targets, PileType.Hand, P2PurpleFlame, null);
            },
            new CardDebuffIntent()
        );

        MoveState dreadSpecialMove = new MoveState(
            "DREAD_SPECIAL",
            async targets =>
            {
                await CreatureCmd.TriggerAnim(Creature, "Skill2", 0);
                SfxCmd.Play("event:/ArknightsMap/sfx/AllFlamesReturned/skill");
                List<Task> list = new List<Task>();

                foreach (Creature target in targets)
                {
                    AttackCommand command = DamageCmd.Attack(P2AttackDamage).FromMonster(this);
                    FieldInfo field = typeof(AttackCommand).GetField("_combatState", BindingFlags.Instance | BindingFlags.NonPublic)!;
                    field.SetValue(command, null);
                    list.Add(command.Targeting(target).WithHitCount(CalcRepeatTimes(target)).WithNoAttackerAnim().Execute(null));
                    if (target.Player != null)
                    {
                        IEnumerable<CardModel> cards = target.Player.PlayerCombatState!.Hand.Cards.Where(c => c is PurpleFlame);
                        foreach (CardModel card in cards.Take(Math.Min(3, cards.Count())))
                        {
                            await CardCmd.Exhaust(new ThrowingPlayerChoiceContext(), card);
                        }
                    }
                }

                await Task.WhenAll(list);
            }
        );

        p1Attack.FollowUpState = p1Attack;
        DeadState.FollowUpState = DeadState2;
        DeadState2.FollowUpState = DeadState3;

        p2Attack.FollowUpState = breeth;
        breeth.FollowUpState = (AscensionHelper.IsDread()?dreadSpecialMove:p2Attack);
        dreadSpecialMove.FollowUpState = p2Attack;

        list.Add(p1Attack);
        list.Add(DeadState);
        list.Add(DeadState2);
        list.Add(DeadState3);

        list.Add(p2Attack);
        list.Add(breeth);

        return new MonsterMoveStateMachine(list, p1Attack);
    }

    public int CalcRepeatTimes()
    {
        Player? me = LocalContext.GetMe(CombatState);
        if (me == null)
        {
            return 1;
        }
        return CalcRepeatTimes(me.Creature);
    }

    public int CalcRepeatTimes(Creature target)
    {
        int times = 1;
        if (target.Player != null)
        {
            times += target.Player.PlayerCombatState!.Hand.Cards.Count(c => c is PurpleFlame);
        }
        return Math.Min(4, times);
    }

    public async Task TriggerDeadState()
    {
        foreach (var player in CombatState.Players)
        {
            if (player.PlayerCombatState == null)
            {
                continue;
            }
            await CardPileCmd.AddToCombatAndPreview<PurpleFlame>(player.Creature, PileType.Hand, RevivePurpleFlame, null);
        }

        SfxCmd.PlayLoop("event:/ArknightsMap/sfx/AllFlamesReturned/reborn");
        SetMoveImmediate(DeadState!, forceTransition: true);
    }

    private async Task RespawnMove(IReadOnlyList<Creature> targets)
    {
        Creature.GetPower<RebornPower>()?.DoRevive();
        await CreatureCmd.Heal(Creature, Creature.MaxHp - Creature.CurrentHp);
        bool hasPurpleFlameRemain = false;
        List<CardModel> purpleFlames = new List<CardModel>();
        SfxCmd.StopLoop("event:/ArknightsMap/sfx/AllFlamesReturned/reborn");
        foreach (var player in targets)
        {
            if (player.Player?.PlayerCombatState == null)
            {
                continue;
            }

            foreach (CardModel card in player.Player.PlayerCombatState.AllCards)
            {
                if ((card is PurpleFlame) && (card.Pile?.Type != PileType.Exhaust))
                {
                    hasPurpleFlameRemain = true;
                    purpleFlames.Add(card);
                }
            }
        }

        if (hasPurpleFlameRemain)
        {
            await CreatureCmd.TriggerAnim(Creature, "Revive1", 0);
            NextMove.FollowUpState = p1Attack;
            foreach (CardModel card in purpleFlames)
            {
                await CardCmd.Exhaust(new ThrowingPlayerChoiceContext(), card);
            }
        }
        else
        {
            _isStage2 = true;
            await CreatureCmd.TriggerAnim(Creature, "Revive2", 0);
            NRunMusicController.Instance?.PlayCustomMusic("event:/ArknightsMap/music/all_flames_returned_bat_2");
            NextMove.FollowUpState = p2Attack;
            await PowerCmd.Remove<RebornPower>(Creature);
            await PowerCmd.Remove<StrengthPower>(Creature);
            await PowerCmd.Apply<FlameBathPower>(new ThrowingPlayerChoiceContext(), Creature, AscensionHelper.IsDread() ? 75 : 50, Creature, null);
            await ModelDb.RunModel<ReedBed>().SetBurningDurningCombat(true, CombatState);
            if (AscensionHelper.IsDread())
            {
                await CreatureCmd.SetMaxAndCurrentHp(
                    Creature,
                    Creature.ScaleHpForMultiplayer(
                        153,
                        targets.First(c => c.CombatState != null).CombatState!.Encounter,
                        targets.Count(c => c.IsPlayer),
                        targets.First(c => c.CombatState?.RunState != null).CombatState!.RunState.CurrentActIndex
                    )
                );
            }
        }
    }

    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        AnimState idleState1 = new AnimState("A_Idle", isLooping: true);
        AnimState attackState1 = new AnimState("A_Attack");
        AnimState dieState1 = new AnimState("A_Die_Start");
        AnimState dieLoopState = new AnimState("A_Die_Loop");
        AnimState reviveState1 = new AnimState("A_Die_End");
        AnimState reviveState2 = new AnimState("A_Die_End_2");
        AnimState idleState2 = new AnimState("B_Idle", isLooping: true);
        AnimState attackState2 = new AnimState("B_Attack");
        AnimState skillState1 = new AnimState("B_Skill_1");
        AnimState skillState2 = new AnimState("B_Skill_1");
        AnimState dieState2 = new AnimState("B_Die");
        attackState1.NextState = idleState1;
        dieState1.NextState = dieLoopState;
        reviveState1.NextState = idleState1;
        reviveState2.NextState = idleState2;
        attackState2.NextState = idleState2;
        skillState1.NextState = idleState2;
        skillState2.NextState = idleState2;
        CreatureAnimator creatureAnimator = new CreatureAnimator(idleState1, controller);
        creatureAnimator.AddAnyState("Attack", attackState1, () => !_isStage2);
        creatureAnimator.AddAnyState("Attack", attackState2, () => _isStage2);
        creatureAnimator.AddAnyState("Dead", dieState1, () => !_isStage2);
        creatureAnimator.AddAnyState("Dead", dieState2, () => _isStage2);
        creatureAnimator.AddAnyState("Revive1", reviveState1);
        creatureAnimator.AddAnyState("Revive2", reviveState2);
        creatureAnimator.AddAnyState("Skill1", skillState1);
        creatureAnimator.AddAnyState("Skill2", skillState2);
        return creatureAnimator;
    }
}
