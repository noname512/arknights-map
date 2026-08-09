using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Rooms;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Relics;

[RegisterRelic(typeof(SharedRelicPool))]
public sealed class CactusTart : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Ancient;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new EnergyVar(1)];

    public int Turn = 0;

    public override RelicAssetProfile AssetProfile =>
        new(
            // 小图标（原版85x85）
            IconPath: $"res://ArknightsMap/images/relics/{GetType().Name}.png",
            // 轮廓图标（原版85x85）
            IconOutlinePath: $"res://ArknightsMap/images/relics/{GetType().Name}.png",
            // 大图标（原版256x256）
            BigIconPath: $"res://ArknightsMap/images/relics/{GetType().Name}.png"
        );

    public override bool ShowCounter => true;

    public override int DisplayAmount
    {
        get => Turn;
    }

    public override Task AfterCombatVictory(CombatRoom room)
    {
        Turn = 0;
        InvokeDisplayAmountChanged();
        return Task.CompletedTask;
    }

    public override decimal ModifyMaxEnergy(Player player, decimal amount)
    {
        if (player != Owner)
        {
            return amount;
        }
        return amount + DynamicVars.Energy.IntValue;
    }

    public override async Task BeforeHandDraw(Player player, PlayerChoiceContext choiceContext, ICombatState combatState)
    {
        if (player == Owner)
        {
            Flash();
            Turn++;
            InvokeDisplayAmountChanged();
            if (Turn <= 2)
            {
                foreach (Creature c in combatState.HittableEnemies)
                {
                    await PowerCmd.Apply<ThornsPower>(choiceContext, c, 2, Owner.Creature, null);
                }
            }
            
        }
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side == CombatSide.Enemy && Turn <= 2)
        {
            var ownerCreature = Owner?.Creature;
            var enemies = ownerCreature?.CombatState?.HittableEnemies;
            if (enemies is not null)
            {
                foreach (Creature c in enemies)
                {
                    await PowerCmd.Apply<ThornsPower>(choiceContext, c, -2, ownerCreature, null);
                }
            }
        }
    }
}