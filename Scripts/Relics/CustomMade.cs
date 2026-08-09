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
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Relics;

[RegisterRelic(typeof(SharedRelicPool))]
public sealed class CustomMade : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Ancient;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(2)];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.FromKeyword(CustomKeyword.Keyword)];

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
        set { _strength = value; }
    }

    [SavedProperty]
    public int Dexterity
    {
        get => _dexterity;
        set { _dexterity = value; }
    }

    [SavedProperty]
    public int Cards
    {
        get => _cards;
        set { _cards = value; }
    }

    [SavedProperty]
    public int Block
    {
        get => _block;
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

        foreach (CardModel c in _triggeredTypes)
        {
            if (c.Type == CardType.Attack)
            {
                _strength += 2;
            }
            else if (c.Type == CardType.Skill)
            {
                _dexterity += 2;
            }
            else if (c.Type == CardType.Power)
            {
                _cards += 3;
            }
            else
            {
                _block += 6;
            }
        }

        CardModel custom = Owner.RunState.CreateCard<CustomMadeCard>(Owner);
        await CardPileCmd.Add(custom, PileType.Deck);
        CardCmd.Preview(custom, 1.0f);

        _triggeredTypes.Clear();
    }
}
