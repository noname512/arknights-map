using ArknightsMap.Scripts.Enchantments;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Relics;

[RegisterRelic(typeof(SharedRelicPool))]
public class WitherAndThrive : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Ancient;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(2)];
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => HoverTipFactory.FromEnchantment<BlockHeal>();

    public override RelicAssetProfile AssetProfile => new(
        // 小图标（原版85x85）
        IconPath: $"res://ArknightsMap/images/relics/{GetType().Name}.png",
        // 轮廓图标（原版85x85）
        IconOutlinePath: $"res://ArknightsMap/images/relics/{GetType().Name}.png",
        // 大图标（原版256x256）
        BigIconPath: $"res://ArknightsMap/images/relics/{GetType().Name}.png"
    );


    public override async Task AfterObtained()
    {
        CardSelectorPrefs prefs = new CardSelectorPrefs(CardSelectorPrefs.EnchantSelectionPrompt, base.DynamicVars.Cards.IntValue);
        BlockHeal enchantment = ModelDb.Enchantment<BlockHeal>();
        foreach (CardModel item in await CardSelectCmd.FromDeckForEnchantment(base.Owner, enchantment, 1, prefs))
        {
            CardCmd.Enchant(enchantment.ToMutable(), item, 1m);
            CardCmd.Preview(item);
        }
    }
}
