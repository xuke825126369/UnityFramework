using UnityEngine;
using System.Collections.Generic;
using System;
using xk_System.Debug;
using System.Collections;
using xk_System.Model;
using xk_System.AssetPackage;
using xk_System.View.Modules;

namespace xk_System.View
{
    /// <summary>
    /// 提供给Unity的View管理器接口
    /// </summary>
    public class WindowManager : SingleTonMonoBehaviour<WindowManager>
    {
        private Dictionary<Type, xk_View> mViewPrefabDic=new Dictionary<Type, xk_View>();

        public IEnumerator InitWindowManager()
        {
            yield return InitAsyncLoadGlobalView<WindowLoadingView>();
        }
        /// <summary>
        /// 唯一对外接口
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        public void ShowView<T>(object data = null) where T : xk_View
        {
            Type mType = typeof(T);
            if (!mViewPrefabDic.ContainsKey(mType))
            {
                StartCoroutine(AsyncLoadView<T>(data));
            }
            else
            {
                mViewPrefabDic[mType].transform.SetAsLastSibling();
                if (!mViewPrefabDic[mType].gameObject.activeSelf)
                {
                    if (mViewPrefabDic[mType] is xk_DialogView && data != null)
                    {
                        xk_DialogView mView = (xk_DialogView)mViewPrefabDic[mType];
                        mView.InitView(data);
                    }
                    mViewPrefabDic[mType].gameObject.SetActive(true);
                    DebugSystem.Log("显示界面：" + mType.Name);
                }
            }
        }

        public T GetView<T>() where T : xk_View
        {
            Type mType = typeof(T);
            if (mViewPrefabDic.ContainsKey(mType))
            {
                T view = mViewPrefabDic[mType] as T;
                return view;
            }

            return null;
        }

        public void HideView<T>(bool orDestroy = false) where T : xk_View
        {
            Type mType = typeof(T);
            if (mViewPrefabDic.ContainsKey(mType))
            {
                mViewPrefabDic[mType].gameObject.SetActive(false);
                if (orDestroy)
                {
                    RemoveView<T>();
                }
            }
        }

        private IEnumerator InitAsyncLoadGlobalView<T>(object data = null) where T : xk_View
        {
            AssetInfo mAssetInfo = ViewCollection.Instance.GetViewAssetInfo<T>();
            yield return AssetBundleManager.Instance.AsyncLoadAsset(mAssetInfo);
            GameObject viewPrefab = AssetBundleManager.Instance.LoadAsset(mAssetInfo) as GameObject;
            if (viewPrefab == null)
            {
                DebugSystem.LogError("没有找到资源:" + mAssetInfo.assetName);
                yield break;
            }
            
            yield return AddView<T>(viewPrefab);
            HideView<T>();
        }

        private IEnumerator AsyncLoadView<T>(object data = null) where T : xk_View
        {
            ShowView<WindowLoadingView>();
            AssetInfo mAssetInfo=ViewCollection.Instance.GetViewAssetInfo<T>();           
            yield return AssetBundleManager.Instance.AsyncLoadAsset(mAssetInfo);
            GameObject viewPrefab = AssetBundleManager.Instance.LoadAsset(mAssetInfo) as GameObject;
            if (viewPrefab == null)
            {
                DebugSystem.LogError("没有找到资源:" + mAssetInfo.assetName);
                yield break;
            }
            yield return AddView<T>(viewPrefab);
            ShowView<T>(data);
            HideView<WindowLoadingView>();            
        }

        private IEnumerator AddView<T>(GameObject obj) where T : xk_View
        {
            Type mType = typeof(T);
            T mView = obj.GetComponent(mType) as T;
            if (mView == null)
            {
                mView = obj.AddComponent<T>();
            }
            yield return mView.PrepareResource();
            mView.addLayer();            
            if (!mViewPrefabDic.ContainsKey(mType))
            {
                mViewPrefabDic.Add(mType, mView);
            }
            DebugSystem.Log("Load xk_View Component： " + obj.name);
        }

        private void RemoveView<T>() where T : xk_View
        {
            Type mView = typeof(T);
            if (mViewPrefabDic.ContainsKey(mView))
            {
                Destroy(mViewPrefabDic[mView].gameObject);
                mViewPrefabDic.Remove(mView);
            }
        }

        public void CleanManager()
        {
            List<Type> mRemoveTypeList = new List<Type>();
            foreach(var v in mViewPrefabDic)
            {
                if(!(v.Value is xk_GlobalView))
                {
                    mRemoveTypeList.Add(v.Key);
                    Destroy(v.Value.gameObject);
                }
            }
            foreach(var v in mRemoveTypeList)
            {
                mViewPrefabDic.Remove(v);
            }
        }
    }

	public class ViewCollection:Singleton<ViewCollection>
	{
		public AssetInfo GetViewAssetInfo<T>() where T : xk_View
		{
			switch (typeof(T).Name)
			{
			case "ChatView":
				return ResourceABsFolder.Instance.getAsseetInfo("view", "ChatView");
			case "LoginView":
				return ResourceABsFolder.Instance.getAsseetInfo("view", "LoginView");
			case "MainView":
				return ResourceABsFolder.Instance.getAsseetInfo("view", "MainView");
			case "SelectServerView":
				return ResourceABsFolder.Instance.getAsseetInfo("view", "SelectServerView");
			case "ShareView":
				return ResourceABsFolder.Instance.getAsseetInfo("view", "ShareView");
			case "WindowLoadingView":
				return ResourceABsFolder.Instance.getAsseetInfo("view", "WindowLoadingView");
			case "MsgBoxView":
				return ResourceABsFolder.Instance.getAsseetInfo("view", "MsgBoxView");
			case "SceneLoadingView":
				return ResourceABsFolder.Instance.getAsseetInfo("view", "SceneLoadingView");
			case "StoreView":
				return ResourceABsFolder.Instance.getAsseetInfo("view", "StoreView");
			case "RoleCreateView":
				return ResourceABsFolder.Instance.getAsseetInfo("view", "RoleCreateView");
			case "RoleSelectView":
				return ResourceABsFolder.Instance.getAsseetInfo("view", "RoleSelectView");
			}
			DebugSystem.LogError("没有找到资源信息");
			return null;
		}
	}

    public abstract class xk_Object:MonoBehaviour
    {
        protected virtual void Awake()
        {

        }

        protected virtual void OnEnable()
        {
            
        }

        protected virtual void Start()
        {

        }

        protected virtual void OnDisable()
        {
            
        }

        protected virtual void OnDestroy()
        {

        }
    }


    public  abstract  class xk_View : xk_Object
    {
        protected override void Awake()
        {
            
        }

        protected override void OnEnable()
        {
            
        }

        protected override void Start()
        {

        }

        protected override void OnDisable()
        {
            
        }

        protected override void OnDestroy()
        {
            
        }

        public virtual IEnumerator PrepareResource()
        {
            DebugSystem.Log("准备加载xk_View资源");
            yield return 0;
        }

        public void addLayer()
        {
            SetViewParent();
            SetViewTransform();
        }

        protected virtual void SetViewParent()
        {
            this.transform.SetParent(ObjectRoot.Instance.ui_2d_root.mParent);
        }

        protected virtual void SetViewTransform()
        {
            transform.localScale = Vector3.one;
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            RectTransform mRT = transform.GetComponent<RectTransform>();
            if (mRT == null)
            {
                mRT = gameObject.AddComponent<RectTransform>();
            }
            if (mRT != null)
            {
                mRT.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, 0);
                mRT.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, 0, 0);
                mRT.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Bottom, 0, 0);
                mRT.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, 0);
                mRT.anchorMin = new Vector2(0, 0);
                mRT.anchorMax = new Vector2(1, 1);
            }
        }

        public T GetModel<T>() where T : xk_Model, new()
        {
            return ModelSystem.Instance.GetModel<T>();
        }

        public T GetView<T>() where T : xk_View
        {
            return WindowManager.Instance.GetView<T>();
        }

        public void ShowView<T>(object data=null) where T:xk_View
        {
            WindowManager.Instance.ShowView<T>(data);
        }       

        public void HideView<T>(bool orDestroy=false) where T :xk_View
        {
            WindowManager.Instance.HideView<T>(orDestroy);
        }
    }

    public abstract class xk_WindowView:xk_View
    {

    }

    public abstract class xk_DialogView:xk_View
    {
        protected object data = null;

        public virtual void InitView(object data = null)
        {
            if (data != null)
            {
                this.data = data;
            }
        }
    }

    public abstract class xk_GlobalView:xk_View
    {
        

    }


}