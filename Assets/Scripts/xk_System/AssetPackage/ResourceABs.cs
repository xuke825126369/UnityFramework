using System.Collections.Generic;
using UnityEngine;
using System.IO;
using xk_System.Debug;

namespace xk_System.AssetPackage
{
    public partial class ResourceABsFolder : Singleton<ResourceABsFolder>
    {
        //bundleName->assetPath
        private Dictionary<string, string> mBundleInfoDic = new Dictionary<string, string>();

        public AssetInfo[] getAsseetInfoList(string BundleName,string[] assetNameList=null)
        {
            List<AssetInfo> mAssetInfoList = new List<AssetInfo>();

            BundleName = CheckBundleName(BundleName);
            if (assetNameList != null)
            {
                foreach (var v in assetNameList)
                {
                    mAssetInfoList.Add(getAsseetInfo(BundleName, v));
                }
            }else
            {

                assetNameList = AssetBundleManager.Instance.getAllBundleAssetInfo(getAsseetInfo(BundleName,""));         
                foreach (var v in assetNameList)
                {
                    mAssetInfoList.Add(getAsseetInfo(BundleName, v));
                }
            }
            return mAssetInfoList.ToArray();
        }

        public AssetInfo getAsseetInfo(string BundleName, string AssetName)
        {
            string bundleName = "";
            string assetName = AssetName;
            string assetPath = "";

            BundleName = CheckBundleName(BundleName);
            bundleName = BundleName;
#if UNITY_EDITOR
            assetPath = mBundleInfoDic[bundleName] + "/" + assetName;
#endif
            return new AssetInfo(assetPath, bundleName, AssetName);
        }

        private string CheckBundleName(string BundleName)
        {
            if (BundleName.Contains("/"))
            {
                int index = BundleName.LastIndexOf("/");
                BundleName = BundleName.Substring(index + 1);
            }

            if (BundleName.Contains("."))
            {
                int index = BundleName.LastIndexOf(".");
                string extention = BundleName.Substring(index);
                if (extention != AssetBundlePath.ABExtention)
                {
                    BundleName = BundleName.Remove(index);
                }
            }

            if (!BundleName.EndsWith(AssetBundlePath.ABExtention))
            {
                BundleName += AssetBundlePath.ABExtention;
            }
            return BundleName;
        }
    }
}