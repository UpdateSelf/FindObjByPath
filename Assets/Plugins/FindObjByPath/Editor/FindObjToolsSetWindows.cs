using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System;

namespace FindObjByPath
{
  
    class FindObjToolsSetWindows: EditorWindow
    {
        private string _savePath = ""; //存储路径
        private string _scriptSuffix = ""; //脚本后缀  ItemCtrl
        private string _classSuffix = ""; //类的后缀
        private string _varPrefix = "";//变量前缀
        private string _namespaceSuffix = "";//命名空间后缀 ItemCtrlNamespace
        private string _addNamespaces = "";

        private static string _windowsTitle = "FindObjToolsSetWindow";

        private void Awake()
        {
            DoInit();
        }

        void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            _savePath = EditorGUILayout.TextField("SavePath", _savePath);
            if (GUILayout.Button("Open"))
            {
                OpenSavePath();
            }
            EditorGUILayout.EndHorizontal();
            _scriptSuffix = EditorGUILayout.TextField("ScriptSuffix", _scriptSuffix);
            _varPrefix = EditorGUILayout.TextField("VarPrefix", _varPrefix);
            _classSuffix = EditorGUILayout.TextField("ClassSuffix", _classSuffix);
            _namespaceSuffix = EditorGUILayout.TextField("NamespaceSuffix", _namespaceSuffix);
            _addNamespaces = EditorGUILayout.TextField("AddNameSpaces", _addNamespaces);
            EditorGUILayout.LabelField("Tip:多个命名空间用“,”隔开.");
            if (GUILayout.Button("Save"))
            {
                DoSaveSet();
            }
        }


        void OpenSavePath()
        {
            if (!Directory.Exists(_savePath))
            {
                _savePath = Application.dataPath;
            }
            _savePath = EditorUtility.OpenFolderPanel(_windowsTitle, _savePath, "");
            if (_savePath.IndexOf(Application.dataPath) == -1)
            {
                _savePath = "";
            }
            else
            {
                _savePath = _savePath.Replace(Application.dataPath, "").Replace("\\", "/");
            }
        }

        void DoSaveSet()
        {
            CreateConfig config = new CreateConfig();
            config.savePath = _savePath;
            config.classSuffix = _classSuffix;
            config.varPrefix = _varPrefix;
            config.scriptSuffix = _scriptSuffix;
            config.namespaceSuffix = _namespaceSuffix;
            config.addNamespaces = _addNamespaces;

            string dir = Path.GetDirectoryName(CreateConfig.configPath);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            File.WriteAllText(CreateConfig.configPath, JsonUtility.ToJson(config));
            AssetDatabase.Refresh();
            Close();
        }

        void DoInit() {
            CreateConfig config = null;
            if (File.Exists(CreateConfig.configPath))
            {
                string text = File.ReadAllText(CreateConfig.configPath);
                config = JsonUtility.FromJson<CreateConfig>(text);
            }
            else {
                config = new CreateConfig();
            }
            _savePath = config.savePath;
            _classSuffix = config.classSuffix; 
            _varPrefix = config.varPrefix;
            _scriptSuffix = config.scriptSuffix ;
            _namespaceSuffix = config.namespaceSuffix;
            _addNamespaces = config.addNamespaces;
        }
    }
    
    #region 菜单
    public class AddMenuItemClass : Editor
    {


        [MenuItem("FindObjByPath/AddFindObjType/UIRoot", priority = 0)]
        static void AddUIType_UIRoot()
        {
            AddFindObjType(FindObjType.UIRoot);
        }

        [MenuItem("FindObjByPath/AddFindObjType/UIMultiItem", priority = 0)]
        static void AddUIType_UIMultiItem()
        {
            AddFindObjType(FindObjType.MultiItem);
        }
    
        [MenuItem("FindObjByPath/CreateScripts", priority = 0)]
        static void CreateScripts()
        {
            if (Selection.gameObjects != null && Selection.gameObjects.Length == 0)
            {
                Debug.LogError("select sign root gameObject !!!!");
                return;
            }
            FindObjScriptsCreate.ReadConfig();
            GameObject[] objs = Selection.gameObjects;
            for (int i = 0; i < objs.Length; i++)
            {
                FindObjSign[] signs = objs[i].GetComponents<FindObjSign>();
                if (FindObjScriptsCreate.CheckObjIsSignRootObj(signs))
                {
                    FindObjScriptsCreate.CreateScriptsToFile(objs[i].transform);
                }
                else
                {
                    Debug.LogError(string.Format("{0} not is a sign root gameObject!!!", objs[i].name));
                }
            }
        }

        [MenuItem("FindObjByPath/RemoveSignScript", priority = 0)]
        static void RemoveSignScript()
        {
            if (!EditorUtility.DisplayDialog("Are you sure?", "remove UITypeSign for this gameobject and it's all chils?", "ok", "cancel")) return;
            Transform[] trans = Selection.GetTransforms(SelectionMode.Deep);
            if (trans == null || trans.Length == 0) return;
            for (int i = 0; i < trans.Length; i++)
            {
                FindObjSign[] signs = trans[i].GetComponents<FindObjSign>();
                if (signs == null || signs.Length == 0) continue;
                for (int j = 0; j < signs.Length; j++)
                {
                    DestroyImmediate(signs[j]);
                }
            }
        }

        [MenuItem("FindObjByPath/Set", priority = 0)]
        static void FindObjToolsSet()
        {
            FindObjToolsSetWindows window = (FindObjToolsSetWindows)EditorWindow.GetWindow(typeof(FindObjToolsSetWindows));
            window.Show();
        }

        static void AddFindObjType(FindObjType type)
        {
            if (Selection.gameObjects != null && Selection.gameObjects.Length >= 1)
            {
                int count = Selection.gameObjects.Length;
                for (int i = 0; i < count; i++)
                {
                    GameObject go = Selection.gameObjects[i];
                    FindObjSign sign = go.AddComponent<FindObjSign>();
                    sign.type = type;
                    sign.objTypeName = type.ToString();
                    sign.isAddFunction = false;
                }
            }
        }
    }
    #endregion

}
