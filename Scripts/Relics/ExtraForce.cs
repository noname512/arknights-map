using ArknightsMap.Scripts.Cards;
using Godot;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Saves.Runs;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Relics;

[RegisterRelic(typeof(SharedRelicPool))]
public sealed class ExtraForce : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Ancient;

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => _extraHoverTips;

    public override RelicAssetProfile AssetProfile =>
        new(
            // 小图标（原版85x85）
            IconPath: $"res://ArknightsMap/images/relics/{GetType().Name}.png",
            // 轮廓图标（原版85x85）
            IconOutlinePath: $"res://ArknightsMap/images/relics/{GetType().Name}.png",
            // 大图标（原版256x256）
            BigIconPath: $"res://ArknightsMap/images/relics/{GetType().Name}.png"
        );

    private const string _starterCardKey = "StarterCard";
    private const string _ancientCardKey = "AncientCard";

    private SerializableCard? _serializableStarterCard;
    private SerializableCard? _serializableAncientCard;
    private List<IHoverTip> _extraHoverTips = new();

    private static Dictionary<ModelId, CardModel> TranscendenceUpgrades => new()
    {
        { ModelDb.Card<StrikeIronclad>().Id,    ModelDb.Card<StrikePlus>()},
        { ModelDb.Card<Survivor>().Id,   ModelDb.Card<SurvivorPlus>()     },
        { ModelDb.Card<Zap>().Id,      ModelDb.Card<ZapPlus>()    },
        { ModelDb.Card<Venerate>().Id,  ModelDb.Card<VeneratePlus>() },
        { ModelDb.Card<Bodyguard>().Id,     ModelDb.Card<BodyGuardPlus>()     },
    };

    public static List<CardModel> TranscendenceCards => TranscendenceUpgrades.Values.ToList();

    [SavedProperty]
    public SerializableCard? StarterCard
    {
        get => _serializableStarterCard;
        private set { AssertMutable(); _serializableStarterCard = value; UpdateHoverTips(); }
    }

    [SavedProperty]
    public SerializableCard? AncientCard
    {
        get => _serializableAncientCard;
        private set { AssertMutable(); _serializableAncientCard = value; UpdateHoverTips(); }
    }

    private readonly StringVar _starterCardVar = new("StarterCard");
    private readonly StringVar _ancientCardVar = new("AncientCard");

    protected override IEnumerable<DynamicVar> CanonicalVars => [_starterCardVar, _ancientCardVar];

    protected override void AfterCloned()
    {
        base.AfterCloned();
        _extraHoverTips = new List<IHoverTip>();
    }

    public bool SetupForPlayer(Player player)
    {
        AssertMutable();
        CardModel? transcendenceStarterCard = GetTranscendenceStarterCard(player);
        if (transcendenceStarterCard != null)
        {
            StarterCard = transcendenceStarterCard.ToSerializable();
            AncientCard = GetTranscendenceTransformedCard(transcendenceStarterCard).ToSerializable();
            UpdateHoverTips();
            return true;
        }
        return false;
    }

    private void UpdateHoverTips()
    {
        _extraHoverTips.Clear();
        if (StarterCard != null)
        {
            CardModel cardModel = CardModel.FromSerializable(StarterCard);
            _extraHoverTips.AddRange(cardModel.HoverTips);
            _extraHoverTips.Add(HoverTipFactory.FromCard(cardModel));
            ((StringVar)DynamicVars["StarterCard"]).StringValue = cardModel.Title;
        }
        if (AncientCard != null)
        {
            CardModel cardModel2 = CardModel.FromSerializable(AncientCard);
            _extraHoverTips.AddRange(cardModel2.HoverTips);
            _extraHoverTips.Add(HoverTipFactory.FromCard(cardModel2));
            ((StringVar)DynamicVars["AncientCard"]).StringValue = cardModel2.Title;
        }
    }

    private CardModel? GetTranscendenceStarterCard(Player player)
    {
        return player.Deck.Cards.FirstOrDefault(c => TranscendenceUpgrades.ContainsKey(c.Id));
    }

    private CardModel GetTranscendenceTransformedCard(CardModel starterCard)
    {
        if (TranscendenceUpgrades.TryGetValue(starterCard.Id, out CardModel? value))
        {
            CardModel cardModel = starterCard.Owner.RunState.CreateCard(value, starterCard.Owner);
            if (starterCard.IsUpgraded)
                CardCmd.Upgrade(cardModel);
            if (starterCard.Enchantment != null)
            {
                EnchantmentModel enchantmentModel = (EnchantmentModel)starterCard.Enchantment.MutableClone();
                CardCmd.Enchant(enchantmentModel, cardModel, enchantmentModel.Amount);
            }
            return cardModel;
        }
        return base.Owner.RunState.CreateCard<Doubt>(starterCard.Owner);
    }

    public override async Task AfterObtained()
{
    if (StarterCard == null && SetupForPlayer(Owner)) { }  // 获得时自愈，顺便刷 StringVar

    CardModel? starter = GetTranscendenceStarterCard(base.Owner);
    if (starter == null)
        return;

    await CardCmd.Transform(starter, GetTranscendenceTransformedCard(starter));
}
}
