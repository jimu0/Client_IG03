using UnityEngine;

namespace Tools.StaticLightBakingTool.Light
{
    [DisallowMultipleComponent]
    [ExecuteInEditMode]
    public class StaticLightingLayoutClass : MonoBehaviour
    {
        [HideInInspector] public Color emissionColor = Color.white;

        /// <summary>
        /// 设置颜色
        /// </summary>
        /// <param name="r">r</param>
        /// <param name="g">g</param>
        /// <param name="b">b</param>
        /// <param name="a">a</param>
        /// <returns>Color</returns>
        protected Color SetColor(float r,float g,float b,float a)
        {
            Color c; 
            c.r = r;
            c.g = g;
            c.b = b;
            c.a = a;
            return c;
        }

        /// <summary>
        /// 设置自发光强度
        /// </summary>
        /// <param name="color1">颜色</param>
        /// <param name="pow">强度</param>
        /// <returns></returns>
        protected Color SetEmissionColor(Color color1, float pow)
        {
            return color1 * Mathf.Pow(2f, pow);
        }

        /// <summary>
        /// 绘制一束光线(矩形光源)
        /// </summary>
        /// <param name="color1">光线颜色</param>
        /// <param name="area">光线面积</param>
        protected void DrawLightRay(Color color1, float area)
        {
            Vector3 startPos = transform.position;
            float distance = GetDrawLightRayDistance(color1, area);
            Vector3 endPos = startPos + transform.forward * distance;
            DrawLightRay(color1, startPos, endPos,1,16);
        }

        /// <summary>
        /// 绘制一束光线(主光源)
        /// </summary>
        /// <param name="color1">光线颜色</param>
        /// <param name="area">光线面积</param>
        /// <param name="endPos">强制结束点</param>
        protected void DrawLightRay(Color color1, float area, Vector3 endPos)
        {
            Vector3 startPos = transform.position;
            float distance = GetDrawLightRayDistance(color1, area);
            float ratio = Vector3.Distance(startPos, endPos) / distance;
            DrawLightRay(color1, startPos, endPos, ratio,16);
        }

        /// <summary>
        /// 绘制一束光线
        /// </summary>
        /// <param name="color1">光线颜色</param>
        /// <param name="startPos">起始点</param>
        /// <param name="endPos">结束点</param>
        /// <param name="ratio">范围比率</param>
        /// <param name="dist">分辨率</param>
        private void DrawLightRay(Color color1, Vector3 startPos, Vector3 endPos,float ratio, int dist)
        {
            Color gizmosColor = Gizmos.color;
            if (dist > 32)
            {
                dist = 32;
            }
            Vector3 pos1 = startPos;
            for (int i = 0; i < dist; i++)
            {
                float a = (float)i / (dist - 1);
                Vector3 pos2 = Vector3.Lerp(startPos, endPos, a);
                Gizmos.DrawLine(pos1, pos2);
                Color damp = Color.Lerp(color1, Color.clear, a * ratio);
                Gizmos.color = damp;
                pos1 = pos2;
            }

            Gizmos.color = gizmosColor;
        }

        /// <summary>
        /// 计算光照的最大距离
        /// </summary>
        /// <param name="color">光线颜色</param>
        /// <param name="area">光线面积</param>
        /// <returns>最大距离</returns>
        private float GetDrawLightRayDistance(Color color, float area)
        {
            
            return Mathf.Sqrt(175 * ((color.r + color.g + color.b) * 5f) * Mathf.Abs(area));
        }

        /// <summary>
        /// 生成球体模型
        /// </summary>
        /// <param name="meshName">命名</param>
        /// <param name="longitudeSegments">经度段</param>
        /// <param name="latitudeSegments">维度段</param>
        /// <param name="radius">半径</param>
        /// <param name="invertNormals">法线方向(外true内false)</param>
        /// <returns>返回一个面向z轴的球体Mesh</returns>
        protected Mesh CreateSphere(string meshName, int longitudeSegments, int latitudeSegments, float radius, bool invertNormals)
        {
            Mesh mesh = new() { name = meshName };
            // 创建顶点位置、三角形和纹理坐标数据
            Vector3[] vertices = new Vector3[(latitudeSegments + 1) * (longitudeSegments + 1)];
            int[] triangles = new int[latitudeSegments * longitudeSegments * 6];
            Vector2[] uv = new Vector2[vertices.Length];

            // 填充顶点位置和纹理坐标数据
            for (int lat = 0; lat <= latitudeSegments; lat++)
            {
                float v = (float)lat / latitudeSegments;
                float theta = v * Mathf.PI;

                for (int lon = 0; lon <= longitudeSegments; lon++)
                {
                    float u = (float)lon / longitudeSegments;
                    float phi = u * Mathf.PI * 2;

                    float x = Mathf.Sin(theta) * Mathf.Cos(phi);
                    float y = Mathf.Cos(theta);
                    float z = Mathf.Sin(theta) * Mathf.Sin(phi);

                    int index = lat * (longitudeSegments + 1) + lon;
                    vertices[index] = new Vector3(x, y, z) * radius;
                    uv[index] = new Vector2(u, -v);
                }
            }
            // 填充三角形数组
            int triangleIndex = 0;
            for (int lat = 0; lat < latitudeSegments; lat++)
            {
                for (int lon = 0; lon < longitudeSegments; lon++)
                {
                    int currentVertex = lat * (longitudeSegments + 1) + lon;
                    int nextVertex = currentVertex + longitudeSegments + 1;

                    if (invertNormals)
                    {
                        triangles[triangleIndex++] = currentVertex;
                        triangles[triangleIndex++] = currentVertex + 1;
                        triangles[triangleIndex++] = nextVertex + 1;

                        triangles[triangleIndex++] = currentVertex;
                        triangles[triangleIndex++] = nextVertex + 1;
                        triangles[triangleIndex++] = nextVertex;
                    }
                    else
                    {
                        triangles[triangleIndex++] = currentVertex;
                        triangles[triangleIndex++] = nextVertex + 1;
                        triangles[triangleIndex++] = currentVertex + 1;

                        triangles[triangleIndex++] = currentVertex;
                        triangles[triangleIndex++] = nextVertex;
                        triangles[triangleIndex++] = nextVertex + 1;
                    }
                }
            }
            // 计算法线向量
            Vector3[] normals = new Vector3[vertices.Length];
            for (int i = 0; i < triangles.Length; i += 3)
            {
                Vector3 a = vertices[triangles[i]];
                Vector3 b = vertices[triangles[i + 1]];
                Vector3 c = vertices[triangles[i + 2]];
                Vector3 normal = Vector3.Cross(b - a, c - a).normalized;

                if (invertNormals)
                {
                    normal = -normal;
                }

                normals[triangles[i]] += normal;
                normals[triangles[i + 1]] += normal;
                normals[triangles[i + 2]] += normal;
            }
            // 设置Mesh对象的属性
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uv;
            mesh.normals = normals;
            mesh.RecalculateBounds();

            return mesh;
        }

        /// <summary>
        /// 生成面片模型
        /// </summary>
        /// <param name="meshName">命名</param>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        /// <param name="numRows">行数</param>
        /// <param name="numColumns">列数</param>
        /// <returns>返回一个面向Z轴的平面Mesh</returns>
        protected Mesh CreatePlane(string meshName, float width, float height, int numRows, int numColumns)
        {
            Mesh mesh = new() { name = meshName };
            // 创建顶点位置、三角形和纹理坐标数组
            Vector3[] vertices = new Vector3[(numRows + 1) * (numColumns + 1)];
            int[] triangles = new int[numRows * numColumns * 6];
            Vector2[] uv = new Vector2[vertices.Length];
            // 填充顶点位置和纹理坐标数组
            for (int row = 0; row <= numRows; row++)
            {
                for (int col = 0; col <= numColumns; col++)
                {
                    int index = row * (numColumns + 1) + col;
                    float x = col * (width / numColumns);
                    float y = row * (height / numRows);
                    vertices[index] = new Vector3(x - (width / 2f), -(y - (height / 2f)), 0f);
                    uv[index] = new Vector2((float)col / numColumns, -((float)row / numRows));
                }
            }
            // 填充三角形数组
            int triangleIndex = 0;
            for (int row = 0; row < numRows; row++)
            {
                for (int col = 0; col < numColumns; col++)
                {
                    int currentVertex = row * (numColumns + 1) + col;
                    int nextVertex = currentVertex + numColumns + 1;
                    triangles[triangleIndex++] = currentVertex;
                    triangles[triangleIndex++] = nextVertex + 1;
                    triangles[triangleIndex++] = currentVertex + 1;
                    triangles[triangleIndex++] = currentVertex;
                    triangles[triangleIndex++] = nextVertex;
                    triangles[triangleIndex++] = nextVertex + 1;
                }
            }
            // 设置Mesh对象的属性
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uv;
            mesh.RecalculateNormals();

            return mesh;
        }
    }
}
