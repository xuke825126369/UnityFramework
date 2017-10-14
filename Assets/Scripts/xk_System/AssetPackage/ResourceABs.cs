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

			BundleName = CheckBundleName (BundleName);
			bundleName = BundleName;

			if (!GameConfig.Instance.orUseAssetBundle) {
				if (mBundleInfoDic.ContainsKey (bundleName)) {
					if (!assetName.StartsWith (mBundleInfoDic [bundleName])) {
						assetPath = mBundleInfoDic [bundleName] + "/" + assetName;
					} else {
						assetPath = assetName;
					}
				} else {
					DebugSystem.LogError ("读取本地资源 有误：" + bundleName);
				}
			}
			return new AssetInfo(assetPath, bundleName, assetName);
        }

		//得到Atlas的Bundle名
		public string getBundleNameByAltasName(string AtlasName)
		{
			string bundleName = AtlasName;
			if (!bundleName.StartsWith ("atlas_")) {
				bundleName = "atlas_" + bundleName;
			}

			return getRealBundleName (bundleName);
		}

		public string getRealBundleName(string bundleName)
		{
			bundleName = CheckBundleName (bundleName);
			return bundleName;
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