using ArknightsMap.Scripts.Acts;
using ArknightsMap.Scripts.Enchantments;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Events;

[RegisterActEvent(typeof(Wilds))]
public sealed class Campfire : ModEventTemplate
{
    public override EventAssetProfile AssetProfile => new(InitialPortraitPath: $"res://ArknightsMap/images/events/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [
            new DamageVar(24m, ValueProp.Unblockable | ValueProp.Unpowered),
            new HpLossVar(8),
            new HealVar(0),
            new StringVar("EnchantedCard"),
            new DynamicVar("HpGain", 1),
        ];

    protected override Task BeforeEventStarted(bool isPreFinished)
    {
        DynamicVars.Heal.BaseValue = (decimal)(Owner!.Creature.MaxHp * 0.4);
        return Task.CompletedTask;
    }

    protected override IReadOnlyList<EventOption> GenerateInitialOptions()
    {
        
        List<EventOption> list = new List<EventOption>();
        if (!HasAttackCard(Owner))
        {
            list.Add(new EventOption(this, null, InitialOptionKey("LOCKED")));
        }
        else
        {
            list.Add(new EventOption(this, AttachFlaming, InitialOptionKey("ATTACH_FLAMING"), HoverTipFactory.FromEnchantment<Flaming>())
                .ThatDoesDamage(DynamicVars.Damage.IntValue));
        }
        list.Add(new EventOption(this, LoseMaxHpAndHeal, InitialOptionKey("LOSE_MAX_HP_AND_HEAL")));
        list.Add(new EventOption(this, GainMaxHp, InitialOptionKey("GAIN_MAX_HP")));
        return list;
    }

    public bool HasAttackCard(Player player)
    {
        EnchantmentModel enchantment = ModelDb.Enchantment<Flaming>();
        return PileType.Deck.GetPile(player).Cards.Any((CardModel c) => enchantment.CanEnchant(c));
    }

    private async Task AttachFlaming()
    {
        await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), Owner!.Creature, DynamicVars.Damage, null, null);
        foreach (
            CardModel item in await CardSelectCmd.FromDeckForEnchantment(
                Owner,
                ModelDb.Enchantment<Flaming>(),
                1,
                new CardSelectorPrefs(CardSelectorPrefs.EnchantSelectionPrompt, 1)
            )
        )
        {
            CardCmd.Enchant<Flaming>(item, 1);
            NCardEnchantVfx? nCardEnchantVfx = NCardEnchantVfx.Create(item);
            if (nCardEnchantVfx != null)
            {
                NRun.Instance?.GlobalUi.CardPreviewContainer.AddChildSafely(nCardEnchantVfx);
            }
            StringVar stringVar = (StringVar)DynamicVars["EnchantedCard"];
            stringVar.StringValue = item.Title;
        }
        SetEventFinished(L10NLookup($"{Id.Entry}.pages.ATTACH_FLAMING.description"));
    }

    private async Task LoseMaxHpAndHeal()
    {
        await CreatureCmd.LoseMaxHp(new ThrowingPlayerChoiceContext(), Owner!.Creature, DynamicVars.HpLoss.BaseValue, false);
        await CreatureCmd.Heal(Owner!.Creature, DynamicVars.Heal.BaseValue, true);
        SetEventFinished(L10NLookup($"{Id.Entry}.pages.LOSE_MAX_HP_AND_HEAL.description"));
    }

    private async Task GainMaxHp()
    {
        await CreatureCmd.GainMaxHp(Owner!.Creature, DynamicVars["HpGain"].BaseValue);
        SetEventFinished(L10NLookup($"{Id.Entry}.pages.GAIN_MAX_HP.description"));
    }
}
