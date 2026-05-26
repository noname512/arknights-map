using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.RelicPools;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Relics;

[RegisterRelic(typeof(SharedRelicPool))]
public class LiveFlame : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Ancient;

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[2]
     {
            new EnergyVar(1),
            new CardsVar(2)
     };

    protected override IEnumerable<IHoverTip> AdditionalHoverTips
    {
        get
        {
            List<IHoverTip> list = new List<IHoverTip>();
            list.Add(HoverTipFactory.ForEnergy(this));
            list.AddRange(HoverTipFactory.FromCardWithCardHoverTips<Burn>());
            return list;
        }
    }

    public override RelicAssetProfile AssetProfile => new(
        // 小图标（原版85x85）
        IconPath: $"res://ArknightsMap/images/relics/{GetType().Name}.png",
        // 轮廓图标（原版85x85）
        IconOutlinePath: $"res://ArknightsMap/images/relics/{GetType().Name}.png",
        // 大图标（原版256x256）
        BigIconPath: $"res://ArknightsMap/images/relics/{GetType().Name}.png"
    );

    public override decimal ModifyMaxEnergy(Player player, decimal amount)
    {
        if (player != Owner)
        {
            return amount;
        }
        return amount + DynamicVars.Energy.IntValue;
    }

    public override async Task BeforeHandDraw(Player player, PlayerChoiceContext choiceContext, ICombatState combatState)
    {
        if (player == Owner && combatState.RoundNumber == 1)
        {
            Flash();
            List<CardModel> list = new List<CardModel>();
            for (int i = 0; i < DynamicVars.Cards.IntValue; i++)
            {
                list.Add(combatState.CreateCard<Burn>(Owner));
            }
            CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardsToCombat(list, PileType.Hand, player, CardPilePosition.Random));
            await Cmd.Wait(3f);
        }
    }

}
