using UnityEngine;
using System.Collections;
using xk_System.AssetPackage;
using xk_System.HotUpdate;
using xk_System.Debug;

public class GameEngine : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(this);
        if (GameConfig.Instance.orUseLog)
        {
            if (GameObject.FindObjectOfType<LogManager>() == null)
            {
                gameObject.AddComponent<LogManager>();
            }
        }
    }

    private void Start()
    {
        StartCoroutine(StartUpdate());
    }

    public IEnumerator StartUpdate()
    {
        yield return AssetBundleHotUpdateManager.Instance.CheckUpdate();
        //yield return AssemblyManager.Instance.LoadAssembly();
        yield return initManager.Instance.StartInitSystem();
    }
}




