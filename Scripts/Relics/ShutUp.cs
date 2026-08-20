using ArknightsMap.Scripts.Cards;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Models.Capabilities;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Relics;

[RegisterRelic(typeof(SharedRelicPool))]
public sealed class ShutUp : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Ancient;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(8, ValueProp.Unpowered)];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.FromCard<ShotgunCard>()];

    public override RelicAssetProfile AssetProfile =>
        new(
            // 小图标（原版85x85）
            IconPath: $"res://ArknightsMap/images/relics/{GetType().Name}.png",
            // 轮廓图标（原版85x85）
            IconOutlinePath: $"res://ArknightsMap/images/relics/{GetType().Name}.png",
            // 大图标（原版256x256）
            BigIconPath: $"res://ArknightsMap/images/relics/{GetType().Name}.png"
        );


    public override async Task<Task> AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player == Owner && player.Creature is not null && player.Creature.CombatState is not null)
        {
            foreach (Creature c in player.Creature.CombatState.GetOpponentsOf(player.Creature))
            {
                if (c.Monster is not null && c.Monster.GetIntents().Any(i => i is not SingleAttackIntent && i is not MultiAttackIntent))
                {
                    await CreatureCmd.Damage(choiceContext, c, DynamicVars.Damage, Owner.Creature);
                    await PowerCmd.Apply<VulnerablePower>(choiceContext, c, 1, Owner.Creature, null);
                }
            }
        }
        return base.AfterPlayerTurnStart(choiceContext, player);
    }
    
}