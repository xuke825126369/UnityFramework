using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using xk_System.AssetPackage;

public class AudioManager : SingleTonMonoBehaviour<AudioManager>
{
    private Queue<AudioSource> mAudioSoureList = new Queue<AudioSource>();
    public override void Init()
    {
        base.Init();
        gameObject.AddComponent<AudioListener>();
        StartCoroutine(LoadAudio());
    }

    private IEnumerator LoadAudio()
    {
		AssetInfo mAssetInfo = ResourceABsFolder.Instance.getAsseetInfo("","") ;
        yield return StartCoroutine(AssetBundleManager.Instance.AsyncLoadAsset(mAssetInfo));
        AudioClip obj = AssetBundleManager.Instance.LoadAsset(mAssetInfo) as AudioClip;
        Play(obj);
    }

    public void Play(AudioClip mClip)
    {
        AudioSource mSource = null;
        if (mAudioSoureList.Count > 0)
        {
            mSource = mAudioSoureList.Dequeue();
        }
        else
        {
            mSource = gameObject.AddComponent<AudioSource>();
            mAudioSoureList.Enqueue(mSource);
        }
        mSource.Stop();
        mSource.clip = mClip;
        mSource.loop = false;
        mSource.volume = 1f;
        mSource.mute = false;
        mSource.Play();
    }



}
