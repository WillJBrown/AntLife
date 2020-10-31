using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TurnDir { Right3, Right2, Right, Straight, Left, Left2, Left3, Left4 };

public class Ant
{
    TileMap TileMap;
    int NumberOfDirections;
    public List<TurnDir> Behaviour { get; protected set; }
    public Vector3Int LastPosition { get; protected set; }
    private Vector3Int position;
    public Vector3Int Position
    {
        get { return this.position; }
        protected set
        {
            this.LastPosition = this.position;
            this.LastTile = this.TileMap.GetTileAt(this.position);
            this.Tile = this.TileMap.GetTileAt(value);
            this.position = value;
        }
    }
    private Tile tile;
    public Tile Tile
    {
        get { return this.tile; }
        protected set
        {
	        if (this.tile != null)
	        {
	        	this.tile.RemoveAnt(this);
	        }
            this.tile = value;
            this.tile.AddAnt(this);
        }
    }
    public Tile LastTile { get; protected set; }

    // Using North=0, East=1, South=2, West=3
    private int lastFacing;
    public int LastFacing
    {
        get { return this.lastFacing; }
        protected set { this.lastFacing = (value + this.NumberOfDirections) % this.NumberOfDirections; }
    }

    // Using North=0, East=1, South=2, West=3
    private int facing;
    public int Facing
    {
        get { return this.facing; }
        protected set
        {
            this.lastFacing = this.facing;
            this.facing = (value + this.NumberOfDirections) % this.NumberOfDirections;
        }
    }

    public Ant(TileMap tileMap, List<TurnDir> behaviour, float speed, Tile t, int facing = 1)
    {
        this.TileMap = tileMap;
        this.NumberOfDirections = this.TileMap.numDirections;
        int numStates = this.TileMap.numStates;
        this.Behaviour = new List<TurnDir>(behaviour);
        if (numStates < this.Behaviour.Count)
        {
            this.Behaviour.RemoveRange(numStates, this.Behaviour.Count - numStates);
            Debug.Log("Too many Instructions for the number of Tile States");
        }
        else if (numStates > this.Behaviour.Count)
        {
            World_Controller.Instance.CapTileStates(this.Behaviour.Count);
            Debug.Log("Too many Tile States for the number of instructions");
        }
	    this.Tile = this.LastTile = t;
        this.LastPosition = this.Position = t.Position;
        this.LastFacing = this.Facing = facing;
    }

    public void MoveForward()
    {
        this.Position += this.TileMap.GetNeighbourDirections(this.Position)[this.facing];
    }

    public void Turn(TurnDir dir)
    {
        this.Facing = this.Facing - ((int)dir - 3);
    }

    public void LangtonStep()
    {
        this.Turn(this.Behaviour[this.Tile.State]);
        this.Tile.State++;
        this.MoveForward();
    }

}
