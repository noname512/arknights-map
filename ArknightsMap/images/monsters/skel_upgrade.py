import os
import subprocess

def process_skel_files():
    # 获取当前脚本所在目录
    root_dir = os.getcwd()
    
    # 遍历当前目录下的所有子文件夹
    for subdir in os.listdir(root_dir):
        subdir_path = os.path.join(root_dir, subdir)
        
        # 确保是文件夹
        if os.path.isdir(subdir_path):
            # 在子文件夹中寻找 .skel 文件
            for file in os.listdir(subdir_path):
                if file.endswith(".skel"):
                    skel_path = os.path.join(subdir_path, file)
                    
                    # 构建输出文件名：B.skel -> B_424.skel
                    base_name = os.path.splitext(file)[0]
                    output_name = f"{base_name}_424.skel"
                    output_path = os.path.join(subdir_path, output_name)
                    
                    # 构建执行命令
                    # 假设 SpineSkeletonDataConverterFixed.exe 在当前环境变量中或与脚本在同一目录
                    cmd = ["SpineSkeletonDataConverterFixed.exe", skel_path, output_path, "-v", "4.2.4"]
                    
                    print(f"正在处理: {subdir}/{file} ...")
                    try:
                        # 执行命令
                        subprocess.run(cmd, check=True)
                        print(f"成功生成: {output_name}")
                    except subprocess.CalledProcessError as e:
                        print(f"处理失败: {file}, 错误代码: {e}")
                    except FileNotFoundError:
                        print("错误: 未找到 SpineSkeletonDataConverterFixed.exe，请确保其在 PATH 中或与脚本同目录。")
                        return

if __name__ == "__main__":
    process_skel_files()
    print("所有任务执行完毕。")