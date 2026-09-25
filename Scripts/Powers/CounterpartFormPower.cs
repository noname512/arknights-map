using ArknightsMap.Scripts.Cards;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Powers;

[RegisterPower]
public class CounterpartFormPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(2), new IntVar("CurrentCards", 0)];
    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

    // 自定义图标路径。1:1即可。原版游戏大图256x256，小图64x64。
    public override PowerAssetProfile AssetProfile =>
        new(IconPath: $"res://ArknightsMap/images/powers/{GetType().Name}.png", BigIconPath: $"res://ArknightsMap/images/powers/{GetType().Name}.png");

    public override async Task AfterCardExhausted(PlayerChoiceContext choiceContext, CardModel card, bool causedByEthereal)
    {
        if (card is EmergencyHeater)
        {
            DynamicVars["CurrentCards"].BaseValue++;
        }
    }

    public override bool ShouldAllowTargeting(Creature target)
    {
        UpdateCurrentCards();
        if (DynamicVars["CurrentCards"].IntValue < DynamicVars.Cards.IntValue)
        {
            return true;
        }
        if ((target.CombatState?.Enemies.Contains(target) ?? false) && !target.HasPower<TauntPower>())
        {
            return false;
        }
        return true;
    }

    public override decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource, CardPlay? cardPlay)
    {
        if (Owner != target)
            return 0;
        UpdateCurrentCards();
        return -Amount * DynamicVars["CurrentCards"].IntValue;
    }

    public override Task AfterModifyingHpLostAfterOsty()
    {
        if (DynamicVars["CurrentCards"].IntValue > 0)
        {
            Flash();
        }
        return Task.CompletedTask;
    }

    private void UpdateCurrentCards()
    {
        int cnt = 0;
        foreach (CardModel c in Target!.Player!.PlayerCombatState!.ExhaustPile.Cards)
            if (c is EmergencyHeater)
            {
                cnt++;
            }
        DynamicVars["CurrentCards"].BaseValue = cnt;
    }
}
