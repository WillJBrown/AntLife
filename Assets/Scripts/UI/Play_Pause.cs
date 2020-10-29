using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Play_Pause : MonoBehaviour
{
    World_Controller WC;
    Sprite PlaySprite, PauseSprite;
    Image myImage;
    // Start is called before the first frame update
    void Start()
    {
        this.WC = World_Controller.Instance;
        this.WC.RegisterSpeedChangedCallBack(OnSpeedChanged);
        myImage = this.GetComponent<Image>();
        if (WC.baseSprites["Play_Button"] == null){
            Debug.LogError("There is no Play Button Sprite");
        }
        PlaySprite = WC.baseSprites["Play_Button"];
        if (WC.baseSprites["Pause_Button"] == null){
            Debug.LogError("There is no Pause Button Sprite");
        }
        PauseSprite = WC.baseSprites["Pause_Button"];
    }

    public void OnSpeedChanged(World_Controller WC){
        if (WC.paused){
            myImage.sprite = PlaySprite;
        } else{
            myImage.sprite = PauseSprite;
        }
    }

}
