using ArknightsMap.Scripts.Monsters;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Powers;

[RegisterPower]
public class ChaseFlamePower : ModPowerTemplate
{
    // 类型，Buff或Debuff
    public override PowerType Type => PowerType.Buff;
    // 叠加类型，Counter表示可叠加，Single表示不可叠加
    public override PowerStackType StackType => PowerStackType.Counter;
    public int CurState { get; set; } = 0; // 0: 初始状态，1: 余烬
    public int InitialHp { get; set; } = 0;
    public int MaxHp { get; set; } = 0;
    MoveState NextMove = null!;

    public override Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        InitialHp = Amount;
        MaxHp = base.Owner.MaxHp;
        return base.AfterApplied(applier, cardSource);
    }

    public override bool ShouldScaleInMultiplayer => true;

    // 自定义图标路径。1:1即可。原版游戏大图256x256，小图64x64。
    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://Test/images/powers/test_power.png",
        BigIconPath: "res://Test/images/powers/test_power.png"
    );

    public override LocString Description => new LocString("powers", CurState == 0 ? "ARKNIGHTS_MAP_POWER_CHASE_FLAME_POWER.description" : "ARKNIGHTS_MAP_POWER_CHASE_FLAME_RES.description");

    private static Task SleepMove(IReadOnlyList<Creature> targets)
    {
        return Task.CompletedTask;
    }

    protected override string SmartDescriptionLocKey
    {
        get
        {
            return CurState == 0 ? "ARKNIGHTS_MAP_POWER_CHASE_FLAME_POWER.smartDescription" : "ARKNIGHTS_MAP_POWER_CHASE_FLAME_RES.smartDescription";
        }
    }

    public override bool ShouldCreatureBeRemovedFromCombatAfterDeath(Creature creature)
    {
        if (creature != base.Owner)
        {
            return true;
        }
        return CurState == 1;
    }

    public override bool ShouldPowerBeRemovedAfterOwnerDeath()
    {
        return false;
    }

    public override async Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
    {
        if (creature != base.Owner) return;
        if (CurState == 0)
        {
            creature.GetCreatureNode().SetAnimationTrigger("Revive");
            CurState = 1;
            await CreatureCmd.SetMaxAndCurrentHp(base.Owner, InitialHp);
            await PowerCmd.Apply<IntangiblePower>(choiceContext, base.Owner, 3m, base.Owner, null);
            NextMove = creature.Monster.NextMove;
            MoveState sleep = new MoveState("SLEEP", SleepMove, new SleepIntent());
            sleep.FollowUpState = sleep;
            creature.Monster.SetMoveImmediate(sleep, true);
            base.SetAmount(3);
            InitialHp--;
        }
    }

    public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (side != base.Owner.Side)
        {
            return;
        }
        if (CurState == 1)
        {
            int remainingTurns = Amount - 1;
            base.SetAmount(remainingTurns);
            if (remainingTurns <= 0)
            {
                base.Owner.GetCreatureNode().SetAnimationTrigger("Revive2");
                await CreatureCmd.SetMaxAndCurrentHp(base.Owner, MaxHp);
                base.Owner.Monster.SetMoveImmediate(NextMove, true);
                base.SetAmount(InitialHp);
                CurState = 0;
            }
        }
    }
}