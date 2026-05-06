using Godot;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Relics;

[RegisterRelic(typeof(SharedRelicPool))]
public class SoulSpark : ModRelicTemplate
{
	public override RelicRarity Rarity => RelicRarity.Ancient;

	protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(20m, ValueProp.Unpowered)];

	public override RelicAssetProfile AssetProfile => new(
		// 小图标（原版85x85）
		IconPath: $"res://ArknightsMap/images/relics/{GetType().Name}.png",
		// 轮廓图标（原版85x85）
		IconOutlinePath: $"res://ArknightsMap/images/relics/{GetType().Name}.png",
		// 大图标（原版256x256）
		BigIconPath: $"res://ArknightsMap/images/relics/{GetType().Name}.png"
	);

	public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
	{
		if (player == Owner)
		{
			ICombatState combatState = player.Creature.CombatState;
			if (combatState.RoundNumber == 1)
			{
				Flash();
				VfxCmd.PlayOnCreatureCenters(combatState.HittableEnemies, "vfx/vfx_attack_slash");
				if (Owner.RunState.CurrentRoom == null || Owner.RunState.CurrentRoom.RoomType != RoomType.Boss || Owner.RunState.CurrentActIndex != 1)
				{
					await CreatureCmd.Damage(choiceContext, combatState.HittableEnemies, base.DynamicVars.Damage, base.Owner.Creature);
				}
				else
				{
					foreach (Creature hittableEnemy in combatState.HittableEnemies)
					{
						await CreatureCmd.Damage(choiceContext, hittableEnemy, new DamageVar(hittableEnemy.CurrentHp / 2, ValueProp.Unpowered), base.Owner.Creature);
					}
				}
			}
		}
	}
}
