using ArknightsMap.Scripts.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Relics;

[RegisterRelic(typeof(SharedRelicPool))]
public class BlizzardCrossbow : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Ancient;

    private int _extraValue = 0;

    [SavedProperty]
    public int ExtraValue
    {
        get => _extraValue;
        set
        {
            AssertMutable();
            _extraValue = value;
        }
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(1 + ExtraValue), new ExtraDamageVar(5 + ExtraValue)];
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [StaticTips.SnowStorm];

    public override RelicAssetProfile AssetProfile =>
        new(
            // 小图标（原版85x85）
            IconPath: $"res://ArknightsMap/images/relics/{GetType().Name}.png",
            // 轮廓图标（原版85x85）
            IconOutlinePath: $"res://ArknightsMap/images/relics/{GetType().Name}.png",
            // 大图标（原版256x256）
            BigIconPath: $"res://ArknightsMap/images/relics/{GetType().Name}.png"
        );

    private bool Added = false;
    private int _currentPlayed = 0;
    public override int DisplayAmount => _currentPlayed;
    public override bool ShowCounter => true;

    public override Task BeforeCombatStart()
    {
        Added = false;
        _currentPlayed = 0;
        InvokeDisplayAmountChanged();
        return Task.CompletedTask;
    }

    public Task OnBlowWind()
    {
        if (!Added)
        {
            ExtraValue++;
            DynamicVars.Cards.BaseValue++;
            DynamicVars.ExtraDamage.BaseValue++;
            Added = true;
        }
        return Task.CompletedTask;
    }

    public override decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource, CardPlay? cardPlay)
    {
        if (cardSource != null && cardSource.Owner == Owner && cardSource.Type == CardType.Attack && _currentPlayed < DynamicVars.Cards.IntValue)
        {
            return DynamicVars.ExtraDamage.IntValue;
        }
        return 0;
    }

    public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner == Owner && cardPlay.Card.Type == CardType.Attack)
        {
            _currentPlayed++;
            InvokeDisplayAmountChanged();
        }
        return Task.CompletedTask;
    }
}
