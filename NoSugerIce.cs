using Archetto.Scripts.Enums;
using Archetto.Scripts.Pools;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Characters;
using STS2RitsuLib.Scaffolding.Content;

namespace Archetto.Scripts.Relics.Ancient.Paganini;

[RegisterRelic(typeof(SharedRelicPool))]
public sealed class NoSugarIce : ModRelicTemplate

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

    public override Task AfterSideTurnEndLate(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (base.Owner.GetEnergy() > 0 && side == CombatSide.Player)
        {
            for (int i = 0; i < base.Owner.GetEnergy(); i++)
            {
                CreatureCmd.GainBlock(base.Owner.Creature, 9, ValueProp.Unpowered,null);
            }
        }
        return base.AfterSideTurnEndLate(choiceContext, side, participants);
    }
        
    
    }