using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Powers;

[RegisterPower]
public class EmotionExpansionPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => HoverTipFactory.FromPowerWithPowerHoverTips<StrengthPower>();

    public int currentAmount = 0;



    public override PowerAssetProfile AssetProfile =>
        new(IconPath: $"res://ArknightsMap/images/powers/{GetType().Name}.png", BigIconPath: $"res://ArknightsMap/images/powers/{GetType().Name}.png");


    public override async Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        
    }

    public override async Task BeforeApplied(Creature target, decimal amount, Creature? applier, CardModel? cardSource)
    {
        int currHp = target.CurrentHp;
        int maxHp = target.MaxHp;
        if (currentAmount == 0)
        {
            await CreatureCmd.SetMaxHp(target, amount* maxHp);
            await CreatureCmd.SetCurrentHp(target, amount* currHp);
        }
        else
        {
            await CreatureCmd.SetMaxHp(target, (currentAmount + amount)/currentAmount * maxHp);
            await CreatureCmd.SetCurrentHp(target, (currentAmount + amount)/currentAmount * currHp);
        }

        currentAmount += (int)amount;

        
    }
}
