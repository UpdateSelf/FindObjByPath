// -------------
//<<< ---  由工具自动生成，请勿修改 --->>>
// ------------- >>>  Date : 2019-06-17 17:27:12
using UnityEngine;
using System;
using FindObjByPath;
using UnityEngine.UI;

public class StartUI_icn
{
    public class StartUI_ic :IFindObjs
    {
         public Transform self = null;
         public RectTransform content = null;
         public Image background = null;
         public Button button = null;
         public Text text = null;
         public ScrollRect scrollView = null;
         public Transform tempItem = null;

        public StartUI_ic () {}
        public StartUI_ic (Transform transform)
        {
            findObjs(transform);
        }
        public void findObjs(Transform transform)
        {
            self = transform;
            content = transform.Find("Content").GetComponent<RectTransform>();
            background = transform.Find("Content/Background").GetComponent<Image>();
            button = transform.Find("Content/Button").GetComponent<Button>();
            text = transform.Find("Content/Text").GetComponent<Text>();
            scrollView = transform.Find("Content/Scroll View").GetComponent<ScrollRect>();
            tempItem = transform.Find("Content/TempItem");
        }
        public void initEvent(StartUI_ic eventHander)
        {
            button.onClick.AddListener(()=>{eventHander.OnClickButton(button);});
            scrollView.onValueChanged.AddListener((value)=>{eventHander.OnValueChangedScrollView(scrollView,value);});
        }
        public virtual void OnClickButton(Button obj){}
        public virtual void OnValueChangedScrollView(ScrollRect obj,Vector2 value){}
    }
    public class TempItem_ic :IFindObjs
    {
         public Transform self = null;
         public Text tempText = null;
         public Image icon = null;

        public TempItem_ic () {}
        public TempItem_ic (Transform transform)
        {
            findObjs(transform);
        }
        public void findObjs(Transform transform)
        {
            self = transform;
            tempText = transform.Find("TempText").GetComponent<Text>();
            icon = transform.Find("Icon").GetComponent<Image>();
        }
        public void initEvent(TempItem_ic eventHander)
        {
        }
    }
}
