using ArknightsMap.Scripts.Enchantments;
using ArknightsMap.Scripts.Powers;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Characters;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Relics;

[RegisterRelic(typeof(SharedRelicPool))]
public class Luckily : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Ancient;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(5, ValueProp.Unpowered)];

    public override RelicAssetProfile AssetProfile =>
        new(
            // 小图标（原版85x85）
            IconPath: $"res://ArknightsMap/images/relics/{GetType().Name}.png",
            // 轮廓图标（原版85x85）
            IconOutlinePath: $"res://ArknightsMap/images/relics/{GetType().Name}.png",
            // 大图标（原版256x256）
            BigIconPath: $"res://ArknightsMap/images/relics/{GetType().Name}.png"
        );
    
    protected override IEnumerable<IHoverTip> AdditionalHoverTips
    {
        get
        {
            List<IHoverTip> list = new List<IHoverTip>();
            list.Add(HoverTipFactory.ForEnergy(this));
            return list;
        }
    }

    public override decimal ModifyPowerAmountGivenAdditive(PowerModel power, Creature giver, decimal amount, Creature? target,
        CardModel? cardSource)
    {
        if ((giver == Owner.Creature) && (target.Monster != null) && (power.GetTypeForAmount(amount) == PowerType.Debuff))
        {
            return amount + 1;
        }
        return amount;
    }
    
    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (!(amount == 0m) && power.GetTypeForAmount(amount) == PowerType.Debuff && power.Owner.IsEnemy && applier == Owner.Creature && !(power is ITemporaryPower))
        {
            Flash();
            await CreatureCmd.Damage(choiceContext, power.Owner, DynamicVars.Damage, Owner.Creature);
        }
    }    
}
