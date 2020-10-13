using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    public enum MouseMode{None, PlaceAnt}

public class Input_Controller : MonoBehaviour
{
    static Camera mainCamera;
    TileMap_Controller tileMap_Controller;
    Ant_Controller ant_Controller;
    Transform camtransform;
    public float mouseSensitivityX;
    public float mouseSensitivityY;
    public float Camera_Speed, Scroll_Speed;
    Vector3 dragOrigin, clickPosition;
    public MouseMode mouseMode;

    float rotY, rotX;
    float maxHeight = 500f;


    // Start is called before the first frame update
    void Start()
    {
        mainCamera = Camera.main;
        this.mouseMode = MouseMode.None;
        this.tileMap_Controller = TileMap_Controller.Instance;
        this.ant_Controller = FindObjectOfType<Ant_Controller>();
        this.camtransform = mainCamera.transform;
        this.rotX = this.camtransform.localEulerAngles.y;
        this.rotY = -this.camtransform.localEulerAngles.x;

    }

    // Update is called once per frame
    void Update()
    {
        CameraFunctions();
        AntControllerFunctions();
    }

    void AntControllerFunctions(){
        if (Input.GetKeyDown(KeyCode.Space)){
            this.ant_Controller.Pause();
        }
        PlaceAnt();
    }

    void PlaceAnt(){
        if (this.mouseMode == MouseMode.PlaceAnt){
            if(Input.GetMouseButtonDown(0) && !MouseInputUIBlocker.BlockedByUI){
                clickPosition = getPosOnXZPlane();
                int i = (int)Mathf.Round(clickPosition.x);
                int j = (int)Mathf.Round(clickPosition.z);
                Vector2Int Position = new Vector2Int(i,j);
                if (!this.tileMap_Controller.tileMap.Tiles.ContainsKey(Position)){
                    //Debug.Log("No Tile at this Position");
                    return;
                }
                if (this.ant_Controller.GetAntsAtPosition(Position).Count != 0){
                    foreach(Ant ant in this.ant_Controller.GetAntsAtPosition(Position)){
                        this.ant_Controller.DestroyAntGraphics(ant);
                        ant.TurnRight();
                        if (ant.Facing != 3){
                            this.ant_Controller.CreateAntGraphics(ant);
                        }
                    }
                }
                else{
                    this.ant_Controller.MakeAnt(Position, 0);
                    this.tileMap_Controller.MakeTiles(this.tileMap_Controller.instantiateRadius, Position);
                }

            }
        }
    }

    public void PlaceAntMouseMode(){
        if (this.mouseMode == MouseMode.PlaceAnt){
            this.mouseMode = MouseMode.None;
        } else {
            this.mouseMode = MouseMode.PlaceAnt;
        }
    }

    void CameraFunctions(){
        CameraPosition();
        CameraRotation();
    }
    void CameraPosition(){
        Drag();
        WASDInput();
        MouseScrollInput();
        ShiftInput();
        //Normalise the position;
        this.camtransform.position = new Vector3(
            this.camtransform.position.x, 
            Mathf.Clamp(this.camtransform.position.y, 0.5f, this.maxHeight),
            this.camtransform.position.z);
    }

    public static Vector3 Clamp(Vector3 value, Vector3 min, Vector3 max)
    {
        return new Vector3(Mathf.Clamp(value.x, min.x, max.x),
                            Mathf.Clamp(value.y, min.y, max.y),
                            Mathf.Clamp(value.z, min.z, max.z));
    }

    private void CameraRotation(){
        if (Input.GetMouseButton(1) && !MouseInputUIBlocker.BlockedByUI) {
        this.rotX = this.camtransform.localEulerAngles.y + Input.GetAxis("Mouse X") * mouseSensitivityX;
        this.rotY += Input.GetAxis("Mouse Y") * mouseSensitivityY;
        this.rotY = Mathf.Clamp(this.rotY, -89.5f, 89.5f);
        this.camtransform.localEulerAngles = new Vector3(-this.rotY, this.rotX, 0.0f);
        }
    }

    private void WASDInput()
    { //returns the basic values, if it's 0 than it's not active.
        Vector3 WASDVector = new Vector3();
        if (Input.GetKey(KeyCode.W))
        {
            WASDVector += new Vector3(0, 0, 1);
        }
        if (Input.GetKey(KeyCode.S))
        {
            WASDVector += new Vector3(0, 0, -1);
        }
        if (Input.GetKey(KeyCode.A))
        {
            WASDVector += new Vector3(-1, 0, 0);
        }
        if (Input.GetKey(KeyCode.D))
        {
            WASDVector += new Vector3(1, 0, 0);
        }
        Vector3 MovementVector = Camera_Speed * Time.deltaTime * WASDVector;
        this.camtransform.position += this.camtransform.TransformDirection(MovementVector);
    }

    private void ShiftInput()
    { //returns the basic values, if it's 0 than it's not active.
        Vector3 ShiftVector = new Vector3();
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            ShiftVector += new Vector3(0, 1, 0);
        }
        this.camtransform.position += Camera_Speed * Time.deltaTime * ShiftVector;
    }

    private void MouseScrollInput()
    {
        Vector3 MovementVector = Camera_Speed * Time.deltaTime * Scroll_Speed * Input.mouseScrollDelta.y * new Vector3(0, 0, 1);
        this.camtransform.position += this.camtransform.TransformDirection(MovementVector);
    }
    private void Drag()
    {
        if (Input.GetMouseButtonDown(2) && !MouseInputUIBlocker.BlockedByUI)
        {
            dragOrigin = getPosOnXZPlane();
        }

        if (Input.GetMouseButton(2) && !MouseInputUIBlocker.BlockedByUI)
        {
            Vector3 pos = getPosOnXZPlane() -dragOrigin;
            this.camtransform.position += (new Vector3(-pos.x, 0, -pos.z));
        }
    }

    Vector3 getPosOnXZPlane()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        // create a plane at 0,0,0 whose normal points to +Y:
        Plane hPlane = new Plane(Vector3.up, Vector3.zero);
        // Plane.Raycast stores the distance from ray.origin to the hit point in this variable:
        float distance = 0;
        // if the ray hits the plane...
        if (hPlane.Raycast(ray, out distance))
        {
            // get the hit point:
            return ray.GetPoint(distance);
        }
        else
        {
            return Vector3.zero;
        }
    }

}
