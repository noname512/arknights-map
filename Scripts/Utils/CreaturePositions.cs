using ArknightsMap.Scripts.Encounters;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Models;

namespace ArknightsMap.Scripts.Utils;

[RegisterSingleton]
public sealed class CreaturePositions : HookedSingletonModel
{
    private static Dictionary<Creature, int> Positions = new();
    private static DamageVar damage = new DamageVar(20, ValueProp.Move);

    public CreaturePositions()
        : base(HookType.Combat) { }

    public static List<Creature> GetCreaturesInPosition(int pos)
    {
        return Positions.Where(kv => kv.Value == pos).Select(kv => kv.Key).ToList();
    }

    public override Task AfterRoomEntered(AbstractRoom room)
    {
        Positions.Clear();
        return Task.CompletedTask;
    }

    public override async Task BeforeCombatStart()
    {
        int playerPos = 3;
        if (CurrentCombatState!.Encounter is AbstractSnowyMountainEncounter myEncounter)
        {
            playerPos = myEncounter.playerStartPosition;
        }
        foreach (Creature c in CurrentCombatState.PlayerCreatures)
        {
            Positions[c] = playerPos;
        }
        foreach (Creature c in CurrentCombatState.Enemies)
        {
            Positions[c] = 6;
        }
    }

    public static bool IsBlock(Creature x, Creature y)
    {
        return Math.Abs(Positions.GetValueOrDefault(x) - Positions.GetValueOrDefault(y)) <= 1;
    }

    public static async Task BlowWind(int direction)
    {
        List<Creature> allAffectedCreatures = [];
        if (direction == -1)
        {
            List<Creature> creaturesInPos = GetCreaturesInPosition(1);
            int startPos = 2;
            if (creaturesInPos.Count > 0)
            {
                await CreatureCmd.Damage(new BlockingPlayerChoiceContext(), creaturesInPos, damage, null, null, null);
                foreach (Creature c in creaturesInPos)
                {
                    if (c.IsAlive)
                    {
                        if (c.IsPlayer)
                        {
                            await PowerCmd.Apply<RingingPower>(new BlockingPlayerChoiceContext(), c, 1, c, null);
                        }
                        else
                        {
                            await CreatureCmd.Stun(c);
                        }
                    }
                }
                while (GetCreaturesInPosition(startPos).Count > 0)
                {
                    startPos++;
                }
            }
            for (int pos = startPos; pos <= 9; pos++)
            {
                List<Creature> creatures = GetCreaturesInPosition(pos);
                foreach (Creature c in creatures)
                {
                    Positions[c] = pos - 1;
                    allAffectedCreatures.Add(c);
                    allAffectedCreatures.AddRange(c.Pets);
                }
            }
        }
        else
        {
            List<Creature> creaturesInPos = GetCreaturesInPosition(9);
            int startPos = 8;
            if (creaturesInPos.Count > 0)
            {
                await CreatureCmd.Damage(new BlockingPlayerChoiceContext(), creaturesInPos, damage, null, null, null);
                foreach (Creature c in creaturesInPos)
                {
                    if (c.IsAlive)
                    {
                        if (c.IsPlayer)
                        {
                            await PowerCmd.Apply<RingingPower>(new BlockingPlayerChoiceContext(), c, 1, c, null);
                        }
                        else
                        {
                            await CreatureCmd.Stun(c);
                        }
                    }
                }
                while (GetCreaturesInPosition(startPos).Count > 0)
                {
                    startPos--;
                }
            }
            for (int pos = startPos; pos >= 1; pos--)
            {
                List<Creature> creatures = GetCreaturesInPosition(pos);
                foreach (Creature c in creatures)
                {
                    Positions[c] = pos + 1;
                    allAffectedCreatures.Add(c);
                    allAffectedCreatures.AddRange(c.Pets);
                }
            }
        }

        Tween tween = NCombatRoom.Instance!.CreateTween().SetParallel().SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
        foreach (Creature c in allAffectedCreatures)
        {
            NCreature creatureNode = NCombatRoom.Instance.GetCreatureNode(c)!;
            tween.TweenProperty(creatureNode, "global_position:x", creatureNode.GlobalPosition.X + 300 * direction, 0.25);
        }
    }

    [HarmonyPatch(typeof(NCombatRoom), "CreateAllyNodes")]
    public static class PositionPlayersAndPetsPatch
    {
        [HarmonyPostfix]
        public static void Postfix(NCombatRoom _inst, ICombatRoomVisuals ___visuals)
        {
            if (___visuals.Encounter is AbstractSnowyMountainEncounter encounter)
            {
                int diff = encounter.playerStartPosition - 3;
                if (diff != 0)
                {
                    foreach (NCreature creature in _inst.CreatureNodes)
                        if (___visuals.Allies.Contains(creature.Entity))
                            creature.Position = new Vector2(creature.Position.X + 300 * diff, creature.Position.Y);
                }
            }
        }
    }
}
