using ArknightsMap.Scripts.Cards;
using HarmonyLib;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Monsters;

[RegisterMonster]
public class TheSaint : AbstractSankta
{
    protected override int BulletMax => 0;
    protected override int InitialBullet => 0;

    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.DoubleBoss, 500, 500);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.DoubleBoss, 500, 500);

    private int heavyAttackPhase1 => AscensionHelper.GetValueIfAscension(AscensionLevel.DoubleBoss, 40, 40);

    private int heavyAttackPhase2 => AscensionHelper.GetValueIfAscension(AscensionLevel.DoubleBoss, 45, 45);
    private int multiAttackPhase1 => AscensionHelper.GetValueIfAscension(AscensionLevel.DoubleBoss, 3, 3);
    private int multiAttackPhase2 => AscensionHelper.GetValueIfAscension(AscensionLevel.DoubleBoss, 4, 4);
    private int summonNumPhase1 => AscensionHelper.GetValueIfAscension(AscensionLevel.DoubleBoss, 1, 1);
    private int summonNumPhase2 => AscensionHelper.GetValueIfAscension(AscensionLevel.DoubleBoss, 2, 2);

    private int debuffAttackPhase1 => AscensionHelper.GetValueIfAscension(AscensionLevel.DoubleBoss, 10, 10);
    private int debuffAttackPhase2 => AscensionHelper.GetValueIfAscension(AscensionLevel.DoubleBoss, 12, 12);

    private int block => AscensionHelper.GetValueIfAscension(AscensionLevel.DoubleBoss, 10, 10);

    private int Phase = 1;

    private bool ShouldPreventDamage = true;

    private bool HasStun = false;

    // 怪物场景
    public override MonsterAssetProfile AssetProfile => new(VisualsScenePath: $"res://ArknightsMap/scenes/monsters/{GetType().Name}.tscn");

    public override async Task AfterAddedToRoom()
    {
        foreach (Player p in CombatState.Players)
        {
            int num = p.PlayerCombatState!.DrawPile.Cards.Count;
            foreach (var card in p.PlayerCombatState.DrawPile.Cards.TakeRandom(num / 2, p.RunState.Rng.CombatCardSelection))
            {
                await CardCmd.Exhaust(new ThrowingPlayerChoiceContext(), card);
            }
        }
    }

    public override decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource, CardPlay? cardPlay)
{
    if (target != Creature) 
    return base.ModifyDamageAdditive(target, amount, props, dealer, cardSource, cardPlay);
    
    // 只在 Phase 1 且还没触发转阶段时锁血
    if (Phase != 1 || !ShouldPreventDamage)
    {
        return base.ModifyDamageAdditive(target, amount, props, dealer, cardSource, cardPlay);
    }
    
    var threshold = Creature.MaxHp / 2;
    
    // 这次伤害本来就不会打到半血以下，不用管
    if (Creature.CurrentHp - amount >= threshold)
    {
        return base.ModifyDamageAdditive(target, amount, props, dealer, cardSource, cardPlay);
    }

    if (Creature.CurrentHp == threshold && ShouldPreventDamage)
    {
        return -amount;
    }
    
    // 只扣到半血为止
    var targetDamage = Creature.CurrentHp - threshold;
    return targetDamage - amount;
}

    

    public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target == Creature && Phase == 1 && Creature.CurrentHp <= Creature.MaxHp / 2 && !HasStun)
        {
            await CreatureCmd.Stun(Creature, "REVIVE");
            HasStun = true;
            await CreatureCmd.TriggerAnim(Creature, "A_Revive_1", 0.8f);
        }
        
    }



    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        List<MonsterState> list = new List<MonsterState>();
        MoveState GiveConfused = new MoveState(
            "GIVE_CONFUSED",
            async targets =>
            {
                foreach (Creature p in targets)
                {
                    await CardPileCmd.AddToCombatAndPreview<Confused>(p, PileType.Draw, 1, null, CardPilePosition.Random);
                    await CardPileCmd.AddToCombatAndPreview<Confused>(p, PileType.Discard, 1, null, CardPilePosition.Random);
                    await CardPileCmd.AddToCombatAndPreview<Confused>(p, PileType.Hand, 1, null, CardPilePosition.Random);
                }
            },
            new StatusIntent(3)
        );

        MoveState HeavyAttackPhase1 = new MoveState(
            "HEAVY_ATTACK_PHASE1",
            async targets =>
            {
                await CreatureCmd.TriggerAnim(Creature, "A_Attack", 0.8f);
                await Cmd.Wait(1.0f);
                await DamageCmd.Attack(heavyAttackPhase1).FromMonster(this).WithNoAttackerAnim().Execute(null);
                
            },
            [new SingleAttackIntent(heavyAttackPhase1)]
        );

        MoveState HeavyAttackPhase2 = new MoveState(
            "HEAVY_ATTACK_PHASE2",
            async targets =>
            {
                await CreatureCmd.TriggerAnim(Creature, "B_Attack_Begin_2", 0.8f);
                await Cmd.Wait(1.0f);
                await DamageCmd.Attack(heavyAttackPhase2).FromMonster(this).WithNoAttackerAnim().Execute(null);
                
            },
            [new SingleAttackIntent(heavyAttackPhase2)]
        );

        MoveState MultiAttackPhase1 = new MoveState(
            "MULTI_ATTACK_PHASE1",
            async targets =>
            {
                await CreatureCmd.TriggerAnim(Creature, "A_Attack", 0.8f);
                await Cmd.Wait(1.0f);
                await DamageCmd.Attack(multiAttackPhase1).WithHitCount(7).FromMonster(this).Execute(null);
            },
            [new MultiAttackIntent(multiAttackPhase1, 7)]
        );

        MoveState MultiAttackPhase2 = new MoveState(
            "MULTI_ATTACK_PHASE2",
            async targets =>
            {
                await CreatureCmd.TriggerAnim(Creature, "B_Attack_Begin_2", 0.8f);
                await Cmd.Wait(1.0f);
                await DamageCmd.Attack(multiAttackPhase2).WithHitCount(7).FromMonster(this).Execute(null);
            },
            [new MultiAttackIntent(multiAttackPhase2, 7)]
        );
        MoveState SummonPhase1 = new MoveState(
            "SUMMON_PHASE1",
            async targets =>
            {
                if (CombatState.HittableEnemies.Count == 1)
                {
                    List<MonsterModel> enemies = new List<MonsterModel>
                    {
                        ModelDb.Monster<SanktaBlade>().ToMutable(),
                        ModelDb.Monster<SanktaPriest>().ToMutable(),
                        ModelDb.Monster<SanktaSniper>().ToMutable()
                    };
                    MonsterModel chosen = enemies.TakeRandom(1, CombatState.Players[0].RunState.Rng.CombatCardGeneration).First();

                    await CreatureCmd.TriggerAnim(Creature, "A_Attack", 0.8f);
                    await Cmd.Wait(1.0f);
                    await CreatureCmd.Add(chosen, CombatState);
                    await PowerCmd.Apply<MinionPower>(
                        new ThrowingPlayerChoiceContext(),
                        CombatState.Enemies.First(c => c.Monster == chosen),
                        1m,
                        Creature,
                        null
                    );
                }
                foreach (Creature c in CombatState.HittableEnemies)
                {
                    await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), c, 2m, Creature, null);
                }
            },
            [new SummonIntent(), new BuffIntent()]
        );

        MoveState SummonPhase2 = new MoveState(
            "SUMMON_PHASE2",
            async targets =>
            {
                if (CombatState.HittableEnemies.Count == 1)
                {
                    List<MonsterModel> enemies = new List<MonsterModel>
                    {
                        ModelDb.Monster<SanktaBlade>().ToMutable(),
                        ModelDb.Monster<SanktaPriest>().ToMutable(),
                        ModelDb.Monster<SanktaSniper>().ToMutable()
                    };
                    MonsterModel chosen = enemies.TakeRandom(1, CombatState.Players[0].RunState.Rng.CombatCardGeneration).First();

                    await CreatureCmd.TriggerAnim(Creature, "B_Skill_Begin_2", 0.8f);
                    await Cmd.Wait(1.0f);
                    await CreatureCmd.Add(chosen, CombatState, MegaCrit.Sts2.Core.Combat.CombatSide.Enemy, "first");
                    await PowerCmd.Apply<MinionPower>(
                        new ThrowingPlayerChoiceContext(),
                        CombatState.Enemies.First(c => c.Monster == chosen),
                        1m,
                        Creature,
                        null
                    );
                }
                foreach (Creature c in CombatState.HittableEnemies)
                {
                    await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), c, 2m, Creature, null);
                }
            },
            [new SummonIntent(), new BuffIntent()]
        );

        MoveState AttackDebuffPhase1 = new MoveState(
            "ATTACK_DEBUFF_PHASE1",
            async targets =>
            {
                await CreatureCmd.TriggerAnim(Creature, "A_Attack", 0.8f);
                await Cmd.Wait(1.0f);
                await DamageCmd.Attack(debuffAttackPhase1).FromMonster(this).WithNoAttackerAnim().Execute(null);
                foreach (Creature c in targets) { }
            },
            [new SingleAttackIntent(debuffAttackPhase1), new DebuffIntent()]
        );

        MoveState AttackDebuffPhase2 = new MoveState(
            "ATTACK_DEBUFF_PHASE2",
            async targets =>
            {
                await CreatureCmd.TriggerAnim(Creature, "B_Attack_Begin_2", 0.8f);
                await Cmd.Wait(1.0f);
                await DamageCmd.Attack(debuffAttackPhase1).FromMonster(this).WithNoAttackerAnim().Execute(null);
                foreach (Creature c in targets) { }
            },
            [new SingleAttackIntent(debuffAttackPhase1), new DebuffIntent()]
        );

        MoveState Revive = new MoveState(
            "REVIVE",
            async targets =>
            {
                
                await CreatureCmd.TriggerAnim(Creature, "A_Revive_3", 0.8f);
                await Cmd.Wait(1.0f);
            },
            [new BuffIntent()]
        );

        MoveState Fly = new MoveState(
            "FLY",
            async targets =>
            {
                ShouldPreventDamage = false;
                Phase = 2;
                await CreatureCmd.TriggerAnim(Creature, "B_Leave_1", 0.8f);
                await PowerCmd.Apply<SoarPower>(new ThrowingPlayerChoiceContext(), base.Creature, 1m, base.Creature, null);
                await Cmd.Wait(1.0f);
            },
            [new DebuffIntent()]
        );

        list.Add(GiveConfused);
        list.Add(HeavyAttackPhase1);
        list.Add(HeavyAttackPhase2);
        list.Add(MultiAttackPhase1);
        list.Add(MultiAttackPhase2);
        list.Add(SummonPhase1);
        list.Add(SummonPhase2);
        list.Add(AttackDebuffPhase1);
        list.Add(AttackDebuffPhase2);
        list.Add(Revive);
        list.Add(Fly);

        GiveConfused.FollowUpState = HeavyAttackPhase1;
        HeavyAttackPhase1.FollowUpState = SummonPhase1;

        SummonPhase1.FollowUpState = MultiAttackPhase1;
        MultiAttackPhase1.FollowUpState = AttackDebuffPhase1;
        AttackDebuffPhase1.FollowUpState = SummonPhase1;
        
        Revive.FollowUpState = Fly;
        Fly.FollowUpState = HeavyAttackPhase2;
        HeavyAttackPhase2.FollowUpState = SummonPhase2;
        SummonPhase2.FollowUpState = MultiAttackPhase2;
        MultiAttackPhase2.FollowUpState = AttackDebuffPhase2;
        AttackDebuffPhase2.FollowUpState = SummonPhase2;

        return new MonsterMoveStateMachine(list, GiveConfused);
    }

    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        AnimState idleStatePhase1 = new AnimState("A_Idle", isLooping: true);
        AnimState idleStateRevivePhase1 = new AnimState("B_Idle_1", isLooping: true);
        AnimState idleStatePhase2 = new AnimState("B_Idle_2", isLooping: true);

        AnimState Phase1AttackState = new AnimState("A_Attack");
        AnimState Phase2ReviveState1 = new AnimState("A_Revive_1");
        AnimState Phase2ReviveState2 = new AnimState("A_Revive_2", isLooping: true);
        AnimState Phase2ReviveState3 = new AnimState("A_Revive_3");
        

        AnimState Phase2AttackStateBegin = new AnimState("B_Attack_Begin_2");
        AnimState Phase2AttackStateLoop = new AnimState("B_Attack_Loop_2");
        AnimState Phase2AttackStateEnd = new AnimState("B_Attack_End_2");
        AnimState Phase2SkillStateBegin = new AnimState("B_Skill_Begin_2");
        AnimState Phase2SkillStateLoop = new AnimState("B_Skill_Loop_2");
        AnimState Phase2SkillStateEnd = new AnimState("B_Skill_End_2");
        AnimState Phase2FlyState = new AnimState("B_Leave_1");

        AnimState dieState = new AnimState("Die");
        AnimState skillState = new AnimState("Skill");

        Phase1AttackState.NextState = idleStatePhase1;
        Phase2AttackStateBegin.NextState = Phase2AttackStateLoop;
        Phase2AttackStateLoop.NextState = Phase2AttackStateEnd;
        Phase2AttackStateEnd.NextState = idleStatePhase2;

        Phase2SkillStateBegin.NextState = Phase2SkillStateLoop;
        Phase2SkillStateLoop.NextState = Phase2SkillStateEnd;
        Phase2SkillStateEnd.NextState = idleStatePhase2;

        Phase2ReviveState1.NextState = Phase2ReviveState2;
        Phase2ReviveState3.NextState = idleStateRevivePhase1;
        Phase2FlyState.NextState = idleStatePhase2;

        CreatureAnimator creatureAnimator = new CreatureAnimator(idleStatePhase1, controller);

        creatureAnimator.AddAnyState("A_Attack", Phase1AttackState);
        creatureAnimator.AddAnyState("A_Revive_1", Phase2ReviveState1);
        creatureAnimator.AddAnyState("A_Revive_2", Phase2ReviveState2);
        creatureAnimator.AddAnyState("A_Revive_3", Phase2ReviveState3);

        creatureAnimator.AddAnyState("B_Attack_Begin_2", Phase2AttackStateBegin);
        creatureAnimator.AddAnyState("B_Attack_Loop_2", Phase2AttackStateLoop);
        creatureAnimator.AddAnyState("B_Attack_End_2", Phase2AttackStateEnd);
        creatureAnimator.AddAnyState("B_Skill_Begin_2", Phase2SkillStateBegin);
        creatureAnimator.AddAnyState("B_Skill_Loop_2", Phase2SkillStateLoop);
        creatureAnimator.AddAnyState("B_Skill_End_2", Phase2SkillStateEnd);

        creatureAnimator.AddAnyState("B_Leave_1", Phase2FlyState);

        creatureAnimator.AddAnyState("Skill", skillState);
        creatureAnimator.AddAnyState("Die", dieState);

        return creatureAnimator;
    }
}
