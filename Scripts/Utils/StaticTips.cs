using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;

namespace ArknightsMap.Scripts.Utils;

public static class StaticTips
{
    public static IHoverTip MyHoverTip(string text)
    {
        string fullText = "ARKNIGHTS_MAP_STATIC_HOVER_TIPS_" + text;
        return new HoverTip(new LocString("static_hover_tips", fullText + ".title"), new LocString("static_hover_tips", fullText + ".description"));
    }

    public static IHoverTip ReedBed => MyHoverTip("REED_BED");
    public static IHoverTip Custom => MyHoverTip("CUSTOM");
    public static IHoverTip Block => MyHoverTip("BLOCK");
    public static IHoverTip SnowStorm => MyHoverTip("SNOW_STORM");
}
