using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Gold;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Runs;

namespace ArknightsMap.Scripts.Utils.MerchantEnchantment;

public class EnchantSynchronizer : IDisposable
{
    private struct BufferedMessage
    {
        public ulong senderId;
        public EnchantMessage enchantmentMessage;
    }

    private readonly RunLocationTargetedMessageBuffer _messageBuffer;

    private readonly INetGameService _gameService;

    private readonly IPlayerCollection _playerCollection;

    private readonly ulong _localPlayerId;

    private readonly List<BufferedMessage> _bufferedMessages = new List<BufferedMessage>();

    private Player LocalPlayer => _playerCollection.GetPlayer(_localPlayerId);

    public EnchantSynchronizer(
        RunLocationTargetedMessageBuffer messageBuffer,
        INetGameService gameService,
        IPlayerCollection playerCollection,
        ulong localPlayerId
    )
    {
        _gameService = gameService;
        _playerCollection = playerCollection;
        _localPlayerId = localPlayerId;
        _messageBuffer = messageBuffer;
        _messageBuffer.RegisterMessageHandler<EnchantMessage>(HandleEnchantMessage);
    }

    public Task<bool> DoLocalEnchant(int goldCost, EnchantmentModel enchantmentModel, bool cancelable = true)
    {
        EnchantMessage message = new EnchantMessage { goldCost = goldCost, enchantmentModel = enchantmentModel };
        _gameService.SendMessage(message);
        return DoEnchant(LocalPlayer, goldCost, enchantmentModel, cancelable);
    }

    private void HandleEnchantMessage(EnchantMessage message, ulong senderId)
    {
        Player player = _playerCollection.GetPlayer(senderId);
        if (player == LocalPlayer)
        {
            throw new InvalidOperationException("HandleEnchantMessage should not be sent to the player removing the card!");
        }
        TaskHelper.RunSafely(DoEnchant(player, message.goldCost, message.enchantmentModel));
    }

    private async Task<bool> DoEnchant(Player player, int goldCost, EnchantmentModel enchantmentModel, bool cancelable = true)
    {
        CardSelectorPrefs prefs = new CardSelectorPrefs(CardSelectorPrefs.EnchantSelectionPrompt, 1)
        {
            Cancelable = cancelable,
            RequireManualConfirmation = true,
        };
        EnchantmentModel canonicalModel = ModelDb.GetById<EnchantmentModel>(enchantmentModel.Id);
        CardModel? card = (await CardSelectCmd.FromDeckForEnchantment(player, canonicalModel, enchantmentModel.Amount, prefs)).FirstOrDefault();
        if (card != null)
        {
            await PlayerCmd.LoseGold(goldCost, player, GoldLossType.Spent);
            CardCmd.Enchant(enchantmentModel, card, enchantmentModel.Amount);
            NCardEnchantVfx? nCardEnchantVfx = NCardEnchantVfx.Create(card);
            if (nCardEnchantVfx != null)
            {
                NRun.Instance?.GlobalUi.CardPreviewContainer.AddChildSafely(nCardEnchantVfx);
            }
        }
        return card != null;
    }

    public void Dispose()
    {
        _messageBuffer.UnregisterMessageHandler<EnchantMessage>(HandleEnchantMessage);
    }
}
