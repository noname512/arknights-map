using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Messages.Game;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Multiplayer.Transport;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace ArknightsMap.Scripts.Utils.MerchantEnchantment;

public class EnchantMessage : INetMessage, IPacketSerializable, IRunLocationTargetedMessage
{
    public int goldCost;
    public EnchantmentModel? enchantmentModel;
    public bool ShouldBroadcast => true;

    public NetTransferMode Mode => NetTransferMode.Reliable;

    public LogLevel LogLevel => LogLevel.VeryDebug;

    public bool ShouldBuffer => true;

    public RunLocation Location { get; set; }

    public void Deserialize(PacketReader reader)
    {
        goldCost = reader.ReadInt();
        enchantmentModel = EnchantmentModel.FromSerializable(reader.Read<SerializableEnchantment>());
        Location = reader.Read<RunLocation>();
    }

    public void Serialize(PacketWriter writer)
    {
        writer.WriteInt(goldCost);
        writer.Write(enchantmentModel!.ToSerializable());
        writer.Write(Location);
    }
}
