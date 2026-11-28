using ArknightsMap.Scripts.Cards;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Powers;

[RegisterPower]
public class WelcomePower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<DynamicVar> CanonicalVars => new[] { new IntVar("Time", 0) };

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.FromCard<Milk>()];

    public override PowerAssetProfile AssetProfile =>
        new(IconPath: $"res://ArknightsMap/images/powers/{GetType().Name}.png", BigIconPath: $"res://ArknightsMap/images/powers/{GetType().Name}.png");

    public override int DisplayAmount => DynamicVars["Time"].IntValue;

    private int MilkCounter = 0;

    public int Time
    {
        get => MilkCounter;
        set
        {
            DynamicVars["Time"].BaseValue = value;
            InvokeDisplayAmountChanged();
        }
    }

    public override async Task AfterCardPlayedLate(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (DynamicVars["Time"].BaseValue > 0 && cardPlay.Card is Milk)
        {
            MilkCounter++;
            if (MilkCounter >= 4 * CombatState.Players.Count)
            {
                DynamicVars["Time"].BaseValue--;
                await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Owner, 1m, null, null);
                InvokeDisplayAmountChanged();
                MilkCounter = 0; 
            }
        }
    }

    public async Task UpdateTime(int time)
    {

        DynamicVars["Time"].BaseValue = time;
        InvokeDisplayAmountChanged();
    }
}
