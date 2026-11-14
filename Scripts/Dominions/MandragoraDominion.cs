using ArknightsMap.Scripts.Monsters;
using ArknightsMap.Scripts.Powers;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Dread;

namespace ArknightsMap.Scripts.Dominions;

public class MandragoraDominion : DominionModel
{
    public override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<TauntPower>(), HoverTipFactory.FromPower<CollapsePower>()];

    public override async Task BeforeSideTurnStart(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState
    )
    {
        if (side == CombatSide.Player && participants.First(c => c.IsPlayer).Player!.PlayerCombatState!.TurnNumber <= 1)
        {
            Creature m = await CreatureCmd.Add<TatteredPillar>(combatState); // TODO: 定位置
            await PowerCmd.Remove<CollapsePower>(m);
            await PowerCmd.Apply<TauntPower>(choiceContext, m, 1m, null, null);
            await CreatureCmd.SetMaxAndCurrentHp(
                m,
                Creature.ScaleHpForMultiplayer(40, combatState.Encounter, combatState.Players.Count, combatState.RunState.CurrentActIndex)
            );
        }
    }
}
