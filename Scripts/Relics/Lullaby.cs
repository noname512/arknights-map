using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Rooms;
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
        if (creature.Player != Owner && creature.PetOwner != Owner)
        {
            return amount;
        }
        return Owner.Creature.MaxHp;
    }

    public override Task AfterRestSiteHeal(Player player, bool isMimicked)
    {
        if (player != Owner)
        {
            return Task.CompletedTask;
        }
        Flash();
        Status = RelicStatus.Normal;
        return Task.CompletedTask;
    }

    public override IReadOnlyList<LocString> ModifyExtraRestSiteHealText(Player player, IReadOnlyList<LocString> currentExtraText)
    {
        if (!LocalContext.IsMe(Owner))
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
        array[num] = AdditionalRestSiteHealText!;
        return array;
    }

    public override Task AfterRoomEntered(AbstractRoom room)
    {
        Status = (room is RestSiteRoom) ? RelicStatus.Active : RelicStatus.Normal;
        return Task.CompletedTask;
    }
}
