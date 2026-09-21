using System.Reflection;
using STS2RitsuLib;
using STS2RitsuLib.Data;
using STS2RitsuLib.Settings;
using STS2RitsuLib.Utils.Persistence;

namespace ArknightsMap.Scripts.Utils;

public sealed class ArknightsSettings
{
    public bool Act1 { get; set; } = true;
    public bool Act2 { get; set; } = true;
    public bool Act3 { get; set; } = true;
    public bool Wilds { get; set; } = true;
    public bool Laterano { get; set; } = true;
    public bool SnowyMountain { get; set; } = true;
}

public static class SettingsPage
{
    private const string DataKey = "ArknightsMap";

    private static IModSettingsValueBinding<bool> Bind(string propertyName)
    {
        PropertyInfo property = typeof(ArknightsSettings).GetProperty(propertyName)!;

        return ModSettingsBindings.WithDefault(
            ModSettingsBindings.Global<ArknightsSettings, bool>(
                Entry.ModId,
                DataKey,
                s => (bool)property.GetValue(s)!,
                (s, v) => property.SetValue(s, v) //
            ),
            () => true
        );
    }

    public static readonly IModSettingsValueBinding<bool> Act1Binding = Bind("Act1");
    public static readonly IModSettingsValueBinding<bool> Act2Binding = Bind("Act2");
    public static readonly IModSettingsValueBinding<bool> Act3Binding = Bind("Act3");
    public static readonly IModSettingsValueBinding<bool> WildsBinding = Bind("Wilds");
    public static readonly IModSettingsValueBinding<bool> LateranoBinding = Bind("Laterano");
    public static readonly IModSettingsValueBinding<bool> SnowyMountainBinding = Bind("SnowyMountain");

    public static void Register()
    {
        // 注册 DataStore
        ModDataStore
            .For(Entry.ModId)
            .Register(
                key: DataKey, // 持久化数据ID，需要和别人防撞
                fileName: "settings.json", // 你的数据文件名
                scope: SaveScope.Global, // Profile 表示每个存档独立，可改成 Global 表示所有存档共享
                defaultFactory: () => new ArknightsSettings(),
                autoCreateIfMissing: true
            );

        // 注册页面UI
        RitsuLibFramework.RegisterModSettings(
            Entry.ModId,
            page =>
            {
                var result = page.WithModDisplayName(ModSettingsText.Literal("明日方舟地图 ArknightsMap"))
                    .WithTitle(ModSettingsText.Literal("地图启用设置"))
                    .WithDescription(ModSettingsText.Literal("重启游戏后生效"))
                    .WithVisibleOnHostSurfaces(ModSettingsHostSurface.MainMenu | ModSettingsHostSurface.RunPause)
                    .AddSection(
                        "act2",
                        section =>
                            section
                                .WithTitle(ModSettingsText.Literal("第二幕启用地图"))
                                .AddToggle("act2origin", ModSettingsText.Literal("原版地图"), Act2Binding, ModSettingsText.Literal("Original Map"))
                                .AddToggle("wilds", ModSettingsText.Literal("原野"), WildsBinding, ModSettingsText.Literal("Wilds"))
                    );
                if (Entry.isDemo)
                {
                    result.AddSection(
                        "act3",
                        section =>
                            section
                                .WithTitle(ModSettingsText.Literal("第三幕启用地图"))
                                .AddToggle("act3origin", ModSettingsText.Literal("原版地图"), Act3Binding, ModSettingsText.Literal("Original Map"))
                                .AddToggle("snowymountain", ModSettingsText.Literal("雪山"), SnowyMountainBinding, ModSettingsText.Literal("Snowy Mountain"))
                                .AddToggle("laterano", ModSettingsText.Literal("拉特兰"), LateranoBinding, ModSettingsText.Literal("Laterano"))
                    );
                }
            }
        );
    }
}
