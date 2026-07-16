using Archetto.Scripts.Cards.Basic;
using Archetto.Scripts.Cards.Track;
using Archetto.Scripts.Pools;

using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;

using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rooms;
using STS2RitsuLib.Scaffolding.Content;
using Archetto.Scripts.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.CardSelection;
using Archetto.Scripts.Enchant;
using Archetto.Scripts.Enums;
using Archetto.Scripts.Cards.Ancient;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models.RelicPools;

namespace ArknightsMap.Scripts.Relics;



[RegisterRelic(typeof(SharedRelicPool))]
public sealed class GiantGun : ModRelicTemplate

{
    public override RelicRarity Rarity => RelicRarity.Ancient;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(3)];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips
 => [
            
        ];

    
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
		if (player == base.Owner && base.Owner.PlayerCombatState.TurnNumber == 1)
        {
            await PowerCmd.Apply<ThornsPower>(choiceContext, base.Owner.Creature, 6, base.Owner.Creature, null);
        }
    }

    [HarmonyPatch(typeof(ThornsPower),"BeforeDamageReceived")]
public static class ThornsPowerBeforePatch
{
    
    [HarmonyPostfix]
    public static void Postfix(ThornsPower __instance, PlayerChoiceContext choiceContext, Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
        {
            if (target == __instance.Owner && dealer != null && (props.IsPoweredAttack() || cardSource is Omnislice))
		{
			foreach (Creature m in __instance.CombatState.HittableEnemies)
                {
                    if (m != dealer)
                    {
                        CreatureCmd.Damage(choiceContext, m, __instance.Amount, ValueProp.Unpowered | ValueProp.SkipHurtAnim, __instance.Owner, null, null);
                    }
                }
			
		}
        }
}
	
                
}