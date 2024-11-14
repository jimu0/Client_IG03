using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneManager : MonoBehaviour
{
    public Vector2Int mapSize;// 总场所尺寸
    public Vector2Int scLocSize = new Vector2Int(24,14);// 每单位尺寸(16-32,8-16比较合适)
    public int currentId;// 当前场所Id
    private Vector2Int[] scLocPos = new Vector2Int[4];// 场所根
    private GameObject[] scLocCol = new GameObject[4];// 场所碰撞
    private List<GameObject> scLocCurrentGObjs = new();// 当前应加载的模型
    public List<Vector2Int> scActivePoints = new();// 活跃点
    private List<Vector2Int> oldScActivePoints = new();// 缓存活跃点
    private List<Vector2Int> scActiveLocations = new();// 活跃场所
    public GameObject playerGObj;// 玩家单位.只获取位置
    private Vector2Int playerPos = Vector2Int.zero;//玩家位置
    private Vector2Int oldPlayerPos = Vector2Int.zero;//缓存玩家位置
    public Vector2Int scLocStartPos = Vector2Int.zero;// 场景加载起始位置
    void Start()
    {
        scLocPos[0] = scLocStartPos;
        scLocPos[1] = scLocStartPos + Vector2Int.right;
        scLocPos[2] = scLocStartPos + Vector2Int.up;
        scLocPos[3] = scLocStartPos + Vector2Int.one;
    }

    void Update()
    {
        // 临时更新
        Vector3 position = playerGObj.transform.position;
        playerPos.x = Mathf.RoundToInt(position.x);
        playerPos.y = Mathf.RoundToInt(position.y);
        if (playerPos != oldPlayerPos)
        {
            if (scActivePoints.Count > 0)
            {
                scActivePoints[0] = playerPos;
            }
            else
            {
                scActivePoints.Add(playerPos);
            }
            if (oldScActivePoints.Count > 0)
            {
                
            }
            else
            {
                oldScActivePoints.Add(playerPos);
            }
            oldPlayerPos = playerPos;
        }

        for (int i = 0; i < scActivePoints.Count; i++)
        {
            if (scActivePoints[i] != oldScActivePoints[i])
            {
                RefreshLocCurrentGObjs(scActivePoints);
                oldScActivePoints[i] = scActivePoints[i];
                break;
            }
        }
    }

    private void InitLocCurrentGObjs(List<Vector2Int> activePoints)
    {
        
    }

    private void RefreshLocCurrentGObjs(List<Vector2Int> activePoints)
    {
        for (int i = 0; i < activePoints.Count; i++)
        {
            if (scActivePoints[i] != oldScActivePoints[i])
            {
                oldScActivePoints[i] = scActivePoints[i];
                break;
            }
        }
        scActiveLocations.Clear();
        Vector2Int adjacentPoint = Vector2Int.zero;
        foreach (Vector2Int point in activePoints)
        {
            // 计算临近坐标，排除重样
            adjacentPoint.x = point.x / scLocSize.x;
            adjacentPoint.y = point.y / scLocSize.y;
            scActiveLocations.Add(adjacentPoint);
            if (point.x % scLocSize.x > scLocSize.x)
            {
                scActiveLocations.Add(adjacentPoint + Vector2Int.right);
            }
            if (point.y % scLocSize.y > scLocSize.y / 2)
            {
                scActiveLocations.Add(adjacentPoint + Vector2Int.up);
            }
            if (point.x % scLocSize.x > scLocSize.x / 2 && point.y % scLocSize.y > scLocSize.y / 2)
            {
                scActiveLocations.Add(adjacentPoint + Vector2Int.one);
            }
        }
        // 坐标对应哪个场景
        foreach (var activeLocation in scActiveLocations)
        {
            Debug.Log($"loc坐标：{activeLocation}");
        }
           
        // 场景加载
    }
}
