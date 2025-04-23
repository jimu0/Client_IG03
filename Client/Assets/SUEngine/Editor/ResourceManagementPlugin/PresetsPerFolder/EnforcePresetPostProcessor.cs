using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental;
using UnityEditor.Presets;
using UnityEngine;

namespace PresetsPerFolder
{
    /// <summary>
    /// 本示例类实现自动将预设（Preset）应用到所在文件夹及其子文件夹内的资源。
    /// 代码分为三个部分，通过设置导入器依赖确保所有资源的导入过程保持确定性。
    ///
    /// OnPreprocessAsset：
    /// 该方法从根文件夹递归遍历至资源所在文件夹，
    /// 为每个包含预设的文件夹注册自定义依赖项（CustomDependency）。
    /// 随后加载该文件夹内所有预设并尝试应用到资源导入器。
    /// 若应用成功，则为每个预设添加直接依赖，确保预设值变更时触发资源重新导入。
    /// </summary>
    public class EnforcePresetPostProcessor : AssetPostprocessor
    {
        void OnPreprocessAsset()
        {
            // 通过条件过滤确保：
            // 1. 仅处理"Assets/"路径下的资源，避免影响Package资源
            // 2. 排除.cs文件防止预设操作触发代码编译
            // 3. 排除.preset文件自身，避免无限导入循环
            // 根据项目需求可添加更多排除条件
            if (assetPath.StartsWith("Assets/") && !assetPath.EndsWith(".cs") && !assetPath.EndsWith(".preset"))
            {
                var path = Path.GetDirectoryName(assetPath);
                ApplyPresetsFromFolderRecursively(path);
            }
        }

        void ApplyPresetsFromFolderRecursively(string folder)
        {
            // 从父文件夹向资源所在文件夹递归应用预设，
            // 确保最接近资源的预设最后应用（遵循就近覆盖原则）
            var parentFolder = Path.GetDirectoryName(folder);
            if (!string.IsNullOrEmpty(parentFolder))
                ApplyPresetsFromFolderRecursively(parentFolder);

            // 为当前文件夹注册预设变更自定义依赖
            // 当文件夹内预设增删时触发资源重新导入
            context.DependsOnCustomDependency($"PresetPostProcessor_{folder}");

            // 使用系统目录方法（非AssetDatabase）查找预设文件
            // 避免全局搜索在独立导入进程中引发问题
            var presetPaths =
                Directory.EnumerateFiles(folder, "*.preset", SearchOption.TopDirectoryOnly)
                    .OrderBy(a => a);

            foreach (var presetPath in presetPaths)
            {
                var preset = AssetDatabase.LoadAssetAtPath<Preset>(presetPath);
                
                // 添加预设依赖的两种情形：
                // 1. 资源先于预设导入时，需建立依赖触发重新导入
                // 2. 预设成功应用时，需建立值变更依赖
                if (preset == null || preset.ApplyTo(assetImporter))
                {
                    // 使用DependsOnArtifact因预设为原生资源
                    context.DependsOnArtifact(presetPath);
                }
            }
        }

        /// <summary>
        /// 在项目加载或代码编译后调用（didDomainReload=true）
        /// 需正确初始化哈希值，因：
        /// 1. Unity不会对已导入预设触发OnPostprocessAllAssets
        /// 2. 自定义依赖不会跨会话保存，需每次重建
        /// </summary>
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets,
            string[] movedFromAssetPaths, bool didDomainReload)
        {
            if (didDomainReload)
            {
                // 使用glob过滤器全局搜索预设文件
                var allPaths = AssetDatabase.FindAssets("glob:\"**.preset\"")
                    .Select(AssetDatabase.GUIDToAssetPath)
                    .OrderBy(a => a)
                    .ToList();

                bool atLeastOnUpdate = false;
                string previousPath = string.Empty;
                Hash128 hash = new Hash128();

                foreach (var path in allPaths)
                {
                    var folder = Path.GetDirectoryName(path);
                    if (folder != previousPath)
                    {
                        // 发现新文件夹时注册自定义依赖
                        if (!string.IsNullOrEmpty(previousPath))
                        {
                            AssetDatabase.RegisterCustomDependency($"PresetPostProcessor_{previousPath}", hash);
                            atLeastOnUpdate = true;
                        }
                        hash = new Hash128();
                        previousPath = folder;
                    }

                    // 组合路径和预设类型生成哈希
                    hash.Append(path);
                    hash.Append(AssetDatabase.LoadAssetAtPath<Preset>(path).GetTargetFullTypeName());
                }

                // 注册最后一个文件夹的依赖
                if (!string.IsNullOrEmpty(previousPath))
                {
                    AssetDatabase.RegisterCustomDependency($"PresetPostProcessor_{previousPath}", hash);
                    atLeastOnUpdate = true;
                }

                // 依赖更新后触发资源刷新
                if (atLeastOnUpdate)
                    AssetDatabase.Refresh();
            }
        }
    }

    /// <summary>
    /// InitPresetDependencies：
    /// 项目加载时扫描所有预设，为每个含预设的文件夹创建基于预设列表和类型的自定义依赖哈希
    ///
    /// OnAssetsModified：
    /// 处理预设的增删改移动操作，更新相关文件夹的依赖哈希
    ///
    /// 优化建议：
    /// 理想情况下应按预设类型区分依赖（如"Preset_{presetType}_{folder}"）
    /// 以避免纹理资源因FBX预设变更被重新导入
    /// 本示例为简化未实现该逻辑
    /// </summary>
    public class UpdateFolderPresetDependency : AssetsModifiedProcessor
    {
        /// <summary>
        /// 资产变更回调方法
        /// 检测预设相关操作并标记需更新的文件夹
        /// </summary>
        protected override void OnAssetsModified(string[] changedAssets, string[] addedAssets, string[] deletedAssets, AssetMoveInfo[] movedAssets)
        {
            HashSet<string> folders = new HashSet<string>();

            // 收集所有受影响的文件夹路径
            void ProcessAsset(string asset, bool isMoveDestination = false)
            {
                if (asset.EndsWith(".preset"))
                {
                    folders.Add(Path.GetDirectoryName(asset));
                }
            }

            foreach (var asset in changedAssets) ProcessAsset(asset);
            foreach (var asset in addedAssets) ProcessAsset(asset);
            foreach (var asset in deletedAssets) ProcessAsset(asset);
            foreach (var movedAsset in movedAssets)
            {
                ProcessAsset(movedAsset.sourceAssetPath);
                ProcessAsset(movedAsset.destinationAssetPath, true);
            }

            // 延迟执行依赖更新以避免冲突
            if (folders.Count > 0)
            {
                EditorApplication.delayCall += () => DelayedDependencyRegistration(folders);
            }
        }

        /// <summary>
        /// 延迟更新：重新计算指定文件夹的预设依赖哈希
        /// </summary>
        static void DelayedDependencyRegistration(HashSet<string> folders)
        {
            foreach (var folder in folders)
            {
                var presetPaths = AssetDatabase.FindAssets("glob:\"**.preset\"", new[] { folder })
                    .Select(AssetDatabase.GUIDToAssetPath)
                    .Where(p => Path.GetDirectoryName(p) == folder)
                    .OrderBy(p => p);

                Hash128 hash = new Hash128();
                foreach (var path in presetPaths)
                {
                    hash.Append(path);
                    hash.Append(AssetDatabase.LoadAssetAtPath<Preset>(path).GetTargetFullTypeName());
                }

                AssetDatabase.RegisterCustomDependency($"PresetPostProcessor_{folder}", hash);
            }

            AssetDatabase.Refresh(); // 触发依赖检查
        }
    }
}