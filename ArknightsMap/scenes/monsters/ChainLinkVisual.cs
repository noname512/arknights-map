using Godot;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace ArknightsMap.Scenes.Monsters;

public partial class ChainLinkVisual : Node2D
{
    private NCreature? _source; // OpForGun
    private NCreature? _target; // OpCar

    private Line2D? _chain1;
    private Line2D? _chain2;

    // 锁链纹理路径，你需要准备一张链节的透明PNG（比如 64x64 的单个链节）
    [Export]
    public string ChainTexturePath = "res://ArknightsMap/images/monsters/chain_link.png";

    // 锁链视觉参数
    [Export]
    public float ChainWidth = 18f;

    [Export]
    public float CrossOffset = 8f; // 两条链的横向间距，形成X形

    [Export]
    public float VerticalOffset = -25f; // 相对于怪物中心的Y轴偏移（连接到身体中部偏上）

    [Export]
    public int ZLayer = -5; // 确保锁链在怪物下方渲染

    private Texture2D? _texture;

    /// <summary>
    /// 初始化锁链，传入两端怪物节点
    /// </summary>
    public void Setup(NCreature source, NCreature target)
    {
        _source = source;
        _target = target;

        ZIndex = ZLayer;

        // 加载纹理
        _texture = GD.Load<Texture2D>(ChainTexturePath);

        // 创建第一条链
        _chain1 = CreateChainLine();
        AddChild(_chain1);

        // 创建第二条链（交叉）
        _chain2 = CreateChainLine();
        AddChild(_chain2);

        // 添加到战斗房间，确保坐标系正确
        NCombatRoom.Instance?.AddChild(this);
    }

    private Line2D CreateChainLine()
    {
        var line = new Line2D
        {
            Width = ChainWidth,
            Texture = _texture,
            TextureMode = Line2D.LineTextureMode.Tile,
            JointMode = Line2D.LineJointMode.Round,
            BeginCapMode = Line2D.LineCapMode.Round,
            EndCapMode = Line2D.LineCapMode.Round,
            ZIndex = ZLayer,
        };
        return line;
    }

    public override void _Process(double delta)
    {
        // 任一目标失效时销毁自身
        if (_source == null || _target == null || !IsInstanceValid(_source) || !IsInstanceValid(_target))
        {
            QueueFree();
            return;
        }

        // 获取两端位置，加上垂直偏移让锁链连接到身体而非脚底
        Vector2 sourcePos = _source.GlobalPosition + new Vector2(0, VerticalOffset);
        Vector2 targetPos = _target.GlobalPosition + new Vector2(0, VerticalOffset);

        Vector2 direction = (targetPos - sourcePos).Normalized();
        Vector2 perpendicular = direction.Orthogonal(); // 垂直方向，用于制造X形

        // 计算两条链的端点：一条向左偏，一条向右偏
        Vector2 offset = perpendicular * CrossOffset;

        // 更新两条线的点
        _chain1!.Points =
        [
            ToLocal(sourcePos + offset),
            ToLocal(targetPos - offset), // 到对侧形成X
        ];

        _chain2!.Points =
        [
            ToLocal(sourcePos - offset),
            ToLocal(targetPos + offset), // 到对侧形成X
        ];
    }

    /// <summary>
    /// 手动销毁锁链（例如怪物死亡时）
    /// </summary>
    public void Break()
    {
        // 可以在这里添加断裂动画/音效
        QueueFree();
    }
}
