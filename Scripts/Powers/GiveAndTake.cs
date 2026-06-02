using ArknightsMap.Scripts.Monsters;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Powers;

[RegisterPower]
public class GiveAndTake : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    protected override IEnumerable<DynamicVar> CanonicalVars => [];
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => HoverTipFactory.FromPowerWithPowerHoverTips<FlamingDamagePower>();

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"res://ArknightsMap/images/powers/{GetType().Name}.png",
        BigIconPath: $"res://ArknightsMap/images/powers/{GetType().Name}.png"
    );

    public override async Task AfterModifyingPowerAmountReceived(PowerModel modifiedPower)
    {
        if (modifiedPower == this && Owner.Monster is TheLeader ld)
        {
            if (Amount >= 50 && !ld._isstage2 && Owner.Monster.NextMove.Id != "IGNITE1")
            {
                Owner.Monster.SetMoveImmediate((MoveState)Owner.Monster.MoveStateMachine.States["RETURN_FIRE1"]);
            }
            else if (ld._isstage2 && Owner.Monster.NextMove.Id == "RETURN_FIRE2")
            {
                Owner.Monster.SetMoveImmediate((MoveState)Owner.Monster.MoveStateMachine.States["RETURN_FIRE2"]);
            }
        }
    }
}