using UnityEngine;
using xk_System.Debug;
namespace xk_System.AssetPackage
{
    [System.Serializable]
	public class AssetBundlePath : Singleton<AssetBundlePath>
    {
        public const string ABExtention = ".xk_unity3d";
        public const string versionConfigBundleName = "version."+ABExtention;
        public const string versionConfigAssetName = "version.xml";
        public const string AssetDependentFileBundleName = "StreamingAssets";
        public const string AssetDependentFileAssetName = "AssetBundleManifest";

        public readonly string StreamingAssetPathUrl;
        public readonly string ExternalStorePathUrl;
        public readonly string WebServerPathUrl;

        public readonly string ExternalStorePath;
        public AssetBundlePath()
        {
			if (Application.platform == RuntimePlatform.OSXEditor)
			{
				WebServerPathUrl = "file:///F:/WebServer";
				StreamingAssetPathUrl = "file://" + Application.streamingAssetsPath;
				ExternalStorePathUrl = StreamingAssetPathUrl;
				ExternalStorePath = Application.streamingAssetsPath;
			}
			else if (Application.platform == RuntimePlatform.WindowsEditor)
			{
				WebServerPathUrl = "file:///F:/WebServer";
				StreamingAssetPathUrl = "file:///" + Application.streamingAssetsPath;
				ExternalStorePathUrl = StreamingAssetPathUrl;
				//ExternalStorePathUrl = "file:///" + Application.persistentDataPath;
				ExternalStorePath = Application.streamingAssetsPath;
			}
			else if (Application.platform == RuntimePlatform.OSXPlayer)
			{
				WebServerPathUrl = "file:///F:/WebServer";
				StreamingAssetPathUrl = "file:///" + Application.streamingAssetsPath;
				ExternalStorePathUrl = StreamingAssetPathUrl;
				//ExternalStorePathUrl = "file:///" + Application.persistentDataPath;
				ExternalStorePath = Application.streamingAssetsPath;
			}
            else if(Application.platform==RuntimePlatform.WebGLPlayer)
            {
                WebServerPathUrl = "file:///F:/WebServer";
                StreamingAssetPathUrl = "file:///" + Application.streamingAssetsPath;
                ExternalStorePathUrl = StreamingAssetPathUrl;
                ExternalStorePath = Application.streamingAssetsPath;
            }
            else if (Application.platform == RuntimePlatform.WindowsPlayer)
            {
                WebServerPathUrl = "file:///F:/WebServer";
                StreamingAssetPathUrl = "file:///" + Application.streamingAssetsPath;
                ExternalStorePathUrl = "file:///" + Application.persistentDataPath;
                ExternalStorePath = Application.persistentDataPath;
            }
            else if (Application.platform == RuntimePlatform.Android)
            {
                WebServerPathUrl = "file:///F:/WebServer";
                StreamingAssetPathUrl = "jar:file://" + Application.dataPath + "!/assets";
                ExternalStorePathUrl = "file://" + Application.persistentDataPath;
                ExternalStorePath = Application.persistentDataPath;
            }
			else if (Application.platform == RuntimePlatform.IPhonePlayer)
			{
				WebServerPathUrl = "file:///F:/WebServer";
				StreamingAssetPathUrl = "jar:file://" + Application.dataPath + "!/assets";
				ExternalStorePathUrl = "file://" + Application.persistentDataPath;
				ExternalStorePath = Application.persistentDataPath;
			}

            DebugSystem.LogError("www server path: " + WebServerPathUrl);
            DebugSystem.LogError("www local Stream Path: " + StreamingAssetPathUrl);
            DebugSystem.LogError("www local external Path: " + ExternalStorePathUrl);
        }
    }
}