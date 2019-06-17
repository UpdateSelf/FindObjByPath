using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System;
using System.Xml;

namespace FindObjByPath
{

    [Serializable]
    public class CreateConfig
    {
        public static string configPath
        {
            get
            {
                return Application.dataPath + "/Plugins/FindObjByPath/FindObjByPath.json";
            }
        }

        [NonSerialized]
        public Transform root = null;

        public string savePath = ""; //存储路径
        public string scriptSuffix = "_goc"; //脚本后缀  GameObjectCtrl
        public string classSuffix = "_ic"; //类的后缀
        public string varPrefix = "";//变量前缀
        public string namespaceSuffix = "_icn";//命名空间后缀 ItemCtrlNamespace
        public string addNamespaces = "UnityEngine.UI"; //需要加入的命名空间
        public CreateConfig() { }
        public CreateConfig(Transform root)
        {
            this.root = root;
        }
    }

    public class FindObjScriptsCreate : Editor
    {
        //不可配置
        static private string _tabStr = "    ";

        //可配置
        static private string _scriptsPath = "";
        static private string _scriptSuffix = "";
        static private string _classSuffix = "";
        static private string _varPrefix = "";
        static private string _namespaceSuffix = "";
        static private string _addNamespaces = "";
        public static bool CheckObjIsSignRootObj(FindObjSign[] signs)
        {
            if (signs == null || signs.Length == 0) return false;
            for (int i = 0; i < signs.Length; i++)
            {
                if (signs[i].type == FindObjType.UIRoot) return true;
            }
            return false;
        }

        public static void ReadConfig()
        {
            string configPath = CreateConfig.configPath;
            CreateConfig config = null;
            if (!File.Exists(configPath))
            {
                config = new CreateConfig();
            }
            else
            {
                config = JsonUtility.FromJson<CreateConfig>(File.ReadAllText(configPath));
            }

            _scriptsPath = Application.dataPath + config.savePath;
            _classSuffix = config.classSuffix;
            _varPrefix = config.varPrefix;
            _scriptSuffix = config.scriptSuffix;
            _namespaceSuffix = config.namespaceSuffix;
            _addNamespaces = config.addNamespaces;
        }

        static string GetVarName(string objName)
        {
            objName = Regex.Replace(objName, "[^A-Za-z0-9_]", "");
            return Regex.Replace(objName, "^[A-Z]?", (old) => { return old.Value.ToLower(); }); //让首字母小写
        }

        static string GetFunctionHandName(string varName, string eventName)
        {
            varName = Regex.Replace(varName, "^[a-z]?", (old) => { return old.Value.ToUpper(); });
            eventName = Regex.Replace(eventName, "^[a-z]?", (old) => { return old.Value.ToUpper(); });
            return eventName + varName;
        }

        public static bool CreateScriptsToFile(Transform root)
        {
            if (root == null) return false;

            //插入文件开头部分
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("// -------------");
            sb.AppendLine("//<<< ---  由工具自动生成，请勿修改 --->>>");
            sb.AppendLine(string.Format("// ------------- >>>  Date : {0}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
            sb.AppendLine("using UnityEngine;");
            sb.AppendLine("using System;");
            sb.AppendLine("using FindObjByPath;");
            AddOtherNameSpace(sb);
            sb.AppendLine();
            //插入命名空间
            sb.AppendLine(string.Format("public class {0}{1}", root.name, _namespaceSuffix));
            sb.AppendLine("{");
            CreateFindMultiObjPart(root, sb);
            //插入命名空间 结束符
            sb.AppendLine("}");

            WriteStrToFile(sb.ToString(), _scriptsPath, root.name + _scriptSuffix);

            //刷新资源
            AssetDatabase.Refresh();
            return true;
        }

        static void AddOtherNameSpace(StringBuilder sb)
        {
            if (string.IsNullOrEmpty(_addNamespaces)) return;
            string[] nameSpaces = _addNamespaces.Split(',');
            for (int i = 0; i < nameSpaces.Length; i++)
            {
                sb.AppendLine("using " + nameSpaces[i] + ";");
            }
        }

        static void CreateFindMultiObjPart(Transform root, StringBuilder sb)
        {
            Dictionary<string, Dictionary<string, FindObjSign>> multiObjsMap = new Dictionary<string, Dictionary<string, FindObjSign>>();
            CreateMultiObjsMaps(multiObjsMap, root, true);
            if (multiObjsMap.Count == 0) return;
            foreach (var multiItem in multiObjsMap)
            {
                if (multiItem.Key.Length < 2) continue;
                string className = multiItem.Key.Substring(0, 1).ToUpper() + multiItem.Key.Substring(1) + _classSuffix;
                AddLineCode(sb, 1, string.Format("public class {0} :IFindObjs", className));
                AddLineCode(sb, 1, "{");
                //插入变量
                AddLineCode(sb, 2, " public Transform self = null;");
                Dictionary<string, string> varCache = new Dictionary<string, string>();
                CreateVarPart(sb, multiItem.Value, varCache, "public", false, 2);
                sb.AppendLine();

                //插入无参数的构造函数
                AddLineCode(sb, 2, string.Format("public {0} () {1}", className, "{}"));

                //插入有参数构造函数
                AddLineCode(sb, 2, string.Format("public {0} (Transform transform)", className));
                AddLineCode(sb, 2, "{");
                AddLineCode(sb, 3, "findObjs(transform);");
                AddLineCode(sb, 2, "}");

                //插入寻找物体的部分
                AddLineCode(sb, 2, string.Format("public void findObjs(Transform transform)"));
                AddLineCode(sb, 2, "{");
                AddLineCode(sb, 3, "self = transform;");
                CreateFindObjs(varCache, multiItem.Value, sb, "transform", 3);
                AddLineCode(sb, 2, "}");

                //插入 事件部分
                AddLineCode(sb, 2, string.Format("public void initEvent({0} eventHander)", className));
                AddLineCode(sb, 2, "{");
                CreateInitEvents(varCache, multiItem.Value, sb, 3);
                AddLineCode(sb, 2, "}");

                //插入事件的 hander
                CreateEeventHander(varCache, multiItem.Value, sb, 2);

                AddLineCode(sb, 1, "}");
            }
        }


        //创建变量代码
        static void CreateVarPart(StringBuilder sb, Dictionary<string, FindObjSign> pathToObjDic, Dictionary<string, string> varCache, string type = "protected", bool needPrefix = true, int tablCount = 1)
        {
            if (pathToObjDic == null || pathToObjDic.Count == 0 || varCache == null) return;
            foreach (var item in pathToObjDic)
            {
                string typeName = item.Value.objTypeName;
                if (!string.IsNullOrEmpty(typeName))
                {
                    StringBuilder sb_variable = new StringBuilder();
                    sb_variable.Append(" ");
                    sb_variable.Append(type);
                    sb_variable.Append(" ");
                    sb_variable.Append(typeName);
                    sb_variable.Append(" ");
                    if (needPrefix)
                    {
                        sb_variable.Append(_varPrefix);//变量名前缀
                    }
                    string name = null;
                    if (item.Key.Contains("/"))
                    {
                        int index = item.Key.LastIndexOf("/");
                        name = item.Key.Substring(index + 1);
                    }
                    else
                    {
                        name = item.Key;
                    }
                    name = GetVarName(name);
                    //保存变量名，以path为key
                    if (needPrefix)
                    {
                        varCache.Add(item.Key, _varPrefix + name);
                    }
                    else
                    {
                        varCache.Add(item.Key, name);
                    }
                    sb_variable.Append(name);
                    sb_variable.Append(" = null;");
                    for (int i = 0; i < tablCount; i++)
                    {
                        sb.Append(_tabStr);
                    }
                    sb.AppendLine(sb_variable.ToString());
                }
            }
        }

        //创建寻找物体的代码
        static void CreateFindObjs(Dictionary<string, string> varCahe, Dictionary<string, FindObjSign> pathToObjDic, StringBuilder sb, string parentName = "transform", int tablCount = 2)
        {
            if (pathToObjDic == null || pathToObjDic.Count == 0) return;
            foreach (var item in pathToObjDic)
            {
                for (int i = 0; i < tablCount; i++)
                {
                    sb.Append(_tabStr);
                }
                sb.Append(varCahe[item.Key]);
                sb.Append(" = ");
                sb.Append(parentName);
                sb.Append(".Find(");
                sb.Append("\"");
                sb.Append(item.Key);

                if (item.Value.objTypeName == "GameObject")
                {
                    sb.AppendLine("\").gameObject;");
                    continue;
                }

                if (item.Value.objTypeName == "Transform")
                {
                    sb.AppendLine("\");");
                    continue;
                }

                sb.Append("\").GetComponent<");
                sb.Append(item.Value.objTypeName);
                sb.AppendLine(">();");
            }
        }


        static string _eventCodeString = "{0}.{1}.AddListener(({2})=>{5}eventHander.{3}({4});{6});";
        static string _actionCodeSting = "{0}.{1}+=({2})=>{5}eventHander.{3}({4});{6};";
        static void CreateInitEvents(Dictionary<string, string> varCahe, Dictionary<string, FindObjSign> pathToObjDic, StringBuilder sb, int tablCount = 2)
        {
            if (pathToObjDic == null || pathToObjDic.Count == 0) return;
            foreach (var item in pathToObjDic)
            {
                if (!item.Value.isAddFunction) continue;
                FunctionData[] functionsData = item.Value.functionDatas;
                if (functionsData == null || functionsData.Length == 0) continue;
                for (int i = 0; i < functionsData.Length; i++)
                {
                    FunctionData fd = functionsData[i];
                    string funcCode = fd.functionType == FunctionType.Event ? _eventCodeString : _actionCodeSting;
                    string varName = varCahe[item.Key];
                    string handerName = GetFunctionHandName(varName, fd.functionName);
                    string delegeAgr = FunctionData.GetFunctionCallAgr(fd.functionAgr);
                    string handcallAgr = string.IsNullOrEmpty(delegeAgr) ? varName : varName + "," + delegeAgr;
                    AddLineCode(sb, tablCount, string.Format(funcCode, varName, fd.functionName, delegeAgr, handerName, handcallAgr, "{", "}"));
                }
            }
        }

        static void CreateEeventHander(Dictionary<string, string> varCahe, Dictionary<string, FindObjSign> pathToObjDic, StringBuilder sb, int tablCount = 2)
        {
            if (pathToObjDic == null || pathToObjDic.Count == 0) return;
            foreach (var item in pathToObjDic)
            {
                if (!item.Value.isAddFunction) continue;
                FunctionData[] functionsData = item.Value.functionDatas;
                if (functionsData == null || functionsData.Length == 0) continue;
                for (int i = 0; i < functionsData.Length; i++)
                {
                    FunctionData fd = functionsData[i];
                    string funcCode = "public virtual void {0}({1})";
                    string varName = varCahe[item.Key];
                    string handerName = GetFunctionHandName(varName, fd.functionName);
                    string agr = item.Value.objTypeName + " obj";
                    agr = string.IsNullOrEmpty(fd.functionAgr) ? agr : agr + "," + fd.functionAgr;
                    string codeLine = string.Format(funcCode, handerName, agr) + "{}";
                    AddLineCode(sb, tablCount, codeLine);
                }
            }
        }


        /// <summary>
        /// 根据类别和变量名返回事件函数名
        /// </summary>
        /// <param name="type">类别</param>
        /// <param name="varName">变量名</param>
        static string GetFunctionNameByTypeVarName(Dictionary<FindObjType, string> signTypeToEventName, FindObjType type, string varName)
        {
            if (string.IsNullOrEmpty(varName))
            {
                return null;
            }
            if (!signTypeToEventName.ContainsKey(type))
            {
                return null;
            }
            string eventStr = signTypeToEventName[type];
            //这里的命名规范是On + varName + eventName
            //举例：type = UIType.Button,varName = CloseBtn，结果是OnCloseBtnClick
            return "On" + varName + eventStr.Substring(2);//截掉eventStr开头的on
        }

        static void InsertFunction(StringBuilder sb, string functionName)
        {
            if (sb == null || string.IsNullOrEmpty(functionName))
            {
                return;
            }
            sb.Append(_tabStr);
            sb.Append("private void ");
            sb.Append(functionName);
            sb.AppendLine("()");
            sb.Append(_tabStr);
            sb.AppendLine("{");
            sb.AppendLine();
            sb.Append(_tabStr);
            sb.AppendLine("}");
        }

        static void CreateMultiObjsMaps(Dictionary<string, Dictionary<string, FindObjSign>> multiObjsMap, Transform root, bool isRoot)
        {
            if (root == null || multiObjsMap == null) return;
            bool needContinue = true;
            FindObjSign[] signs = root.GetComponents<FindObjSign>();
            if (signs != null && signs.Length > 0)
            {
                for (int i = 0; i < signs.Length; i++)
                {
                    if (signs[i].type == FindObjType.UIRoot)
                    {
                        if (!isRoot)
                        {
                            needContinue = false;
                            break;
                        }
                    }

                    if (signs[i].type == FindObjType.MultiItem || signs[i].type == FindObjType.UIRoot)
                    {
                        Dictionary<string, FindObjSign> pathToObjsMap = new Dictionary<string, FindObjSign>();
                        GetUITypeSignDic(pathToObjsMap, root, "");
                        if (multiObjsMap.ContainsKey(root.name))
                        {
                            Debug.LogWarning("CreateMultiObjsMaps Have Same Name " + root.name + ",Has been ignored");
                        }
                        else
                        {
                            multiObjsMap.Add(root.name, pathToObjsMap);
                        }
                        break;//避免同一个物体添加了多个 type== UIType.MultiItem 的脚本
                    }
                }
            }

            if (!needContinue) return; //遇到UIRoot则不继续找下去

            if (root.childCount > 0)
            {
                for (int i = 0; i < root.childCount; i++)
                {
                    CreateMultiObjsMaps(multiObjsMap, root.GetChild(i), false);
                }
            }
            return;
        }

        static void GetUITypeSignDic(Dictionary<string, FindObjSign> pathToObjDic, Transform root, string path)
        {
            if (root == null) return;
            bool needContinue = true;
            FindObjSign[] signs = root.GetComponents<FindObjSign>();
            if (signs != null && signs.Length > 0)
            {
                for (int i = 0; i < signs.Length; i++)
                {
                    if (signs[i].type == FindObjType.UIRoot || signs[i].type == FindObjType.MultiItem)
                    {
                        if (!string.IsNullOrEmpty(path)) needContinue = false;
                        continue;
                    }
                    if (string.IsNullOrEmpty(path)) continue;
                    pathToObjDic.Add(path, signs[i]);
                }
            }

            if (!needContinue) return; //遇到UIRoot 与 MultiItem 则不继续找下去

            if (root.childCount > 0)
            {
                for (int i = 0; i < root.childCount; i++)
                {
                    GetUITypeSignDic(pathToObjDic, root.GetChild(i), string.IsNullOrEmpty(path) ? root.GetChild(i).name : path + "/" + root.GetChild(i).name);
                }
            }
            return;
        }

        static void WriteStrToFile(string txt, string path, string fileName)
        {
            if (string.IsNullOrEmpty(txt) || string.IsNullOrEmpty(path))
            {
                return;
            }
            if (path.EndsWith("/")) path = path.Substring(0, path.LastIndexOf("/"));
            path = path.Replace('\\', '/');
            File.WriteAllText(path + "/" + fileName + ".cs", txt, Encoding.UTF8);
        }

        static void AddTableStr(StringBuilder builder, int count)
        {
            for (int i = 0; i < count; i++)
            {
                builder.Append(_tabStr);
            }
        }

        static void AddLineCode(StringBuilder builder, int count, string code)
        {
            AddTableStr(builder, count);
            builder.AppendLine(code);
        }
    }
}
