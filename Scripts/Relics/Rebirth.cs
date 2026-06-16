using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.RelicPools;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Relics;

[RegisterRelic(typeof(EventRelicPool))]
public class Rebirth : ModRelicTemplate
{
	public override RelicRarity Rarity => RelicRarity.Event;

	protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar("IntangibleAmount", 2)];
	protected override IEnumerable<IHoverTip> AdditionalHoverTips => HoverTipFactory.FromPowerWithPowerHoverTips<IntangiblePower>();

	public override RelicAssetProfile AssetProfile => new(
		// 小图标（原版85x85）
		IconPath: $"res://ArknightsMap/images/relics/{GetType().Name}.png",
		// 轮廓图标（原版85x85）
		IconOutlinePath: $"res://ArknightsMap/images/relics/{GetType().Name}.png",
		// 大图标（原版256x256）
		BigIconPath: $"res://ArknightsMap/images/relics/{GetType().Name}.png"
	);

	public override async Task BeforeCombatStart()
	{
		await PowerCmd.Apply<IntangiblePower>(new ThrowingPlayerChoiceContext(), Owner.Creature, DynamicVars["IntangibleAmount"].IntValue, Owner.Creature, null);
	}
}
