using UnityEngine;
using System.Collections;
using System.Reflection;
using xk_System.Debug;
using System;

namespace xk_System.AssetPackage
{
    public class AssemblyManager : Singleton<AssemblyManager>
    {
        private Assembly mHotUpdateAssembly;
        private Assembly mCurrentAssembly;
        /// <summary>
        /// 加载程序集
        /// </summary>
        /// <returns></returns>
        public IEnumerator LoadAssembly()
        {
            AssetInfo mAssetInfo = ResourceABsFolder.Instance.getAsseetInfo("scripts", "test");
            string path = AssetBundlePath.Instance.ExternalStorePathUrl;

            string bundleName1 = mAssetInfo.bundleName;
            string url = path + "/" + bundleName1;
            WWW www = new WWW(url);
            yield return www;
            if (www.isDone)
            {
                if (!string.IsNullOrEmpty(www.error))
                {
                    DebugSystem.LogError("www Load Error:" + www.error);
                    yield break;
                }
            }
            AssetBundle mConfigBundle = www.assetBundle;
            TextAsset mAsset = mConfigBundle.LoadAsset<TextAsset>(mAssetInfo.assetName);
            mHotUpdateAssembly = Assembly.Load(mAsset.bytes);
            if (mHotUpdateAssembly != null)
            {
                DebugSystem.Log("加载程序集：" + mHotUpdateAssembly.FullName);
            }
            else
            {
                DebugSystem.Log("加载程序集： null");
            }
            mCurrentAssembly = this.GetType().Assembly;
            DebugSystem.Log("当前程序集：" + mCurrentAssembly.FullName);
            if (mCurrentAssembly.FullName.Equals(mHotUpdateAssembly.FullName))
            {
                DebugSystem.LogError("加载程序集名字有误");
            }
            mConfigBundle.Unload(false);
        }

        public object CreateInstance(string typeFullName)
        {
            if (mHotUpdateAssembly != null)
            {
                return mHotUpdateAssembly.CreateInstance(typeFullName);
            }
            else
            {
               return mCurrentAssembly.CreateInstance(typeFullName);
            }
        }

        /// <summary>
        /// 仅仅写入口时，调用。（否则，会使程序变得混乱，反正我是搞乱了）
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="typeFullName"></param>
        /// <returns></returns>
        public Component AddComponent(GameObject obj, string typeFullName)
        {
            DebugSystem.Log("Type: " + typeFullName);
            if (mHotUpdateAssembly != null)
            {
                Type mType = mHotUpdateAssembly.GetType(typeFullName);
                return obj.AddComponent(mType);
            }
            else
            {
                Type mType = typeFullName.GetType();
                return obj.AddComponent(mType);
            }
        }

       

    }
}