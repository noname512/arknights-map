<<<<<<< Updated upstream
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
=======

using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
>>>>>>> Stashed changes
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Relics;

[RegisterRelic(typeof(SharedRelicPool))]
public sealed class MementoMori : ModRelicTemplate
<<<<<<< Updated upstream
=======

>>>>>>> Stashed changes
{
    public override RelicRarity Rarity => RelicRarity.Ancient;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(10, ValueProp.Unpowered)];
<<<<<<< Updated upstream

=======
    
>>>>>>> Stashed changes
    public override RelicAssetProfile AssetProfile =>
        new(
            // 小图标（原版85x85）
            IconPath: $"res://ArknightsMap/images/relics/{GetType().Name}.png",
            // 轮廓图标（原版85x85）
            IconOutlinePath: $"res://ArknightsMap/images/relics/{GetType().Name}.png",
            // 大图标（原版256x256）
            BigIconPath: $"res://ArknightsMap/images/relics/{GetType().Name}.png"
        );

    public int turn = 0;

    public override int DisplayAmount
    {
<<<<<<< Updated upstream
        get { return turn; }
    }

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side == CombatSide.Player && (Owner.PlayerCombatState?.TurnNumber == 1 || turn == 4))
        {
            Flash();
            await Shoot(combatState);
=======
        get
        {
            return turn;
        }
    }

    public override Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        
        if (side == CombatSide.Player && (base.Owner.PlayerCombatState?.TurnNumber == 1 || turn == 4))
        {
            Flash();
            Shoot(combatState);
>>>>>>> Stashed changes
        }
        else
        {
            turn++;
            InvokeDisplayAmountChanged();
        }
<<<<<<< Updated upstream
    }

    public async Task Shoot(ICombatState combatState)
=======
        return base.AfterSideTurnStart(side, participants, combatState);
    }

    public void Shoot(ICombatState combatState)
>>>>>>> Stashed changes
    {
        if (combatState != null)
        {
            turn = 0;
            InvokeDisplayAmountChanged();
<<<<<<< Updated upstream
            for (int i = 0; i < 8; i++)
            {
                await CreatureCmd.Damage(
                    new ThrowingPlayerChoiceContext(),
                    Owner.Creature.CombatState!.HittableEnemies.TakeRandom(1, Owner.RunState.Rng.CombatTargets),
                    DynamicVars.Damage,
                    Owner.Creature
                );
            }
        }
    }
}
=======
            DamageCmd.Attack(DynamicVars.Damage.IntValue)
            .WithHitCount(8)
            .Targeting(combatState.HittableEnemies.TakeRandom(1, base.Owner.RunState.Rng.CombatTargets).FirstOrDefault());
        }
    }

    

    
}
>>>>>>> Stashed changes
