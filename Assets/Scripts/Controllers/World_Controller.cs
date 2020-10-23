using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class World_Controller : MonoBehaviour
{
    public static World_Controller Instance { get; protected set; }
    public Ant_Controller AC { get; protected set; }
    Action<World_Controller> cbSpeedChanged;
    public float movePercentage {get; protected set;}
    int numStates;
    public int initialRadius, instantiateRadius, speedCutoff, antRad, multiplier;
    public float TileSize {get; protected set;} = 1f;
    public GameObject TilePrefab;
    public GameObject AntPrefab;
    public Material material;
    public float StepsPerSecond=0f;
    List<Material> Materials;
    public TileMap tileMap { get; protected set; }
    Dictionary<Tile, GameObject> TileGameObjectMap;
    Dictionary<Tile, float> UpdatingTiles;
    public List<TurnDir> defaultBehaviour;

    float timer = 0f;
    public int Steps {get; protected set;}= 0;
    int lastSteps = 0;

    
    public float speed {get; protected set;} = 1f;
    float speed_store = 1f;
    public bool paused {get; protected set;} = true;

    // Start is called before the first frame update
    void OnEnable()
    {   
        if (Instance != null)
        {
            Debug.LogError("There should never be two TileMap_Controllers");
        }
        Instance = this;
        this.Materials = new List<Material>();
        this.numStates = this.defaultBehaviour.Count;
        for (int i=0; i < this.numStates; i++){
            Vector3 HSV = HSVfromState(i);
            Color StateColor = Color.HSVToRGB(HSV.x, HSV.y,HSV.z);
            this.Materials.Add(Material.Instantiate(material));
            Materials[i].SetColor("_Color",StateColor);
        }
        this.tileMap = new TileMap(this.numStates);
        this.TileGameObjectMap = new Dictionary<Tile, GameObject>();
        this.BuildWorld();
        this.UpdatingTiles = new Dictionary<Tile, float>();
        this.AC = this.gameObject.GetComponent<Ant_Controller>();
        this.AC.Initialse(this);
    }

    // Update is called once per frame
    void Update()
    {
        this.speed = Mathf.Clamp(speed, 0, this.speedCutoff);

        if (!this.paused){
            this.movePercentage += Time.deltaTime * this.speed;
            UpdateTiles();
            if(AC.Tick()){
                this.Steps += this.multiplier;
            };
            updateSteps(Time.deltaTime);
        }
    }

    public void ResetMovementPercentage(){
        this.movePercentage = 0f;
    }

    Vector3 HSVfromState(int i){
        i = i % this.numStates;
        if( i == 0 ){
            return new Vector3(0f, 0f, 1f);
        } else if (i == 1){
            return Vector3.zero;
        } else {
            return new Vector3( ( (float)i - 2 ) /( (float)this.numStates - 2 ),1f, 1f);
        }
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
        if (cbSpeedChanged != null){
            cbSpeedChanged(this);
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
        if (cbSpeedChanged != null){
            cbSpeedChanged(this);
        } 
    }

    public void ResetTile(Tile t){
        if(this.UpdatingTiles.ContainsKey(t)){
            GameObject tile_go = this.TileGameObjectMap[t];
            tile_go.GetComponent<MeshRenderer>().material = Materials[t.State];
            this.UpdatingTiles.Remove(t);
        }
    }

    void UpdateTiles(){
        foreach (Tile t in this.UpdatingTiles.Keys.ToList())
            {
                GameObject tile_go = this.TileGameObjectMap[t];
                this.UpdatingTiles[t] += this.speed * Time.deltaTime;
                if (this.UpdatingTiles[t] >= 1)
                {
                    tile_go.GetComponent<MeshRenderer>().material = Materials[t.State];
                    this.UpdatingTiles.Remove(t);
                }
                else
                {
                    int previousState = (t.State - 1 + this.numStates) % this.numStates;
                    tile_go.GetComponent<MeshRenderer>().material.Lerp(Materials[previousState], Materials[t.State], this.UpdatingTiles[t]);
                }
            }
    }

    void updateSteps(float deltaTime)
        {
            this.timer += deltaTime;
            if (this.Steps == 0) {
                // Stops it crashing in the first second
                return;
            }
            if (this.timer >= 1)
            {
                this.StepsPerSecond = this.Steps - this.lastSteps;
                this.lastSteps = this.Steps;
                this.timer = 0f;
            }
        }

    public Vector3 GetWorldPosition(Vector3Int TileMapPosition)
    {
        if (this.tileMap.Shape != TileShape.Quad)
        {
            Debug.LogError("TileShapes other than Quad are not yet implemented.");
            return Vector3.zero;
        }
        else
        {
            // We can position tile (0,0) at the origin.
            float x = TileMapPosition.x * this.TileSize;
            float y = 0f;
            float z = TileMapPosition.y * this.TileSize;
            return new Vector3(x, y, z);
        }
    }

    void BuildWorld()
    {
        for (int i = - this.initialRadius; i <= this.initialRadius; i++)
        {
            for (int j = -this.initialRadius; j <= this.initialRadius; j++)
            {
                Tile tile_data = this.tileMap.AddTile(new Vector3Int(i,j,0));
                this.CreateTileGraphics(tile_data);
            }
        }
    }

    void OnTileStateChanged(Tile tile_data)
    {
        if (TileGameObjectMap.ContainsKey(tile_data) == false)
        {
            Debug.LogError("No entry in the TileGameObjectMap for this tile");
            return;
        }
        GameObject tile_go = this.TileGameObjectMap[tile_data];
        if (tile_go == null)
        {
            Debug.LogError("TileGameObjectMap returned a null GameObject for this Tile");
            return;
        }

        if (this.speed <= 60)
        {
            if (this.UpdatingTiles.ContainsKey(tile_data) == false)
            {
                this.UpdatingTiles.Add(tile_data, 0f);
            }
        }
        else
        {
            tile_go.GetComponent<MeshRenderer>().material = Materials[tile_data.State];
        }
    }

    void CreateTileGraphics(Tile tile_data)
    {
        if (TileGameObjectMap.ContainsKey(tile_data) == false)
        {
            GameObject tile_go = Instantiate(TilePrefab, this.GetWorldPosition(tile_data.Position), Quaternion.Euler(-90, 0, 0));
            tile_go.GetComponent<MeshRenderer>().material = Materials[tile_data.State];
            tile_go.name = "Tile_" + tile_data.Position.x + "_" + tile_data.Position.y;
            tile_go.transform.parent = this.transform;
            this.TileGameObjectMap.Add(tile_data, tile_go);
            tile_data.RegisterTileStateChangedCallBack(OnTileStateChanged);
        }
        else
        {
            Debug.LogError("Tried to make Graphics for a tile which already has them");
        }
    }

    void DestroyTileGraphics(Tile tile_data)
    {
        if (TileGameObjectMap.ContainsKey(tile_data))
        {
            GameObject tile_go = TileGameObjectMap[tile_data];
            tile_data.UnregisterTileStateChangedCallBack(OnTileStateChanged);
            this.TileGameObjectMap.Remove(tile_data);
            Destroy(tile_go);
        }
        else
        {
            Debug.LogError("Tried to destroy Graphics for a tile which doesn't have them");
        }

    }

    public void MakeTiles(int radius, Vector3Int position)
    {
        for (int i = position.x - radius; i <= position.x + radius; i++)
        {
            for (int j = position.y - radius; j <= position.y + radius; j++)
            {
                Vector3Int newposition = new Vector3Int(i, j,0);
                if (this.tileMap.Tiles.ContainsKey(newposition) == false)
                {
                    Tile tile_data = this.tileMap.AddTile(newposition);
                    this.CreateTileGraphics(tile_data);
                }
            }
        }
    }

    public void CapTileStates(int numStates){
        this.tileMap.CapTileStates(numStates);
        this.numStates = numStates;
    }

    public void RegisterSpeedChangedCallBack(Action<World_Controller> callback)
    {
        cbSpeedChanged += callback;
    }

    public void UnregisterSpeedChangedCallBack(Action<World_Controller> callback)
    {
        cbSpeedChanged -= callback;
    }
}
