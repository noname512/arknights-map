using ArknightsMap.Scripts.Relics;
using Godot;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Multiplayer;
using MegaCrit.Sts2.Core.Nodes.RestSite;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Screens.Capstones;
using MegaCrit.Sts2.Core.Nodes.Screens.Overlays;
using MegaCrit.Sts2.Core.Nodes.Vfx.Cards;
using MegaCrit.Sts2.Core.Platform;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Utils;

public class PassRestSiteOption : ModRestSiteOptionTemplate
{
    private const string _hasTargetKey = "HasTarget";

    private const string _playerNameKey = "Name";

    private LocString? _description;

    public override RestSiteOptionAssetProfile AssetProfile => new(IconPath: $"res://ArknightsMap/images/ui/rest_site/option_pass.png");

    public override string OptionId => "PASS";

    public RelicModel OwnerRelic;

    public override LocString Description
    {
        get
        {
            if (_description == null)
            {
                _description = base.Description;
                _description.Add(_hasTargetKey, variable: false);
                _description.Add(_playerNameKey, "");
            }
            return _description;
        }
    }

    public PassRestSiteOption(Player owner, RelicModel relic)
        : base(owner)
    {
        OwnerRelic = relic;
    }

    public override async Task<bool> OnSelect()
    {
        uint choiceId = RunManager.Instance.PlayerChoiceSynchronizer.ReserveChoiceId(Owner);
        Player? target = null;
        if (LocalContext.IsMe(Owner))
        {
            NRestSiteRoom.Instance!.AnimateDescriptionDown();
            NRestSiteButton buttonForOption = NRestSiteRoom.Instance.GetButtonForOption(this)!;
            Vector2 startPosition = buttonForOption.GlobalPosition + buttonForOption.Size / 2f;
            bool usingController = NControllerManager.Instance!.IsUsingDirectionalNavigation;
            NTargetManager targetManager = NTargetManager.Instance;
            targetManager.StartTargeting(
                TargetType.AnyPlayer,
                startPosition,
                usingController ? TargetMode.Controller : TargetMode.ClickMouseToTarget,
                ShouldCancelTargeting,
                AllowHoveringNode
            );
            if (usingController)
            {
                List<NRestSiteCharacter> list = NRestSiteRoom.Instance.characterAnims.Where((NRestSiteCharacter c) => c.Player != Owner).ToList();
                for (int num = 0; num < list.Count; num++)
                {
                    list[num].Hitbox.SetFocusMode(Control.FocusModeEnum.All);
                    list[num].Hitbox.FocusNeighborTop = list[num].Hitbox.GetPath();
                    list[num].Hitbox.FocusNeighborBottom = list[num].Hitbox.GetPath();
                    Control hitbox = list[num].Hitbox;
                    NodePath path;
                    if (num <= 0)
                    {
                        path = list[list.Count - 1].Hitbox.GetPath();
                    }
                    else
                    {
                        path = list[num - 1].Hitbox.GetPath();
                    }
                    hitbox.FocusNeighborLeft = path;
                    list[num].Hitbox.FocusNeighborRight = (num < list.Count - 1) ? list[num + 1].Hitbox.GetPath() : list[0].Hitbox.GetPath();
                }
                list.FirstOrDefault()?.Hitbox.TryGrabFocus();
            }
            targetManager.Connect(NTargetManager.SignalName.NodeHovered, Callable.From<Node>(OnNodeHovered));
            targetManager.Connect(NTargetManager.SignalName.NodeUnhovered, Callable.From<Node>(OnNodeUnhovered));
            try
            {
                target = NodeToPlayer(await targetManager.SelectionFinished());
                RunManager.Instance.PlayerChoiceSynchronizer.SyncLocalChoice(Owner, choiceId, PlayerChoiceResult.FromPlayerId(target?.NetId));
            }
            finally
            {
                targetManager.Disconnect(NTargetManager.SignalName.NodeHovered, Callable.From<Node>(OnNodeHovered));
                targetManager.Disconnect(NTargetManager.SignalName.NodeUnhovered, Callable.From<Node>(OnNodeUnhovered));
                if (usingController)
                {
                    foreach (NRestSiteCharacter characterAnim in NRestSiteRoom.Instance.characterAnims)
                    {
                        characterAnim.Hitbox.SetFocusMode(Control.FocusModeEnum.None);
                    }
                }
            }
        }
        else
        {
            ulong? num2 = (await RunManager.Instance.PlayerChoiceSynchronizer.WaitForRemoteChoice(Owner, choiceId)).AsPlayerId();
            if (num2.HasValue)
            {
                target = Owner.RunState.GetPlayer(num2.Value);
            }
        }
        NRestSiteRoom.Instance?.AnimateDescriptionUp();
        Description.Add(_hasTargetKey, variable: false);
        NRestSiteRoom.Instance?.GetButtonForOption(this)?.RefreshTextState();
        if (target != null)
        {
            CardSelectorPrefs prefs = new CardSelectorPrefs(CardSelectorPrefs.RemoveSelectionPrompt, 1) { Cancelable = true };
            IEnumerable<CardModel> enumerable = await CardSelectCmd.FromDeckForRemoval(Owner, prefs);
            if (!enumerable.Any())
            {
                return false;
            }
            foreach (CardModel item in enumerable)
            {
                await CardPileCmd.GiveToAnotherPlayer(item, target, PileType.Deck);
            }
            if (LocalContext.IsMe(target))
            {
                CardModel card = enumerable.FirstOrDefault()!;
                CardPileAddResult result = new CardPileAddResult
                {
                    success = true,
                    cardAdded = card,
                    oldPile = card.Pile,
                    modifyingModels = null,
                };
                CardCmd.PreviewCardPileAdd(result);
            }
            else if (LocalContext.IsMe(Owner))
            {
                CardModel card = enumerable.FirstOrDefault()!;
                NCard cardNode = NCard.Create(card)!;
                if (cardNode != null)
                {
                    NRun.Instance!.GlobalUi.CardPreviewContainer.AddChildSafely(cardNode);
                    cardNode.UpdateVisuals(PileType.None, CardPreviewMode.Normal);
                    Tween tween = cardNode.CreateTween();
                    tween
                        .TweenProperty(cardNode, "scale", Vector2.One * 1f, 0.25)
                        .From(Vector2.Zero)
                        .SetEase(Tween.EaseType.Out)
                        .SetTrans(Tween.TransitionType.Cubic);
                    tween.TweenInterval(0.25);
                    tween.TweenCallback(
                        Callable.From(
                            delegate
                            {
                                NCardRemoveVfx child = NCardRemoveVfx.Create(cardNode)!;
                                NRun.Instance.GlobalUi.AboveTopBarVfxContainer.AddChildSafely(child);
                            }
                        )
                    );
                    tween.TweenInterval(0.4000000059604645);
                    tween.TweenCallback(Callable.From(cardNode.QueueFreeSafely));
                }
                Owner.Deck.InvokeCardRemoveFinished();
            }

            await RelicCmd.Remove(OwnerRelic);
            await RelicCmd.Obtain<NunHabit>(target);
            return true;
        }
        return false;
    }

    private void OnNodeHovered(Node node)
    {
        Player? player = NodeToPlayer(node);
        if (player != null)
        {
            Description.Add(_hasTargetKey, variable: true);
            Description.Add(_playerNameKey, PlatformUtil.GetPlayerName(RunManager.Instance.NetService.Platform, player.NetId));
            NRestSiteRoom.Instance?.GetButtonForOption(this)?.RefreshTextState();
        }
    }

    /// <summary>
    /// Called when a targetable node (rest site character, player state display) is unhovered.
    /// </summary>
    private void OnNodeUnhovered(Node _)
    {
        Description.Add(_hasTargetKey, variable: false);
        NRestSiteRoom.Instance?.GetButtonForOption(this)?.RefreshTextState();
    }

    /// <summary>
    /// Translates a node target to a player.
    /// The node target can be a character at the rest site or a remote player's state display at the top-left.
    /// </summary>
    private Player? NodeToPlayer(Node? node)
    {
        if (node == null)
        {
            return null;
        }
        if (!(node is NMultiplayerPlayerState nMultiplayerPlayerState))
        {
            if (node is NRestSiteCharacter nRestSiteCharacter)
            {
                return nRestSiteCharacter.Player;
            }
            return null;
        }
        return nMultiplayerPlayerState.Player;
    }

    private bool ShouldCancelTargeting()
    {
        if (NOverlayStack.Instance!.ScreenCount <= 0)
        {
            return NCapstoneContainer.Instance!.InUse;
        }
        return true;
    }

    private bool AllowHoveringNode(Node node)
    {
        return !LocalContext.IsMe(NodeToPlayer(node));
    }
}
