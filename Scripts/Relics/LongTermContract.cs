using ArknightsMap.Scripts.Cards;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Relics;

[RegisterRelic(typeof(SharedRelicPool))]
public sealed class LongTermContract : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Ancient;

    protected override IEnumerable<DynamicVar> CanonicalVars => [];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [];

    public override RelicAssetProfile AssetProfile =>
        new(
            // 小图标（原版85x85）
            IconPath: $"res://ArknightsMap/images/relics/{GetType().Name}.png",
            // 轮廓图标（原版85x85）
            IconOutlinePath: $"res://ArknightsMap/images/relics/{GetType().Name}.png",
            // 大图标（原版256x256）
            BigIconPath: $"res://ArknightsMap/images/relics/{GetType().Name}.png"
        );

    public override async Task AfterObtained()
    {
        foreach (
            CardModel item in await CardSelectCmd.FromDeckForTransformation(
                prefs: new CardSelectorPrefs(CardSelectorPrefs.TransformSelectionPrompt, 1),
                player: Owner,
                cardToTransformation: c =>
                {
                    SomethingForm somethingForm = (SomethingForm)ModelDb.Card<SomethingForm>().ToMutable();
                    somethingForm.SetBaseCard((CardModel)c.MutableClone());
                    return new CardTransformation(c, somethingForm);
                }
            )
        )
        {
            SomethingForm somethingForm = Owner.RunState.CreateCard<SomethingForm>(Owner);
            somethingForm.SetBaseCard((CardModel)item.MutableClone());
            await CardCmd.Transform(item, somethingForm);
        }
    }
}
