using ArknightsMap.Scripts.Monsters;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.ValueProps;
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
    protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar("DecreaseHp", 0), new IntVar("ReviveTurn", 0), new IntVar("AshDecreaseHp", 0)];
    public int CurState = 0; // 0: 初始状态，1: 余烬
    public int InitialHp = 0;
    public int MaxHp = 0;
    public int DecreaseHp = 0;
    public int ReviveTurn = 0;
    public int AshDecreaseHp = 0;
    public MoveState NextMove = null!;
    private ChaseFlamePowerRes res = new ChaseFlamePowerRes();

    public override Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        InitialHp = Amount;
        MaxHp = Owner.MaxHp;
        AshDecreaseHp = CombatState.Players.Count();
        DecreaseHp = AshDecreaseHp * MaxHp / Amount;
        if (Owner.Monster is DublinnFlamechaserGuard)
        {
            ReviveTurn = 2;
        }
        else
        {
            ReviveTurn = 3;
        }

        DynamicVars["DecreaseHp"].BaseValue = DecreaseHp;
        DynamicVars["ReviveTurn"].BaseValue = ReviveTurn;
        DynamicVars["AshDecreaseHp"].BaseValue = AshDecreaseHp;
        return base.AfterApplied(applier, cardSource);
    }

    public override bool ShouldScaleInMultiplayer => CurState == 0;

    // 自定义图标路径。1:1即可。原版游戏大图256x256，小图64x64。
    public override PowerAssetProfile AssetProfile =>
        new(IconPath: $"res://ArknightsMap/images/powers/{GetType().Name}.png", BigIconPath: $"res://ArknightsMap/images/powers/{GetType().Name}.png");

    public override LocString Title =>
        new LocString("powers", CurState == 0 ? "ARKNIGHTS_MAP_POWER_CHASE_FLAME_POWER.title" : "ARKNIGHTS_MAP_POWER_CHASE_FLAME_RES.title");
    public override LocString Description =>
        new LocString("powers", CurState == 0 ? "ARKNIGHTS_MAP_POWER_CHASE_FLAME_POWER.description" : "ARKNIGHTS_MAP_POWER_CHASE_FLAME_RES.description");

    protected override IEnumerable<IHoverTip> AdditionalHoverTips
    {
        get
        {
            res.CurState = 1 - CurState;
            return res.HoverTips;
        }
    }

    protected override string SmartDescriptionLocKey =>
        CurState == 0 ? "ARKNIGHTS_MAP_POWER_CHASE_FLAME_POWER.smartDescription" : "ARKNIGHTS_MAP_POWER_CHASE_FLAME_RES.smartDescription";

    private static Task SleepMove(IReadOnlyList<Creature> targets) => Task.CompletedTask;

    public override bool ShouldCreatureBeRemovedFromCombatAfterDeath(Creature creature) => creature != Owner || CurState == 1 || InitialHp <= 0;

    public override bool ShouldPowerBeRemovedAfterOwnerDeath() => false;

    public override async Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
    {
        if (creature != Owner)
            return;
        if (InitialHp <= 0)
            return;
        if (CurState == 0)
        {
            Owner.GetCreatureNode()!.SetAnimationTrigger("Revive");
            CurState = 1;
            await CreatureCmd.SetMaxAndCurrentHp(Owner, InitialHp);
            NextMove = Owner.Monster!.NextMove;
            if (!(Owner.Monster.MoveStateMachine.States.ContainsKey(NextMove.StateId)))
            {
                NextMove = (MoveState)Owner.Monster.MoveStateMachine!.States["ATTACK1"];
            }
            MoveState stun = (MoveState)Owner.Monster.MoveStateMachine!.States["STUN1"];
            Owner.Monster.SetMoveImmediate(stun, true);
            ((MoveState)Owner.Monster.MoveStateMachine.States["STUN3"]).FollowUpState = NextMove;
            SetAmount(ReviveTurn);
        }
    }

    public async Task Revive()
    {
        MaxHp -= DecreaseHp;
        if (MaxHp <= 0)
        {
            await CreatureCmd.Kill(Owner);
            return;
        }
        Owner.GetCreatureNode()!.SetAnimationTrigger("Revive2");
        await CreatureCmd.SetMaxAndCurrentHp(Owner, MaxHp);
        Owner.Monster!.NextMove.FollowUpState = NextMove;
        List<PowerModel> debuffs = Owner.Powers.Where(p => p.Type == PowerType.Debuff).ToList();
        foreach (PowerModel power in debuffs)
        {
            if ((power.Type == PowerType.Debuff) && (!(power is ITemporaryPower)))
            {
                await PowerCmd.Remove(power);
            }
        }
        InitialHp -= AshDecreaseHp;
        SetAmount(InitialHp);
        CurState = 0;
    }

    public override async Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side != Owner.Side)
        {
            return;
        }
        if (CurState == 1)
        {
            int remainingTurns = Amount - 1;
            SetAmount(remainingTurns);
            if (remainingTurns <= 0)
            {
                await Revive();
            }
        }
    }

    public override Decimal ModifyHpLostAfterOsty(Creature target, Decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (CurState == 0)
            return amount;
        return !CombatManager.Instance.IsInProgress || target != this.Owner || amount < 1M ? amount : 1M;
    }

    public override Task AfterModifyingHpLostAfterOsty()
    {
        if (CurState == 1)
            Flash();
        return Task.CompletedTask;
    }

    public override Decimal ModifyDamageCap(Creature? target, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (CurState == 0)
            return Decimal.MaxValue;
        return target != this.Owner ? Decimal.MaxValue : 1M;
    }

    public override Task AfterModifyingDamageAmount(CardModel? cardSource)
    {
        if (CurState == 1)
            Flash();
        return Task.CompletedTask;
    }

    public override bool ShouldPowerBeRemovedOnDeath(PowerModel power)
    {
        if ((power.Type == PowerType.Debuff) && (!(power is ITemporaryPower)))
        {
            return true;
        }
        return false;
    }
}
