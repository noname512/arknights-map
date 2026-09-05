using ArknightsMap.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Relics;

[RegisterRelic(typeof(SharedRelicPool))]
public sealed class GiantGun : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Ancient;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(3)];

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

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player == Owner && Owner.PlayerCombatState!.TurnNumber == 1)
        {
            await PowerCmd.Apply<ThornsPower>(choiceContext, Owner.Creature, 6, Owner.Creature, null);
            await PowerCmd.Apply<ThornsEnhancePower>(choiceContext, Owner.Creature, 1, Owner.Creature, null);
        }
    }

    public override async Task BeforeDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource
    )
    {
        if (target != Owner.Creature)
        {
            return;
        }
        if (target.Player?.GetRelic<GiantGun>() == null)
        {
            return;
        }
        if (dealer == null)
        {
            return;
        }
        if (!props.IsPoweredAttack() && cardSource is not Omnislice)
        {
            return;
        }
        if (target.GetPowerAmount<ThornsPower>() <= 0 || target.GetPower<ThornsEnhancePower>() == null)
        {
            return;
        }

        Flash();

        var ownerCreature = Owner.Creature;
        var hittableEnemies = ownerCreature?.CombatState?.HittableEnemies;
        if (ownerCreature == null || hittableEnemies == null)
        {
            return;
        }

        foreach (Creature c in hittableEnemies)
        {
            if (c != dealer)
            {
                await CreatureCmd.Damage(
                    choiceContext,
                    c,
                    Owner.Creature.GetPowerAmount<ThornsPower>(),
                    ValueProp.Unpowered | ValueProp.SkipHurtAnim,
                    Owner.Creature,
                    null,
                    null
                );
            }
        }
    }
}
