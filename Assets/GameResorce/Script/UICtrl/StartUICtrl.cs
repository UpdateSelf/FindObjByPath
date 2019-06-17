using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using IC = StartUI_icn;
public class StartUICtrl : MonoBehaviour {
    private IC.StartUI_ic startIc = new IC.StartUI_ic();
	// Use this for initialization
	void Start () {
        startIc.findObjs(transform);
        
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public class StartIcEventHander : IC.StartUI_ic {
        public override void OnClickButton(Button obj)
        {
            
        }

        public override void OnValueChangedScrollView(ScrollRect obj, Vector2 value)
        {
           
        }
    }
}
