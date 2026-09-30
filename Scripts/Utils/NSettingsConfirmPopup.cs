using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens.ScreenContext;

namespace ArknightsMap.Scripts.Utils;

public partial class NSettingsConfirmPopup : Control, IScreenContext
{
    public Control? DefaultFocusedControl => throw new NotImplementedException();
    private NVerticalPopup _verticalPopup;
    private static readonly string _scenePath = "res://ArknightsMap/scenes/ui/settings_confirm_popup.tscn";

    public override void _Ready()
    {
        _verticalPopup = GetNode<NVerticalPopup>("VerticalPopup");
        _verticalPopup.SetText(
            new LocString("settings_ui", "ARKNIGHTS_MAP_SETTINGS_UI_SETTINGS_CONFIRMATION.header"),
            new LocString("settings_ui", "ARKNIGHTS_MAP_SETTINGS_UI_SETTINGS_CONFIRMATION.body")
        );
        _verticalPopup.InitYesButton(new LocString("main_menu_ui", "GENERIC_POPUP.confirm"), OnButtonPressed);
        _verticalPopup.InitNoButton(new LocString("main_menu_ui", "GENERIC_POPUP.confirm"), OnButtonPressed);
    }

    public static NSettingsConfirmPopup? Create()
    {
        return PreloadManager.Cache.GetScene(_scenePath).Instantiate<NSettingsConfirmPopup>(PackedScene.GenEditState.Disabled);
    }

    private void OnButtonPressed(NButton _)
    {
        this.QueueFreeSafely();
    }
}
