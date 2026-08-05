using ArknightsMap.Scripts.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Rooms;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

[RegisterRelic(typeof(SharedRelicPool))]
public class Target : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Ancient;
    protected override IEnumerable<DynamicVar> CanonicalVars => [];
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [];

<<<<<<< Updated upstream
    // 添加后备字段

=======
      // 添加后备字段

    
>>>>>>> Stashed changes
    public override RelicAssetProfile AssetProfile =>
        new(
            // 小图标（原版85x85）
            IconPath: $"res://ArknightsMap/images/relics/{GetType().Name}.png",
            // 轮廓图标（原版85x85）
            IconOutlinePath: $"res://ArknightsMap/images/relics/{GetType().Name}.png",
            // 大图标（原版256x256）
            BigIconPath: $"res://ArknightsMap/images/relics/{GetType().Name}.png"
        );
<<<<<<< Updated upstream

    public override async Task AfterCombatEnd(CombatRoom room)
    {
        CardModel? last = CombatManager
            .Instance.History.CardPlaysStarted.LastOrDefault(
                (CardPlayStartedEntry e) => e.CardPlay.Card.Owner == Owner && e.CardPlay.Card.Type == CardType.Attack
            )
            ?.CardPlay.Card;

        if (last != null)
        {
            CardModel copy = Owner.RunState.CreateCard(last.CanonicalInstance, Owner);

            CardCmd.ApplyKeyword(copy, UseOnceKeyword.Keyword);
            await CardPileCmd.Add(copy, PileType.Deck);

            CardCmd.Preview(copy, 1.0f);
        }
    }
}
=======
    

    public override async Task AfterCombatEnd(CombatRoom room)
    {
        // 修复：this.Owner 而不是 (CardModel)this
        
        CardModel last = CombatManager.Instance.History.CardPlaysStarted.LastOrDefault(
            (CardPlayStartedEntry e) => 
                e.CardPlay.Card.Owner == this.Owner && 
                e.CardPlay.Card.Type == CardType.Attack
        )?.CardPlay.Card;

        

        if (last != null)
        {
            CardModel copy = base.Owner.RunState.CreateCard(last.CanonicalInstance, base.Owner);
            
            CardCmd.ApplyKeyword(copy, UseOnceKeyword.Keyword);
            await CardPileCmd.Add(copy, PileType.Deck);
        
            CardCmd.Preview(copy, 1.0f);
        }
    }
}
>>>>>>> Stashed changes
