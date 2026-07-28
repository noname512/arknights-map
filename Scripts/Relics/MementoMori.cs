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
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Relics;

[RegisterRelic(typeof(SharedRelicPool))]
public sealed class MementoMori : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Ancient;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(10, ValueProp.Unpowered)];

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
        get { return turn; }
    }

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side == CombatSide.Player && (Owner.PlayerCombatState?.TurnNumber == 1 || turn == 4))
        {
            Flash();
            await Shoot(combatState);
        }
        else
        {
            turn++;
            InvokeDisplayAmountChanged();
        }
    }

    public async Task Shoot(ICombatState combatState)
    {
        if (combatState != null)
        {
            turn = 0;
            InvokeDisplayAmountChanged();
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
