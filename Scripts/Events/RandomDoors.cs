using ArknightsMap.Scripts.Acts;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.PotionPools;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Events;

[RegisterActEvent(typeof(Laterano))]
public sealed class RandomDoors : ModEventTemplate
{
    public override EventAssetProfile AssetProfile => new(InitialPortraitPath: $"res://ArknightsMap/images/events/{GetType().Name}.png");

    private string DegenbrecherPortraitPath => $"res://ArknightsMap/images/events/{GetType().Name}Degenbrecher.png";
    private string KnightsPortraitPath => $"res://ArknightsMap/images/events/{GetType().Name}Knights.png";
    private int NumberOfDoors;
    private int LastDoor = -1;
    private int CurrentHpLoss => 3 + NumberOfDoors;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("HpLoss", CurrentHpLoss)];

    protected override IReadOnlyList<EventOption> GenerateInitialOptions() =>
        [new EventOption(this, OpenDoor, InitialOptionKey("OPEN_DOOR")), new EventOption(this, Leave, InitialOptionKey("LEAVE"))];

    private async Task OpenDoor()
    {
        EventOption option = GetNewRandomOption(out string page);
        SetEventState(
            L10NLookup($"{Id.Entry}.pages.{page}.description"),
            [option, new EventOption(this, ReopenDoor, ModOptionKey("COMMON", "CHANGE_DOOR")).ThatDoesDamage(CurrentHpLoss)]
        );
    }

    private async Task ReopenDoor()
    {
        await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), Owner!.Creature, CurrentHpLoss, ValueProp.Unpowered, null, null);
        NumberOfDoors++;
        DynamicVars["HpLoss"].BaseValue = CurrentHpLoss;
        EventOption option = GetNewRandomOption(out string page);
        SetEventState(
            L10NLookup($"{Id.Entry}.pages.{page}.description"),
            [option, new EventOption(this, ReopenDoor, ModOptionKey("COMMON", "CHANGE_DOOR")).ThatDoesDamage(CurrentHpLoss)]
        );
    }

    private async Task Leave()
    {
        SetEventFinished(L10NLookup($"{Id.Entry}.pages.ENDING2.description"));
    }

    private EventOption GetNewRandomOption(out string page)
    {
        while (true)
        {
            float chance = Rng.NextFloat();
            if (chance > 0.8f)
            {
                if (LastDoor == 1)
                    continue;
                LastDoor = 1;
                page = "ATTENDANT";
                if (LocalContext.IsMe(Owner))
                    NEventRoom.Instance!.SetPortrait(PreloadManager.Cache.GetTexture2D(AssetProfile.InitialPortraitPath!));
                return new EventOption(this, Attendant, ModOptionKey(page, "ACCEPT"));
            }
            else if (chance > 0.6f)
            {
                if (LastDoor == 2)
                    continue;
                LastDoor = 2;
                page = "DEGENBRECHER";
                if (LocalContext.IsMe(Owner))
                    NEventRoom.Instance!.SetPortrait(PreloadManager.Cache.GetTexture2D(DegenbrecherPortraitPath));
                return new EventOption(this, Degenbrecher, ModOptionKey(page, "ACCEPT")).ThatDoesDamage(18);
            }
            else if (chance > 0.4f)
            {
                if (LastDoor == 3)
                    continue;
                LastDoor = 3;
                page = "KNIGHTS";
                if (LocalContext.IsMe(Owner))
                    NEventRoom.Instance!.SetPortrait(PreloadManager.Cache.GetTexture2D(KnightsPortraitPath));
                if (Owner!.Gold >= 50)
                {
                    return new EventOption(this, Knights, ModOptionKey(page, "ACCEPT"));
                }
                else
                {
                    return new EventOption(this, null, ModOptionKey(page, "ACCEPT_LOCKED"));
                }
            }
            else if (chance > 0.2f)
            {
                if (LastDoor == 4)
                    continue;
                LastDoor = 4;
                page = "VISITOR";
                if (LocalContext.IsMe(Owner))
                    NEventRoom.Instance!.SetPortrait(PreloadManager.Cache.GetTexture2D(AssetProfile.InitialPortraitPath!));
                return new EventOption(this, Visitor, ModOptionKey(page, "ACCEPT"));
            }
            else
            {
                chance *= 5 * 7;
                if (chance < 1)
                {
                    if (LastDoor == 5)
                        continue;
                    LastDoor = 5;
                    page = "AEGIR";
                    if (LocalContext.IsMe(Owner))
                        NEventRoom.Instance!.SetPortrait(PreloadManager.Cache.GetTexture2D(AssetProfile.InitialPortraitPath!));
                    return new EventOption(this, Aegir, ModOptionKey(page, "ACCEPT"));
                }
                else if (chance < 2)
                {
                    if (LastDoor == 6)
                        continue;
                    LastDoor = 6;
                    page = "ANGELINA";
                    if (LocalContext.IsMe(Owner))
                        NEventRoom.Instance!.SetPortrait(PreloadManager.Cache.GetTexture2D(AssetProfile.InitialPortraitPath!));
                    return new EventOption(this, Angelina, ModOptionKey(page, "ACCEPT"));
                }
                else if (chance < 3)
                {
                    if (LastDoor == 7)
                        continue;
                    LastDoor = 7;
                    page = "RHINE";
                    if (LocalContext.IsMe(Owner))
                        NEventRoom.Instance!.SetPortrait(PreloadManager.Cache.GetTexture2D(AssetProfile.InitialPortraitPath!));
                    return new EventOption(this, Rhine, ModOptionKey(page, "ACCEPT"));
                }
                else if (chance < 4)
                {
                    if (LastDoor == 8)
                        continue;
                    LastDoor = 8;
                    page = "EMPERORS_BLADE";
                    if (LocalContext.IsMe(Owner))
                        NEventRoom.Instance!.SetPortrait(PreloadManager.Cache.GetTexture2D(AssetProfile.InitialPortraitPath!));
                    return new EventOption(this, EmperorsBlade, ModOptionKey(page, "ACCEPT")).ThatDecreasesMaxHp(5);
                }
                else if (chance < 5)
                {
                    if (LastDoor == 9)
                        continue;
                    LastDoor = 9;
                    page = "SUI";
                    if (LocalContext.IsMe(Owner))
                        NEventRoom.Instance!.SetPortrait(PreloadManager.Cache.GetTexture2D(AssetProfile.InitialPortraitPath!));
                    return new EventOption(this, Sui, ModOptionKey(page, "ACCEPT"));
                }
                else if (chance < 6)
                {
                    if (LastDoor == 10)
                        continue;
                    LastDoor = 10;
                    page = "WARFARIN";
                    if (LocalContext.IsMe(Owner))
                        NEventRoom.Instance!.SetPortrait(PreloadManager.Cache.GetTexture2D(AssetProfile.InitialPortraitPath!));
                    return new EventOption(this, Warfarin, ModOptionKey(page, "ACCEPT"));
                }
                else
                {
                    if (LastDoor == 11)
                        continue;
                    LastDoor = 11;
                    page = "APHRISSA";
                    if (LocalContext.IsMe(Owner))
                        NEventRoom.Instance!.SetPortrait(PreloadManager.Cache.GetTexture2D(AssetProfile.InitialPortraitPath!));
                    return new EventOption(this, Aphrissa, ModOptionKey(page, "ACCEPT"));
                }
            }
        }
    }

    private async Task Attendant()
    {
        await CreatureCmd.GainMaxHp(Owner!.Creature, 2);
        SetEventFinished();
    }

    private async Task Degenbrecher()
    {
        await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), Owner!.Creature, 18, ValueProp.Unpowered, null, null);
        SetEventFinished();
    }

    private async Task Knights()
    {
        await PlayerCmd.LoseGold(50, Owner!);
        SetEventFinished();
    }

    private async Task Visitor()
    {
        await CreatureCmd.Heal(Owner!.Creature, 5);
        SetEventFinished();
    }

    private async Task Aegir()
    {
        CardCreationOptions options = CardCreationOptions
            .ForNonCombatWithDefaultOdds([ModelDb.CardPool<ColorlessCardPool>()])
            .WithFlags(CardCreationFlags.NoRarityModification | CardCreationFlags.NoCardPoolModifications);
        CardReward reward = new CardReward(options, 3, Owner!);
        await RewardsCmd.OfferCustom(Owner!, [reward]);
        SetEventFinished();
    }

    private async Task Angelina()
    {
        CardCreationOptions rerollOptions = CardCreationOptions.ForNonCombatWithDefaultOdds(Array.Empty<CardPoolModel>());
        IEnumerable<CardPoolModel> enumerable = Owner!
            .UnlockState.CharacterCardPools.Where((CardPoolModel p) => p != Owner.Character.CardPool)
            .ToList()
            .StableShuffle(Owner.RunState.Rng.Niche)
            .Take(3);
        List<CardModel> list = new List<CardModel>();
        foreach (CardPoolModel item in enumerable)
        {
            CardCreationOptions options = new CardCreationOptions([item], CardCreationSource.Other, CardRarityOddsType.RegularEncounter).WithFlags(
                CardCreationFlags.NoCardPoolModifications
            );
            list.Add(CardFactory.CreateForReward(Owner, 1, options).First().Card);
        }
        CardReward reward = new CardReward(list, CardCreationSource.Other, Owner, rerollOptions);
        await RewardsCmd.OfferCustom(Owner!, [reward]);
        SetEventFinished();
    }

    private async Task Rhine()
    {
        List<CardModel> list = PileType.Deck.GetPile(Owner!).Cards.Where((CardModel c) => c.IsUpgradable).ToList();
        if (list.Count != 0)
        {
            CardModel card = Rng.NextItem(list)!;
            CardCmd.Upgrade(card);
        }
        SetEventFinished();
    }

    private async Task EmperorsBlade()
    {
        await CreatureCmd.LoseMaxHp(new ThrowingPlayerChoiceContext(), Owner!.Creature, 5, false);
        SetEventFinished();
    }

    private async Task Sui()
    {
        CardCreationOptions options = CardCreationOptions
            .ForNonCombatWithUniformOdds([Owner!.Character.CardPool], c => c.Rarity == CardRarity.Rare)
            .WithFlags(CardCreationFlags.NoRarityModification);
        List<CardCreationResult> cards = CardFactory.CreateForReward(Owner, 4, options).ToList();
        foreach (CardCreationResult item in cards)
        {
            CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(item.Card, PileType.Deck));
        }
        SetEventFinished();
    }

    private async Task Warfarin()
    {
        IEnumerable<PotionModel> items = Owner!
            .Character.PotionPool.GetUnlockedPotions(Owner.UnlockState)
            .Concat(ModelDb.PotionPool<SharedPotionPool>().GetUnlockedPotions(Owner.UnlockState));
        PotionModel? potionModel = Owner.PlayerRng.Rewards.NextItem(items);
        if (potionModel != null)
        {
            await RewardsCmd.OfferCustom(Owner, [new PotionReward(potionModel.ToMutable(), Owner)]);
        }
        SetEventFinished();
    }

    private async Task Aphrissa()
    {
        List<CardModel> list = (
            await CardSelectCmd.FromDeckForTransformation(prefs: new CardSelectorPrefs(CardSelectorPrefs.TransformSelectionPrompt, 1), player: Owner!)
        ).ToList();
        foreach (CardModel item in list)
        {
            await CardCmd.TransformToRandom(item, base.Rng, CardPreviewStyle.EventLayout);
        }
        SetEventFinished();
    }

    private void SetEventFinished()
    {
        SetEventFinished(L10NLookup($"{Id.Entry}.pages.ENDING.description"));
    }

    public override bool IsAllowed(IRunState runState)
    {
        return runState.Act is SnowyMountain;
    }
}
