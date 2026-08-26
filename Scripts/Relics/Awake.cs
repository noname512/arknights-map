using ArknightsMap.Scripts.Cards;
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

namespace ArknightsMap.Scripts.Relics;

[RegisterRelic(typeof(SharedRelicPool))]
public sealed class Awake : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Event;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<StrengthPower>(1)];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromCard<Confused>()
    ];

    public override RelicAssetProfile AssetProfile =>
        new(
            // 小图标（原版85x85）
            IconPath: $"res://ArknightsMap/images/relics/{GetType().Name}.png",
            // 轮廓图标（原版85x85）
            IconOutlinePath: $"res://ArknightsMap/images/relics/{GetType().Name}.png",
            // 大图标（原版256x256）
            BigIconPath: $"res://ArknightsMap/images/relics/{GetType().Name}.png"
        );


    public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        if (card.Owner == Owner && card is Confused)
        {
            await CardCmd.Exhaust(choiceContext, card);
            await PowerCmd.Apply<StrengthPower>(choiceContext, Owner.Creature, DynamicVars["StrengthPower"].BaseValue, Owner.Creature, null);
        }
    }
}
