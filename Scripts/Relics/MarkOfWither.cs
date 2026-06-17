using ArknightsMap.Scripts.Powers;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.RelicPools;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Relics;

[RegisterRelic(typeof(SharedRelicPool))]
public class MarkOfWither : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Ancient;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<StrengthPower>(3)];
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => HoverTipFactory.FromPowerWithPowerHoverTips<StrengthPower>();
    protected PlayerChoiceContext t = new ThrowingPlayerChoiceContext();

    public override RelicAssetProfile AssetProfile =>
        new(
            // 小图标（原版85x85）
            IconPath: $"res://ArknightsMap/images/relics/{GetType().Name}.png",
            // 轮廓图标（原版85x85）
            IconOutlinePath: $"res://ArknightsMap/images/relics/{GetType().Name}.png",
            // 大图标（原版256x256）
            BigIconPath: $"res://ArknightsMap/images/relics/{GetType().Name}.png"
        );

    public override async Task BeforeCombatStart()
    {
        Flash();
        IReadOnlyList<Creature> hittableEnemies = Owner.Creature.CombatState!.HittableEnemies;
        await PowerCmd.Apply<WitherPower>(t, hittableEnemies, DynamicVars["StrengthPower"].IntValue, null, null);
    }

    public override async Task AfterCreatureAddedToCombat(Creature creature)
    {
        if (creature.Side == CombatSide.Enemy)
        {
            Flash();
            await PowerCmd.Apply<WitherPower>(t, creature, DynamicVars["StrengthPower"].IntValue, null, null);
        }
    }
}
