using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.RelicPools;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Relics;

[RegisterRelic(typeof(SharedRelicPool))]
public sealed class SeventhHall : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Ancient;

    protected override IEnumerable<DynamicVar> CanonicalVars => [];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [];

    public override RelicAssetProfile AssetProfile =>
        new(
            // 小图标（原版85x85）
            IconPath: $"res://ArknightsMap/images/relics/{GetType().Name}.png",
            // 轮廓图标（原版85x85）
            IconOutlinePath: $"res://ArknightsMap/images/relics/{GetType().Name}.png",
            // 大图标（原版256x256）
            BigIconPath: $"res://ArknightsMap/images/relics/{GetType().Name}.png"
        );

    public override async Task BeforeHandDraw(Player player, PlayerChoiceContext choiceContext, ICombatState combatState)
    {
        if (player == Owner && combatState.RoundNumber == 1)
        {
            Flash();
            await PlayerCmd.GainEnergy(1,Owner);
            await DrawSpecificCard(choiceContext, CardType.Attack);
            await DrawSpecificCard(choiceContext, CardType.Skill);
            await DrawSpecificCard(choiceContext, CardType.Power);
            await DrawSpecificCard(choiceContext, CardType.Status);
            await DrawSpecificCard(choiceContext, CardType.Curse);
            await DrawSpecificCard(choiceContext, CardType.Quest);
            
        }
    }    
    

    public async Task DrawSpecificCard(PlayerChoiceContext choiceContext,CardType cardType)
    {
        
            List<CardModel> cardsIn = (from c in PileType.Draw.GetPile(base.Owner).Cards
			orderby c.Rarity, c.Id
			select c).ToList();

            List<CardModel> list = new List<CardModel>();

            foreach(CardModel c in cardsIn)
            {
                if (c.Type == cardType)
                {
                    list.Add(c);
                }
                
            }

            if (list.Count != 0)
            {
                CardModel c = list.TakeRandom(1, base.Owner.RunState.Rng.CombatCardSelection).First();
                await CardPileCmd.Add(c, PileType.Draw, CardPilePosition.Top, null, true);
                await CardPileCmd.Draw(choiceContext,1,base.Owner);
            }
    }
}