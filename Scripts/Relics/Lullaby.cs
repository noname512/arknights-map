using ArknightsMap.Scripts.Enchantments;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Relics;

[RegisterRelic(typeof(SharedRelicPool))]
public class Lullaby : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Ancient;

    public override RelicAssetProfile AssetProfile =>
        new(
            // 小图标（原版85x85）
            IconPath: $"res://ArknightsMap/images/relics/{GetType().Name}.png",
            // 轮廓图标（原版85x85）
            IconOutlinePath: $"res://ArknightsMap/images/relics/{GetType().Name}.png",
            // 大图标（原版256x256）
            BigIconPath: $"res://ArknightsMap/images/relics/{GetType().Name}.png"
        );
    public override decimal ModifyRestSiteHealAmount(Creature creature, decimal amount)
    {
        if (creature.Player != base.Owner && creature.PetOwner != base.Owner)
        {
            return amount;
        }
        return Owner.Creature.MaxHp;
    }

    public override Task AfterRestSiteHeal(Player player, bool isMimicked)
    {
        if (player != base.Owner)
        {
            return Task.CompletedTask;
        }
        Flash();
        base.Status = RelicStatus.Normal;
        return Task.CompletedTask;
    }

    public override IReadOnlyList<LocString> ModifyExtraRestSiteHealText(Player player, IReadOnlyList<LocString> currentExtraText)
    {
        if (!LocalContext.IsMe(base.Owner))
        {
            return currentExtraText;
        }
        int num = 0;
        LocString[] array = new LocString[1 + currentExtraText.Count];
        foreach (LocString item in currentExtraText)
        {
            array[num] = item;
            num++;
        }
        array[num] = AdditionalRestSiteHealText;
        return array;
    }

    public override Task AfterRoomEntered(AbstractRoom room)
    {
        base.Status = ((room is RestSiteRoom) ? RelicStatus.Active : RelicStatus.Normal);
        return Task.CompletedTask;
    }
}
