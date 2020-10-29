using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniMap : MonoBehaviour
{
    Camera MainCamera, MiniMapCamera;
    World_Controller WC;

    float LastSize = 10f, Size = 10f, timer = 0f, speed = 1f, angle = 85f;
    
    // Start is called before the first frame update
    void Start()
    {
        WC = World_Controller.Instance;
        MainCamera = Camera.main;
        MiniMapCamera = GetComponent<Camera>();
        MiniMapCamera.transform.position += WC.tileMap.CartesianCoords(WC.tileMap.Centrepoint);
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        this.Size = this.MiniMapCamera.orthographicSize;
        if (this.Size != WC.maxTileRad){
            this.MiniMapCamera.orthographicSize = Mathf.Lerp(this.LastSize, WC.maxTileRad, this.speed * this.timer);
        } else {
            timer = 0f;
            this.LastSize = this.Size;
        }
    }

    public void MiniMapClick(){

        MainCamera.transform.position = new Vector3(0, 1.6f * Mathf.Sin(Mathf.Deg2Rad * angle) * WC.maxTileRad, -Mathf.Cos(Mathf.Deg2Rad * angle) * 1.6f * WC.maxTileRad);
        MainCamera.transform.rotation = Quaternion.Euler(angle,0,0);
        
    }
}
