using UnityEngine;
using System.Collections;
using xk_System.Debug;
using System.Collections.Generic;
using xk_System.AssetPackage;
using System.IO;
using System.Xml;

namespace xk_System.HotUpdate
{
    public class AssetBundleHotUpdateManager : Singleton<AssetBundleHotUpdateManager>
    {
        private int mStreamFolderVersionId=-1;
        private int mExternalStoreVersionId=-1;
        private int mWebServerVersionId=-1;

        private List<AssetBundleInfo> mExternalStoreABInfoList = new List<AssetBundleInfo>();
        private List<AssetBundleInfo> mWebServerABInfoList = new List<AssetBundleInfo>();
        private List<AssetBundleInfo> mStreamFolderABInfoList = new List<AssetBundleInfo>();

        private DownLoadAssetInfo mDownLoadAssetInfo = new DownLoadAssetInfo();

        public LoadProgressInfo mTask = new LoadProgressInfo();

        public IEnumerator CheckUpdate()
        {
            mTask.progress = 0;
            mTask.Des = "正在检查资源";
            yield return CheckVersionConfig();
            if (mDownLoadAssetInfo.mAssetNameList.Count > 0)
            {
                mTask.progress += 10;
                mTask.Des = "正在下载资源";
                yield return DownLoadAllNeedUpdateBundle();
            }
            else
            {
                mTask.progress = 100;
            }
        }

        /// <summary>
        /// 检查版本配置文件
        /// </summary>
        /// <returns></returns>
        private IEnumerator CheckVersionConfig()
        {
            yield return InitLoadExternalStoreVersionConfig();
            yield return InitLoadStreamFolderVersionConfig();
            yield return InitLoadWebServerVersionConfig();

            DebugSystem.Log("本地版本号：" + mExternalStoreVersionId);
            DebugSystem.Log("WebServer版本号：" + mWebServerVersionId);
            DebugSystem.Log("StreamFolder版本号：" + mStreamFolderVersionId);
            if (mWebServerVersionId > mExternalStoreVersionId)
            {
                yield return InitLoadExternalStoreABConfig();
                if (mWebServerVersionId > mStreamFolderVersionId)
                {
                    yield return InitLoadWebServerABConfig();
                    CheckAssetInfo(AssetBundlePath.Instance.WebServerPathUrl, mWebServerABInfoList);
                }
                else
                {
                    yield return InitLoadStreamFolderABConfig();
                    CheckAssetInfo(AssetBundlePath.Instance.StreamingAssetPathUrl, mStreamFolderABInfoList);
                }
            }
            else if (mStreamFolderVersionId > mExternalStoreVersionId)
            {
                yield return InitLoadExternalStoreABConfig();
                yield return InitLoadStreamFolderABConfig();
                CheckAssetInfo(AssetBundlePath.Instance.StreamingAssetPathUrl, mStreamFolderABInfoList);
            }
        }

        /// <summary>
        /// 检查资源配置文件
        /// </summary>
        /// <returns></returns>
        private void CheckAssetInfo(string url, List<AssetBundleInfo> mUpdateABInfoList)
        {
            mDownLoadAssetInfo.url = url;
            foreach (AssetBundleInfo k in mUpdateABInfoList)
            {
                AssetBundleInfo mBundleInfo = mExternalStoreABInfoList.Find((x) =>
                {
                    if (x.mHash.isValid && k.mHash.isValid)
                    {
                        return x.mHash.Equals(k.mHash);
                    }
                    else
                    {
                        DebugSystem.LogError("Hash is no Valid");
                        return false;
                    }
                });
                if (mBundleInfo == null)
                {
                    mDownLoadAssetInfo.mAssetNameList.Add(k.bundleName);
                }
            }
            if (mDownLoadAssetInfo.mAssetNameList.Count > 0)
            {
                mDownLoadAssetInfo.mAssetNameList.Add(AssetBundlePath.AssetDependentFileBundleName);
            }
            DebugSystem.Log("需要下载更新的个数：" + mDownLoadAssetInfo.mAssetNameList.Count);
        }

        private IEnumerator InitLoadWebServerVersionConfig()
        {
            string url = AssetBundlePath.Instance.WebServerPathUrl + "/" + AssetBundlePath.versionConfigBundleName;
            WWW www = new WWW(url);
            yield return www;
            if (www.isDone)
            {
                if (!string.IsNullOrEmpty(www.error))
                {
                    DebugSystem.LogError("www Load Error:" + www.error);
                    www.Dispose();
                    yield break;
                }
            }

            AssetBundle mConfigBundle = www.assetBundle;
            TextAsset mVersionConfig = mConfigBundle.LoadAsset<TextAsset>(AssetBundlePath.versionConfigAssetName);
            mWebServerVersionId = ParseXML(mVersionConfig);
            mConfigBundle.Unload(false);
            www.Dispose();
        }

        private IEnumerator InitLoadExternalStoreVersionConfig()
        {
            string url = AssetBundlePath.Instance.ExternalStorePathUrl + "/" + AssetBundlePath.versionConfigBundleName;
            WWW www = new WWW(url);
            yield return www;
            if (www.isDone)
            {
                if (!string.IsNullOrEmpty(www.error))
                {
                    DebugSystem.LogError("www Load Error:" + www.error);
                    www.Dispose();
                    yield break;
                }
            }
            AssetBundle mConfigBundle = www.assetBundle;
            TextAsset mVersionConfig = mConfigBundle.LoadAsset<TextAsset>(AssetBundlePath.versionConfigAssetName);
            mExternalStoreVersionId = ParseXML(mVersionConfig);
            mConfigBundle.Unload(false);
            www.Dispose();
        }

        private IEnumerator InitLoadStreamFolderVersionConfig()
        {
            string url = AssetBundlePath.Instance.StreamingAssetPathUrl + "/" + AssetBundlePath.versionConfigBundleName;
            WWW www = new WWW(url);
            yield return www;
            if (www.isDone)
            {
                if (!string.IsNullOrEmpty(www.error))
                {
                    DebugSystem.LogError("www Load Error:" + www.error);
                    www.Dispose();
                    yield break;
                }
            }
            AssetBundle mConfigBundle = www.assetBundle;
            TextAsset mVersionConfig = mConfigBundle.LoadAsset<TextAsset>(AssetBundlePath.versionConfigAssetName);
            mStreamFolderVersionId = ParseXML(mVersionConfig);
            mConfigBundle.Unload(false);
            www.Dispose();
        }

        private IEnumerator InitLoadWebServerABConfig()
        {
            string url = AssetBundlePath.Instance.WebServerPathUrl + "/" + AssetBundlePath.AssetDependentFileBundleName;
            WWW www = new WWW(url);
            yield return www;
            if (www.isDone)
            {
                if (!string.IsNullOrEmpty(www.error))
                {
                    DebugSystem.LogError("www Load Error:" + www.error);
                    www.Dispose();
                    yield break;
                }
            }
            AssetBundle mConfigBundle = www.assetBundle;
            AssetBundleManifest mAllBundleMainifest = mConfigBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
            if (mAllBundleMainifest == null)
            {
                DebugSystem.LogError("Mainifest is Null");
                www.Dispose();
                yield break;
            }
            string[] mAssetNames = mAllBundleMainifest.GetAllAssetBundles();
            if (mAssetNames != null)
            {
                foreach (var v in mAssetNames)
                {
                    string bundleName = v;
                    string[] bundleDependentList = mAllBundleMainifest.GetAllDependencies(v);
                    Hash128 mHash = mAllBundleMainifest.GetAssetBundleHash(v);
                    AssetBundleInfo mABInfo = new AssetBundleInfo(bundleName, mHash, bundleDependentList);
                    mWebServerABInfoList.Add(mABInfo);
                }
            }
            else
            {
                DebugSystem.Log("初始化资源依赖文件： Null");
            }
            mConfigBundle.Unload(false);
            www.Dispose();
        }

        private IEnumerator InitLoadExternalStoreABConfig()
        {
            string url = AssetBundlePath.Instance.ExternalStorePathUrl + "/" + AssetBundlePath.AssetDependentFileBundleName;
            WWW www = new WWW(url);
            yield return www;
            if (www.isDone)
            {
                if (!string.IsNullOrEmpty(www.error))
                {
                    DebugSystem.LogError("www Load Error:" + www.error);
                    www.Dispose();
                    yield break;
                }
            }
            AssetBundle mConfigBundle = www.assetBundle;
            AssetBundleManifest mAllBundleMainifest = mConfigBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
            if (mAllBundleMainifest == null)
            {
                DebugSystem.LogError("Mainifest is Null");
                www.Dispose();
                yield break;
            }
            string[] mAssetNames = mAllBundleMainifest.GetAllAssetBundles();
            if (mAssetNames != null)
            {
                foreach (var v in mAssetNames)
                {
                    string bundleName = v;
                    string[] bundleDependentList = mAllBundleMainifest.GetAllDependencies(v);
                    Hash128 mHash = mAllBundleMainifest.GetAssetBundleHash(v);
                    AssetBundleInfo mABInfo = new AssetBundleInfo(bundleName, mHash, bundleDependentList);
                    mExternalStoreABInfoList.Add(mABInfo);
                }
            }
            else
            {
                DebugSystem.Log("初始化资源依赖文件： Null");
            }
            mConfigBundle.Unload(false);
            www.Dispose();
        }

        private IEnumerator InitLoadStreamFolderABConfig()
        {
            string url = AssetBundlePath.Instance.StreamingAssetPathUrl + "/" + AssetBundlePath.AssetDependentFileBundleName;
            WWW www = new WWW(url);
            yield return www;
            if (www.isDone)
            {
                if (!string.IsNullOrEmpty(www.error))
                {
                    DebugSystem.LogError("www Load Error:" + www.error);
                    www.Dispose();
                    yield break;
                }
            }
            AssetBundle mConfigBundle = www.assetBundle;
            AssetBundleManifest mAllBundleMainifest = mConfigBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
            if (mAllBundleMainifest == null)
            {
                DebugSystem.LogError("Mainifest is Null");
                www.Dispose();
                yield break;
            }
            string[] mAssetNames = mAllBundleMainifest.GetAllAssetBundles();
            if (mAssetNames != null)
            {
                foreach (var v in mAssetNames)
                {
                    string bundleName = v;
                    string[] bundleDependentList = mAllBundleMainifest.GetAllDependencies(v);
                    Hash128 mHash = mAllBundleMainifest.GetAssetBundleHash(v);
                    AssetBundleInfo mABInfo = new AssetBundleInfo(bundleName, mHash, bundleDependentList);
                    mStreamFolderABInfoList.Add(mABInfo);
                }
            }
            else
            {
                DebugSystem.Log("初始化资源依赖文件： Null");
            }
            mConfigBundle.Unload(false);
            www.Dispose();
        }

        /// <summary>
        /// 得到版本号
        /// </summary>
        /// <param name="mbytes"></param>
        /// <returns></returns>
        public int ParseXML(TextAsset mTextAsset)
        {
            XmlDocument mdoc = new XmlDocument();
            mdoc.LoadXml(mTextAsset.text);
            foreach (XmlNode v in mdoc.ChildNodes)
            {
                if (v.Name == "root")
                {
                    foreach (XmlNode x in v.ChildNodes)
                    {
                        if (x.Name.Contains("versionId"))
                        {
                            return int.Parse(x.InnerText);
                        }
                    }
                }
            }
            return 0;
        }

        private IEnumerator DownLoadAllNeedUpdateBundle()
        {
            List<string> bundleList = mDownLoadAssetInfo.mAssetNameList;
            List<string>.Enumerator mIter = bundleList.GetEnumerator();

            uint addPro = (uint)Mathf.CeilToInt((LoadProgressInfo.MaxProgress - mTask.progress)/(float)bundleList.Count);
            while (mIter.MoveNext())
            {
                DebugSystem.LogError("下载的文件：" + mDownLoadAssetInfo.url + " | " + mIter.Current);
                yield return DownLoadSingleBundle(mDownLoadAssetInfo.url, mIter.Current);
                mTask.progress+=addPro;
            }
        }

        private IEnumerator DownLoadSingleBundle(string path, string bundleName)
        {
            string url = path + "/" + bundleName;
            WWW www = new WWW(url);
            yield return www;
            if (www.isDone)
            {
                if (!string.IsNullOrEmpty(www.error))
                {
                    DebugSystem.LogError("www Load Error:" + www.error);
                    www.Dispose();
                    yield break;
                }
            }
            string savePath = AssetBundlePath.Instance.ExternalStorePath + "/" + bundleName;
            SaveDownLoadedFile(savePath, www.bytes);
            www.Dispose();
        }

        private void SaveDownLoadedFile(string path, byte[] mdata)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            FileInfo mFileInfo = new FileInfo(path);
            FileStream mFileStream = mFileInfo.OpenWrite();
            mFileStream.Write(mdata, 0, mdata.Length);
            mFileStream.Flush();
            mFileStream.Close();
        }

        private class DownLoadAssetInfo
        {
            public string url;
            public List<string> mAssetNameList = new List<string>();
        }


    }
}