using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpeedSlider : MonoBehaviour
{
    Ant_Controller AC;
    Text myText;
    Slider mySlider;

    int speed;
    // Start is called before the first frame update
    void Start()
    {
        AC = FindObjectOfType<Ant_Controller>();
        AC.RegisterSpeedChangedCallBack(OnSpeedChanged);
        myText = this.GetComponentInChildren<Text>();
        mySlider = this.GetComponentInChildren<Slider>();
        this.speed = (int)Mathf.Round(AC.speed);
        this.mySlider.value = this.speed;
    }

    // Update is called once per frame
    void Update()
    {
        if (AC.paused)
        {
            myText.text = "Paused";
        }
        else
        {
            if (this.speed < 120)
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

    public void OnSpeedChanged(Ant_Controller ant_Controller){
        this.speed = (int)Mathf.Round(ant_Controller.speed);
        this.mySlider.value = this.speed;
    }
}
