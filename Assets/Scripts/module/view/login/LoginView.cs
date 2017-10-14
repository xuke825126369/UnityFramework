using UnityEngine;
using UnityEngine.UI;
using System.Collections;

using xk_System.Model;
using xk_System.Model.Modules;
using xk_System.Debug;
//using game.protobuf.data;
using XkProtobufData;

namespace xk_System.View.Modules
{
    public class LoginView : xk_WindowView
    {
        public GameObject mLoginViewObj;
        public GameObject mRegisterViewObj;

        public InputField mAccount;
        public InputField mPassword;

        public InputField mRegisterAccount;
        public InputField mRegisterPassword;
        public InputField mRepeatPassword;

        public Button mLoginBtn;
        public Button mShowRegtisterViewBtn;

        public Button mReturnLoginBtn;
        public Button mRegisterBtn;

        private LoginMessage mLoginModel = null;

        protected override void Awake()
        {
            base.Awake();
            mLoginModel = GetModel<LoginMessage>();

            mLoginBtn.onClick.AddListener(OnClick_Login);
            mShowRegtisterViewBtn.onClick.AddListener(OnClick_ShowRegisterView);

            mAccount.text = PlayerPrefs.GetString(CacheManager.cache_key_account, "");
            mPassword.text = PlayerPrefs.GetString(CacheManager.cache_key_password, "");

            mLoginViewObj.SetActive(true);
            mRegisterViewObj.SetActive(false);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
        }


        private void OnClick_Login()
        {
            DebugSystem.Log("点击登陆");
			ShowView<MainView>();
			HideView<LoginView>();
        }

        private void OnClick_ShowRegisterView()
        {
            mLoginViewObj.SetActive(false);
            mRegisterViewObj.SetActive(true);
        }
    }
}