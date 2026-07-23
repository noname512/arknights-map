

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

using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace ArknightsMap.Scripts.Relics;



[RegisterRelic(typeof(SharedRelicPool))]
public sealed class CustomMade : ModRelicTemplate

{
    public override RelicRarity Rarity => RelicRarity.Ancient;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(2)];

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

    public static int _strength = 0;
    public static int _dexterity = 0;

    public static int _cards = 0;

    public static int _blocks = 0;

    
	public override async Task AfterObtained()
	{
        
		foreach (CardModel item in await CardSelectCmd.FromDeckForRemoval(
            prefs: new CardSelectorPrefs(CardSelectorPrefs.RemoveSelectionPrompt, base.DynamicVars.Cards.IntValue), 
            player: base.Owner))
		{
            await CardPileCmd.RemoveFromDeck(item);
			_triggeredTypes.Add(item.CreateClone());
		}

        CardModel custom = base.Owner.RunState.CreateCard<Scripts.Cards.CustomMade>(base.Owner);
        foreach (CardModel c in _triggeredTypes)
        {
            if (c.Type == CardType.Attack)
            {
                _strength++;
            }
            else if (c.Type == CardType.Skill)
            {
                _dexterity++;
            }
            else if (c.Type == CardType.Power)
            {
                _cards++;
            }
            else
            {
                _blocks++;
            }
        }
        await CardPileCmd.Add(custom, PileType.Deck);
	}
                
}