using Archetto.Scripts.Cards.Ancient;
using Archetto.Scripts.Enums;
using Archetto.Scripts.Pools;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.RelicPools;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;

namespace Archetto.Scripts.Relics.Ancient.Paganini;

[RegisterRelic(typeof(SharedRelicPool))]
public sealed class Aphasia : ModRelicTemplate

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

    public override async Task AfterObtained()
	{
        await CardPileCmd.AddCurseToDeck<NoCommunication>(base.Owner);
    }

    public override Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
        {
            if (card.Owner == base.Owner && card.Type == CardType.Curse)
            {
                PowerCmd.Apply<StrengthPower>(choiceContext, base.Owner.Creature, 2, base.Owner.Creature, null);
            }
            return base.AfterCardDrawn(choiceContext, card, fromHandDraw);
        }
    
    }