using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Afflictions;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Models;

namespace ArknightsMap.Scripts.Utils;

[RegisterSingleton]
public sealed class CreaturePositions : HookedSingletonModel
{
    public Dictionary<Creature, int> Positions = new();
    private static DamageVar damage = new DamageVar(20, ValueProp.Move);

    public List<Creature> GetCreaturesInPosition(int pos)
    {
        return Positions.Where(kv => kv.Value == pos).Select(kv => kv.Key).ToList();
    }

    public CreaturePositions()
        : base(HookType.Combat) { }

    public override async Task BeforeCombatStart()
    {
        Positions.Clear();
        foreach (Creature c in CurrentCombatState!.PlayerCreatures)
        {
            Positions[c] = 3;
        }
    }

    public async Task BlowWind(int direction)
    {
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
                }
            }
        }
    }
}
