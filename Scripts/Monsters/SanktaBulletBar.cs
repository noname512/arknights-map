using Godot;
using MegaCrit.Sts2.Core.Entities.Creatures;
using ArknightsMap.Scripts.Powers;

public partial class SanktaBulletBar : HBoxContainer
{
    private Creature _creature = null!;
    private readonly List<ColorRect> _pips = new();

    public static SanktaBulletBar Create(Creature creature, int max)
    {
        var bar = new SanktaBulletBar { _creature = creature };
        bar.AddThemeConstantOverride("separation", 2); // 格子间距，做出分格感

        for (int i = 0; i < max; i++)
        {
            var pip = new ColorRect
            {
                CustomMinimumSize = new Vector2(10, 4), // 每格的尺寸，按你的血条宽度调
                Color = Colors.Gold                     // 有子弹的颜色
            };
            bar._pips.Add(pip);
            bar.AddChild(pip);
        }
        bar.Refresh();
        return bar;
    }

    public void FitToWidth(float totalWidth, int max, int sep = 4, float height = 10f)
{
    AddThemeConstantOverride("separation", sep);
    float pipWidth = (totalWidth - sep * (max - 1)) / max;
    foreach (var pip in _pips)
    {
        pip.CustomMinimumSize = new Vector2(pipWidth, height);
    }
}

    public void Refresh()
    {
        int current = _creature.GetPowerAmount<BulletPower>();
        for (int i = 0; i < _pips.Count; i++)
        {
            // 剩余=亮色，已用=暗色半透明（方舟的熄灭格效果）
            _pips[i].Color = i < current
                ? Colors.Gold
                : new Color(0.15f, 0.15f, 0.15f, 0.5f);
        }
    }
}