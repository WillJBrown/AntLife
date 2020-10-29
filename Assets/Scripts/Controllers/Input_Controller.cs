using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    public enum MouseMode{None, PlaceAnt, ChangeTile}

public class Input_Controller : MonoBehaviour
{
    static Camera mainCamera;
    Action<Input_Controller> cbMouseModeChanged;
    static World_Controller WC;
    Transform camtransform;
    public float mouseSensitivityX;
    public float mouseSensitivityY;
    public float Camera_Speed, Scroll_Speed;
    Vector3 dragOrigin;
    public MouseMode mouseMode; //{get; protected set;}

    float rotY, rotX, timer = 0f;
    float maxHeight = 500f;


    // Start is called before the first frame update
    void Start()
    {
        mainCamera = Camera.main;
        this.mouseMode = MouseMode.None;
        WC = World_Controller.Instance;
        this.camtransform = mainCamera.transform;
        this.rotX = this.camtransform.localEulerAngles.y;
        this.rotY = -this.camtransform.localEulerAngles.x;

    }

    // Update is called once per frame
    void Update()
    {
        this.timer += Time.deltaTime;
        CameraFunctions();
        AntControllerFunctions();
        TileControllerFunctions();
    }

    void TileControllerFunctions(){
        ChangeTile();
    }

    void AntControllerFunctions(){
        if (Input.GetKeyDown(KeyCode.Space)){
            WC.Pause();
        }
        PlaceAnt();
        if (this.timer > 0.01f){
            this.timer = 0f;
            ChangeSpeed();
            ChangeMultiplier();
        }
    }

    void ChangeSpeed(){
        if (Input.GetKey(KeyCode.RightArrow)){
            WC.SetSpeed(WC.speed + 1);
            return;
        }
        if (Input.GetKey(KeyCode.LeftArrow)){
            WC.SetSpeed(WC.speed - 1);
            return;
        }
    }

    void ChangeMultiplier(){
        if (Input.GetKey(KeyCode.UpArrow)){
            WC.IncreaseMultiplier();
            return;
        }
        if (Input.GetKey(KeyCode.DownArrow)){
            WC.DecreaseMultiplier();
            return;
        }
    }
    void PlaceAnt(){
        if (this.mouseMode == MouseMode.PlaceAnt){
            if(Input.GetMouseButtonDown(0) && !MouseInputUIBlocker.BlockedByUI){
                Tile t = WC.GetTileAtWorldPosition(getPosOnXZPlane());
                //Debug.Log(Position);
                if (WC.AC.GetAntsAtTile(t).Count != 0){
                    foreach(Ant ant in WC.AC.GetAntsAtTile(t)){
                        WC.AC.DestroyAntGraphics(ant);
                        ant.Turn(TurnDir.Right);
                        if (ant.Facing != 0){
                            WC.AC.CreateAntGraphics(ant);
                        }
                    }
                }
                else{
                    WC.AC.MakeAnt(WC.defaultBehaviour, t, 0);
                }

            }
        }
    }

    void ChangeTile(){
         if (this.mouseMode == MouseMode.ChangeTile){
            if(Input.GetMouseButtonDown(0) && !MouseInputUIBlocker.BlockedByUI){
                Tile t = WC.GetTileAtWorldPosition(getPosOnXZPlane());
                t.State++;
                WC.ResetTile(t);
            }
        }
    }

    public void ChangeMouseMode(MouseMode mouseModein){
        if (this.mouseMode == mouseModein){
            this.mouseMode = MouseMode.None;
        } else {
            this.mouseMode = mouseModein;
        }
        if (cbMouseModeChanged != null){
            cbMouseModeChanged(this);
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
        //this.rotY = -this.camtransform.localEulerAngles.x;
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
        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
        {
            ShiftVector += new Vector3(0, -1, 0);
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

    public void RegisterMouseModeChangedCallBack(Action<Input_Controller> callback)
    {
        cbMouseModeChanged += callback;
    }

    public void UnregisterMouseModeChangedCallBack(Action<Input_Controller> callback)
    {
        cbMouseModeChanged -= callback;
    }

}
