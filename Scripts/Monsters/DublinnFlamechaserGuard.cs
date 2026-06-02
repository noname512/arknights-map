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
public class DublinnFlamechaserGuard : ModMonsterTemplate
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 140, 130);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 140, 130);
    public override MonsterAssetProfile AssetProfile => new(
        VisualsScenePath: $"res://ArknightsMap/scenes/monsters/{GetType().Name}.tscn"
    );
    private int Damage1 => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 14, 12);
    private int Damage2 => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 18, 16);
    public override bool ShouldDisappearFromDoom => !Creature.HasPower<ChaseFlamePower>() || Creature.GetPower<ChaseFlamePower>()?.CurState == 1;

    public override async Task AfterAddedToRoom()
    {
        await PowerCmd.Apply<ChaseFlamePower>(new ThrowingPlayerChoiceContext(), Creature, 10, Creature, null);
        // await PowerCmd.Apply<FlameBathPower>(new ThrowingPlayerChoiceContext(), Creature, 50, Creature, null);
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        List<MonsterState> list = new List<MonsterState>();
        MoveState attack1 = new MoveState(
            "ATTACK1",
            async targets => await DamageCmd
                .Attack(Damage1)
                .FromMonster(this)
                // .WithAttackerAnim("Attack", 0.5f) // 如果有攻击动画，可以取消注释并替换成实际动画名称和延迟
                .Execute(null),
            new SingleAttackIntent(Damage1)
        );
        MoveState attack2 = new MoveState(
            "ATTACK2",
            async targets =>
            {
                await DamageCmd
                    .Attack(Damage2)
                    .FromMonster(this)
                    .Execute(null);
            },
            new SingleAttackIntent(Damage2)
        );
        MoveState buff = new MoveState(
            "ATTACK3",
            async targets =>
            {
                if (ReedBed.Burning)
                {
                    await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Creature, 12, Creature, null);
                }
                else
                {
                    await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Creature, 2, Creature, null);
                    await Entry.reedBed.SetBurningDurningCombat(true, CombatState);
                }
            },
            new BuffIntent()
        );
        MoveState stun1 = new MoveState("STUN1", _ => { return Task.CompletedTask; }, new StunIntent());
        MoveState stun3 = new MoveState("STUN3", async _ =>
        {
            await Creature.GetPower<ChaseFlamePower>()?.Revive();
            await Entry.reedBed.SetBurningDurningCombat(true, CombatState);
        }, new HealIntent(), new BuffIntent());
        // 不要改，复活的意图就是STUN3，改了可能会炸

        attack1.FollowUpState = attack2;
        attack2.FollowUpState = buff;
        buff.FollowUpState = attack1;
        stun1.FollowUpState = stun3;

        list.Add(attack1);
        list.Add(attack2);
        list.Add(buff);
        list.Add(stun1);
        list.Add(stun3);


        return new MonsterMoveStateMachine(list, attack1);
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