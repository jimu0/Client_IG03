using System.Collections.Generic;
using System.IO;
using StarUnion_EfficientAnimation;
using UnityEditor;
using UnityEngine;

namespace StarUnion_EfficientAnimation_Tool
{
    public class CreateAtlasTool : EditorWindow
    {
        private static bool canOpen()
        {
            string pathGUID = Selection.assetGUIDs[0];
            _animationsTopPath = AssetDatabase.GUIDToAssetPath(pathGUID);
            //string[] filePaths = Directory.GetFiles(_animationsTopPath, "*_MergedTexture.png", SearchOption.AllDirectories);
            string[] filePaths = Directory.GetFiles(_animationsTopPath, "*_mtex.png", SearchOption.AllDirectories);
            if (filePaths.Length > 0)
            {
                return false;
            }
            int folderDepth = EfficientAnimationAtlasTool.GetFolderDepth(_animationsTopPath);
            if (folderDepth != 1)
            {
                return false;
            }
            string parentDirectoryName = Path.GetDirectoryName(_animationsTopPath);
            //string[] collectionPath = Directory.GetFiles(parentDirectoryName, "*_animation.asset",SearchOption.AllDirectories);
            string[] collectionPath = Directory.GetFiles(parentDirectoryName, "*_ass.asset",SearchOption.AllDirectories);
            if (collectionPath.Length == 0)
            {
                return false;
            }
            AnimationCollectionInfo collectionInfo = AssetDatabase.LoadAssetAtPath<AnimationCollectionInfo>(collectionPath[0]);
            if (collectionInfo == null)
            {
                return false;
            }
            return true;
        }
        
        [MenuItem("Assets/Merge Similar Height Textures")]
        static void InitMergeTextureWindow()
        {
            if (!canOpen())
            {
                return;
            }
            CreateAtlasTool window = (CreateAtlasTool)GetWindow(typeof(CreateAtlasTool));
            window.minSize = window.maxSize = new Vector2(600, 80);
            window.Show();
        }
        
        private int heightDifference = 40;
        private bool removeEmptyLine = true;
        private static string _animationsTopPath;

        private void OnGUI()
        {
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            heightDifference = EditorGUILayout.IntField("Height Difference", heightDifference);
            removeEmptyLine = EditorGUILayout.Toggle("Remove Empty Line", removeEmptyLine);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            if (GUILayout.Button("Create"))
            {
                if (!canOpen())
                {
                    return;
                }
                MergeTexturesWithSimilarHeight(_animationsTopPath,heightDifference,EfficientAnimationAtlasTool.maxTextureSize,removeEmptyLine);
            }
        }

        private static bool GenerateAtlas(List<Vector2> sizes,int maxTextureSize,ref List<Rect> results)
        {
            results.Clear();
            int heightMax = 0;
            foreach (var size in sizes)
            {
                if (size.y > heightMax)
                {
                    heightMax = (int)size.y;
                }
            }
            int locationX = 0;
            int locationY = 0;
            for (int i = 0; i < sizes.Count; i++)
            {
                if (locationX + sizes[i].x < maxTextureSize)
                {
                    Rect thisRect = new Rect(locationX, locationY, sizes[i].x, sizes[i].y);
                    results.Add(thisRect);
                    locationX = locationX + (int)sizes[i].x;
                }
                else if (locationY + heightMax * 2 < maxTextureSize)
                {
                    locationX = 0;
                    locationY = locationY + heightMax;
                    if (locationX + sizes[i].x < maxTextureSize)
                    {
                        Rect thisRect = new Rect(locationX, locationY, sizes[i].x, sizes[i].y);
                        results.Add(thisRect);
                        locationX = locationX + (int) sizes[i].x;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        public static void MergeTexturesWithSimilarHeight(string path,int heightDifference,int maxTextureSize,bool removeEmpty = true)
        {
            string parentDirectoryName = Path.GetDirectoryName(path);
            //string[] collectionPath = Directory.GetFiles(parentDirectoryName, "*_animation.asset",SearchOption.AllDirectories);
            string[] collectionPath = Directory.GetFiles(parentDirectoryName, "*_ass.asset",SearchOption.AllDirectories);
            if (collectionPath.Length <= 0)
            {
                return;
            }
            
            string collectionName = Path.GetDirectoryName(path);
            collectionName = Path.GetFileName(collectionName);
            string[] allTexturesPath = Directory.GetFiles(path, "*.png",SearchOption.AllDirectories);
            List<Texture2D> allTextures = new List<Texture2D>();
            List<Vector2> allSizes = new List<Vector2>();
            for (int i = 0; i < allTexturesPath.Length; i++)
            {
                TextureImporter textureImporter = AssetImporter.GetAtPath(allTexturesPath[i]) as TextureImporter;
                textureImporter.alphaIsTransparency = true;
                textureImporter.isReadable = true;
                textureImporter.mipmapEnabled = false;
                textureImporter.npotScale = TextureImporterNPOTScale.None;
                AssetDatabase.ImportAsset(allTexturesPath[i]);
                Texture2D texture2D = AssetDatabase.LoadAssetAtPath<Texture2D>(allTexturesPath[i]);
                Vector2 size = new Vector2(texture2D.width, texture2D.height);
                allSizes.Add(size);
                allTextures.Add(texture2D);
            }

            Texture2D[] allTexturesSorted = allTextures.ToArray();
            Vector2[] allSizedSorted = allSizes.ToArray();

            for (int i = allSizedSorted.Length-1; i > 0; i--)
            {
                for (int j = 0; j < i; j++)
                {
                    if (allSizedSorted[j].y > allSizedSorted[j + 1].y)
                    {
                        Vector2 tempSize = allSizedSorted[j];
                        allSizedSorted[j] = allSizedSorted[j + 1];
                        allSizedSorted[j + 1] = tempSize;
                        Texture2D tempTexture = allTexturesSorted[j];
                        allTexturesSorted[j] = allTexturesSorted[j + 1];
                        allTexturesSorted[j + 1] = tempTexture;
                    }
                }
            }   
            Queue<Texture2D> allTexturesQueue = new Queue<Texture2D>();
            Queue<Vector2> allSizeQueue = new Queue<Vector2>();
            for (int i = 0; i < allSizedSorted.Length; i++)
            {
                allTexturesQueue.Enqueue(allTexturesSorted[i]);
                allSizeQueue.Enqueue(allSizedSorted[i]);
            }

            Dictionary<int, List<Texture2D>> atlasTextures = new Dictionary<int, List<Texture2D>>();
            Dictionary<int, List<Rect>> atlasRects = new Dictionary<int, List<Rect>>();

            int atlasId = 0;
            while (allSizeQueue.Count > 1)
            {
                Queue<Texture2D> texture2DInAtlas = new Queue<Texture2D>();
                Queue<Vector2> sizesInAtlas = new Queue<Vector2>();
                Vector2 size = allSizeQueue.Dequeue();
                Texture2D texture2D = allTexturesQueue.Dequeue();
                bool nextSimilarHeight = false;
                do
                {
                    Vector2 sizeNext = allSizeQueue.Peek();
                    if (sizeNext.y - size.y <= heightDifference)
                    {
                        sizesInAtlas.Enqueue(allSizeQueue.Dequeue());
                        texture2DInAtlas.Enqueue(allTexturesQueue.Dequeue());
                        nextSimilarHeight = true;
                    }
                    else
                    {
                        nextSimilarHeight = false;
                    }
                } while (nextSimilarHeight && allSizeQueue.Count > 0);
                if (sizesInAtlas.Count > 0)
                {
                    sizesInAtlas.Enqueue(size);
                    texture2DInAtlas.Enqueue(texture2D);

                    while (sizesInAtlas.Count > 0)
                    {
                        List<Vector2> sizesInAtlasNow = new List<Vector2>();
                        List<Texture2D> texture2DInAtlasNow = new List<Texture2D>();
                        List<Rect> rectsInAtlas = new List<Rect>();
                        bool canFill = true;
                        do
                        {
                            sizesInAtlasNow.Add(sizesInAtlas.Peek());
                            canFill = GenerateAtlas(sizesInAtlasNow, maxTextureSize, ref rectsInAtlas);
                            if (canFill)
                            {
                                sizesInAtlas.Dequeue();
                                texture2DInAtlasNow.Add(texture2DInAtlas.Dequeue());
                            }
                            else
                            {
                                sizesInAtlasNow.RemoveAt(sizesInAtlasNow.Count-1);
                                canFill = false;
                            }
                        } while (canFill && sizesInAtlas.Count > 0);
                        GenerateAtlas(sizesInAtlasNow, maxTextureSize, ref rectsInAtlas);
                        if (sizesInAtlasNow.Count > 0)
                        {
                            atlasTextures.Add(atlasId,texture2DInAtlasNow);
                            atlasRects.Add(atlasId,rectsInAtlas);
                            atlasId++;
                        }
                        else
                        {
                            sizesInAtlas.Dequeue();
                            texture2DInAtlas.Dequeue();
                        }
                    }
                }
            }

            Dictionary<int, Texture2D> mergedTextures = new Dictionary<int, Texture2D>();
            for (int i = 0; i < atlasId; i++)
            {
                int widthMax = 0;
                int heightMax = 0;
                List<Texture2D> texture2DInAtlas = atlasTextures[i];
                List<Rect> rectsInAtlas = atlasRects[i];
                if (removeEmpty)
                {
                    Rect lastRect = rectsInAtlas[rectsInAtlas.Count - 1];
                    if (lastRect.y > 0 && lastRect.xMax < maxTextureSize/2)
                    {
                        float removeY = lastRect.y;
                        bool canRemove = true;
                        while (canRemove && rectsInAtlas.Count > 0)
                        {
                            Rect rect = rectsInAtlas[rectsInAtlas.Count - 1];
                            if (rect.y.Equals(removeY))
                            {
                                rectsInAtlas.RemoveAt(rectsInAtlas.Count-1);
                                texture2DInAtlas.RemoveAt(texture2DInAtlas.Count-1);
                            }
                            else
                            {
                                canRemove = false;
                            }
                        }
                    }
                }
                for (int j = 0; j < rectsInAtlas.Count; j++)
                {
                    if (rectsInAtlas[j].xMax > widthMax)
                    {
                        widthMax = (int)rectsInAtlas[j].xMax;
                    }
                    if (rectsInAtlas[j].yMax > heightMax)
                    {
                        heightMax = (int)rectsInAtlas[j].yMax;
                    }
                }
                Texture2D newTexture2D = new Texture2D(widthMax, heightMax,TextureFormat.RGBA32,false);
                for (int k = 0; k < newTexture2D.width; k++)
                {
                    for (int l = 0; l < newTexture2D.height; l++)
                    {
                        newTexture2D.SetPixel(k,l,Color.clear);
                    }
                }
                newTexture2D.Apply();
                for (int j = 0; j < texture2DInAtlas.Count; j++)
                {
                    Color[] colorBlock = texture2DInAtlas[j].GetPixels();
                    Rect rect = rectsInAtlas[j];
                    newTexture2D.SetPixels((int)rect.x, (int)rect.y, (int)rect.width, (int)rect.height, colorBlock);
                }
                newTexture2D.Apply();
                byte[] textureData = ImageConversion.EncodeToPNG(newTexture2D);
                //string filePath = path + "/" + collectionName + "_" + i + "_MergedTexture.png";
                string filePath = path + "/" + collectionName + "_" + i + "_mtex.png";
                FileStream file = File.Open(filePath, FileMode.Create);
                BinaryWriter writer = new BinaryWriter(file);
                writer.Write(textureData);
                file.Close();
                DestroyImmediate(newTexture2D);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                TextureImporter textureImporter = AssetImporter.GetAtPath(filePath) as TextureImporter;
                textureImporter.alphaIsTransparency = true;
                textureImporter.mipmapEnabled = false;
                textureImporter.npotScale = TextureImporterNPOTScale.None;
                AssetDatabase.ImportAsset(filePath);
                Texture2D mergedTexture2D = AssetDatabase.LoadAssetAtPath<Texture2D>(filePath);
                mergedTextures.Add(i,mergedTexture2D);
            }
            
            if (collectionPath.Length > 0)
            {
                AnimationCollectionInfo collectionInfo = AssetDatabase.LoadAssetAtPath<AnimationCollectionInfo>(collectionPath[0]);
                Dictionary<int, AtlasInfo> texture2DIndex = new Dictionary<int, AtlasInfo>();
                for (int i = 0; i < collectionInfo.atlasInfos.Count; i++)
                {
                    texture2DIndex.Add(i,collectionInfo.atlasInfos[i]);
                }
                Dictionary<AtlasInfo, int> newTexure2Dindex = new Dictionary<AtlasInfo, int>();
                List<AtlasInfo> newAtlasInfos = new List<AtlasInfo>();
                for (int i = 0; i < allTexturesSorted.Length; i++)
                {
                    AtlasInfo atlasInfoToThisIndex = null;
                    for (int j = 0; j < collectionInfo.atlasInfos.Count; j++)
                    {
                        if (allTexturesSorted[i].Equals(collectionInfo.atlasInfos[j].texture))
                        {
                            atlasInfoToThisIndex = collectionInfo.atlasInfos[j];
                            break;
                        }
                    }
                    if (atlasInfoToThisIndex != null)
                    {
                        atlasInfoToThisIndex.id = collectionInfo.firstId + i;
                        newTexure2Dindex.Add(atlasInfoToThisIndex, i);
                        newAtlasInfos.Add(atlasInfoToThisIndex);
                    }
                    else
                    {
                        newAtlasInfos.Add(null);
                    }
                }
                collectionInfo.atlasInfos = newAtlasInfos;
                for (int i = 0; i < collectionInfo.animationLocations.Count; i++)
                {
                    if (texture2DIndex.ContainsKey(collectionInfo.animationLocations[i].x))
                    {
                        AtlasInfo atlasInfo = texture2DIndex[collectionInfo.animationLocations[i].x];
                        if (newTexure2Dindex.ContainsKey(atlasInfo))
                        {
                            collectionInfo.animationLocations[i] = new Vector2Int(newTexure2Dindex[atlasInfo],
                                collectionInfo.animationLocations[i].y);
                        }
                    }
                }

                Dictionary<Texture2D, int> mergedTextureFirstId = new Dictionary<Texture2D, int>();
                for (int i = 0; i < collectionInfo.atlasInfos.Count; i++)
                {
                    Texture2D texture2D = collectionInfo.atlasInfos[i].texture;
                    int atlasIndex = -1;
                    Rect rectInAtlas = Rect.zero;
                    foreach (var atlasTexture in atlasTextures)
                    {
                        for (int j = 0; j < atlasTexture.Value.Count; j++)
                        {
                            if (texture2D.Equals(atlasTexture.Value[j]))
                            {
                                atlasIndex = atlasTexture.Key;
                                rectInAtlas = atlasRects[atlasIndex][j];
                                break;
                            }
                        }
                    }
                    if (atlasIndex > -1 && !rectInAtlas.Equals(Rect.zero) && mergedTextures.ContainsKey(atlasIndex))
                    {
                        collectionInfo.atlasInfos[i].texture = mergedTextures[atlasIndex];
                        Vector4 uvOffsetAndScale = new Vector4();
                        uvOffsetAndScale.x = rectInAtlas.x / mergedTextures[atlasIndex].width;
                        uvOffsetAndScale.y = rectInAtlas.y / mergedTextures[atlasIndex].height;
                        uvOffsetAndScale.z = rectInAtlas.width / mergedTextures[atlasIndex].width;
                        uvOffsetAndScale.w = rectInAtlas.height / mergedTextures[atlasIndex].height;
                        collectionInfo.atlasInfos[i].offsetAndScale = uvOffsetAndScale;
                        if (!mergedTextureFirstId.ContainsKey(mergedTextures[atlasIndex]))
                        {
                            mergedTextureFirstId[mergedTextures[atlasIndex]] = collectionInfo.atlasInfos[i].id;
                        }
                        else
                        {
                            collectionInfo.atlasInfos[i].atlasId = mergedTextureFirstId[mergedTextures[atlasIndex]];
                        }
                    }
                }
                foreach (var atlasTexture in atlasTextures)
                {
                    for (int i = 0; i < atlasTexture.Value.Count; i++)
                    {
                        string texturePath = AssetDatabase.GetAssetPath(atlasTexture.Value[i]);
                        AssetDatabase.DeleteAsset(texturePath);
                    }
                }
                foreach (var atlasInfo in collectionInfo.atlasInfos)
                {
                    string texturePath = AssetDatabase.GetAssetPath(atlasInfo.texture);
                    TextureImporter textureImporter = AssetImporter.GetAtPath(texturePath) as TextureImporter;
                    textureImporter.alphaIsTransparency = true;
                    textureImporter.isReadable = false;
                    textureImporter.mipmapEnabled = false;
                    textureImporter.npotScale = TextureImporterNPOTScale.None;
                    AssetDatabase.ImportAsset(texturePath);
                }
                EditorUtility.SetDirty(collectionInfo);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            
        }
        
    }
}
