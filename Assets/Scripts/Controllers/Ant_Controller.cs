﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MyExtensions;

public class Ant_Controller : MonoBehaviour
{
    int maxAnts = 256;
    static World_Controller WC;
    public Dictionary<Ant, GameObject> AntGameObjectMap { get; protected set; }

    public void Initialse(World_Controller wc)
    {
        WC = wc;
        this.AntGameObjectMap = new Dictionary<Ant, GameObject>();
        int AntRad = WC.antRad;
        for (int i = (1 - AntRad); i < AntRad; i++)
        {
            for (int j = (1 - AntRad); j < AntRad; j++)
            {
                Vector3Int position = new Vector3Int(i, j, 0);
                this.MakeAnt(WC.defaultBehaviour, WC.GameBoard.GetTileAt(WC.GameBoard.Centrepoint + position));
            }
        }
    }

    public bool Tick()
    {
        bool retval = false;
        if (WC.speed > 0)
        {
            if (WC.movePercentage >= 1f || WC.speed >= WC.speedCutoff)
            {
                WC.ResetMovementPercentage();
                for (int i = 0; i < WC.multiplier; i++) { this.LangtonStep(); }
                retval = true;
            }
            UpdateAll(Time.deltaTime, WC.speed);
        }
        return retval;
    }

    public void CreateAntGraphics(Ant ant_data)
    {
        if (this.AntGameObjectMap.ContainsKey(ant_data) == false)
        {
            GameObject ant_go = WC.OP.SpawnFromPool("Ant",
                AntTransform(ant_data, 0) * WC.TileSize,
                AntRotation(ant_data, 0));
            this.AntGameObjectMap.Add(ant_data, ant_go);
            ant_go.name = "Ant";
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
            this.AntGameObjectMap.Remove(ant_data);
            ant_go.Despawn(WC.OP, "Ant");
        }
        else
        {
            Debug.LogError("Tried to destroy Graphics for an ant which isn't in the map");
        }

    }

    public void MakeAnt(List<TurnDir> behaviour, Tile t, int facing = 0)
    {
        if (this.AntGameObjectMap.Count < this.maxAnts)
        {
            Ant ant = new Ant(WC.GameBoard, behaviour, WC.speed, t, facing);
            this.CreateAntGraphics(ant);
            WC.MakeTiles(WC.instantiateRadius, t);
        }
        else
        {
            Debug.Log("No more than " + this.maxAnts + " Ants allowed");
            return;
        }
    }

    void UpdateAll(float deltaTime, float speed)
    {
        foreach (Ant ant in this.AntGameObjectMap.Keys.ToArray())
        {
            UpdateAnt(ant);
        }
    }

    public void LangtonStep()
    {
        foreach (Ant ant in this.AntGameObjectMap.Keys.ToArray())
        {
            ant.LangtonStep();
        }
    }

    public Vector3 AntTransform(Ant ant, int speedcutoff = 120)
    {
        if (WC.speed >= speedcutoff)
        {
            return WC.GetWorldPosition(ant.Position);
        }
        float movementPercentage = Mathf.Clamp((2 * WC.movePercentage) - 1f, 0f, 1f);
        if (movementPercentage == 1f)
        {
            Debug.Log("Reached Destination");
        }
        Vector3 LastWorldPosition = WC.GetWorldPosition(ant.LastPosition);
        Vector3 WorldPosition = WC.GetWorldPosition(ant.Position);
        float x = Mathf.Lerp(LastWorldPosition.x, WorldPosition.x, movementPercentage);
        float z = Mathf.Lerp(LastWorldPosition.z, WorldPosition.z, movementPercentage);
        return new Vector3(x, 0f, z);
    }

    public Quaternion AntRotation(Ant ant, int speedcutoff = 120)
    {
        float factor = 360f / (float)WC.GameBoard.numDirections;
        float offset = 0;
        float lastoffset = 0;
        if (WC.GameBoard.Shape == TileShape.Hex)
        {
            offset = lastoffset = factor / 2f;
        }
        else if (WC.GameBoard.Shape == TileShape.Tri)
        {
            if (WC.GameBoard.IsHexCentre(ant.Position))
            {
                offset = factor / 2f;
            }
            if (WC.GameBoard.IsHexCentre(ant.LastPosition))
            {
                lastoffset = factor / 2f;
            }
        }
        if (WC.speed >= speedcutoff)
        {
            return Quaternion.Euler(new Vector3(0f, offset + factor * ant.Facing, 0f));
        }
        float rotationPercentage = Mathf.Clamp(2 * WC.movePercentage, 0f, 1f);
        float Rotation = Mathf.LerpAngle(lastoffset + factor * ant.LastFacing, offset + factor * ant.Facing, rotationPercentage);
        return Quaternion.Euler(new Vector3(0f, Rotation, 0f));
    }

    void UpdateAnt(Ant ant_data)
    {
        if (this.AntGameObjectMap.ContainsKey(ant_data))
        {
            GameObject ant_go = this.AntGameObjectMap[ant_data];
            ant_go.transform.position = AntTransform(ant_data, WC.speedCutoff) * WC.TileSize;
            ant_go.transform.rotation = AntRotation(ant_data, WC.speedCutoff);
        }
        else
        {
            Debug.LogError("Tried to update an ant which isn't in the map");
        }
    }
}
