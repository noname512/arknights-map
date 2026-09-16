using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Relics;

[RegisterRelic(typeof(SharedRelicPool))]
public class MarkOfTara : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Ancient;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new HealVar(10m), new IntVar("Intangible", 3), new StringVar("Special", GetString())];
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => HoverTipFactory.FromPowerWithPowerHoverTips<IntangiblePower>();

    public override RelicAssetProfile AssetProfile =>
        new(
            // 小图标（原版85x85）
            IconPath: $"res://ArknightsMap/images/relics/{GetType().Name}.png",
            // 轮廓图标（原版85x85）
            IconOutlinePath: $"res://ArknightsMap/images/relics/{GetType().Name}.png",
            // 大图标（原版256x256）
            BigIconPath: $"res://ArknightsMap/images/relics/{GetType().Name}.png"
        );
    private int _wasUsed;

    String GetString()
    {
        if (WasUsed == 0)
        {
            return new LocString("relics", this.Id.Entry + ".Outside").GetRawText();
        }
        if (DisplayAmount == 0)
        {
            return "";
        }
        String res = new LocString("relics", this.Id.Entry + ".Start").GetRawText();
        int cnt = 0;
        if ((WasUsed & (1 << (int)RoomType.Monster)) != 0)
        {
            res += new LocString("relics", this.Id.Entry + ".Normal").GetRawText();
            cnt++;
        }
        if ((WasUsed & (1 << (int)RoomType.Elite)) != 0)
        {
            if (cnt >= 1)
            {
                res += new LocString("relics", this.Id.Entry + ".And").GetRawText();
            }
            res += new LocString("relics", this.Id.Entry + ".Elite").GetRawText();
            cnt++;
        }

        if ((WasUsed & (1 << (int)RoomType.Boss)) != 0)
        {
            if (cnt >= 1)
            {
                res += new LocString("relics", this.Id.Entry + ".And").GetRawText();
            }
            res += new LocString("relics", this.Id.Entry + ".Boss").GetRawText();
            cnt++;
        }
        res += new LocString("relics", this.Id.Entry + ".End").GetRawText();

        return res;
    }

    public override int DisplayAmount
    {
        get
        {
            int cnt = 0;
            if ((WasUsed & (1 << (int)RoomType.Monster)) != 0)
            {
                cnt++;
            }
            if ((WasUsed & (1 << (int)RoomType.Elite)) != 0)
            {
                cnt++;
            }
            if ((WasUsed & (1 << (int)RoomType.Boss)) != 0)
            {
                cnt++;
            }

            return cnt;
        }
    }

    public override bool ShowCounter => true;
    public override bool IsUsedUp => (DisplayAmount == 0);

    [SavedProperty]
    public int WasUsed
    {
        get { return _wasUsed; }
        set
        {
            AssertMutable();
            _wasUsed = value;
            ((StringVar)DynamicVars["Special"]).StringValue = GetString();
            if (IsUsedUp)
            {
                Status = RelicStatus.Disabled;
            }
        }
    }

    public override Task AfterObtained()
    {
        WasUsed = -1;
        InvokeDisplayAmountChanged();
        return Task.CompletedTask;
    }

    public override bool ShouldDieLate(Creature creature)
    {
        if (creature != Owner.Creature)
        {
            return true;
        }
        if (Owner.Creature.CombatState == null)
        {
            return true;
        }
        if ((WasUsed & (1 << (int)Owner.RunState.CurrentRoom!.RoomType)) == 0)
        {
            return true;
        }
        return false;
    }

    public override async Task AfterPreventingDeath(Creature creature)
    {
        if (Owner.Creature.CombatState != null)
        {
            Flash();
            WasUsed ^= 1 << (int)Owner.RunState.CurrentRoom!.RoomType;
            InvokeDisplayAmountChanged();
            await CreatureCmd.Heal(creature, DynamicVars.Heal.BaseValue);
            PlayerChoiceContext t = new ThrowingPlayerChoiceContext();
            await PowerCmd.Apply<IntangiblePower>(t, Owner.Creature, DynamicVars["Intangible"].IntValue, Owner.Creature, null, false);
        }
    }
}
