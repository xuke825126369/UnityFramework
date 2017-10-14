using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using xk_System.Debug;

namespace xk_System.AssetPackage
{
    public class AtlasManager : Singleton<AtlasManager>
    {
        private Dictionary<string, Dictionary<string, Sprite>> mAtlasDic = new Dictionary<string, Dictionary<string, Sprite>>();

        public IEnumerator InitAtlas(string atlasName)
		{
			Dictionary<string, Sprite> mDic = null;
			atlasName = ResourceABsFolder.Instance.getBundleNameByAltasName (atlasName);
			if (!mAtlasDic.TryGetValue (atlasName, out mDic)) {
				if (GameConfig.Instance.orUseAssetBundle) {
					yield return AssetBundleManager.Instance.AsyncLoadBundle (atlasName);
				}

				mDic = new Dictionary<string, Sprite> ();
				AssetInfo[] mAllAssetInfo = ResourceABsFolder.Instance.getAsseetInfoList (atlasName);
				if (mAllAssetInfo != null && mAllAssetInfo.Length > 0) {
					foreach (var v in mAllAssetInfo) {
						Sprite mSprite = null;
						if (GameConfig.Instance.orUseAssetBundle) {
							UnityEngine.Object mObj = AssetBundleManager.Instance.LoadAsset (v);
							if (mObj != null) {
								if (mObj is Texture2D) {
									DebugSystem.LogError ("这是一张Texture:" + v.assetPath);
								} else {
									mSprite = mObj as Sprite;
								}
							}
						} else {
#if UNITY_EDITOR
							mSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite> (v.assetPath);
							if (mSprite == null)
							{
								DebugSystem.LogError ("Editor Load Sprite类型不存在:" + v.assetPath);
							}
#endif
						}

						if (mSprite != null) {
							string spriteName = mSprite.name;
							mDic.Add (spriteName, mSprite);
						} else {
							DebugSystem.LogError ("Sprite类型不存在:" + v.assetName);

						}
					}
				}
				mAtlasDic.Add (atlasName, mDic);
			}
		}

        public Dictionary<string, Sprite> GetAtlas(string atlasName)
        {
			atlasName = ResourceABsFolder.Instance.getBundleNameByAltasName (atlasName);
            if (mAtlasDic.ContainsKey(atlasName))
            {
                return mAtlasDic[atlasName];
            }else
            {
                DebugSystem.LogError("此图集不存在：" + atlasName);
                return null;
            }
        }

        public Sprite GetSprite(string atlasName, string spriteName)
        {
            Dictionary<string, Sprite> mDic = GetAtlas(atlasName);
            if (mDic!=null && mDic.ContainsKey(spriteName))
            {
                return mDic[spriteName];
            }
            else
            {
                DebugSystem.LogError("此图片不存在：" + spriteName);
                return null;
            }
        }

    }
}