using System.Collections.Generic;
using UnityEngine;

namespace SUGame.Runtime.Logics.Utils.ObjectUtils
{
    public class PoolTest : MonoBehaviour
    {
        private ObjectPool<GameObject> pool;
        public GameObject gObja;
        private List<GameObject> gObjs = new List<GameObject>();
        void Start()
        {
            pool = new ObjectPool<GameObject>(OnCreate, OnGet, OnRelease, OnDestory,
                true, 10, 30);
        }
        GameObject OnCreate()
        {
            //return new GameObject("gObj");
            GameObject a = Instantiate(gObja, transform);
            gObjs.Add(a);
            return a;
        }
        void OnGet(GameObject gObj)
        {
            Debug.Log("pool:获取");
            gObj.SetActive(true);
        }
        void OnRelease(GameObject gObj)
        {
            Debug.Log("pool:释放");
            gObj.SetActive(false);
            
        }
        void OnDestory(GameObject gObj)
        {
            Debug.Log("pool:销毁");
        }

        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                pool.Get();
            }

            if (Input.GetMouseButtonDown(1))
            {
                foreach (var g in gObjs)
                {
                    if (g.activeInHierarchy)
                    {
                        pool.Release(g);
                    }
                }
            }
        }
    }
}


