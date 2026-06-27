import re
from pathlib import Path

def generate_tscn():
    print("开始扫描并生成 .tscn 文件...")
    print("-" * 40)

    # 获取当前运行脚本的目录
    base_dir = Path.cwd() / "images" / "monsters"
    output_dir = Path.cwd() / "scenes" / "monsters"
    
    # 编译匹配 uid="..." 的正则表达式，提高匹配效率
    uid_pattern = re.compile(r'uid="([^"]+)"')
    
    # 使用 rglob 递归查找所有层级目录下的 .tres 文件
    tres_files = list(base_dir.rglob("*.tres"))
    
    if not tres_files:
        print("[提示] 当前目录及子目录下未找到任何 .tres 文件。")
        return

    for tres_file in tres_files:
        file_b = tres_file.stem          # 获取文件名（不含后缀），即 B
        folder_a = tres_file.parent.name # 获取直接父文件夹名，即 A
        
        # 读取第一行提取 uid，指定 utf-8 编码防止中文路径报错
        try:
            with open(tres_file, 'r', encoding='utf-8') as f:
                first_line = f.readline()
                
            match = uid_pattern.search(first_line)
            if match:
                uid = match.group(1)
            else:
                print(f"[警告] 文件 {tres_file.name} 的首行未找到 uid，已跳过！")
                continue
        except Exception as e:
            print(f"[错误] 读取文件 {tres_file.name} 时出错: {e}")
            continue

        # 定义输出的 .tscn 文件路径（直接放在当前根目录下）
        output_path = output_dir / f"{file_b}.tscn"

        if output_path.exists():
            print(f"文件 {file_b} 已存在，跳过！")
            continue
        
        # 使用多行 f-string 动态组装 tscn 模板内容
        tscn_content = f"""[gd_scene load_steps=2 format=3]

[ext_resource type="Script" uid="uid://cghtlla3sw0po" path="res://src/Core/Nodes/Combat/NCreatureVisuals.cs" id="1_v77x8"]
[ext_resource type="SpineSkeletonDataResource" uid="{uid}" path="res://ArknightsMap/images/monsters/{folder_a}/{file_b}.tres" id="ssdr"]

[node name="{file_b}" type="Node2D"]
script = ExtResource("1_v77x8")
metadata/_edit_group_ = true
metadata/_edit_lock_ = true

[node name="Visuals" type="SpineSprite" parent="."]
skeleton_data_res = ExtResource("ssdr")
preview_skin = "default"
preview_animation = "Idle"
preview_frame = false
preview_time = 0.0
unique_name_in_owner = true
scale = Vector2(-0.5, 0.5)

[node name="Bounds" type="Control" parent="."]
unique_name_in_owner = true
layout_mode = 3
anchors_preset = 0
offset_left = -100.0
offset_top = -180.0
offset_right = 100.0

[node name="IntentPos" type="Marker2D" parent="."]
unique_name_in_owner = true
position = Vector2(0, -225)

[node name="CenterPos" type="Marker2D" parent="."]
unique_name_in_owner = true
position = Vector2(0, -90)

"""
        
        # 将内容写入到本地，强制使用 utf-8 编码，迎合 Godot 的标准
        try:
            with open(output_path, 'w', encoding='utf-8') as f:
                f.write(tscn_content)
            print(f"[成功] 已生成: {output_path.name} (UID: {uid})")
        except Exception as e:
            print(f"[错误] 写入文件 {output_path.name} 时出错: {e}")

    print("-" * 40)
    print("所有文件处理完毕！")

if __name__ == "__main__":
    generate_tscn()