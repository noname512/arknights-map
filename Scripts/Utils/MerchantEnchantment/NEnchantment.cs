using Godot;
using MegaCrit.Sts2.Core.Models;

namespace ArknightsMap.Scripts.Utils.MerchantEnchantment;

public partial class NEnchantment : Control
{
    public TextureRect? Icon;
    public Label? AmountLabel;
    private EnchantmentModel? _model;

    public EnchantmentModel Model
    {
        get { return _model ?? throw new InvalidOperationException("Model was accessed before it was set."); }
        set
        {
            if (_model != value)
            {
                EnchantmentModel? model = _model;
                _model = value;
                ModelChanged?.Invoke(model, _model);
            }
        }
    }

    public event Action<EnchantmentModel?, EnchantmentModel?>? ModelChanged;

    public static NEnchantment Create(EnchantmentModel enchantment)
    {
        GD.Print($"NEnchantment Create {enchantment.GetType().Name}");
        string scenePath = "res://ArknightsMap/scenes/enchantment.tscn";
        PackedScene? packedScene = GD.Load<PackedScene>(scenePath);
        NEnchantment nEnchantment = packedScene.Instantiate<NEnchantment>();
        nEnchantment.Name = $"NEnchantment-{enchantment.Id}";
        nEnchantment.Model = enchantment;
        return nEnchantment;
    }

    public override void _Ready()
    {
        Icon = GetNode<TextureRect>("%Icon");
        AmountLabel = GetNode<Label>("%AmountLabel");
        Reload();
    }

    private void Reload()
    {
        if (IsNodeReady() && _model != null)
        {
            Icon!.Texture = Model.Icon;
            AmountLabel!.Text = $"{Model.Amount}";
            AmountLabel.Visible = Model.Amount > 0;
        }
    }
}
