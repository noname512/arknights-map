using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.PotionPools;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Potions;

[RegisterPotion(typeof(TokenPotionPool))]
public class FireBomb : ModPotionTemplate
{
    // 稀有度
    public override PotionRarity Rarity => PotionRarity.Token;

    public override PotionUsage Usage => PotionUsage.CombatOnly;

    public override TargetType TargetType => TargetType.None;

    protected override IEnumerable<DynamicVar> CanonicalVars => [];

    public override PotionAssetProfile AssetProfile => new(
        ImagePath: "res://icon.svg",
        OutlinePath: "res://icon.svg"
    );
    protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
    {
        foreach (Creature c in Owner.Creature.CombatState!.Creatures)
        {
            await CreatureCmd.Damage(choiceContext, c, (int)(c.CurrentHp * 0.9), ValueProp.Unpowered, null, null);
        }
    }
}
