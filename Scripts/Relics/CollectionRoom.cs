using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Rewards;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Relics;

[RegisterRelic(typeof(SharedRelicPool))]
public class CollectionRoom : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Ancient;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar("Relics", 5)];
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => HoverTipFactory.FromCardWithCardHoverTips<Guilty>();

    public override RelicAssetProfile AssetProfile =>
        new(
            // 小图标（原版85x85）
            IconPath: $"res://ArknightsMap/images/relics/{GetType().Name}.png",
            // 轮廓图标（原版85x85）
            IconOutlinePath: $"res://ArknightsMap/images/relics/{GetType().Name}.png",
            // 大图标（原版256x256）
            BigIconPath: $"res://ArknightsMap/images/relics/{GetType().Name}.png"
        );

    private int RelicObtainCount = 0;
    List<Reward> Rewards = [];

    public override async Task AfterObtained()
    {
        Rewards.Clear();
        RelicObtainCount = 0;
        for (int i = 0; i < DynamicVars["Relics"].BaseValue; i++)
        {
            Rewards.Add(new RelicReward(Owner));
        }
        await RewardsCmd.OfferCustom(Owner, Rewards);
    }

    public override async Task AfterRewardTaken(Player player, Reward reward)
    {
        if (Rewards.Contains(reward))
        {
            RelicObtainCount++;
            if (RelicObtainCount >= DynamicVars["Relics"].BaseValue)
            {
                await CardPileCmd.AddCurseToDeck<Guilty>(Owner);
            }
        }
    }
}
