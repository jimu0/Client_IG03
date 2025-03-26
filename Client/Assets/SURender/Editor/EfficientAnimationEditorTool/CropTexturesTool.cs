using System.IO;
using UnityEditor;
using UnityEngine;

namespace StarUnion_EfficientAnimation_Tool
{
    public class CropTexturesTool : EditorWindow
    {
        [MenuItem("Assets/Crop Textures")]
        static void InitCropWindow()
        {
            CropTexturesTool window = (CropTexturesTool)EditorWindow.GetWindow(typeof(CropTexturesTool));
            window.minSize = window.maxSize = new Vector2(600, 120);
            window.Show();
            string pathGUID = Selection.assetGUIDs[0];
            _animationsTopPath = AssetDatabase.GUIDToAssetPath(pathGUID);
        }
        
        private Vector2 spriteSize = new Vector2(128,128);
        private Vector4 crop = new Vector4(0, 0, 0, 0);
        private int minCellSize = 4;
        private static string _animationsTopPath;
        private const float alphaClip = 0.001f; 

        private void OnGUI()
        {
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            spriteSize = EditorGUILayout.Vector2Field("Size",spriteSize);
            crop = EditorGUILayout.Vector4Field("Crop", crop);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            minCellSize = EditorGUILayout.IntField("min cell size", minCellSize);
            if (GUILayout.Button("Auto Crop Calculation"))
            {
                string pathGUID = Selection.assetGUIDs[0];
                _animationsTopPath = AssetDatabase.GUIDToAssetPath(pathGUID);
                CalculationCrop(_animationsTopPath,minCellSize,out spriteSize,out crop);
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            if (GUILayout.Button("Crop All"))
            {
                string pathGUID = Selection.assetGUIDs[0];
                _animationsTopPath = AssetDatabase.GUIDToAssetPath(pathGUID);
                if (AllTextureSizeEqualInPath(_animationsTopPath))
                {
                    CropAllTexture(_animationsTopPath, spriteSize, crop);
                    Vector4 offsetAndScale = GetOffsetAndScale(spriteSize, crop);
                    Debug.Log(offsetAndScale.x+","+offsetAndScale.y+","+offsetAndScale.z+","+offsetAndScale.w);
                }
                else
                {
                    EditorUtility.DisplayDialog("Error", "texture size not equal to each other!", "ok");
                }
            }
        }

        public static Vector4 GetOffsetAndScale(Vector2 size, Vector4 crop)
        {
            Vector4 result = new Vector4();
            result.x = (crop.x - crop.y) / size.x / 2;
            result.y = (crop.z - crop.w) / size.y / 2;
            result.z = (size.x - crop.x - crop.y) / size.x;
            result.w = (size.y - crop.z - crop.w) / size.y;
            return result;
        }

        public static bool AllTextureSizeEqualInPath(string path)
        {
            string[] allFilePaths = Directory.GetFiles(path, "*.png",SearchOption.AllDirectories);
            int width = 0;
            int height = 0;
            foreach (var filePath in allFilePaths)
            {
                Texture2D texture2D = AssetDatabase.LoadAssetAtPath<Texture2D>(filePath);
                if (width == 0)
                {
                    width = texture2D.width;
                }
                if (height == 0)
                {
                    height = texture2D.height;
                }
                if (texture2D.width != width || texture2D.height != height)
                {
                    return false;
                }
            }
            return true;
        }

        public static void CropAllTexture(string path,Vector2 size,Vector4 crop)
        {
            int width = (int)(size.x - crop.x - crop.y);
            int height = (int) (size.y - crop.z - crop.w);
            string[] allFilePaths = Directory.GetFiles(path, "*.png",SearchOption.AllDirectories);
            foreach (var filePath in allFilePaths)
            {
                Texture2D texture2D = AssetDatabase.LoadAssetAtPath<Texture2D>(filePath);
                
                TextureImporter textureImporterBefore = AssetImporter.GetAtPath(filePath) as TextureImporter;
                
                if (textureImporterBefore.alphaIsTransparency != true || textureImporterBefore.mipmapEnabled != false || textureImporterBefore.isReadable != true)
                {
                    textureImporterBefore.alphaIsTransparency = true;
                    textureImporterBefore.mipmapEnabled = false;
                    textureImporterBefore.isReadable = true;
                    textureImporterBefore.SaveAndReimport();
                }
                
                Texture2D newTexture2D = new Texture2D(width,height,TextureFormat.RGBA32, false);
                int widthOffset = (int)crop.x;
                int heightOffset = (int)crop.z;
                for (int i = 0; i < newTexture2D.width; i++)
                {
                    for (int j = 0; j < newTexture2D.height; j++)
                    {
                        Color oldColor = Color.clear;
                        if (i + widthOffset >= 0 && j + heightOffset >= 0)
                        {
                            oldColor = texture2D.GetPixel(i + widthOffset, j + heightOffset);
                        }
                        newTexture2D.SetPixel(i,j,oldColor);
                    }
                }
                newTexture2D.Apply();
                byte[] textureData = ImageConversion.EncodeToPNG(newTexture2D);
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
            }
        }

        public static bool CalculationCrop(string path,int minCellSize,out Vector2 oldSize,out Vector4 crop)
        {
            int leftCropAll = 2048;
            int rightCropAll = 2048;
            int downCropAll = 2048;
            int upCropAll = 2048;
            oldSize.x = 0;
            oldSize.y = 0;
            crop.x = 0;
            crop.y = 0;
            crop.z = 0;
            crop.w = 0;
            if (!Directory.Exists(path))
            {
                return false;
            }
            string[] allFilePaths = Directory.GetFiles(path, "*.png",SearchOption.AllDirectories);
            foreach (var filePath in allFilePaths)
            {
                Texture2D texture2D = AssetDatabase.LoadAssetAtPath<Texture2D>(filePath);
                if (oldSize.x == 0)
                {
                    oldSize.x = texture2D.width;
                }
                if (oldSize.y == 0)
                {
                    oldSize.y = texture2D.height;
                }
                if (texture2D.width != oldSize.x || texture2D.height != oldSize.y)
                {
                    EditorUtility.DisplayDialog("Error", "texture size not equal to each other!", "ok");
                    return false;
                }

                int leftCrop = 0;
                for (int i = 0; i < texture2D.width; i++)
                {
                    bool canCrop = true;
                    for (int j = 0; j < texture2D.height; j++)
                    {
                        
                        Color pixel = texture2D.GetPixel(i, j);
                        if (pixel.a > alphaClip)
                        {
                            canCrop = false;
                            break;
                        }
                    }
                    if (!canCrop)
                    {
                        leftCrop = i - 1;
                        if (leftCrop < 0)
                        {
                            leftCrop = 0;
                        }
                        break;
                    }
                }
                
                int rightCrop = 0;
                for (int i = texture2D.width - 1; i >= 0; i--)
                {
                    bool canCrop = true;
                    for (int j = 0; j < texture2D.height; j++)
                    {
                        
                        Color pixel = texture2D.GetPixel(i, j);
                        if (pixel.a > alphaClip)
                        {
                            canCrop = false;
                            break;
                        }
                    }
                    if (!canCrop)
                    {
                        rightCrop = texture2D.width - i - 2;
                        if (rightCrop < 0)
                        {
                            rightCrop = 0;
                        }
                        break;
                    }
                }

                int downCrop = 0;
                for (int i = 0; i < texture2D.height; i++)
                {
                    bool canCrop = true;
                    for (int j = 0; j < texture2D.width; j++)
                    {
                        Color pixel = texture2D.GetPixel(j,i);
                        if (pixel.a > alphaClip)
                        {
                            canCrop = false;
                            break;
                        }
                    }
                    if (!canCrop)
                    {
                        downCrop = i - 1;
                        if (downCrop < 0)
                        {
                            downCrop = 0;
                        }
                        break;
                    }
                }

                int upCrop = 0;
                for (int i = texture2D.height - 1; i >= 0; i--)
                {
                    bool canCrop = true;
                    for (int j = 0; j < texture2D.width; j++)
                    {
                        Color pixel = texture2D.GetPixel(j,i);
                        if (pixel.a > alphaClip)
                        {
                            canCrop = false;
                            break;
                        }
                    }
                    if (!canCrop)
                    {
                        upCrop = texture2D.height - i - 2;
                        if (upCrop < 0)
                        {
                            upCrop = 0;
                        }
                        break;
                    }
                }
               
                if (leftCrop < leftCropAll)
                {
                    leftCropAll = leftCrop;
                }
                if (rightCrop < rightCropAll)
                {
                    rightCropAll = rightCrop;
                }
                if (downCrop < downCropAll)
                {
                    downCropAll = downCrop;
                }
                if (upCrop < upCropAll)
                {
                    upCropAll = upCrop;
                }
            }
            crop.x = leftCropAll / minCellSize * minCellSize;
            crop.y = rightCropAll / minCellSize * minCellSize;
            crop.z = downCropAll / minCellSize * minCellSize;
            crop.w = upCropAll / minCellSize * minCellSize;
            return true;
        }
        
    }
}
