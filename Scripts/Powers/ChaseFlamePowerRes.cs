using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Powers;

public class ChaseFlamePowerRes : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    // 叠加类型，Counter表示可叠加，Single表示不可叠加
    public override PowerStackType StackType => PowerStackType.Counter;
    public int CurState;

    // 自定义图标路径。1:1即可。原版游戏大图256x256，小图64x64。
    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"res://ArknightsMap/images/powers/ChaseFlamePower.png",
        BigIconPath: $"res://ArknightsMap/images/powers/ChaseFlamePower.png"
    );

    public override LocString Title => new LocString("powers", CurState == 0 ? "ARKNIGHTS_MAP_POWER_CHASE_FLAME_POWER.title" : "ARKNIGHTS_MAP_POWER_CHASE_FLAME_RES.title");
    public override LocString Description => new LocString("powers", CurState == 0 ? "ARKNIGHTS_MAP_POWER_CHASE_FLAME_POWER.description" : "ARKNIGHTS_MAP_POWER_CHASE_FLAME_RES.description");
    protected override string SmartDescriptionLocKey => CurState == 0 ? "ARKNIGHTS_MAP_POWER_CHASE_FLAME_POWER.smartDescription" : "ARKNIGHTS_MAP_POWER_CHASE_FLAME_RES.smartDescription";

}