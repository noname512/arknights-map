using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Saves.Runs;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Relics;

[RegisterRelic(typeof(SharedRelicPool))]
public class TowardsTheMountainBow : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Ancient;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar("Turn", 1)];
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.Static(StaticHoverTip.Stun)];

    [SavedProperty]
    private int Turn
    {
        get { return DynamicVars["Turn"].IntValue; }
        set
        {
            AssertMutable();
            DynamicVars["Turn"].BaseValue = value;
        }
    }
    private bool usedThisCombat;

    public override int DisplayAmount => Turn;

    public override RelicAssetProfile AssetProfile =>
        new(
            // 小图标（原版85x85）
            IconPath: $"res://ArknightsMap/images/relics/{GetType().Name}.png",
            // 轮廓图标（原版85x85）
            IconOutlinePath: $"res://ArknightsMap/images/relics/{GetType().Name}.png",
            // 大图标（原版256x256）
            BigIconPath: $"res://ArknightsMap/images/relics/{GetType().Name}.png"
        );

    public override async Task BeforeSideTurnStart(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState
    )
    {
        if (side == Owner.Creature.Side && !usedThisCombat && Owner.PlayerCombatState!.TurnNumber == Turn)
        {
            Flash();
            foreach (var enemy in Owner.Creature.CombatState!.Enemies.Where(e => e.IsAlive))
            {
                await CreatureCmd.Stun(enemy);
            }
            usedThisCombat = true;
            Turn++;
            InvokeDisplayAmountChanged();
        }
    }

    public override async Task BeforeCombatStart()
    {
        usedThisCombat = false;
    }
}
