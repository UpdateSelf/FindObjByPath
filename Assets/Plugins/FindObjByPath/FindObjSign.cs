using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;

namespace FindObjByPath {

    public interface IFindObjs{
        void findObjs(Transform transform);
    }

    public enum FindObjType
    {
        UIRoot,
        MultiItem,
        ItemField,
    }

    public enum FunctionType {
        Event,
        Action
    }

    [Serializable]
    public class FunctionData {
        
        public FunctionType functionType = FunctionType.Event;
        public string functionName = "";
        public string functionAgr = "";

        public FunctionData() { }
        public FunctionData(string functionName, string functionAgr, FunctionType functionType) {
            this.functionName = functionName;
            this.functionAgr = functionAgr;
            this.functionType = functionType;
        }

        public static string GetFunctionCallAgr(string agrString) {
            if (string.IsNullOrEmpty(agrString)) return "";
            string[] agrs = agrString.Split(',');
            string target = agrs[0].Split(' ')[1];
            for (int i = 1; i < agrs.Length; i++) {
                target += ("," + agrs[i].Split(' ')[1]);
            }
            return target;
        }
    }

    public class FindObjSign : MonoBehaviour
    {
        public FindObjType type = FindObjType.ItemField;
        public string objTypeName = "";
        public bool isAddFunction = true;
        public FunctionData[] functionDatas = null;
    }

}

