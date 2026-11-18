using ArknightsMap.Scripts.Monsters;
using ArknightsMap.Scripts.Relics;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Dread;

public class SSSDominion : DominionModel
{
    public override IEnumerable<IHoverTip> ExtraHoverTips => [];

    public override async Task BeforeSideTurnStart(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState
    )
    {
        if (side == CombatSide.Player && participants.First(c => c.IsPlayer).Player!.PlayerCombatState!.TurnNumber <= 1)
        {
            Creature m = await CreatureCmd.Add<SupersweetieDeliveryDrone>(combatState); // TODO: 定位置
            
        }
    }
}
