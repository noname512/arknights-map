using ArknightsMap.Scripts.Utils;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Powers;

[RegisterPower]
public class ChainReactionPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Single;
    protected override IEnumerable<DynamicVar> CanonicalVars => [];
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [];

    // 自定义图标路径。1:1即可。原版游戏大图256x256，小图64x64。
    public override PowerAssetProfile AssetProfile =>
        new(IconPath: $"res://ArknightsMap/images/powers/{GetType().Name}.png", BigIconPath: $"res://ArknightsMap/images/powers/{GetType().Name}.png");

    public override async Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
    {
        GD.Print($"creature: {creature.SlotName}, Owner: {Owner.SlotName}");
        if (wasRemovalPrevented || creature != Owner)
        {
            return;
        }
        await Cmd.Wait(1.23f);
        int pos = CreaturePositions.PositionOfCreature(Owner);
        List<Creature> targets = [];
        GD.Print($"current pos: {pos}");
        if (pos != 1)
        {
            targets.AddRange(CreaturePositions.GetCreaturesInPosition(pos - 1));
        }
        if (pos != 9)
        {
            targets.AddRange(CreaturePositions.GetCreaturesInPosition(pos + 1));
        }
        GD.Print($"targets: {targets.Count()}, dmg: {Owner.MaxHp}");
        if (targets.Any())
        {
            await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), targets, new DamageVar(Owner.MaxHp, ValueProp.Unpowered), null, null, null);
        }
    }
}
