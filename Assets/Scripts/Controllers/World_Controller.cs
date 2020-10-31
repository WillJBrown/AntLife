using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MyExtensions;

public class World_Controller : MonoBehaviour
{
    #region Declarations
    public static World_Controller Instance { get; protected set; }
    public Ant_Controller AC { get; protected set; }
    public Object_Pooler OP { get; protected set; }
    public GameBoard GameBoard { get; protected set; }
    GameObject TileContainer;
    Action<World_Controller> cbSpeedChanged, cbNewWorld;
    public float movePercentage { get; protected set; }
    int numStates;
    public TileShape tileShape;
    public int initialRadius { get; protected set; } = 5;
    public int instantiateRadius { get; protected set; } = 3;
    public int speedCutoff { get; protected set; } = 60;
    public int antRad { get; protected set; } = 1;
    public int multiplier { get; protected set; } = 1;
    public float TileSize { get; protected set; } = 1f;
    public int ageCutoff { get; protected set; } = 10;
    Dictionary<string, GameObject> basePrefabs;
    Dictionary<string, Material> baseMaterials;
    public Dictionary<string, Sprite> baseSprites { get; protected set; }
    GameObject Ground;
    //public float StepsPerSecond=0f;
    List<Material> Materials;
    Dictionary<Tile, GameObject> TileGameObjectMap;
    Dictionary<Tile, float> UpdatingTiles;
    Dictionary<Tile, int> TileGraphicAges;
    public List<TurnDir> defaultBehaviour;
    public float maxTileRad { get; protected set; } = 0f;

    //float timer = 0f;
    public int Steps { get; protected set; } = 0;
    //int lastSteps = 0;
    public float speed { get; protected set; } = 1f;
    public bool paused { get; protected set; } = true;
    List<Tile> InactiveTiles;
    List<Tile> Interior;
    #endregion

    #region Setup Functions
    // Start is called before the first frame update
    void OnEnable()
    {
        this.LoadResources();
        this.NewWorld(false, this.tileShape, this.defaultBehaviour);
    }

    void LoadResources()
    {
        this.basePrefabs = new Dictionary<string, GameObject>();
        GameObject[] TileGameObjects = Resources.LoadAll<GameObject>("Tile_Prefabs/");
        foreach (GameObject go in TileGameObjects)
        {
            this.basePrefabs[go.name] = go;
        }
        GameObject[] GameObjects = Resources.LoadAll<GameObject>("/");
        foreach (GameObject go in GameObjects)
        {
            this.basePrefabs[go.name] = go;
        }
        this.baseMaterials = new Dictionary<string, Material>();
        Material[] Mats = Resources.LoadAll<Material>("Materials/");
        foreach (Material m in Mats)
        {
            this.baseMaterials[m.name] = m;
        }
        this.baseSprites = new Dictionary<string, Sprite>();
        Sprite[] Sprites = Resources.LoadAll<Sprite>("UI/");
        foreach (Sprite s in Sprites)
        {
            this.baseSprites[s.name] = s;
        }
    }

    public void Reset(TileShape shape, List<TurnDir> defaultBehaviour)
    {
        foreach (Ant ant in this.AC.AntGameObjectMap.Keys.ToList())
        {
            this.AC.DestroyAntGraphics(ant);
        }
        Destroy(this.AC);
        foreach (Tile t in this.TileGameObjectMap.Keys.ToList())
        {
            this.DestroyTileGraphics(t);
        }
        foreach (Transform child in this.gameObject.transform)
        {
            Destroy(child.gameObject);
        }
        Destroy(this.OP);
        this.NewWorld(true, shape, defaultBehaviour);
    }
    public void Reset()
    {
        this.Reset(this.tileShape, this.defaultBehaviour);
    }

    public void NewWorld(bool Clone, TileShape shape, List<TurnDir> defaultBehaviour)
    {
        if (!Clone && Instance != null)
        {
            Debug.LogError("There should never be two TileMap_Controllers");
        }
        Instance = this;
        this.paused = true;
        this.multiplier = 1;
        this.defaultBehaviour = defaultBehaviour;
        this.tileShape = shape;
        this.numStates = this.defaultBehaviour.Count;
        this.GenerateMaterials();
        this.MakeGround();
        this.UpdatingTiles = new Dictionary<Tile, float>();
        this.InactiveTiles = new List<Tile>();
        this.Interior = new List<Tile>();
        this.TileGraphicAges = new Dictionary<Tile, int>();
        this.Steps = 0;
        this.maxTileRad = 0f;
        //this.lastSteps = 0;
        //this.StepsPerSecond = 0f;
        this.speed = 1f;
        this.ResetMovementPercentage();
        this.OP = this.gameObject.AddComponent<Object_Pooler>();
        this.OP.Initialise();
        if (this.basePrefabs["Ant_Prefab"] == null)
        {
            Debug.LogError("There is no Ant Prefab");
        }
        this.OP.AddPool("Ant", this.basePrefabs["Ant_Prefab"], 1);
        this.OP.AddPool("Tile", this.GetTilePrefab(false, this.tileShape), 1000);
        this.OP.AddPool("PausedTile", this.GetTilePrefab(true, this.tileShape), 100);
	    this.GameBoard = new GameBoard(this.numStates, this.tileShape);
        this.TileGameObjectMap = new Dictionary<Tile, GameObject>();
        Camera.main.transform.position = new Vector3(0f, 1.6f, -3f) + this.GameBoard.CartesianCoords(this.GameBoard.Centrepoint);
        Camera.main.transform.rotation = Quaternion.Euler(20, 0, 0);
        this.MakeTiles(initialRadius, this.GameBoard.GetTileAt(this.GameBoard.Centrepoint));
        this.AC = this.gameObject.AddComponent<Ant_Controller>();
        this.AC.Initialse(this);

        if (cbNewWorld != null)
        {
            cbNewWorld(this);
        }
        if (cbSpeedChanged != null)
        {
            cbSpeedChanged(this);
        }
    }
    void GenerateMaterials(int count = 0)
    {
        if (count == 0)
        {
            count = this.numStates;
        }
        this.Materials = new List<Material>();
        for (int i = 0; i < count; i++)
        {
            Vector3 HSV = HSVfromState(i, count);
            Color StateColor = Color.HSVToRGB(HSV.x, HSV.y, HSV.z);
            if (this.baseMaterials["Light_Tile"] == null)
            {
                Debug.LogError("There is no Light Tile");
            }
            this.Materials.Add(Material.Instantiate(this.baseMaterials["Light_Tile"]));
            Materials[i].SetColor("_Color", StateColor);
        }
    }

    void MakeGround()
    {
        if (this.basePrefabs["Ground_Prefab"] == null)
        {
            Debug.LogError("There is no Ground Prefab");
        }
        this.Ground = Instantiate(this.basePrefabs["Ground_Prefab"], new Vector3(0f, -0.01f, 0f), Quaternion.Euler(-90, 0, 0));
        if (this.baseMaterials["Transparent_Tile"] == null)
        {
            Debug.LogError("There is no Transparent Tile");
        }
        this.Ground.GetComponent<MeshRenderer>().material = this.baseMaterials["Transparent_Tile"];
        this.Ground.transform.parent = this.transform;
        this.Ground.name = "Ground";
    }
    #endregion

    #region Update Functions
    // Update is called once per frame
    void Update()
    {
        this.speed = Mathf.Clamp(speed, 0, this.speedCutoff);

        if (!this.paused)
        {
            this.movePercentage += Time.deltaTime * this.speed;
            this.AnimateUpdatingTiles();
            if (AC.Tick())
            {
                this.CullTiles(this.GameBoard.Tick(this.CalculateWorklist()));
                this.Steps += this.multiplier;
            };
            //updateSteps(Time.deltaTime);
        }
    }
    void AnimateUpdatingTiles()
    {
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

    // void updateSteps(float deltaTime)
    //     {
    //         this.timer += deltaTime;
    //         if (this.Steps == 0) {
    //             // Stops it crashing in the first second
    //             return;
    //         }
    //         if (this.timer >= 1)
    //         {
    //             this.StepsPerSecond = this.Steps - this.lastSteps;
    //             this.lastSteps = this.Steps;
    //             this.timer = 0f;
    //         }
    //     }

    List<Tile> CalculateWorklist()
    {
        List<Tile> Worklist = this.TileGameObjectMap.Keys.ToList();
        List<Ant> Ants = this.AC.AntGameObjectMap.Keys.ToList();
        List<Tile> AntPositions = new List<Tile>();
        foreach (Ant ant in Ants)
        {
            AntPositions.Add(ant.LastTile);
        }
        return Worklist.Except(AntPositions).ToList();
    }

    #endregion

    #region Utility Functions
    Vector3 HSVfromState(int i, int count = 0)
    {
        if (count == 0)
        {
            count = this.numStates;
        }
        i = i % count;
        if (i == 0)
        {
            return new Vector3(0f, 0f, 1f);
        }
        else if (i == 1)
        {
            return Vector3.zero;
        }
        else
        {
            return new Vector3(((float)i - 2) / ((float)count - 2), 1f, 1f);
        }
    }
    public Vector3 GetWorldPosition(Vector3Int TileMapPosition)
    {
        return this.TileSize * this.GameBoard.CartesianCoords(TileMapPosition);
    }

    public Tile GetTileAtWorldPosition(Vector3 Position)
    {
        return this.GameBoard.GetClosestTile(Position / this.TileSize);
    }

    public void AddDefaultGameofLifeRules(bool lifeGame){
        this.GameBoard.AddDefaultRules(lifeGame);
    }
    
    #endregion

    #region Tile Functions
    public void MakeTiles(int radius, Tile t)
    {
        int TilesMade = 0;
        if (this.TileGraphicAges.ContainsKey(t))
        {
            this.TileGraphicAges[t] = 0;
        }
        if (!this.TileGameObjectMap.ContainsKey(t))
        {
            this.CreateTileGraphics(t);
            TilesMade++;
        }
        int counter = 0;
        List<Tile> WorkList = new List<Tile>();
        WorkList.Add(t);
        while (counter < radius)
        {
            foreach (Tile workingtile in WorkList.ToList())
            {
                WorkList.RemoveAt(0);
                if (!this.Interior.Contains(workingtile) && counter < radius - 1)
                {
                    foreach (Tile neighbour in this.GameBoard.GetNeighbours(workingtile))
                    {
                        if (this.TileGraphicAges.ContainsKey(neighbour))
                        {
                            this.TileGraphicAges[neighbour] = 0;
                        }
                        if (!this.TileGameObjectMap.ContainsKey(neighbour))
                        {
                            WorkList.Add(neighbour);
                            this.CreateTileGraphics(neighbour);
                            TilesMade++;
                        }
                    }
                }
            }
            counter++;
        }
        if (!this.Interior.Contains(t) && TilesMade == 0)
        {
            this.Interior.Add(t);
        }
    }
    public void ResetTile(Tile t)
    {
        if (this.UpdatingTiles.ContainsKey(t))
        {
            GameObject tile_go = this.TileGameObjectMap[t];
            tile_go.GetComponent<MeshRenderer>().material = Materials[t.State];
            this.UpdatingTiles.Remove(t);
        }
    }
    public void CapTileStates(int numStates)
    {
        this.GameBoard.CapTileStates(numStates);
        this.numStates = numStates;
    }

    #endregion

    #region Graphics Functions
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
        if (TileGraphicAges.ContainsKey(tile_data) == false)
        {
            Debug.LogError("No entry in the TileGraphicAges for this tile");
            return;
        }
        this.MakeTiles(this.instantiateRadius, tile_data);

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

    void CullTiles(List<Tile> Worklist = null)
    {
        foreach (Tile t in this.TileGameObjectMap.Keys.ToList().Except(Worklist))
        {
            if (t.State == 0)
            {
                Worklist.Add(t);
            }
        }
        foreach (Tile t in Worklist.ToList().Except(this.UpdatingTiles.Keys.ToList()))
        {
            if (this.TileGameObjectMap.ContainsKey(t))
            {
                if (this.TileGraphicAges[t] > this.ageCutoff)
                {
                    this.DestroyTileGraphics(t);
                }
                else
                {
                    this.TileGraphicAges[t] = this.TileGraphicAges[t] + 1;
                }
            }
            // if (Worklist.Contains(t))
            // {
            //     this.InactiveTiles.Remove(t);
            // }
        }
    }

    GameObject GetTilePrefab(bool paused, TileShape shape)
    {
        string PausedPrefix = "";
        if (paused)
        {
            PausedPrefix = "98%_";
        }
        string shapestring;
        if (shape == TileShape.DiagQuad)
        {
            shapestring = "Quad";
        }
        else
        {
            shapestring = shape.ToString();
        }
        GameObject retval = this.basePrefabs[PausedPrefix + shapestring + "_Prefab"];
        if (retval != null)
        {
            return retval;
        }
        Debug.LogError("No Prefab with that Name");
        return null;
    }

    string GetTileTag(bool paused)
    {
        string tag = "Tile";
        if (paused) { tag = "PausedTile"; }
        return tag;
    }

    void CreateTileGraphics(Tile tile_data)
    {
        if (TileGameObjectMap.ContainsKey(tile_data) == false)
        {
            float flip = 0f;
            if (this.GameBoard.Shape == TileShape.Tri && !this.GameBoard.IsHexCentre(tile_data.Position)) { flip = 180f; }
            GameObject tile_go = this.OP.SpawnFromPool(this.GetTileTag(this.paused), this.GetWorldPosition(tile_data.Position), Quaternion.Euler(-90, flip, 0));
            tile_go.GetComponent<MeshRenderer>().material = Materials[tile_data.State];
            tile_go.name = "Tile_" + tile_data.Position.x + "_" + tile_data.Position.y + "_" + tile_data.Position.z;
            this.TileGameObjectMap.Add(tile_data, tile_go);
            this.TileGraphicAges.Add(tile_data, 0);
            tile_data.RegisterTileStateChangedCallBack(OnTileStateChanged);
            float Scale = Mathf.Max(Mathf.Abs(tile_go.transform.position.x) / 1.6f, Mathf.Abs(tile_go.transform.position.z) / 0.9f);
            if (this.maxTileRad < Scale)
            {
                this.maxTileRad = Scale;
            }

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
            if (this.Interior.Contains(tile_data))
            {
                this.Interior.Remove(tile_data);
            }
            foreach (Tile neighbour in this.GameBoard.GetNeighbours(tile_data))
            {
                if (this.Interior.Contains(neighbour))
                {
                    this.Interior.Remove(neighbour);
                }
            }
            this.TileGameObjectMap.Remove(tile_data);
            if (this.TileGraphicAges.ContainsKey(tile_data))
            {
                this.TileGraphicAges.Remove(tile_data);
            }
            else
            {
                Debug.LogError("No Tile Age in the Dictionary");
            }
            tile_go.Despawn(this.OP, this.GetTileTag(this.paused));
        }
        else
        {
            Debug.LogError("Tried to destroy Graphics for a tile which doesn't have them");
        }

    }

    #endregion

    #region (External) Accessors and Setters
    public void Pause()
    {
        this.paused = !this.paused;
        if (this.paused)
        {
            if (this.baseMaterials["Transparent_Tile"] == null)
            {
                Debug.LogError("There is no Transparent Tile");
            }
            this.Ground.GetComponent<MeshRenderer>().material = this.baseMaterials["Transparent_Tile"];
        }
        else
        {
            if (this.baseMaterials["Light_Tile"] == null)
            {
                Debug.LogError("There is no Light Tile");
            }
            this.Ground.GetComponent<MeshRenderer>().material = this.baseMaterials["Light_Tile"];
        }
        foreach (Tile t in this.TileGameObjectMap.Keys.ToList())
        {
            this.TileGameObjectMap[t] = this.TileGameObjectMap[t].PooledPrefabSwap(this.OP, this.GetTileTag(!this.paused), this.GetTileTag(this.paused));
            this.ResetTile(t);
        }
        if (cbSpeedChanged != null)
        {
            cbSpeedChanged(this);
        }
    }
    public void ResetMovementPercentage()
    {
        this.movePercentage = 0f;
    }
    public void SetSpeed(float speed)
    {
        this.speed = Mathf.Clamp(speed, 1, this.speedCutoff);
        if (cbSpeedChanged != null)
        {
            cbSpeedChanged(this);
        }
    }
    public void IncreaseMultiplier()
    {
        this.multiplier++;
        this.multiplier = Mathf.Clamp(this.multiplier, 1, 999);
    }

    public void DecreaseMultiplier()
    {
        this.multiplier--;
        this.multiplier = Mathf.Clamp(this.multiplier, 1, 999);
    }

    #endregion

    #region Callback Registers

    public void RegisterSpeedChangedCallBack(Action<World_Controller> callback)
    {
        cbSpeedChanged += callback;
    }

    public void UnregisterSpeedChangedCallBack(Action<World_Controller> callback)
    {
        cbSpeedChanged -= callback;
    }

    public void RegisterNewWorldCallBack(Action<World_Controller> callback)
    {
        cbNewWorld += callback;
    }

    public void UnregisterNewWorldCallBack(Action<World_Controller> callback)
    {
        cbNewWorld -= callback;
    }

    #endregion

}
