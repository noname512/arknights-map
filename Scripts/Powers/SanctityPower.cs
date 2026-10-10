using ArknightsMap.Scripts.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Powers;

[RegisterPower]
public class SanctityPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar("direction", 1)];
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [StaticTips.SnowStorm];

    // 自定义图标路径。1:1即可。原版游戏大图256x256，小图64x64。
    public override PowerAssetProfile AssetProfile =>
        new(IconPath: $"res://ArknightsMap/images/powers/{GetType().Name}.png", BigIconPath: $"res://ArknightsMap/images/powers/{GetType().Name}.png");

    public void SetDirection(int dir)
    {
        DynamicVars["direction"].BaseValue = dir;
    }

    public override async Task AfterCardPlayedLate(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target != Owner)
        {
            return;
        }
        if (cardPlay.IsAutoPlay)
        {
            return;
        }
        foreach (Player p in Owner.CombatState!.Players)
        {
            PlayerCmd.EndTurn(p, false);
        }
        await CreaturePositions.BlowWind(DynamicVars["direction"].IntValue, Owner.CombatState);
    }

    public override bool TryModifyPowerAmountReceived(PowerModel canonicalPower, Creature target, decimal amount, Creature? applier, out decimal modifiedAmount)
    {
        if (target == Owner && (canonicalPower.Type == PowerType.Debuff || canonicalPower.GetTypeForAmount(amount) == PowerType.Debuff))
        {
            modifiedAmount = 0;
            return true;
        }
        modifiedAmount = amount;
        return false;
    }
}
