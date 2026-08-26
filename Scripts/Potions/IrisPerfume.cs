using ArknightsMap.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.PotionPools;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Potions;

[RegisterPotion(typeof(TokenPotionPool))]
public class IrisPerfume : ModPotionTemplate
{
    // 稀有度
    public override PotionRarity Rarity => PotionRarity.Token;

    public override PotionUsage Usage => PotionUsage.CombatOnly;

    public override TargetType TargetType => TargetType.AnyPlayer;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new HealVar(20), new EnergyVar(2), new CardsVar(2)];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [];

    public override PotionAssetProfile AssetProfile => new(ImagePath: $"res://ArknightsMap/images/potions/{GetType().Name}.png", OutlinePath: $"res://ArknightsMap/images/potions/{GetType().Name}.png");

    protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
    {
        await CreatureCmd.Heal(target!, target!.MaxHp * DynamicVars.Heal.IntValue/100m, true);
        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.IntValue, target!.Player!);
        await PlayerCmd.GainEnergy(DynamicVars.Energy.IntValue, target!.Player!);
    }
}
