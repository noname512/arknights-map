using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Relics;

[RegisterRelic(typeof(SharedRelicPool))]
public class TheSpear : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Ancient;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar("Sharp", 4)];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => HoverTipFactory.FromEnchantment<Sharp>();

    public override RelicAssetProfile AssetProfile => new(
        // 小图标（原版85x85）
        IconPath: $"res://ArknightsMap/images/relics/{GetType().Name}.png",
        // 轮廓图标（原版85x85）
        IconOutlinePath: $"res://ArknightsMap/images/relics/{GetType().Name}.png",
        // 大图标（原版256x256）
        BigIconPath: $"res://ArknightsMap/images/relics/{GetType().Name}.png"
    );

    public override Task AfterObtained()
    {
        IEnumerable<CardModel> enumerable = PileType.Deck.GetPile(Owner).Cards.Where(
                c => ModelDb.Enchantment<Sharp>().CanEnchant(c)).ToList();
        foreach (CardModel item in enumerable)
        {
            CardCmd.Enchant<Sharp>(item, DynamicVars["Sharp"].IntValue);
            NCardEnchantVfx nCardEnchantVfx = NCardEnchantVfx.Create(item);
            if (nCardEnchantVfx != null)
            {
                NRun.Instance?.GlobalUi.CardPreviewContainer.AddChildSafely(nCardEnchantVfx);
            }
        }

        return Task.CompletedTask;
    }
}
