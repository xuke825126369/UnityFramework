using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using UnityEngine;
using xk_System.Debug;
using xk_System.Reflection;
using xk_System.AssetPackage;

namespace xk_System.Db
{
    public class DbManager:Singleton<DbManager>
    {
        private Dictionary<Type, List<DbBase>> mDbDic=new Dictionary<Type, List<DbBase>>();
        public LoadProgressInfo mTask=new LoadProgressInfo();

        public List<T> GetDb<T>() where T : DbBase
        {
            List<T> mDbList = new List<T>();
            if (mDbDic.ContainsKey(typeof(T)))
            {
                List<DbBase> mDb = mDbDic[typeof(T)];
                foreach(T t in mDb)
                {
                    mDbList.Add(t);
                }
            }
            return mDbList;
        }

        public IEnumerator initDbSystem()
        {
            mTask.progress = 0;
			AssetInfo mSheetIfo = ResourceABsFolder.Instance.getAsseetInfo("sheet", "DB");
            yield return AssetBundleManager.Instance.AsyncLoadAsset(mSheetIfo);
            TextAsset mXml = AssetBundleManager.Instance.LoadAsset(mSheetIfo) as TextAsset;
            mTask.progress += 10;
            XmlDocument mXmlDocument = new XmlDocument();
            mXmlDocument.LoadXml(mXml.text);
            foreach (XmlNode xn1 in mXmlDocument.ChildNodes)
            {
                if (xn1.Name == "Client")
                {
                    uint addPro = 0;
                    if (xn1.ChildNodes.Count > 0)
                    {
                        addPro = (uint)Mathf.CeilToInt(90f / xn1.ChildNodes.Count);
                    }else
                    {
                        mTask.progress = 100;
                    }
                    foreach (XmlNode xn2 in xn1.ChildNodes)
                    {
                        foreach (XmlNode xn3 in xn2.ChildNodes)
                        {
                            Dictionary<string, string> mlist = new Dictionary<string, string>();
                            foreach (XmlNode xe in xn3.ChildNodes)
                            {
                                XmlElement xe1 = xe as XmlElement;
                                mlist.Add(xe1.Attributes[0].Name, xe1.Attributes[0].InnerText);
                            }
                            Type mType = System.Type.GetType("xk_System.Db." + xn3.Name);
                            DbBase mSheet = Activator.CreateInstance(mType) as DbBase;
                            mSheet.SetDbValue(mlist);

                            addDb(mType, mSheet);
                        }
                        yield return 0;
                        mTask.progress += addPro;
                    }
                    break;
                }
            }
        }

        private void addDb(Type mType,DbBase mSheet)
        {
            if (!mDbDic.ContainsKey(mType))
            {
                mDbDic.Add(mType, new List<DbBase>());
            }
            mDbDic[mType].Add(mSheet);
        }

        private void OnDestroy()
        {
            mDbDic.Clear();
            mDbDic = null;
        }
    }

    public class DbBase
    {
        /// <summary>
        /// 序号
        /// </summary>
        public readonly int id;
        public void SetDbValue(Dictionary<string, string> list)
        {
            FieldInfo[] mFieldInfo = this.GetType().GetFields();
            for (int i = 0; i < mFieldInfo.Length; i++)
            {
                FieldInfo mField = mFieldInfo[i];
                string key = mField.Name;
                string value = "";
                if (list.ContainsKey(key))
                {
                    value = list[key];
                }else
                {
                    DebugSystem.LogError("配置表 xml与脚本不对应");
                    continue;
                }
                if(string.IsNullOrEmpty(value))
                {
                    continue;
                }
                if (mField.FieldType.IsArray)
                {
                    string[] valueArray = value.Split('#');
                    Array mArray=null;
                    mArray = mField.GetValue(this) as Array;
                    if (mArray == null)
                    {
                        int Length = valueArray.Length;
                        mArray = Array.CreateInstance(mField.FieldType.GetElementType(), Length);
                        for (int j = 0; j < mArray.Length; j++)
                        {
                            mArray.SetValue(ReflectionSystem.Instance.GetFieldValue(mField.FieldType.GetElementType(), valueArray[j]), j);
                        }
                    }
                    for (int j = 0; j < valueArray.Length; j++)
                    {
                        mArray.SetValue(ReflectionSystem.Instance.GetFieldValue(mField.FieldType.GetElementType(), valueArray[j]), j);
                    }
                    mField.SetValue(this, mArray);

                }
                else if (mField.FieldType.IsGenericType)
                {
                    if (mField.FieldType == typeof(List<int>))
                    {
                        List<int> mGentericList = mField.GetValue(this) as List<int>;
                        if (mGentericList == null)
                        {
                            mGentericList = new List<int>();
                        }
                        string[] valueArray2 = value.Split('#');
                        foreach (string s in valueArray2)
                        {
                            if (!string.IsNullOrEmpty(s))
                            {
                                mGentericList.Add((int)ReflectionSystem.Instance.GetFieldValue(typeof(int), s));
                            }
                        }
                        mField.SetValue(this, mGentericList);
                    }
                    else if (mField.FieldType == typeof(List<string>))
                    {
                        List<string> mGentericList = mField.GetValue(this) as List<string>;
                        if (mGentericList == null)
                        {
                            mGentericList = new List<string>();
                        }
                        string[] valueArray2 = value.Split('#');
                        foreach (string s in valueArray2)
                        {
                            if (!string.IsNullOrEmpty(s))
                            {
                                mGentericList.Add(s);
                            }
                        }
                        mField.SetValue(this, mGentericList);
                    }
                    else
                    {
                        DebugSystem.LogError("不能识别的类型");
                    }
                }
                else
                {
                    mField.SetValue(this, ReflectionSystem.Instance.GetFieldValue(mField.FieldType, value));
                }
            }
        }

        public void PrintDbInfo()
        {
            FieldInfo[] mFieldInfo = this.GetType().GetFields();
            DebugSystem.Log(this.GetType().ToString() + ":");
            for (int i = 0; i < mFieldInfo.Length; i++)
            {
                FieldInfo mField = mFieldInfo[i];
                if (mField.FieldType.IsArray)
                {
                    Array mArray = (Array)mField.GetValue(this);
                    for (int j = 0; j < mArray.Length; j++)
                    {
                        DebugSystem.Log(mField.Name+"["+j+"]: "+mArray.GetValue(j));
                    }
                }
                else if (mField.FieldType.IsGenericType)
                {
                    IList mlist= mField.GetValue(this) as IList;
                    for (int j = 0; j < mlist.Count; j++)
                    {
                        DebugSystem.Log(mField.Name + "[" + j + "]: " + mlist[j]);
                    }
                }
                else
                {
                    DebugSystem.Log(mField.Name + ": " + mField.GetValue(this));
                }
            }
        }


    }
}