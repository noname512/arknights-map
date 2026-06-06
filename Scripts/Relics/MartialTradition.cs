using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models.RelicPools;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace ArknightsMap.Scripts.Relics;

[RegisterRelic(typeof(SharedRelicPool))]
public class MartialTradition : ModRelicTemplate
{
	public override RelicRarity Rarity => RelicRarity.Ancient;
	protected override IEnumerable<DynamicVar> CanonicalVars => [new EnergyVar(1)];

	public override RelicAssetProfile AssetProfile => new(
		// 小图标（原版85x85）
		IconPath: $"res://ArknightsMap/images/relics/{GetType().Name}.png",
		// 轮廓图标（原版85x85）
		IconOutlinePath: $"res://ArknightsMap/images/relics/{GetType().Name}.png",
		// 大图标（原版256x256）
		BigIconPath: $"res://ArknightsMap/images/relics/{GetType().Name}.png"
	);

	public override async Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
	{
		if (side == Owner.Creature.Side && combatState.RoundNumber <= 1)
		{
			Flash();
			foreach (var enemy in Owner.Creature.CombatState!.Enemies.Where(e => e.IsAlive))
			{
				await CreatureCmd.Stun(enemy);
			}
		}
	}

	public override async Task BeforeHandDraw(Player player, PlayerChoiceContext choiceContext, ICombatState combatState)
	{
		if (player == Owner && Owner.PlayerCombatState!.TurnNumber == 1)
		{
			await PlayerCmd.LoseEnergy(DynamicVars.Energy.BaseValue, Owner);
		}
	}

	public override bool ShouldPlay(CardModel card, AutoPlayType _)
	{
		if (card.Owner.Creature != Owner.Creature) return true;
		if (Owner.Creature.CombatState!.RoundNumber > 1) return true;
		if (card.Type != CardType.Attack) return true;
		return false;
	}
}
