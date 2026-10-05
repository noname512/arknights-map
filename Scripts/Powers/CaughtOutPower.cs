using ArknightsMap.Scripts.Monsters;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Powers;

[RegisterPower]
public class CaughtOutPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
    protected override IEnumerable<DynamicVar> CanonicalVars => [];
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => HoverTipFactory.FromPowerWithPowerHoverTips<FlamingDamagePower>();

    // 自定义图标路径。1:1即可。原版游戏大图256x256，小图64x64。
    public override PowerAssetProfile AssetProfile =>
        new(IconPath: $"res://ArknightsMap/images/powers/{GetType().Name}.png", BigIconPath: $"res://ArknightsMap/images/powers/{GetType().Name}.png");
    private bool summoned = false;
    private int correctIntent = 0;

    public override async Task AfterDeath(PlayerChoiceContext choiceContext, Creature target, bool wasRemovalPrevented, float deathAnimLength)
    {
        if (wasRemovalPrevented || target != Owner)
        {
            return;
        }
        for (int i = 0; i < 3; i++)
        {
            Creature c = await CreatureCmd.Add<CabbageSeedling>(CombatState, $"seed{i + 1}");
            c.SetNodeVisible(false);
            TaskHelper.RunSafely(RevealSeedAfterDeathAnim(c, deathAnimLength));
        }
    }

    public override async Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (!participants.Contains(Owner))
        {
            return;
        }
        if (Owner.Monster!.IntendsToAttack)
        {
            correctIntent++;
        }
        if (correctIntent >= AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 6, 5))
        {
            await CreatureCmd.Kill(Owner);
        }
    }

    public async Task RevealSeedAfterDeathAnim(Creature c, float animLength)
    {
        await Cmd.CustomScaledWait(animLength, animLength);
        c.SetNodeVisible(true);
        await CreatureCmd.TriggerAnim(c, "Start", 0f);
    }

    public override bool ShouldStopCombatFromEnding()
    {
        return true;
    }
}
