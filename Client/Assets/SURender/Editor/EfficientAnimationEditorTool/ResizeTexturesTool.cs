using System.IO;
using UnityEditor;
using UnityEngine;

namespace StarUnion_EfficientAnimation_Tool
{
    public class ResizeTexturesTool : EditorWindow
    {
        [MenuItem("Assets/Resize Textures")]
        static void InitResizeWindow()
        {
            ResizeTexturesTool window = (ResizeTexturesTool)EditorWindow.GetWindow(typeof(ResizeTexturesTool));
            window.minSize = window.maxSize = new Vector2(600, 120);
            window.Show();
            string pathGUID = Selection.assetGUIDs[0];
            _animationsTopPath = AssetDatabase.GUIDToAssetPath(pathGUID);
        }

        private int spriteWidth = 128;
        private int spriteHeight = 128;
        private bool square = true;
        private int minCellSize = 4;
        private static string _animationsTopPath;

        private void OnGUI()
        {
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            spriteWidth = EditorGUILayout.IntField("width", spriteWidth);
            spriteHeight = EditorGUILayout.IntField("height", spriteHeight);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            square = EditorGUILayout.Toggle("square size", square);
            minCellSize = EditorGUILayout.IntField("min cell size", minCellSize);
            if (GUILayout.Button("Auto Size Calculation"))
            {
                string pathGUID = Selection.assetGUIDs[0];
                _animationsTopPath = AssetDatabase.GUIDToAssetPath(pathGUID);
                Vector2Int size = CalculationSize(_animationsTopPath,minCellSize);
                if (square)
                {
                    spriteWidth = spriteHeight = size.x >= size.y ? size.x : size.y;
                }
                else
                {
                    spriteWidth = size.x;
                    spriteHeight = size.y;
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            if (GUILayout.Button("Resize All"))
            {
                string pathGUID = Selection.assetGUIDs[0];
                _animationsTopPath = AssetDatabase.GUIDToAssetPath(pathGUID);
                string[] allFilePaths = Directory.GetFiles(_animationsTopPath, "*.png",SearchOption.AllDirectories);
                bool canResize = true;
                foreach (var filePath in allFilePaths)
                {
                    Texture2D texture2D = AssetDatabase.LoadAssetAtPath<Texture2D>(filePath);
                    if (texture2D.width > spriteWidth)
                    {
                        canResize = false;
                    }
                    if (texture2D.height > spriteHeight)
                    {
                        canResize = false;
                    }
                }
                if (canResize)
                {
                    ResizeAllTexture(_animationsTopPath,spriteWidth,spriteHeight);    
                }
                else
                {
                    EditorUtility.DisplayDialog("Error", "some texture size is bigger than width or height!", "ok");
                }
            }
        }

        public static void ResizeAllTexture(string path,int widthResize,int heightResize)
        {
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

                if (texture2D.width != widthResize || texture2D.height != heightResize)
                {
                    Texture2D newTexture2D = new Texture2D(widthResize, heightResize,TextureFormat.RGBA32,false);
                    for (int i = 0; i < newTexture2D.width; i++)
                    {
                        for (int j = 0; j < newTexture2D.height; j++)
                        {
                            newTexture2D.SetPixel(i,j,Color.clear);
                        }
                    }
                    newTexture2D.Apply();
                    int widthOffset = (widthResize - texture2D.width) / 2;
                    int heightOffset = (heightResize - texture2D.height) / 2;
                    Color[] colorsBlock = texture2D.GetPixels(0);
                    newTexture2D.SetPixels(widthOffset,heightOffset,texture2D.width,texture2D.height,colorsBlock);
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
        }

        public static Vector2Int CalculationSize(string path,int minCellSize)
        {
            int width = minCellSize;
            int height = minCellSize;
            string[] allFilePaths = Directory.GetFiles(path, "*.png",SearchOption.AllDirectories);
            foreach (var filePath in allFilePaths)
            {
                Texture2D texture2D = AssetDatabase.LoadAssetAtPath<Texture2D>(filePath);
                if (texture2D.width > width)
                {
                    width = (texture2D.width + minCellSize - 1) / minCellSize * minCellSize;
                }

                if (texture2D.height > height)
                {
                    height = (texture2D.height + minCellSize - 1) / minCellSize * minCellSize;
                }
            }
            return new Vector2Int(width, height);
        }
        
    }
}
