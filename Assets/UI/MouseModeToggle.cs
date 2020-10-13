using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MouseModeToggle : MonoBehaviour
{
    Input_Controller input_Controller;
    Toggle myToggle;
    public MouseMode thisTogglesMode;
    // Start is called before the first frame update
    void Start()
    {
        this.input_Controller = FindObjectOfType<Input_Controller>();
        this.input_Controller.RegisterMouseModeChangedCallBack(OnMouseModeChanged);
        this.myToggle = this.GetComponent<Toggle>();
        this.myToggle.isOn = (input_Controller.mouseMode == thisTogglesMode);
    }

    public void ChangeMouseMode(){
        this.input_Controller.ChangeMouseMode(thisTogglesMode);
    }

    public void onToggleChanged(){
        if (this.myToggle.isOn == (input_Controller.mouseMode == thisTogglesMode)){
            return;
        }
        else {
            ChangeMouseMode();
        }
    }

    public void OnMouseModeChanged(Input_Controller input_Controller){
        //Debug.Log("OnMouseModeChanged: " + this.thisTogglesMode);
        this.myToggle.isOn = (input_Controller.mouseMode == thisTogglesMode);
    }
}
