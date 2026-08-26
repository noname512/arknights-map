using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Relics;

[RegisterRelic(typeof(SharedRelicPool))]
public sealed class Hand : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Ancient;

    public override RelicAssetProfile AssetProfile =>
        new(
            // 小图标（原版85x85）
            IconPath: $"res://ArknightsMap/images/relics/{GetType().Name}.png",
            // 轮廓图标（原版85x85）
            IconOutlinePath: $"res://ArknightsMap/images/relics/{GetType().Name}.png",
            // 大图标（原版256x256）
            BigIconPath: $"res://ArknightsMap/images/relics/{GetType().Name}.png"
        );

    public override async Task<Task> AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        if (card.Owner == Owner && card.Type == MegaCrit.Sts2.Core.Entities.Cards.CardType.Attack 
        && card.DynamicVars.TryGetValue("Damage", out var damageVar) 
                && damageVar != null 
                && damageVar.BaseValue >= 15
                && this.Status == RelicStatus.Active)
        {
            Flash();
            foreach (Creature c in Owner.Creature.CombatState!.HittableEnemies)
            {
                await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), c, card.DynamicVars.Damage.BaseValue, ValueProp.Unpowered, base.Owner.Creature);
            }
            Status = RelicStatus.Disabled;
        }
        return base.AfterCardDrawn(choiceContext, card, fromHandDraw);
    }

    public override Task AfterCombatVictory(CombatRoom room)
    {
        Status = RelicStatus.Active;
        return base.AfterCombatVictory(room);
    }

}
