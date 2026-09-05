using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Characters;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Relics;

[RegisterRelic(typeof(SharedRelicPool))]
public class LoanSharking : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Ancient;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new EnergyVar(10)];

    public override RelicAssetProfile AssetProfile =>
        new(
            // 小图标（原版85x85）
            IconPath: $"res://ArknightsMap/images/relics/{GetType().Name}.png",
            // 轮廓图标（原版85x85）
            IconOutlinePath: $"res://ArknightsMap/images/relics/{GetType().Name}.png",
            // 大图标（原版256x256）
            BigIconPath: $"res://ArknightsMap/images/relics/{GetType().Name}.png"
        );

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.ForEnergy(this)];
    private int _restEnergy = 0;

    public override int DisplayAmount => _restEnergy;

    public override Task BeforeCombatStart()
    {
        _restEnergy = DynamicVars.Energy.IntValue;
        return Task.CompletedTask;
    }

    public override async Task AfterSideTurnEndLate(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side == CombatSide.Player && Owner.GetEnergy() == _restEnergy && _restEnergy > 0)
        {
            await PlayerCmd.LoseEnergy(1, Owner);
        }
    }

    [HarmonyPatch(typeof(PlayerCombatState), nameof(PlayerCombatState.ResetEnergy))]
    public static class ResetEnergyPatch
    {
        [HarmonyPostfix]
        public static void Postfix(PlayerCombatState __instance, Player ____player)
        {
            RelicModel? relic = ____player.GetRelic<LoanSharking>();
            if (relic != null)
            {
                LoanSharking loanSharking = (LoanSharking)relic;
                __instance.Energy += loanSharking._restEnergy;
            }
        }
    }

    [HarmonyPatch(typeof(PlayerCombatState), nameof(PlayerCombatState.LoseEnergy))]
    public static class LoseEnergyPatch
    {
        [HarmonyPostfix]
        public static void Postfix(PlayerCombatState __instance, Player ____player)
        {
            RelicModel? relic = ____player.GetRelic<LoanSharking>();
            if (relic != null)
            {
                LoanSharking loanSharking = (LoanSharking)relic;
                loanSharking._restEnergy = Math.Min(loanSharking._restEnergy, __instance.Energy);
            }
        }
    }
}
