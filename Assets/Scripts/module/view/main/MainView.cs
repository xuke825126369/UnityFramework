using UnityEngine;
using System.Collections;
using xk_System.View;
using xk_System.Debug;
using UnityEngine.UI;
using xk_System.AssetPackage;


namespace xk_System.View.Modules
{
    public class MainView :xk_WindowView
    {
        public Button mShareBtn;
        public Button mChatBtn;
        public Button mStoreBtn;

        protected override void Awake()
        {
            base.Awake();
            mShareBtn.onClick.AddListener(OnClick_Share);
            mChatBtn.onClick.AddListener(OnClick_Chat);
            mStoreBtn.onClick.AddListener(OnClick_Store);
        }

		public override IEnumerator PrepareResource ()
		{
			yield return AtlasManager.Instance.InitAtlas ("atlas_emoj");
		}

        private void OnClick_Share()
        {
            //ShowView<ShareView>();
        }

        private void OnClick_Chat()
        {
            ShowView<ChatView>();
        }

        private void OnClick_Store()
        {
            ShowView<StoreView>();
        }

    }
}