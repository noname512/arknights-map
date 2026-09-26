using System;
using ArknightsMap.Scripts;
using ArknightsMap.Scripts.Utils;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using MegaCrit.Sts2.Core.Nodes.Rooms;

public partial class NSnowStormWarning : Control
{
    public Control Hitbox { get; private set; }
    public bool IsFocused { get; private set; }
    public List<IHoverTip> HoverTips = [Entry.MyHoverTip("SNOW_STORM"), new HoverTip()];
    public CombatState CurrentCombatState;
    public TextureRect Rect { get; private set; }
    public DynamicVar DirectionVar = new IntVar("direction", 0);

    public bool ShowThisTurn()
    {
        return CreaturePositions.WindBlowTurn != 0 && CurrentCombatState.RoundNumber % CreaturePositions.WindBlowTurn == 0;
    }

    public override void _Ready()
    {
        Hitbox = GetNode<Control>("Hitbox");
        Rect = GetNode<TextureRect>("TextureRect");
        Hitbox.Connect(Control.SignalName.FocusEntered, Callable.From(OnFocus));
        Hitbox.Connect(Control.SignalName.FocusExited, Callable.From(OnUnfocus));
        Hitbox.Connect(Control.SignalName.MouseEntered, Callable.From(OnFocus));
        Hitbox.Connect(Control.SignalName.MouseExited, Callable.From(OnUnfocus));

        if (CreaturePositions.WindBlowDirection != 1)
        {
            Rect.Scale = new Vector2(-Rect.Scale.X, Rect.Scale.Y);
        }
        if (CreaturePositions.WindBlowTurn != 1)
        {
            Visible = false;
        }
        DirectionVar.BaseValue = CreaturePositions.WindBlowDirection;
        string text = "ARKNIGHTS_MAP_STATIC_HOVER_TIPS_SNOW_STORM_WARNING";
        LocString title = new LocString("static_hover_tips", text + ".title");
        LocString description = new LocString("static_hover_tips", text + ".description");
        description.Add(DirectionVar);
        HoverTips[1] = new HoverTip(title, description);
    }

    private void OnFocus()
    {
        GD.Print("OnFocus called!!!");
        if (IsFocused)
        {
            return;
        }
        IsFocused = true;
        if (NTargetManager.Instance.IsInSelection)
        {
            NTargetManager.Instance.OnNodeHovered(this);
            return;
        }
        ShowHoverTips();
        CombatManager.Instance.StateTracker.CombatStateChanged += ShowWarningHoverTips;
    }

    private void OnUnfocus()
    {
        GD.Print("OnUnfocus called!!!");
        IsFocused = false;
        NTargetManager.Instance.OnNodeUnhovered(this);
        CombatManager.Instance.StateTracker.CombatStateChanged -= ShowWarningHoverTips;
        HideHoverTips();
    }

    public void OnTargetingStarted()
    {
        if (IsFocused)
        {
            NTargetManager.Instance.OnNodeHovered(this);
            CombatManager.Instance.StateTracker.CombatStateChanged -= ShowWarningHoverTips;
            HideHoverTips();
        }
    }

    private void ShowWarningHoverTips(CombatState _)
    {
        if (CurrentCombatState != null)
        {
            ShowHoverTips();
        }
    }

    public void ShowHoverTips()
    {
        if (!NCombatRoom.Instance!.Ui.Hand.InCardPlay)
        {
            HideHoverTips();
            NHoverTipSet.CreateAndShow(Hitbox, HoverTips, HoverTip.GetHoverTipAlignment(this, 0.5f));
        }
    }

    public void HideHoverTips()
    {
        NHoverTipSet.Remove(Hitbox);
    }

    public static NSnowStormWarning Create(CombatState combatState)
    {
        string scenePath = "res://ArknightsMap/scenes/ui/snow_storm_warning.tscn";
        PackedScene? packedScene = GD.Load<PackedScene>(scenePath);
        NSnowStormWarning nSnowStormWarning = packedScene.Instantiate<NSnowStormWarning>();
        nSnowStormWarning.CurrentCombatState = combatState;
        return nSnowStormWarning;
    }
}
