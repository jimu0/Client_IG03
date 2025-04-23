using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tools.StaticLightBakingTool.Baking;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Matrix4x4 = UnityEngine.Matrix4x4;
using Object = UnityEngine.Object;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;
using Vector4 = UnityEngine.Vector4;

namespace Tools.StaticLightBakingTool.Forging
{
    public class StaticUVUnwrapPackSys
    {
        private static StaticUVUnwrapPackSys instance;
        public static StaticUVUnwrapPackSys Instance => instance ??= new StaticUVUnwrapPackSys();

        private readonly Dictionary<TextureImporter,bool> texturesReadableBuffer = new();//纹理可读性缓存
        public string SavePath = "";//资源生成储存路径
        public string ShaderPath = "";
        private const string CompleteFusionName = "newCf";//全展功能输出资源的名称
        private const string TexturesCorrectionName = "newTc";//纹理校色功能输出资源的名称
        private const string BatchOfObjectsName = "newBo";
        private struct MeshComponents
        {
            public MeshFilter[] Filters;
            public MeshRenderer[] Renderers;
        }
        private MeshComponents meshComponents;//模型和渲染器组件
        private MeshComponents cloneMeshComponents;//模型和渲染器组件
        private int[] lightmapIndex;//克隆LightmapID
        private int batch;//Lightmap纹理数量
        public readonly List<GameObject> CGameObjects = new(); //计算产生的物体或临时产生的物体
        private readonly List<Material> cloneMaterials = new();//克隆材质
        public readonly List<Texture2D> CloneTextures = new();//克隆纹理
        private readonly List<Color[]> clonePixels = new();//克隆纹理像素组
        public readonly List<float> TexturesValue = new();//像素组灰度计算值
        private static readonly int Emission = Shader.PropertyToID("_EMISSION");
        private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
        private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");
        private static readonly int EmissionMap = Shader.PropertyToID("_EmissionMap");
        private static readonly int BaseMap = Shader.PropertyToID("_BaseMap");
        private static readonly int Cutoff = Shader.PropertyToID("_Cutoff");
        private static readonly int AlphaClip = Shader.PropertyToID("_AlphaClip");
        public Shader DefaultShader; 

        private abstract class BaseClass
        {
            public abstract string GetName();
            //public abstract string GetPath();
        }
        
        private class CompleteFusion1 : BaseClass
        {
            public override string GetName() { return CompleteFusionName; }
            //public override string GetPath() { return SavePath; }
        }
        private class TexturesCorrection1 : BaseClass
        {
            public override string GetName() { return TexturesCorrectionName; }
            //public override string GetPath() { return SavePath; }
        }
        private class BatchOfObjects1 : BaseClass
        {
            public override string GetName() { return BatchOfObjectsName; }
            //public override string GetPath() { return SavePath; }
        }

        /// <summary>
        /// 生成全展纹理
        /// </summary>
        public void CompleteFusion()
        {
            BaseClass prentClass = new CompleteFusion1();
            //克隆
            CloneResources(prentClass);
            SetTexturesReadable(true);
            //绘制
            Texture2D[] newTexture = SetTexturePixelsOfCf();
            PrintingTextures(prentClass, SavePath, newTexture);
            AssetDatabase.Refresh();
            PrintingMaterials(prentClass, SavePath, newTexture);
            AssetDatabase.Refresh();
            PrintingModels(new CompleteFusion1(), SavePath, batch);

            //还原/清理
            ReductionTexturesReadable();
            AssetDatabase.Refresh();
            RemoveOriginalObject();
        }

        /// <summary>
        /// 纹理明度校准
        /// </summary>
        public void TexturesColourCorrection()
        {
            BaseClass prentClass = new TexturesCorrection1();
            //克隆
            CloneResources(prentClass);
            //获取灰度值
            GetTextureGrayLevel();
            AssetDatabase.Refresh();
            
        }
        
        /// <summary>
        /// 资源合批
        /// </summary>
        public void BatchOfObjects(int textureSize)
        {
            BaseClass prentClass = new BatchOfObjects1();
            //克隆
            CloneResources(prentClass);
            SetTexturesReadable(true);
            //绘制
            Texture2D newTexture = TextureDecompression(SetTexturePixelsOfBo(CloneTextures,textureSize));
            PrintingTexture(prentClass, SavePath,0, newTexture);
            AssetDatabase.Refresh();
            PrintingMaterial(prentClass, SavePath,0, out Texture2D _);
            AssetDatabase.Refresh();
            PrintingModels(prentClass, SavePath,1);
            
            //还原/清理
            ReductionTexturesReadable();
            AssetDatabase.Refresh();
            RemoveOriginalObject();
        }
        
        //------------------------------------------------------------------------------

        /// <summary>
        /// 纹理校色-调整纹理像素颜色
        /// </summary>
        /// <param name="m"></param>
        /// <param name="c"></param>
        /// <param name="v"></param>
        public void SetColorOfTexture(int m, Color c, float v)
        {
            if (CloneTextures.Count == 0 || !CloneTextures[m]) return;
            Color[] colors = CloneTextures[m].GetPixels();
            for (int i = 0; i < colors.Length; i++)
            {
                Color cc = clonePixels[m][i];
                Color color = colors[i];
                color.r = cc.r * c.r * (v / TexturesValue[m]);
                color.g = cc.g * c.g * (v / TexturesValue[m]);
                color.b = cc.b * c.b * (v / TexturesValue[m]);
                colors[i] = color;
                
            }
            CloneTextures[m].SetPixels(colors);
            CloneTextures[m].Apply();
        }

        /// <summary>
        /// 获取纹理灰度值
        /// </summary>
        private void GetTextureGrayLevel()
        {
            TexturesValue.Clear();
            foreach (Texture2D t in CloneTextures)
            {
                TexturesValue.Add(GetTexturePixelAverages(t));
            }
        }

        /// <summary>
        /// 获取纹理像素均值灰度
        /// </summary>
        /// <returns>0-1的灰度值</returns>
        private float GetTexturePixelAverages(Texture2D texture)
        {
            float fs = 0.5f;
            if (texture == null) return fs;
            int sizeW = texture.width;
            int sizeH = texture.height;
            for (int y = 0; y < sizeH; y++)
            {
                for (int x = 0; x < sizeW; x++)
                {
                    Color color = texture.GetPixel(x, y);
                    float f = (color.r + color.g + color.b) / 3;
                    fs += f;
                }
            }
            fs /= sizeW * sizeH;
            return fs;
        }
        
        /// <summary>
        /// 替换为克隆的模型
        /// </summary>
        /// <param name="baseClass">模式</param>
        private void CloneResources(BaseClass baseClass)
        {
            MeshComponents component = new()
            {
                Filters = StaticBakingSys.Instance.GetObjs().Keys.ToArray(),
                Renderers = StaticBakingSys.Instance.GetObjs().Values.ToArray()
            };
            meshComponents = component;

            int length = meshComponents.Filters.Length;
            cloneMeshComponents.Filters = new MeshFilter[length];
            cloneMeshComponents.Renderers = new MeshRenderer[length];
            GameObject root = new($"{baseClass.GetName()}_Clone");
            for (int i = 0; i < length; i++)
            {
                GameObject clone = Object.Instantiate(meshComponents.Filters[i].gameObject, root.transform);
                cloneMeshComponents.Filters[i] = clone.GetComponent<MeshFilter>();
                cloneMeshComponents.Renderers[i] = clone.GetComponent<MeshRenderer>();
            }
            Mesh[] meshes = new Mesh[length];
            lightmapIndex = new int[length];
            for (int i = 0; i < length; i++)
            {
                meshes[i] = new Mesh
                {
                    name = meshComponents.Filters[i].sharedMesh.name,
                    vertices = meshComponents.Filters[i].sharedMesh.vertices,
                    triangles = meshComponents.Filters[i].sharedMesh.triangles,
                    normals = meshComponents.Filters[i].sharedMesh.normals,
                    uv = meshComponents.Filters[i].sharedMesh.uv,
                    uv2 = meshComponents.Filters[i].sharedMesh.uv2,
                    bounds = meshComponents.Filters[i].sharedMesh.bounds
                };
                cloneMeshComponents.Filters[i].mesh = meshes[i];
                lightmapIndex[i] = meshComponents.Renderers[i].lightmapIndex;
            }

            if (baseClass.GetName() != TexturesCorrectionName && baseClass.GetName() != BatchOfObjectsName) return;
            cloneMaterials.Clear();
            CloneTextures.Clear();
            CGameObjects.Add(root);
            Dictionary<Material, Vector2Int> mats = new();
            List<Texture> texts = new();
            Vector2Int mt = new(-1, -1);
            for (int i = 0; i < length; i++)
            {
                MeshRenderer sourceRenderer = meshComponents.Renderers[i];
                MeshRenderer targetRenderer = cloneMeshComponents.Renderers[i];
                targetRenderer.receiveGI = sourceRenderer.receiveGI;
                targetRenderer.lightmapIndex = sourceRenderer.lightmapIndex;
                targetRenderer.lightmapScaleOffset = sourceRenderer.lightmapScaleOffset;
                Material mat = sourceRenderer.sharedMaterial;
                Texture tex = mat.mainTexture;// ? mat.mainTexture : new Texture2D(1, 1);
                if (!mats.ContainsKey(mat))
                {
                    mt.x++;
                    Material newMat = new(DefaultShader) { name = mat.name };
                    Texture2D tex2d = tex as Texture2D;
                    if (!texts.Contains(tex))
                    {
                        mt.y++;
                        Texture2D newTex = tex ? new Texture2D(tex.width, tex.height) { name = tex.name } : null;
                        if (newTex != null)
                        {
                            if (tex2d != null) newTex.SetPixels(tex2d.GetPixels());
                            newTex.Apply();
                            texts.Add(tex);
                            
                            CloneTextures.Add(newTex);
                            clonePixels.Add(newTex.GetPixels());
                        }
                        else
                        {
                            CloneTextures.Add(null);
                            clonePixels.Add(null);
                        }
                        mats.Add(mat, mt);
                    }
                    else
                    {
                        mats.Add(mat, new Vector2Int(mt.x, texts.IndexOf(tex)));
                    }
                    cloneMaterials.Add(newMat);
                    
                }
                cloneMeshComponents.Renderers[i].sharedMaterial = cloneMaterials[mats[mat].x];
                cloneMeshComponents.Renderers[i].sharedMaterial.mainTextureScale =
                    meshComponents.Renderers[i].sharedMaterial.mainTextureScale;
                cloneMeshComponents.Renderers[i].sharedMaterial.mainTextureOffset =
                    meshComponents.Renderers[i].sharedMaterial.mainTextureOffset;
            }
            Material[] newMats = mats.Keys.ToArray();
            for (int m = 0; m < cloneMaterials.Count; m++)
            {
                Material newMat = cloneMaterials[m];
                Material mat = newMats[m];
                Texture2D tex2D = CloneTextures[mats[mat].y];
                newMat.SetFloat(AlphaClip, 1);
                newMat.SetFloat(Cutoff, 0.25f);
                newMat.SetInt(Emission, 1);
                newMat.SetColor(BaseColor, Color.white);
                newMat.SetTexture(BaseMap, tex2D);
            }
            
        }
        
        /// <summary>
        /// 全透明背景纹理
        /// </summary>
        /// <param name="x">宽(像素)</param>
        /// <param name="y">高(像素)</param>
        /// <param name="c">底色</param>
        /// <returns>透明纹理</returns>
        private static Texture2D TransparentColors0(int x,int y,Color c)
        {
            Color[] colors = Enumerable.Repeat(c, x * y).ToArray();
            Texture2D tex = new(x, y);
            tex.SetPixels(colors);
            return tex;
        }
        
        /// <summary>
        /// 设置烘培纹理可读性
        /// </summary>
        /// <param name="b">可读性</param>
        private void SetTexturesReadable(bool b)
        {
            texturesReadableBuffer.Clear();
            batch = LightmapSettings.lightmaps.Length;
            if (batch == 0) return;
            for (int i = 0; i < batch; i++)
            {
                LightmapData t = LightmapSettings.lightmaps[i];
                string lightmapPath = AssetDatabase.GetAssetPath(t.lightmapColor);
                TextureImporter importer = AssetImporter.GetAtPath(lightmapPath) as TextureImporter;
                if (importer != null)
                {
                    texturesReadableBuffer.Add(importer, importer.isReadable);
                    importer.isReadable = b;
                }
                AssetDatabase.ImportAsset(lightmapPath);
            }
        }
        
        /// <summary>
        /// 还原烘培纹理可读性
        /// </summary>
        private void ReductionTexturesReadable()
        {
            foreach (KeyValuePair<TextureImporter, bool> t in texturesReadableBuffer.Where(t => t.Key))
                t.Key.isReadable = t.Value;
            texturesReadableBuffer.Clear();
            batch = 0;
        }

        /// <summary>
        /// 全展融合的纹理计算
        /// </summary>
        private Texture2D[] SetTexturePixelsOfCf()
        {
            Texture2D[] textures = new Texture2D[batch];
            Material[] newMaterial = new Material[batch];
            for (int i = 0; i < batch; i++)
            {
                Texture2D lightmapTex = LightmapSettings.lightmaps[i].lightmapColor;
                int width = lightmapTex.width;
                int height = lightmapTex.height;
                textures[i] = TransparentColors0(width,height, Color.clear);
                Dictionary<Vector2Int, Color> pix = SmithPixels(meshComponents.Renderers, width,height, i);
                foreach (KeyValuePair<Vector2Int, Color> coord in pix)
                {
                    Color pixelColor = coord.Value * lightmapTex.GetPixelBilinear((float)coord.Key.x/width, (float)coord.Key.y/height);
                    pixelColor.a = coord.Value.a;
                    textures[i].SetPixel(coord.Key.x, coord.Key.y, pixelColor);
                }
                textures[i].Apply();
                newMaterial[i] = new Material(DefaultShader);
                newMaterial[i].SetTexture(BaseMap, textures[i]);
                newMaterial[i].SetFloat(AlphaClip, 1);
                newMaterial[i].SetFloat(Cutoff, 0.5f);
            }
            for (int i = 0; i < cloneMeshComponents.Filters.Length; i++)
            {
                cloneMeshComponents.Renderers[i].sharedMaterial = newMaterial[lightmapIndex[i]];
            }
            return textures;
        }
        
        
        Rect[] PackTextures(List<Texture2D> smallTextures, int size, out Texture2D packedTex)
        {
            packedTex = new Texture2D(size, size);
            //调用PackTextures方法进行纹理合并
            Rect[] packingResult = packedTex.PackTextures(smallTextures.ToArray(), 0, size); //参数2表示边框厚度
            return packingResult;
        }
        List<Vector2Int> GetTexturePositions(List<Texture2D> smallTextures , Rect[] packingResult, int width, int height, out List<Vector2Int> texSize)
        {
            List<Vector2Int> texPos = new();
            texSize = new List<Vector2Int>();
            for (int i = 0; i < smallTextures.Count; i++)
            {
                //计算每个小纹理在大纹理中的位置
                int x = (int)(packingResult[i].x * width);
                int y = (int)(packingResult[i].y * height);
                //int w = (int)(packingResult[i].width * width);
                //int h = (int)(packingResult[i].height * height);
                int sx = (int)(packingResult[i].size.x * width);
                int sy = (int)(packingResult[i].size.y * height);
                texPos.Add(new Vector2Int(x, y));
                texSize.Add(new Vector2Int(sx,sy));
            }
            return texPos;
        }
        /// <summary>
        /// 资源合批的纹理计算
        /// </summary>
        private Texture2D SetTexturePixelsOfBo(List<Texture2D> textures,int size)
        {
            Rect[] packingResult = PackTextures(textures, size, out Texture2D packedTex);
            List<Vector2Int> texPos = GetTexturePositions(textures, packingResult, packedTex.width, packedTex.height,
                out List<Vector2Int> texSize);
            for (int i = 0; i < cloneMeshComponents.Filters.Length; i++)
            {
                Mesh mesh = cloneMeshComponents.Filters[i].sharedMesh;
                Texture2D tex = cloneMeshComponents.Renderers[i].sharedMaterial.mainTexture as Texture2D;
                int index = textures.IndexOf(tex);
                if (index < 0)  continue;
                Vector2[] newUv = new Vector2[mesh.uv.Length];
                for (int j = 0; j < newUv.Length; j++)
                {
                    Vector2 uv = mesh.uv[j];
                    newUv[j].x = (uv.x * texSize[index].x + texPos[index].x) / size;
                    newUv[j].y = (uv.y * texSize[index].y + texPos[index].y) / size;
                }
                mesh.uv = newUv;
                mesh.uv2 = null;
            }
            return packedTex;
        }
        /// <summary>
        /// 构造全展数据
        /// </summary>
        /// <param name="meshRenderer">模型渲染组件</param>
        /// <param name="x">宽(像素)</param>
        /// <param name="y">高(像素)</param>
        /// <param name="index">批次</param>
        /// <returns>键：坐标，值：颜色</returns>
        private Dictionary<Vector2Int,Color> SmithPixels(MeshRenderer[] meshRenderer, int x,int y,int index)
        {
            HashSet<Vector2Int> visitedUvs = new();
            Dictionary<Vector2Int,Color> vi = new();
            Vector2Int uv = new();
            for (int m = 0; m < cloneMeshComponents.Filters.Length; m++)
            {
                MeshRenderer renderer = meshRenderer[m];
                if (renderer.lightmapIndex != index) continue;
                Material sharedMaterial = renderer.sharedMaterial;
                Vector2 matScale = sharedMaterial.mainTextureScale;
                Vector2 matOffset = sharedMaterial.mainTextureOffset;
                Vector4 offset = renderer.lightmapScaleOffset;
                Vector2[] uv1 = cloneMeshComponents.Filters[m].sharedMesh.uv;
                Vector2[] uv2 = cloneMeshComponents.Filters[m].sharedMesh.uv2;
                if (uv2.Length == 0) uv2 = cloneMeshComponents.Filters[m].sharedMesh.uv;
                Vector2[] uvs = cloneMeshComponents.Filters[m].sharedMesh.uv;
                int[] triangles = cloneMeshComponents.Filters[m].sharedMesh.triangles;
                for (int i = 0; i < triangles.Length; i += 3)
                {
                    //1UV
                    Vector2 uv1A = uv1[triangles[i]] * matScale + matOffset;
                    Vector2 uv1B = uv1[triangles[i + 1]] * matScale + matOffset;
                    Vector2 uv1C = uv1[triangles[i + 2]] * matScale + matOffset;
                    //2UV
                    Vector2 uv2A = uv2[triangles[i]];
                    Vector2 uv2B = uv2[triangles[i + 1]];
                    Vector2 uv2C = uv2[triangles[i + 2]];
                    //将2UV位置转换为LightmapUV位置
                    Vector2 uv3A, uv3B, uv3C;
                    uv3A.x = uv2A.x * offset.x + offset.z;
                    uv3A.y = uv2A.y * offset.y + offset.w;
                    uv3B.x = uv2B.x * offset.x + offset.z;
                    uv3B.y = uv2B.y * offset.y + offset.w;
                    uv3C.x = uv2C.x * offset.x + offset.z;
                    uv3C.y = uv2C.y * offset.y + offset.w;
                    uvs[triangles[i]] = uv3A;
                    uvs[triangles[i+1]] = uv3B;
                    uvs[triangles[i+2]] = uv3C;
                    cloneMeshComponents.Filters[m].sharedMesh.uv = uvs;
                    uv3A.x *= x;
                    uv3A.y *= y;
                    uv3B.x *= x;
                    uv3B.y *= y;
                    uv3C.x *= x;
                    uv3C.y *= y;
                    //遍历三角面内像素
                    int minU = Mathf.FloorToInt((Mathf.Min(uv2A.x, Mathf.Min(uv2B.x, uv2C.x)) * offset.x + offset.z) * x);
                    int maxU = Mathf.CeilToInt((Mathf.Max(uv2A.x, Mathf.Max(uv2B.x, uv2C.x)) * offset.x + offset.z) * x);
                    int minV = Mathf.FloorToInt((Mathf.Min(uv2A.y, Mathf.Min(uv2B.y, uv2C.y)) * offset.y + offset.w) * y);
                    int maxV = Mathf.CeilToInt((Mathf.Max(uv2A.y, Mathf.Max(uv2B.y, uv2C.y)) * offset.y + offset.w) * y);
                    for (int u = minU; u < maxU; u++)
                    {
                        for (int v = minV; v < maxV; v++)
                        {
                            //得到UV面内的像素位置
                            uv.x = u;
                            uv.y = v;
                            if (visitedUvs.Contains(uv)) continue;
                            visitedUvs.Add(uv);
                            //得到像素对应1UV处的颜色
                            Vector2 uvCoord = TransformPoint(uv, uv3A, uv3B, uv3C, uv1A, uv1B, uv1C);
                            vi.Add(uv, GetModelTextureColorAtPoint(renderer, uvCoord));
                        }
                    }
                }
                cloneMeshComponents.Filters[m].sharedMesh.uv2 = null;
            }
            return vi;
        }

        /// <summary>
        /// 打印所有纹理
        /// </summary>
        /// <param name="baseClass">模式</param>
        /// <param name="p">路径</param>
        /// <param name="textures">纹理组</param>
        private void PrintingTextures(BaseClass baseClass,string p,  Texture2D[] textures)
        {
            for (int i = 0; i < textures.Length; i++)
            {
                PrintingTexture(baseClass, p, i, textures[i]);
            }
        }

        /// <summary>
        /// 打印纹理
        /// </summary>
        /// <param name="baseClass">模式</param>
        /// <param name="p">路径</param>
        /// <param name="i">批次</param>
        /// <param name="tex">纹理</param>
        private void PrintingTexture(BaseClass baseClass,string p, int i, Texture2D tex)
        {
            string absolutePath = $"{p}/{baseClass.GetName()}_{i}.png";
            byte[] bytes = tex.EncodeToPNG();
            File.WriteAllBytes(absolutePath, bytes);
        }

        /// <summary>
        /// 打印材质
        /// </summary>
        /// <param name="baseClass">模式</param>
        /// <param name="p">路径</param>
        /// <param name="textures">纹理组</param>
        private void PrintingMaterials(BaseClass baseClass, string p,  Texture2D[] textures)
        {
            for (int m = 0; m < textures.Length; m++)
            {
                PrintingMaterial(baseClass, p, m, out Texture2D texture);
                textures[m] = texture;
            }
        }

        /// <summary>
        /// 打印材质
        /// </summary>
        /// <param name="baseClass"></param>
        /// <param name="p">路径</param>
        /// <param name="m">批次</param>
        /// <param name="texture">纹理</param>
        private void PrintingMaterial(BaseClass baseClass, string p, int m, out Texture2D texture)
        {
            int index = p.IndexOf("Assets", StringComparison.Ordinal);
            string newPath = index != -1 && index > 0 ? p.Substring(index) : "Assets/";
            string matPath = $"{newPath}/{baseClass.GetName()}_{m}.mat";
            string pngPath = $"{newPath}/{baseClass.GetName()}_{m}.png";
            Material newMat = new(DefaultShader) { name = baseClass.GetName()};
            texture = AssetDatabase.LoadAssetAtPath<Texture2D>(pngPath);
            if (baseClass.GetName() == CompleteFusionName)
            {
                newMat.SetInt(Emission, 1);
                newMat.SetColor(BaseColor, Color.black);
                newMat.SetColor(EmissionColor, Color.white);
                newMat.SetTexture(EmissionMap, texture);
                newMat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.BakedEmissive;
            }
            if (baseClass.GetName() == BatchOfObjectsName)
            {
                newMat.SetInt(Emission, 0);
                newMat.SetColor(BaseColor, Color.white);
            }

            newMat.SetTexture(BaseMap,texture);
            newMat.SetFloat(Cutoff, 0.25f);
            newMat.SetFloat(AlphaClip,1);
            
            AssetDatabase.DeleteAsset(matPath);
            AssetDatabase.CreateAsset(newMat, matPath);
        }

        /// <summary>
        /// 打印模型
        /// </summary>
        /// <param name="pc">执行模式</param>
        /// <param name="p">路径</param>
        /// <param name="number">数量</param>
        private void PrintingModels(BaseClass pc, string p, int number)
        {
            GameObject[] gameObjects = new GameObject[number];
            Object[] objects = new Object[number];
            for (int i = 0; i < number; i++)
            {
                gameObjects[i] = CombineMesh(pc, p, cloneMeshComponents.Filters, i);
                objects[i] = gameObjects[i];
                CGameObjects?.Add(gameObjects[i]);
            }
            //Selection.objects = objects;//选中生成的物体
        }

        /// <summary>
        /// 组装模型
        /// </summary>
        /// <param name="baseClass">模式</param>
        /// <param name="p">路径</param>
        /// <param name="filters">模型组</param>
        /// <param name="m">批次</param>
        private GameObject CombineMesh(BaseClass baseClass, string p, MeshFilter[] filters,int m)
        {
            string name = baseClass.GetName();
            string meshName = $"{name}_{m}";
            GameObject newObj = new(meshName) { transform = { position = Vector3.zero } };
            MeshFilter newMeshFilter = newObj.AddComponent<MeshFilter>();
            MeshRenderer newRenderer = newObj.AddComponent<MeshRenderer>();
            Matrix4x4 matrix = newObj.transform.worldToLocalMatrix;
            Mesh mesh = new() { name = meshName };
            MeshCollider collider = new();
            List<CombineInstance> combine = new();
            for (int i = 0; i < filters.Length; i++)
            {
                if(name == CompleteFusionName) if (lightmapIndex[i] != m) continue;
                if (!filters[i].sharedMesh) continue;
                CombineInstance ci = new()
                {
                    mesh = filters[i].sharedMesh, 
                    transform = matrix * filters[i].transform.localToWorldMatrix
                };
                combine.Add(ci);
            }
            mesh.CombineMeshes(combine.ToArray());
            int index = p.IndexOf("Assets", StringComparison.Ordinal);
            string newPath = index != -1 && index > 0 ? p.Substring(index) : "Assets/";
            string tempPath = $"{newPath}/{meshName}";
            AssetDatabase.DeleteAsset($"{tempPath}.asset");
            AssetDatabase.CreateAsset(mesh, $"{tempPath}.asset");
            AssetDatabase.Refresh();
            Material material = AssetDatabase.LoadAssetAtPath<Material>($"{tempPath}.mat");
            newRenderer.sharedMaterial = material;
            newRenderer.shadowCastingMode = ShadowCastingMode.Off;
            newRenderer.receiveGI = ReceiveGI.Lightmaps;
            mesh = AssetDatabase.LoadAssetAtPath<Mesh>($"{tempPath}.asset");
            newMeshFilter.sharedMesh = mesh;
            if (collider != null) collider.sharedMesh = mesh;
            
            return newObj;
        }
        
        /// <summary>
        /// 坐标在两个三角面的位置转换
        /// </summary>
        /// <param name="p">目标位置</param>
        /// <param name="a1">当前三角面的顶点a</param>
        /// <param name="b1">当前三角面的顶点b</param>
        /// <param name="c1">当前三角面的顶点c</param>
        /// <param name="a2">目标三角形的顶点a</param>
        /// <param name="b2">目标三角形的顶点b</param>
        /// <param name="c2">目标三角形的顶点c</param>
        /// <returns>返回新的目标位置</returns>
        private Vector2 TransformPoint(Vector2 p, Vector2 a1, Vector2 b1, Vector2 c1, Vector2 a2, Vector2 b2, Vector2 c2)
        {
            // 计算原始P点在a1, b1, c1三个顶点上的重心坐标
            float w1 = ((b1.y - c1.y) * (p.x - c1.x) + (c1.x - b1.x) * (p.y - c1.y)) /
                       ((b1.y - c1.y) * (a1.x - c1.x) + (c1.x - b1.x) * (a1.y - c1.y));
            float w2 = ((c1.y - a1.y) * (p.x - c1.x) + (a1.x - c1.x) * (p.y - c1.y)) /
                       ((b1.y - c1.y) * (a1.x - c1.x) + (c1.x - b1.x) * (a1.y - c1.y));
            float w3 = 1 - w1 - w2;
            // 根据重心坐标计算新的P点坐标
            Vector2 p2 = w1 * a2 + w2 * b2 + w3 * c2;
            return p2;
        }

        /// <summary>
        /// 获取像素坐标对应的源纹理颜色
        /// </summary>
        /// <param name="renderer">Renderer</param>
        /// <param name="uvCoord">像素坐标</param>
        /// <returns>返回颜色值</returns>
        private Color GetModelTextureColorAtPoint(Renderer renderer,Vector2 uvCoord)
        {
            Texture2D tex;
            if (renderer.sharedMaterial.mainTexture)
            {
                tex = (Texture2D)renderer.sharedMaterial.mainTexture;
                if (!tex.isReadable)
                {
                    string path = AssetDatabase.GetAssetPath(tex);
                    TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                    if (importer != null) importer.isReadable = true;
                    AssetDatabase.ImportAsset(path);
                } 
            }
            else
            {
                tex = TransparentColors0(1, 1, Color.gray);
            }
            Color color = tex.GetPixelBilinear(uvCoord.x, uvCoord.y);
            return color;
        }

        /// <summary>
        /// 删除原始烘培对象
        /// </summary>
        private void RemoveOriginalObject()
        {
            if (cloneMeshComponents.Filters == null) return;
            List<GameObject> parents = new();
            foreach (MeshFilter t in cloneMeshComponents.Filters)
            {
                GameObject child = t != null ? t.gameObject : null;
                if (child == null) continue;
                GameObject root = child.transform.root.gameObject;
                // 解除绑定
                if (PrefabUtility.IsPartOfAnyPrefab(root)) 
                {
                    PrefabUtility.UnpackPrefabInstance(root, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                }
                if (!parents.Contains(root)) parents.Add(root);
                Undo.DestroyObjectImmediate(child);

            }
            foreach (var obj in parents)
            {
                Undo.DestroyObjectImmediate(obj);
            }
            parents.Clear();
        }

        /// <summary>
        /// 删除新生资源
        /// </summary>
        public void RemoveCGameObjects()
        {
            if (CGameObjects == null || CGameObjects.Count == 0) return;
            foreach (GameObject obj in CGameObjects.Where(obj => obj))
            {
                Undo.DestroyObjectImmediate(obj.gameObject);
            }
        }
        
        /// <summary>
        /// 保存纹理为png格式
        /// </summary>
        /// <param name="path"></param>
        public void SaveTextures(string path)
        {
            foreach (Texture2D textureToSave in CloneTextures)
            {
                if (textureToSave == null) continue;
                byte[] bytes = textureToSave.EncodeToPNG();
                string filePath = Path.Combine(path, textureToSave.name + ".png");
                File.WriteAllBytes(filePath, bytes);
            }
        }

        /// <summary>
        /// 解压纹理
        /// </summary>
        /// <param name="compressedTexture">纹理</param>
        /// <returns>已解压纹理</returns>
        private Texture2D TextureDecompression(Texture2D compressedTexture)
        {
            Texture2D uncompressedTexture = new (compressedTexture.width, compressedTexture.height,
                TextureFormat.RGBA32, false);
            RenderTexture currentRT = RenderTexture.active;
            RenderTexture renderTexture = RenderTexture.GetTemporary(compressedTexture.width, compressedTexture.height,
                0, RenderTextureFormat.Default, RenderTextureReadWrite.sRGB);
            Graphics.Blit(compressedTexture, renderTexture);
            RenderTexture.active = renderTexture;
            uncompressedTexture.ReadPixels(new Rect(0, 0, compressedTexture.width, compressedTexture.height), 0, 0);
            uncompressedTexture.Apply();
            RenderTexture.active = currentRT;
            RenderTexture.ReleaseTemporary(renderTexture);
            return uncompressedTexture;
        }
        
        /// <summary>
        /// 判断烘培资源是否为空
        /// </summary>
        /// <returns></returns>
        public bool CheckResourcesNotEmpty()
        {
            bool b = StaticBakingSys.Instance.GetObjs() == null;
            if (b) Debug.Log("包围盒中没有资源");
            return b;
        }
    }
    
}