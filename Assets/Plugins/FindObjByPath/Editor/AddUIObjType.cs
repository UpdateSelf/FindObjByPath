using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System;

namespace FindObjByPath
{

    public enum CommonUIObjType
    {
        RectTransform,
        Transform,
        Image,
        Button,
        Text,
        ScrollRect,
        Toggle,
        InputField,
        Slider,
        Scrollbar,
        Dropdown,
        RawImage,
    }

    class AddUIObjType : Editor
    {

        [MenuItem("FindObjByPath/AddUIObjType/Transform", priority = 0)]
        static void AddUIType_Transform()
        {
            AddUIType(CommonUIObjType.Transform);
        }
        [MenuItem("FindObjByPath/AddUIObjType/RectTransform", priority = 0)]
        static void AddUIType_RectTransform()
        {
            AddUIType(CommonUIObjType.RectTransform);
        }
        [MenuItem("FindObjByPath/AddUIObjType/UIImage", priority = 0)]
        static void AddUIType_UIImage()
        {
            AddUIType(CommonUIObjType.Image);
        }

        [MenuItem("FindObjByPath/AddUIObjType/UIButton", priority = 0)]
        static void AddUIType_UIButton()
        {
            AddUIType(CommonUIObjType.Button);
        }

        [MenuItem("FindObjByPath/AddUIObjType/UIText", priority = 0)]
        static void AddUIType_Text()
        {
            AddUIType(CommonUIObjType.Text);
        }

        [MenuItem("FindObjByPath/AddUIObjType/UIScrollRect", priority = 0)]
        static void AddUIType_ScrollRect()
        {
            AddUIType(CommonUIObjType.ScrollRect);
        }

        [MenuItem("FindObjByPath/AddUIObjType/UIToggle", priority = 0)]
        static void AddUIType_Toggle()
        {
            AddUIType(CommonUIObjType.Toggle);
        }

        [MenuItem("FindObjByPath/AddUIObjType/UIInputField", priority = 0)]
        static void AddUIType_InputField()
        {
            AddUIType(CommonUIObjType.InputField);
        }

        [MenuItem("FindObjByPath/AddUIObjType/UISlider", priority = 0)]
        static void AddUIType_Slider()
        {
            AddUIType(CommonUIObjType.Slider);
        }


        [MenuItem("FindObjByPath/AddUIObjType/UIScrollbar", priority = 0)]
        static void AddUIType_Scrollbar()
        {
            AddUIType(CommonUIObjType.Scrollbar);
        }

        [MenuItem("FindObjByPath/AddUIObjType/UIDropDown", priority = 0)]
        static void AddUIType_Dropdown()
        {
            AddUIType(CommonUIObjType.Dropdown);
        }

        [MenuItem("FindObjByPath/AddUIObjType/UIRawImage", priority = 0)]
        static void AddUIType_UIRawImage()
        {
            AddUIType(CommonUIObjType.RawImage);
        }

        static void AddUIType(CommonUIObjType uiObjType) {
            if (Selection.gameObjects != null && Selection.gameObjects.Length >= 1)
            {
                int count = Selection.gameObjects.Length;
                for (int i = 0; i < count; i++)
                {
                    GameObject go = Selection.gameObjects[i];
                    FindObjSign sign = go.AddComponent<FindObjSign>();
                    sign.type = FindObjType.ItemField;
                    sign.objTypeName = uiObjType.ToString();
                    sign.functionDatas = GetFuncitonDatas(uiObjType);
                }
            }
        }
        static FunctionData[] GetFuncitonDatas(CommonUIObjType type)
        {
            List<FunctionData> taget = new List<FunctionData>();
            string[] functionNames = GetFunctionNames(type);
            if (functionNames == null || functionNames.Length == 0) return null;
            string[] functionAgrs = GetFunctionAgrs(type);
            for (int i = 0; i < functionNames.Length; i++) {
                FunctionData fd = new FunctionData(functionNames[i], functionAgrs[i], FunctionType.Event);
                taget.Add(fd);
            }
            return taget.ToArray();
        }

        static string GetCallArg(CommonUIObjType type)
        {
            string target = "";
            switch (type)
            {
                case CommonUIObjType.InputField:
                    target = "content";
                    break;
                case CommonUIObjType.ScrollRect:
                    break;
                case CommonUIObjType.Toggle:
                    target = "isOn";
                    break;
                case CommonUIObjType.Slider:
                    target = "value";
                    break;
                case CommonUIObjType.Scrollbar:
                    break;
                case CommonUIObjType.Dropdown:
                    target = "arg0";
                    break;
            }
            return target;
        }

        static string[] GetFunctionNames(CommonUIObjType uiType)
        {
            switch (uiType) {
                case CommonUIObjType.Button: return new string[] { "onClick" };
                case CommonUIObjType.Toggle: return new string[] { "onValueChanged" };
                case CommonUIObjType.Scrollbar: return new string[] { "onValueChanged" };
                case CommonUIObjType.ScrollRect: return new string[] { "onValueChanged" };
                case CommonUIObjType.Slider: return new string[] { "onValueChanged" };
                case CommonUIObjType.Dropdown: return new string[] { "onValueChanged" };
                case CommonUIObjType.InputField: return new string[] { "onEndEdit" };
            }
            return null;
        }

        static string[] GetFunctionAgrs(CommonUIObjType uiType)
        {
            switch (uiType)
            {
                case CommonUIObjType.Button: return new string[] { "" };
                case CommonUIObjType.Toggle: return new string[] { "bool isOn" };
                case CommonUIObjType.Scrollbar: return new string[] { "float value" };
                case CommonUIObjType.ScrollRect: return new string[] { "Vector2 value" };
                case CommonUIObjType.Slider: return new string[] {"float value" };
                case CommonUIObjType.Dropdown: return new string[] {"int index" };
                case CommonUIObjType.InputField: return new string[] {"string content"};
            }
            return null;
        }
    }
}
