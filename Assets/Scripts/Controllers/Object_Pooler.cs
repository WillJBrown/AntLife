using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyExtensions;

namespace MyExtensions
    {
        public static class GameObjectExtensions{
            public static void Despawn(this GameObject go, Object_Pooler OP, string tag){
                if(OP.poolDictionary.ContainsKey(tag)){
                    OP.poolDictionary[tag].objectPool.Enqueue(go);
                    go.SetActive(false);
                } else {
                    GameObject.Destroy(go);
                }
            }

            public static GameObject PooledPrefabSwap(this GameObject go, Object_Pooler OP, string intag, string outtag){
                if(OP.poolDictionary.ContainsKey(intag) && OP.poolDictionary.ContainsKey(outtag)){
                    GameObject retval = OP.SpawnFromPool(outtag, go.transform.position, go.transform.rotation);
                    retval.GetComponent<MeshRenderer>().material = go.GetComponent<MeshRenderer>().material;
                    retval.name = go.name;
                    go.Despawn(OP, intag);
                    return retval;
                } else {
                    Debug.Log("Tried to swap prefab on non-pooled Object");
                    return null;
                }
            }
        }
    }

public class Object_Pooler : MonoBehaviour
{
    public class Pool
    {
        public string tag {get; protected set;}
        GameObject prefab;
        public GameObject container;
        public Queue<GameObject> objectPool;

        public Pool(string tag, GameObject prefab, int initialsize){
            this.tag = tag;
            this.prefab = prefab;
            this.container = new GameObject(tag + "_Pool");
            this.objectPool = new Queue<GameObject>();
            this.AddtoPool(initialsize);
        }

        public void AddtoPool(int num = 1){
        for (int i = 0; i < num; i++){
            GameObject obj = Instantiate(this.prefab);
            obj.transform.parent = this.container.transform;
            obj.SetActive(false);
            this.objectPool.Enqueue(obj);
        }     
    }

    }
    public Dictionary<string, Pool> poolDictionary;
    // Start is called before the first frame update
    public void Initialise() {
        this.poolDictionary = new Dictionary<string, Pool>();
    }

    public void AddPool(string tag, GameObject prefab, int initialsize){
        Pool pool = new Pool(tag, prefab,initialsize);
        this.poolDictionary.Add(pool.tag, pool);
        pool.container.transform.parent = this.gameObject.transform;
    }

    public GameObject SpawnFromPool (string tag, Vector3 position, Quaternion rotation){
        if (!poolDictionary.ContainsKey(tag)){
            Debug.LogWarning("Pool with tag " + tag + " doesn't exist.");
            return null;
        }
        Queue<GameObject> objectPool = poolDictionary[tag].objectPool;
        if (objectPool.Count == 0) {
            poolDictionary[tag].AddtoPool();
        }
        GameObject objectToSpawn = poolDictionary[tag].objectPool.Dequeue();
        objectToSpawn.SetActive(true);
        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;

        return objectToSpawn;
    }

}
