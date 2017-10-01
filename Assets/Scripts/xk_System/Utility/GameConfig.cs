using UnityEngine;

public class GameConfig : SingleTonMonoBehaviour<GameConfig>
{
    public bool orUseAssetBundle = true;
    public bool orUseLog = false;
    public bool orUseNet = true;

    public UnityConfig mUnityPlatformConfig;

    private void Awake()
    {
#if !UNITY_EDITOR
        orUseAssetBundle = true;
#endif
    }
}

public class UnityConfig
{
    public LayerManager mLayerManager = new LayerManager();
}

public class LayerManager
{
    public string layer_rubbish = "rubbish";
}
