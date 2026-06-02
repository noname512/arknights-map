using ArknightsMap.Scripts.Cards;
using ArknightsMap.Scripts.Encounters;
using ArknightsMap.Scripts.Monsters;
using ArknightsMap.Scripts.Powers;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using STS2RitsuLib;

public sealed class ReedBed : ILifecycleObserver
{
    public static bool Burning;
    public static Node? Foreground;

    public ReedBed()
    { }

    public async void OnEvent(IFrameworkLifecycleEvent evt)
    {
        if (evt is CombatStartingEvent cse)
        {
            Burning = false;
            EncounterModel encounter = cse.CombatState.Encounter;
            if (encounter is AbstractWildsEncounter myEncounter)
            {
                Control control = NCombatRoom.Instance?.Background ?? throw new InvalidOperationException();
                Foreground = control.GetNodeOrNull("Foreground");
                if (myEncounter.isBurningAtStart)
                {
                    await SetBurningDurningCombat(true, cse.CombatState);
                }
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
            string texturePath = "res://ArknightsMap/images/rooms/wilds/";
            if (Burning)
            {
                await PowerCmd.Apply<DealFlamingDamagePower>(new ThrowingPlayerChoiceContext(), combatState.Enemies, 1m, null, null);
                if (combatState.Encounter is AFRBoss || combatState.Encounter is HerFlame) texturePath += "wilds_01_c.png";
                else texturePath += "wilds_01_b.png";
            }
            else
            {
                foreach (var m in combatState.Enemies)
                {
                    await PowerCmd.Remove<DealFlamingDamagePower>(m);
                    /*
                    if (m.Monster is TheLeader leader)
                    {
                        leader.SetMoveImmediate((MoveState)leader.MoveStateMachine.States["IGNITE" + (leader._isstage2 ? "2" : "1")]);
                    }*/
                }
                texturePath += "wilds_01_a.png";
            }

            // Change foreground picture
            if (Foreground != null && Foreground.GetChildCount() > 0)
            {
                Node textureNode = Foreground.GetChild(0);
                if (textureNode is TextureRect textureRect)
                {
                    Texture2D newTexture = GD.Load<Texture2D>(texturePath);
                    textureRect.Texture = newTexture;
                }
            }
        }
    }
}