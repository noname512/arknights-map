using ArknightsMap.Scripts.Cards;
using ArknightsMap.Scripts.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Saves.Runs;
<<<<<<< Updated upstream
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
=======
using ArknightsMap.Scripts.Utils;
>>>>>>> Stashed changes

namespace ArknightsMap.Scripts.Relics;

[RegisterRelic(typeof(SharedRelicPool))]
public sealed class CustomMade : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Ancient;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(2)];

<<<<<<< Updated upstream
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.FromKeyword(CustomKeyword.Keyword)];
=======


    protected override IEnumerable<IHoverTip> AdditionalHoverTips
 => [
            HoverTipFactory.FromKeyword(CustomKeyword.Keyword)
        ];    
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

    private readonly HashSet<CardModel> _triggeredTypes = new();

    private int _strength;
    private int _dexterity;
    private int _cards;
    private int _block;

    [SavedProperty]
    public int Strength
    {
        get => _strength;
<<<<<<< Updated upstream
        set { _strength = value; }
=======
        set
        {
            
            _strength = value;
        }
>>>>>>> Stashed changes
    }

    [SavedProperty]
    public int Dexterity
    {
        get => _dexterity;
<<<<<<< Updated upstream
        set { _dexterity = value; }
=======
        set
        {
            _dexterity = value;
        }
>>>>>>> Stashed changes
    }

    [SavedProperty]
    public int Cards
    {
        get => _cards;
<<<<<<< Updated upstream
        set { _cards = value; }
=======
        set
        {
            
            _cards = value;
        }
>>>>>>> Stashed changes
    }

    [SavedProperty]
    public int Block
    {
        get => _block;
<<<<<<< Updated upstream
        set { _block = value; }
    }

    public override async Task AfterObtained()
    {
        foreach (
            CardModel item in await CardSelectCmd.FromDeckForRemoval(
                prefs: new CardSelectorPrefs(CardSelectorPrefs.RemoveSelectionPrompt, base.DynamicVars.Cards.IntValue),
                player: base.Owner
            )
        )
        {
            await CardPileCmd.RemoveFromDeck(item);
            _triggeredTypes.Add(item.CreateClone());
        }

=======
        set
        {
            
            _block = value;
        }
    }
    
	public override async Task AfterObtained()
	{
        
		foreach (CardModel item in await CardSelectCmd.FromDeckForRemoval(
            prefs: new CardSelectorPrefs(CardSelectorPrefs.RemoveSelectionPrompt, base.DynamicVars.Cards.IntValue), 
            player: base.Owner))
		{
            await CardPileCmd.RemoveFromDeck(item);
			_triggeredTypes.Add(item.CreateClone());
		}
        
        

        
>>>>>>> Stashed changes
        foreach (CardModel c in _triggeredTypes)
        {
            if (c.Type == CardType.Attack)
            {
                _strength += 2;
<<<<<<< Updated upstream
=======
                
>>>>>>> Stashed changes
            }
            else if (c.Type == CardType.Skill)
            {
                _dexterity += 2;
<<<<<<< Updated upstream
=======
                
>>>>>>> Stashed changes
            }
            else if (c.Type == CardType.Power)
            {
                _cards += 3;
<<<<<<< Updated upstream
=======
                
>>>>>>> Stashed changes
            }
            else
            {
                _block += 6;
<<<<<<< Updated upstream
            }
        }

        CardModel custom = Owner.RunState.CreateCard<CustomMadeCard>(Owner);
=======
                
            }
        }

        CardModel custom = base.Owner.RunState.CreateCard<Scripts.Cards.CustomMade>(base.Owner);
>>>>>>> Stashed changes
        await CardPileCmd.Add(custom, PileType.Deck);
        CardCmd.Preview(custom, 1.0f);

        _triggeredTypes.Clear();
<<<<<<< Updated upstream
    }
}
=======
        
	}

    
                
}
>>>>>>> Stashed changes
