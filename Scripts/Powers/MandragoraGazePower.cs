using MegaCrit.Sts2.Core.Commands;
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
public class MandragoraGazePower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;
    protected override IEnumerable<DynamicVar> CanonicalVars => [];

    // 自定义图标路径。1:1即可。原版游戏大图256x256，小图64x64。
    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"res://ArknightsMap/images/powers/{GetType().Name}.png",
        BigIconPath: $"res://ArknightsMap/images/powers/{GetType().Name}.png"
    );

    // TODO: 是永久的吗？
    // public virtual async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    // {
    //     if (cardPlay.Card.Type == CardType.Skill)
    //     {
    //         await DamageCmd.Attack(Amount).Execute(null);
    //     }
    // }
}