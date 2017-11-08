using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using xk_System.AssetPackage;
using System;

public class AAA : IDisposable
{
	public void UnLoad()
	{
		
	}

	public void Dispose()
	{
		

	}

}

public class AssetBundleTest : MonoBehaviour 
{

	private void Start()
	{
		LoadBundle ("view"+AssetBundlePath.ABExtention);
	}


	public void LoadBundle(string bundleName)
	{
		string path = AssetBundlePath.Instance.ExternalStorePath + "/" + bundleName;
		AssetBundle asset = AssetBundle.LoadFromFile(path);
		var Asset111 = asset.LoadAsset<GameObject> ("LoginView");
		Debug.Log ("Asset111 Name:" + Asset111.name);
		asset.Unload (false);
		if (asset == null) {
			Debug.Log("Bundle is NULL");
		}

		Debug.Log ("Asset111 Name:" + Asset111.name);

		asset = AssetBundle.LoadFromFile(path);
		var Asset222 = asset.LoadAsset<GameObject> ("MainView");
		Debug.Log ("Asset222 Name:" + Asset222.name);
		asset.Unload (false);

		Debug.Log ("Asset111 Name:" + Asset111.name);
		Resources.UnloadUnusedAssets ();
	}
}
