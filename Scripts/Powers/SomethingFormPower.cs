using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Powers;

[RegisterPower]
public class SomethingFormPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public PowerStackType _stackType = PowerStackType.Single;
    public override PowerStackType StackType => _stackType;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new StringVar("cardName")];
    public CardModel? baseCard;

    public enum XType
    {
        XEnergy,
        XStar,
        None,
    }

    public XType xType = XType.None;

    // 自定义图标路径。1:1即可。原版游戏大图256x256，小图64x64。
    public override PowerAssetProfile AssetProfile =>
        new(IconPath: $"res://ArknightsMap/images/powers/{GetType().Name}.png", BigIconPath: $"res://ArknightsMap/images/powers/{GetType().Name}.png");

    public override Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        StringVar stringVar = (StringVar)DynamicVars["cardName"];
        stringVar.StringValue = baseCard!.Title;
        return Task.CompletedTask;
    }

    public override async Task AfterAutoPrePlayPhaseEntered(PlayerChoiceContext choiceContext, Player player)
    {
        if (player == base.Owner.Player)
        {
            await CardCmd.AutoPlay(choiceContext, baseCard!, null);
        }
    }

    public override int ModifyXValue(CardModel card, int originalValue)
    {
        if (card == baseCard && xType != XType.None)
            return Amount;
        return originalValue;
    }
}
