using ArknightsMap.Scripts.Enchantments;
using ArknightsMap.Scripts.Powers;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Characters;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Relics;

[RegisterRelic(typeof(SharedRelicPool))]
public class SkullcrackingElegy : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Ancient;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar("Times", 2)];

    public override RelicAssetProfile AssetProfile =>
        new(
            // 小图标（原版85x85）
            IconPath: $"res://ArknightsMap/images/relics/{GetType().Name}.png",
            // 轮廓图标（原版85x85）
            IconOutlinePath: $"res://ArknightsMap/images/relics/{GetType().Name}.png",
            // 大图标（原版256x256）
            BigIconPath: $"res://ArknightsMap/images/relics/{GetType().Name}.png"
        );
    
    public override bool ShowCounter => CombatManager.Instance.IsInProgress;

    private int _remainTimes;
    int remainTimes
    {
        set
        {
            _remainTimes = value;
            InvokeDisplayAmountChanged();
        }
        get
        {
            return _remainTimes;
        }
    }
    public override int DisplayAmount => remainTimes;
    
    public override Task AfterRoomEntered(AbstractRoom room)
    {
        if (!(room is CombatRoom))
        {
            return Task.CompletedTask;
        }

        remainTimes = DynamicVars["Times"].IntValue;
        base.Status = RelicStatus.Active;
        return Task.CompletedTask;
    }
    
    public override int ModifyCardPlayCount(CardModel card, Creature? target, int playCount)
    {
        if (DisplayAmount == 0)
        {
            return playCount;
        }
        if (card.Owner != base.Owner)
        {
            return playCount;
        }
        if (card.Type != CardType.Attack)
        {
            return playCount;
        }
        return playCount + 1;
    }
    
    public override Task AfterModifyingCardPlayCount(CardModel card)
    {
        remainTimes--;
        Flash();
        if (remainTimes == 0)
        {
            Status = RelicStatus.Normal;
        }
        return Task.CompletedTask;
    }

    public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target,
        DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (CombatManager.Instance.IsInProgress && target == Owner.Creature && dealer != null && result.UnblockedDamage > 0)
        {
            Flash();
            remainTimes = DynamicVars["Times"].IntValue;
            Status = RelicStatus.Active;
        }
    }


    public override Task AfterCombatEnd(CombatRoom _)
    {
        remainTimes = 0;
        Status = RelicStatus.Normal;
        return Task.CompletedTask;
    }

}
