using ArknightsMap.Scripts.Cards;
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
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Relics;

[RegisterRelic(typeof(SharedRelicPool))]
public sealed class ExtraForce : ModRelicTemplate
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

    public static bool HasTransformCard(Player owner)
    {
        if (owner.Character is Ironclad)
        {
            return owner.Deck.Cards.Any(c => c is StrikeIronclad);
        }
        if (owner.Character is Silent)
        {
            return owner.Deck.Cards.Any(c => c is Survivor);
        }
        if (owner.Character is Regent)
        {
            return owner.Deck.Cards.Any(c => c is Venerate);
        }
        if (owner.Character is Defect)
        {
            return owner.Deck.Cards.Any(c => c is Zap);
        }
        if (owner.Character is Necrobinder)
        {
            return owner.Deck.Cards.Any(c => c is Bodyguard);
        }
        return false;
    }

    public List<CardModel>? GetTransformCards(Player owner)
    {
        if (owner.Character is Ironclad)
        {
            return owner.Deck.Cards.Where(c => c is StrikeIronclad).ToList();
        }
        if (owner.Character is Silent)
        {
            return owner.Deck.Cards.Where(c => c is Survivor).ToList();
        }
        if (owner.Character is Regent)
        {
            return owner.Deck.Cards.Where(c => c is Venerate).ToList();
        }
        if (owner.Character is Defect)
        {
            return owner.Deck.Cards.Where(c => c is Zap).ToList();
        }
        if (owner.Character is Necrobinder)
        {
            return owner.Deck.Cards.Where(c => c is Bodyguard).ToList();
        }
        return null;
    }

    // public CardModel GetTransformTarget(Player owner)
    // {
    //     if (owner.Character is Ironclad)
    //     {
    //         return owner.Deck.Cards.Where(c => c is StrikeIronclad).FirstOrDefault();
    //     }
    //     if (owner.Character is Silent)
    //     {
    //         return owner.Deck.Cards.Where(c => c is Survivor).FirstOrDefault();
    //     }
    //     if (owner.Character is Regent)
    //     {
    //         return owner.Deck.Cards.Where(c => c is Venerate).FirstOrDefault();
    //     }
    //     if (owner.Character is Defect)
    //     {
    //         return owner.Deck.Cards.Where(c => c is Zap).FirstOrDefault();
    //     }
    //     if (owner.Character is Necrobinder)
    //     {
    //         return owner.Deck.Cards.Where(c => c is Bodyguard).FirstOrDefault();
    //     }
    //     return null;
    // }

    public CardModel? GetTransformResult(Player owner)
    {
        if (owner.Character is Ironclad)
        {
            return ModelDb.Card<StrikePlus>().ToMutable();
        }
        if (owner.Character is Silent)
        {
            return ModelDb.Card<SurvivorPlus>().ToMutable();
        }
        if (owner.Character is Regent)
        {
            return ModelDb.Card<VeneratePlus>().ToMutable();
        }
        if (owner.Character is Defect)
        {
            return ModelDb.Card<ZapPlus>().ToMutable();
        }
        if (owner.Character is Necrobinder)
        {
            return ModelDb.Card<BodyGuardPlus>().ToMutable();
        }
        return null;
    }

    public override async Task AfterObtained()
    {
        foreach (
            CardModel item in await CardSelectCmd.FromDeckForRemoval(
                prefs: new CardSelectorPrefs(CardSelectorPrefs.TransformSelectionPrompt, 1),
                player: Owner!,
                filter: c => GetTransformCards(Owner)!.Contains(c)
            )
        )
        {
            await CardCmd.Transform(item, GetTransformResult(Owner)!);
        }
    }
}
