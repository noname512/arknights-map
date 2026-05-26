using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Saves.Runs;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Relics;

[RegisterRelic(typeof(SharedRelicPool))]
public class MarkOfTara : ModRelicTemplate
{
	public override RelicRarity Rarity => RelicRarity.Ancient;

	protected override IEnumerable<DynamicVar> CanonicalVars => [new HealVar(10m), new IntVar("Intangible", 3)];
	protected override IEnumerable<IHoverTip> AdditionalHoverTips => HoverTipFactory.FromPowerWithPowerHoverTips<IntangiblePower>();

	public override RelicAssetProfile AssetProfile => new(
		// 小图标（原版85x85）
		IconPath: $"res://ArknightsMap/images/relics/{GetType().Name}.png",
		// 轮廓图标（原版85x85）
		IconOutlinePath: $"res://ArknightsMap/images/relics/{GetType().Name}.png",
		// 大图标（原版256x256）
		BigIconPath: $"res://ArknightsMap/images/relics/{GetType().Name}.png"
	);
	private bool _wasUsed;
	public override bool IsUsedUp => _wasUsed;
	[SavedProperty]
	public bool WasUsed
	{
		get
		{
			return _wasUsed;
		}
		set
		{
			AssertMutable();
			_wasUsed = value;
			if (IsUsedUp)
			{
				base.Status = RelicStatus.Disabled;
			}
		}
	}

	public override bool ShouldDieLate(Creature creature)
	{
		if (creature != Owner.Creature)
		{
			return true;
		}
		if (WasUsed)
		{
			return true;
		}
		return false;
	}

	public override async Task AfterPreventingDeath(Creature creature)
	{
		Flash();
		WasUsed = true;
		await CreatureCmd.Heal(creature, DynamicVars.Heal.BaseValue);
		PlayerChoiceContext t = new ThrowingPlayerChoiceContext();
		if (Owner.Creature.CombatState != null)
		{
			await PowerCmd.Apply<IntangiblePower>(t, Owner.Creature, DynamicVars["Intangible"].IntValue, Owner.Creature, null, false);
		}
	}
}
