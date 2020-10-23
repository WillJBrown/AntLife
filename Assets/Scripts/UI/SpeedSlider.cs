using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpeedSlider : MonoBehaviour
{
    World_Controller WC;
    Text myText;
    Slider mySlider;

    int speed;
    // Start is called before the first frame update
    void Start()
    {
        this.WC = World_Controller.Instance;
        this.WC.RegisterSpeedChangedCallBack(OnSpeedChanged);
        myText = this.GetComponentInChildren<Text>();
        mySlider = this.GetComponentInChildren<Slider>();
        mySlider.maxValue = WC.speedCutoff;
        this.speed = (int)Mathf.Floor(this.WC.speed);
        this.mySlider.value = this.speed;
    }

    // Update is called once per frame
    void Update()
    {
        if (this.WC.paused)
        {
            myText.text = "Paused";
        }
        else
        {
            if (this.speed < WC.speedCutoff)
            {
                myText.text = "Speed: " + this.speed.ToString();
            }
            else if(this.speed <= 0)
            {
                myText.text = "Speed: 0";
            }
            else
            {
                myText.text = "Speed: Max";
            }
        }
    }

    public void OnSpeedChanged(World_Controller WC){
        this.speed = (int)Mathf.Floor(WC.speed);
        this.mySlider.value = this.speed;
    }
}
