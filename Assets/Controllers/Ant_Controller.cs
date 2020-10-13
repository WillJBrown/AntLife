using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Ant_Controller : MonoBehaviour
{
    int maxAnts;
    float ips;
    public float speed {get; protected set;}
    float speed_store;
    public bool paused {get; protected set;}
    public GameObject AntPrefab;
    static TileMap_Controller tileMap_Controller;
    public Dictionary<Ant, GameObject> AntGameObjectMap {get; protected set;}
    
    // Start is called before the first frame update
    void Start()
    {
        this.maxAnts = 256;
        this.paused = true;
        this.speed_store = 1f;
        tileMap_Controller = TileMap_Controller.Instance;
        this.AntGameObjectMap = new Dictionary<Ant, GameObject>();
        int AntRad = 0;
        for (int i = (0 - AntRad); i <= AntRad; i++)
        {
           for (int j = (0 - AntRad); j <= AntRad; j++)
           {
                Vector2Int position = new Vector2Int(i, j);
                this.MakeAnt(position);
           }
        }
    }

    void Update()
    {
        this.speed = Mathf.Clamp(speed, 0, 120);
        tileMap_Controller.speed = this.speed;
        if (this.speed > 0)
        {
            this.LangtonStep();
            UpdateAllAnts(Time.deltaTime, this.speed);
        }
    }

    public List<Ant> GetAntsAtPosition(Vector2Int Position){
        List<Ant> returnlist = new List<Ant>();
        foreach(Ant ant in this.AntGameObjectMap.Keys){
            if (ant.Position == Position){
                returnlist.Add(ant);
            }
        }
        return returnlist;
    }

    public void CreateAntGraphics(Ant ant_data)
    {
        if (this.AntGameObjectMap.ContainsKey(ant_data) == false)
        {
            ant_data.ResetMovement();
            GameObject ant_go = Instantiate(AntPrefab, 
                ant_data.ContPosition * tileMap_Controller.TileSize, 
                Quaternion.Euler(new Vector3(0f, ant_data.ContRotation, 0f)));
            this.AntGameObjectMap.Add(ant_data, ant_go);
            ant_go.name = "Ant";
            ant_go.transform.parent = this.transform;
            ant_data.RegisterAntChangedCallBack(OnAntChanged);
        }
        else
        {
            Debug.LogError("Tried to make Graphics for an ant which is alreaady in the map");
        }
    }

    public void DestroyAntGraphics(Ant ant_data)
    {
        if (this.AntGameObjectMap.ContainsKey(ant_data))
        {
            GameObject ant_go = AntGameObjectMap[ant_data];
            ant_data.UnregisterAntChangedCallBack(OnAntChanged);
            this.AntGameObjectMap.Remove(ant_data);
            Destroy(ant_go);
        }
        else
        {
            Debug.LogError("Tried to destroy Graphics for an ant which isn't in the map");
        }

    }

    public void MakeAnt(Vector2Int position, int facing = 1)
    {
        if (this.AntGameObjectMap.Count < this.maxAnts){
            Ant ant = new Ant(this.speed, position, facing);
            this.CreateAntGraphics(ant);
        }
        else{
            Debug.Log("No more than "+this.maxAnts+" Ants allowed");
            return;
        }
    }

    public void OnAntChanged(Ant ant_data)
    {
        if (this.AntGameObjectMap.ContainsKey(ant_data))
        {
            GameObject ant_go = this.AntGameObjectMap[ant_data];
            ant_go.transform.position = ant_data.ContPosition * tileMap_Controller.TileSize;
            ant_go.transform.rotation = Quaternion.Euler(new Vector3(0f, ant_data.ContRotation, 0f));
        }
        else
        {
            Debug.LogError("Tried to update an ant which isn't in the map");
        }
    }

    void UpdateAllAnts(float deltaTime, float speed)
    {
        foreach (Ant ant in this.AntGameObjectMap.Keys.ToArray())
        {
            ant.Speed = speed;
            ant.AntUpdate(deltaTime);
        }
    }

    public void LangtonStep()
    {
        float steps = 0f;
        foreach (Ant ant in this.AntGameObjectMap.Keys.ToArray())
        {
            if (ant.isMoving() == false)
            {
                tileMap_Controller.MakeTiles(tileMap_Controller.instantiateRadius, ant.Position);
                switch (tileMap_Controller.tileMap.GetTileStateAt(ant.Position))
                {
                    case 0:
                        ant.TurnRight();
                        break;
                    case 1:
                        ant.TurnLeft();
                        break;
                    default:
                        Debug.LogError("Tile State Outside state range");
                        break;
                }
                tileMap_Controller.tileMap.IncrementTile(ant.Position);
                ant.MoveForward();
            }
            steps += ant.StepsPerSecond;
        }
        this.ips = steps / ((float)this.AntGameObjectMap.Count);
    }

    public void Pause()
    {
        if (this.paused) 
        {
            this.paused = false;
            this.speed = this.speed_store;
            this.speed_store = 0f;
        }
        else
        {
            this.paused = true;
            this.speed_store = this.speed;
            this.speed = 0f;
        }
    }
    public void SetSpeed(float speed)
    {
        if (!this.paused)
        {
            this.speed = speed;   
        }
        else
        {
            this.speed_store = speed;
        }
    }
}
