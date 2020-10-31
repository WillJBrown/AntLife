using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class GameBoard : TileMap
{
    private List<GameRule> rules;
    public List<GameRule> Rules
    {
    get { return this.rules; } 
    protected set
    {
        this.rules = value;
        if (cbLifeGameChanged != null) 
        {
            cbLifeGameChanged(this);
        }
    } 
    }
    Action<GameBoard> cbLifeGameChanged;
    

    public GameBoard(int numStates, TileShape shape = TileShape.Quad) : base(numStates, shape)
    {
        this.Rules = new List<GameRule>();
    }

    public void AddRule(string[] parameters)
    {
        this.Rules.Add(new GameRule(this, parameters));
        cbLifeGameChanged(this);
    }

    public void AddDefaultRules(bool lifeGame)
    {
        if (lifeGame)
        {
            this.AddRule(new string[] {"State", "is not", "0", "AND", "Count", "<=", "1", "Add to State", "-1"});
            this.AddRule(new string[] {"State", "is not", "0", "AND", "Count", ">=", "4", "Add to State", "-1"});
            this.AddRule(new string[] {"State", "is not", "0", "AND", "State", "is not", "maximum state index", "Add to State", "1"});
            this.AddRule(new string[] {"State", "is", "0", "AND", "Count", "is", "3", "Add to State", "1"});
        }
        else
        {
            this.Rules = new List<GameRule>();
        }
    }

    Dictionary<Tile, int> EvaluateTiles(List<Tile> worklist)
    {
        Dictionary<Tile, int> TileData = new Dictionary<Tile, int>();
        foreach (Tile t in worklist.ToList())
        {
            int count = 0;
            foreach (Tile neighbour in this.GetNeighbours(t))
            {
                if (neighbour.State != 0)
                {
                    count++;
                }
            }
            TileData.Add(t, count);
        }
        return TileData;
    }

    public List<Tile> Tick(List<Tile> worklist = null)
    {
        if (this.Rules.Count == 0 || worklist == null)
        {
            return new List<Tile>();
        }
        Dictionary<Tile, int> TileData = this.EvaluateTiles(worklist);
        List<Tile> InactiveTiles = new List<Tile>();

        foreach (Tile t in TileData.Keys.ToList())
        {
            int state = t.State;
            int counter = 1;
            foreach (GameRule rule in this.Rules)
            {
                if (rule.Execute(t, TileData[t]))
                {
                    //Debug.Log("Rule " + counter + " was true for Tile " + t.Position);
                    break;
                }
                counter++;
            }
            if (TileData[t] == 0 && (state == 0 && t.State == 0))
            {
                InactiveTiles.Add(t);
            }
        }
        return InactiveTiles;
    }

    public void RegisterLifeGameCallBack(Action<GameBoard> callback)
    {
        cbLifeGameChanged += callback;
    }

    public void UnregisterLifeGameCallBack(Action<GameBoard> callback)
    {
        cbLifeGameChanged -= callback;
    }

}
