using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Input_Controller : MonoBehaviour
{
    static Camera mainCamera;
    Transform trans;
    public float mouseSensitivityX;
    public float mouseSensitivityY;
    public float Camera_Speed, Scroll_Speed;
    Vector3 dragOrigin;

    float rotY, rotX;
    float maxHeight = 500f;
    //Vector3 MinVector;
    //Vector3 MaxVector;

    // Start is called before the first frame update
    void Start()
    {
        mainCamera = Camera.main;
        trans = mainCamera.transform;
        rotX = trans.localEulerAngles.y;
        rotY = -trans.localEulerAngles.x;
        //MinVector = new Vector3(-200f, 0.5f, -200f);
        //MaxVector = new Vector3(200f, 200f, 200f);

        if (mainCamera.GetComponent<Rigidbody>())
            mainCamera.GetComponent<Rigidbody>().freezeRotation = true;
    }

    // Update is called once per frame
    void Update()
    {
        Drag();

        Vector3 MovementVector = Camera_Speed * Time.deltaTime * (WASDInput() + MouseScrollInput());
        trans.position += trans.TransformDirection(MovementVector);
        trans.position += Camera_Speed * Time.deltaTime * ShiftInput();
        Vector3 newpos = new Vector3(trans.position.x, Mathf.Clamp(trans.position.y, 0.5f, this.maxHeight), trans.position.z);
        trans.position = newpos;
        //trans.position = Clamp(trans.position, MinVector, MaxVector);
        
        if (Input.GetMouseButton(1))
        {
            trans.localEulerAngles = CameraRotation();
        }
    }

    public static Vector3 Clamp(Vector3 value, Vector3 min, Vector3 max)
    {
        return new Vector3(Mathf.Clamp(value.x, min.x, max.x),
                            Mathf.Clamp(value.y, min.y, max.y),
                            Mathf.Clamp(value.z, min.z, max.z));
    }

    private Vector3 CameraRotation()
    {
        rotX = trans.localEulerAngles.y + Input.GetAxis("Mouse X") * mouseSensitivityX;
        rotY += Input.GetAxis("Mouse Y") * mouseSensitivityY;
        rotY = Mathf.Clamp(rotY, -89.5f, 89.5f);
        return new Vector3(-rotY, rotX, 0.0f);
    }

    private Vector3 WASDInput()
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
        return WASDVector;
    }

    private Vector3 ShiftInput()
    { //returns the basic values, if it's 0 than it's not active.
        Vector3 ShiftVector = new Vector3();
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            ShiftVector += new Vector3(0, 1, 0);
        }
        return ShiftVector;
    }

    private Vector3 MouseScrollInput()
    {
            return Scroll_Speed * Input.mouseScrollDelta.y * new Vector3(0, 0, 1);
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

    void Drag()
    {
        if (Input.GetMouseButtonDown(2))
        {
            dragOrigin = getPosOnXZPlane();
        }

        if (Input.GetMouseButton(2))
        {
            Vector3 pos = getPosOnXZPlane() -dragOrigin;
            trans.position += (new Vector3(-pos.x, 0, -pos.z));
        }
    }
}
