using ArknightsMap.Scripts.Enchantments;
using ArknightsMap.Scripts.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

[RegisterRelic(typeof(SharedRelicPool))]
public class Target : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Ancient;
    protected override IEnumerable<DynamicVar> CanonicalVars => [];
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => 
    [..HoverTipFactory.FromEnchantment<UseOnce>(), ];

    
    public bool ShouldTrigger = false;

    // 添加后备字段

    public override RelicAssetProfile AssetProfile =>
        new(
            // 小图标（原版85x85）
            IconPath: $"res://ArknightsMap/images/relics/{GetType().Name}.png",
            // 轮廓图标（原版85x85）
            IconOutlinePath: $"res://ArknightsMap/images/relics/{GetType().Name}.png",
            // 大图标（原版256x256）
            BigIconPath: $"res://ArknightsMap/images/relics/{GetType().Name}.png"
        );

    public override async Task AfterCombatEnd(CombatRoom room)
    {
        CardModel? last = CombatManager
            .Instance.History.CardPlaysStarted.LastOrDefault(
                (CardPlayStartedEntry e) => e.CardPlay.Card.Owner == Owner && e.CardPlay.Card.Type == CardType.Attack
            )
            ?.CardPlay.Card;

        if (last != null)
        {
            IRunState runState = base.Owner.Creature.CombatState!.RunState;

// 从 RunState 创建新卡牌（不带 Owner）
            CardModel newCard = runState.CreateCard(last.CanonicalInstance, null!);

// 修改属性
            CardCmd.Enchant<UseOnce>(newCard, 1m);

// 添加到 RunState（这会设置 Owner）
            runState.AddCard(newCard, Owner);
            

// 创建奖励
            SpecialCardReward specialCardReward = new SpecialCardReward(newCard, Owner);
            room.AddExtraReward(Owner, specialCardReward);

            
            
        }
        
    }      
}
