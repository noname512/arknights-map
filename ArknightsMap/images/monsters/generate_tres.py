import os
import re

TRES_TEMPLATE = """[gd_resource type="SpineSkeletonDataResource" load_steps=3 format=3]

[ext_resource type="SpineAtlasResource" uid="{atlas_uid}" path="{atlas_path}" id="1_atlas"]
[ext_resource type="SpineSkeletonFileResource" uid="{skel_uid}" path="{skel_path}" id="2_skel"]

[resource]
atlas_res = ExtResource("1_atlas")
skeleton_file_res = ExtResource("2_skel")
"""

def get_uid_from_import(import_file_path):
    """从.import文件中提取uid"""
    if not os.path.exists(import_file_path):
        return None
    with open(import_file_path, 'r', encoding='utf-8') as f:
        content = f.read()
        # 匹配 uid="uid://..."
        match = re.search(r'uid="uid://([^"]+)"', content)
        return match.group(1) if match else None

def generate_tres_files():
    root_dir = os.getcwd()
    
    for subdir in os.listdir(root_dir):
        subdir_path = os.path.join(root_dir, subdir)
        if os.path.isdir(subdir_path):
            tres_filename = f"{subdir}.tres"
            tres_path = os.path.join(subdir_path, tres_filename)
            
            atlas_uid = None
            skel_uid = None
            atlas_path = ""
            skel_path = ""
            
            # 扫描目录下的 .import 文件
            for file in os.listdir(subdir_path):
                if file.endswith(".atlas.import"):
                    atlas_uid = get_uid_from_import(os.path.join(subdir_path, file))
                    atlas_path = f"res://ArknightsMap/images/monsters/{subdir}/{file.replace('.import', '')}"
                elif file.endswith("_424.skel.import"):
                    skel_uid = get_uid_from_import(os.path.join(subdir_path, file))
                    skel_path = f"res://ArknightsMap/images/monsters/{subdir}/{file.replace('.import', '')}"
            
            if atlas_uid and skel_uid:
                content = TRES_TEMPLATE.format(
                    atlas_uid=atlas_uid, atlas_path=atlas_path,
                    skel_uid=skel_uid, skel_path=skel_path
                )
                with open(tres_path, 'w', encoding='utf-8') as f:
                    f.write(content)
                print(f"已生成: {tres_path}")
            else:
                print(f"跳过: {subdir} (未找到足够的 .import 文件)")

if __name__ == "__main__":
    generate_tres_files()