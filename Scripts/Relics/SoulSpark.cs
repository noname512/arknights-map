using Godot;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models.RelicPools;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Relics;

[RegisterRelic(typeof(SharedRelicPool))]
public class SoulSpark : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Common;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::<>z__ReadOnlySingleElementList<DynamicVar>(new DamageVar(20m, ValueProp.Unpowered));

    public override RelicAssetProfile AssetProfile => new(
        // 小图标（原版85x85）
        IconPath: $"res://Test/images/relics/{GetType().Name}.png",
        // 轮廓图标（原版85x85）
        IconOutlinePath: $"res://Test/images/relics/{GetType().Name}.png",
        // 大图标（原版256x256）
        BigIconPath: $"res://Test/images/relics/{GetType().Name}.png"
    );

	public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
	{
		if (player == base.Owner)
		{
			CombatState combatState = player.Creature.CombatState;
			if (combatState.RoundNumber == 1)
			{
				Flash();
				VfxCmd.PlayOnCreatureCenters(combatState.HittableEnemies, "vfx/vfx_attack_slash");
				if (room == null || room.RoomType != RoomType.Boss || base.Owner.RunState.CurrentActIndex != 1)
				{
				    await CreatureCmd.Damage(choiceContext, combatState.HittableEnemies, base.DynamicVars.Damage, base.Owner.Creature);
				}
				else
				{
				    foreach (Creature hittableEnemy in base.CombatState.HittableEnemies)
                	{
                        NCreature nCreature = NCombatRoom.Instance?.GetCreatureNode(hittableEnemy);
                        if (nCreature != null)
                        {
				            await CreatureCmd.Damage(choiceContext, nCreature, nCreature.health / 2, base.Owner.Creature);
                        }
                    }
				}
			}
		}
	}
}
