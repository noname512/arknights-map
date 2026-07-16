using Archetto.Scripts.Cards.Basic;
using Archetto.Scripts.Cards.Track;
using Archetto.Scripts.Pools;

using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;

using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rooms;
using STS2RitsuLib.Scaffolding.Content;
using Archetto.Scripts.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.CardSelection;
using Archetto.Scripts.Enchant;
using Archetto.Scripts.Enums;
using Archetto.Scripts.Cards.Ancient;
using MegaCrit.Sts2.Core.Models.RelicPools;

namespace ArknightsMap.Scripts.Relics;



[RegisterRelic(typeof(SharedRelicPool))]
public sealed class CustomMade : ModRelicTemplate

{
    public override RelicRarity Rarity => RelicRarity.Ancient;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(3)];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips
 => [
            
        ];

    
    public override RelicAssetProfile AssetProfile =>
        new(
            // 小图标（原版85x85）
            IconPath: $"res://ArknightsMap/images/relics/{GetType().Name}.png",
            // 轮廓图标（原版85x85）
            IconOutlinePath: $"res://ArknightsMap/images/relics/{GetType().Name}.png",
            // 大图标（原版256x256）
            BigIconPath: $"res://ArknightsMap/images/relics/{GetType().Name}.png"
        );

    private readonly HashSet<CardModel> _triggeredTypes = new();

	public override async Task AfterObtained()
	{
        
		foreach (CardModel item in await CardSelectCmd.FromDeckForRemoval(
            prefs: new CardSelectorPrefs(CardSelectorPrefs.RemoveSelectionPrompt, base.DynamicVars.Cards.IntValue), 
            player: base.Owner))
		{
            await CardPileCmd.RemoveFromDeck(item);
			_triggeredTypes.Add(item.CreateClone());
		}

        CardModel custom = base.Owner.RunState.CreateCard<Scripts.Cards.Ancient.CustomMade>(base.Owner);
        foreach (CardModel c in _triggeredTypes)
        {
            if (c.Type == CardType.Attack)
            {
                custom.DynamicVars["StrengthPower"].BaseValue++;
            }
            else if (c.Type == CardType.Skill)
            {
                custom.DynamicVars["DexterityPower"].BaseValue++;
            }
            else if (c.Type == CardType.Power)
            {
                custom.DynamicVars["Cards"].BaseValue += 2;
            }
            else
            {
                custom.DynamicVars["Block"].BaseValue += 4;
            }
        }
        await CardPileCmd.Add(custom, PileType.Deck);
	}
                
}