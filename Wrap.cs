using Archetto.Scripts.Enums;
using Archetto.Scripts.Pools;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.RelicPools;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;

namespace Archetto.Scripts.Relics.Ancient.Paganini;

[RegisterRelic(typeof(SharedRelicPool))]
public sealed class Wrap : ModRelicTemplate

{
    public override RelicRarity Rarity => RelicRarity.Ancient;

    protected override IEnumerable<DynamicVar> CanonicalVars => [];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips
 => [
            
        ];

    
    // 小图标
    public override string PackedIconPath => $"res://Archetto/images/relic/{Id.Entry.ToLowerInvariant()}.png";
    // 轮廓图标
    protected override string PackedIconOutlinePath => $"res://Archetto/images/relic/{Id.Entry.ToLowerInvariant()}.png";
    // 大图标
    protected override string BigIconPath => $"res://Archetto/images/relic/{Id.Entry.ToLowerInvariant()}.png";
    // 

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
	{
		if (player == base.Owner && base.Owner.PlayerCombatState.TurnNumber == 1)
        {
            await PowerCmd.Apply<PlatingPower>(choiceContext, base.Owner.Creature, 12, base.Owner.Creature, null);
        }
    }
    }