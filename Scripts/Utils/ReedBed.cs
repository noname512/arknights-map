using System.Runtime.CompilerServices;
using ArknightsMap.Scripts;
using ArknightsMap.Scripts.Cards;
using ArknightsMap.Scripts.Powers;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib;

public sealed class ReedBed : ILifecycleObserver
{
    public static bool Burning;

    public ReedBed() { }

    public async void OnEvent(IFrameworkLifecycleEvent evt)
    {
        if (evt is CombatStartingEvent cse)
        {
            EncounterModel encounter = cse.CombatState.Encounter;
            if (encounter is MyAbstractEncounter myEncounter && myEncounter.isBurningAtStart)
            {
                await SetBurningDurningCombat(true, cse.CombatState);
            }
        }
        else if (evt is SideTurnStartingEvent stse)
        {
            if (stse.Side == CombatSide.Enemy) return;
            if (Burning)
            {
                foreach (var player in stse.CombatState.Players)
                {
                    await CardPileCmd.AddGeneratedCardToCombat(stse.CombatState.CreateCard<PutOutFire>(player), PileType.Hand, null);
                }
            }
        }
        else if (evt is CombatEndedEvent)
        {
            Burning = false;
        }
    }

    public async Task SetBurningDurningCombat(bool burning, ICombatState combatState)
    {
        if (burning != Burning)
        {
            Burning = burning;
            if (Burning)
            {
                await PowerCmd.Apply<DealFlamingDamagePower>(new ThrowingPlayerChoiceContext(), combatState.Enemies, 1m, null, null);
            }
            else
            {
                foreach (var m in combatState.Enemies)
                    await PowerCmd.Remove<DealFlamingDamagePower>(m);
            }
        }
    }
}