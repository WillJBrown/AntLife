using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TileMap_Controller : MonoBehaviour
{
    public static TileMap_Controller Instance { get; protected set; }

    int numStates;
    public int initialRadius, instantiateRadius;
    public float speed = 1f;
    public float TileSize {get; protected set;}
    public GameObject TilePrefab;
    public GameObject AntPrefab;
    public List<Material> Materials;
    public TileMap tileMap { get; protected set; }
    Dictionary<Tile, GameObject> TileGameObjectMap;
    Dictionary<Tile, float> UpdatingTiles;

    // Start is called before the first frame update
    void OnEnable()
    {   
        if (Instance != null)
        {
            Debug.LogError("There should never be Two tilemapcontorllers");
        }
        Instance = this;
        this.TileSize = 1f;
        this.numStates = this.Materials.Count;
        this.tileMap = new TileMap(this.numStates);
        this.TileGameObjectMap = new Dictionary<Tile, GameObject>();
        this.BuildWorld();
        this.UpdatingTiles = new Dictionary<Tile, float>();
    }

    // Update is called once per frame
    void Update()
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

    Vector3 GetWorldPosition(Tile tile_data)
    {
        if (this.tileMap.Shape != TileShape.Quad)
        {
            Debug.LogError("TileShapes other than Quad are not yet implemented.");
            return Vector3.zero;
        }
        else
        {
            // We can position tile (0,0) at the origin.
            float x = tile_data.Position.x * this.TileSize;
            float y = 0f;
            float z = tile_data.Position.y * this.TileSize;
            return new Vector3(x, y, z);
        }
    }

    void BuildWorld()
    {
        for (int i = - this.initialRadius; i <= this.initialRadius; i++)
        {
            for (int j = -this.initialRadius; j <= this.initialRadius; j++)
            {
                Tile tile_data = this.tileMap.AddTile(new Vector2Int(i,j));
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
            GameObject tile_go = Instantiate(TilePrefab, this.GetWorldPosition(tile_data), Quaternion.Euler(-90, 0, 0));
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

    public void MakeTiles(int radius, Vector2Int position)
    {
        for (int i = position.x - radius; i <= position.x + radius; i++)
        {
            for (int j = position.y - radius; j <= position.y + radius; j++)
            {
                Vector2Int newposition = new Vector2Int(i, j);
                if (this.tileMap.Tiles.ContainsKey(newposition) == false)
                {
                    Tile tile_data = this.tileMap.AddTile(newposition);
                    this.CreateTileGraphics(tile_data);
                }
            }
        }
    }
}
