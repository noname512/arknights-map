using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Powers;

[RegisterPower]
public class ExFoederePower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar("UseTime", 0)];

    public override PowerAssetProfile AssetProfile =>
        new(IconPath: $"res://ArknightsMap/images/powers/{GetType().Name}.png", BigIconPath: $"res://ArknightsMap/images/powers/{GetType().Name}.png");


    public override int DisplayAmount => (int)DynamicVars["UseTime"].BaseValue;

    public override Task AfterCardPlayedLate(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (DynamicVars["UseTime"].BaseValue < 6)
        {
            PowerCmd.Apply<StrengthPower>(choiceContext, base.Owner, 1, base.Owner, null);
            DynamicVars["UseTime"].BaseValue++;
            InvokeDisplayAmountChanged();
        }
        else
        {
            PowerCmd.Apply<StrengthPower>(choiceContext, base.Owner, -7, base.Owner, null);
            DynamicVars["UseTime"].BaseValue = 0;
            InvokeDisplayAmountChanged();
        }
        
        return base.AfterCardPlayedLate(choiceContext, cardPlay);
    }

    
}