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
            mRegisterBtn.onClick.AddListener(OnClick_Register);
            mReturnLoginBtn.onClick.AddListener(OnClick_ReturnLogin);

            mAccount.text = PlayerPrefs.GetString(CacheManager.cache_key_account, "");
            mPassword.text = PlayerPrefs.GetString(CacheManager.cache_key_password, "");

            mLoginViewObj.SetActive(true);
            mRegisterViewObj.SetActive(false);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            mLoginModel.mRegisterResult.addDataBind(JudgeOrRegisterSuccess);
            mLoginModel.mLoginResult.addDataBind(JudegeOrLoginSuccess);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            mLoginModel.mLoginResult.removeDataBind(JudegeOrLoginSuccess);
            mLoginModel.mRegisterResult.removeDataBind(JudgeOrRegisterSuccess);
        }


        private void OnClick_Login()
        {
            if (string.IsNullOrEmpty(mAccount.text))
            {
                DebugSystem.LogError("账号不能为空");
                return;
            }
            if (string.IsNullOrEmpty(mPassword.text))
            {
                DebugSystem.LogError("密码不能为空");
                return;
            }
            DebugSystem.Log("点击登陆");
            mLoginModel.send_LoginGame(mAccount.text.Trim(), mPassword.text.Trim());
        }

        private void OnClick_ShowRegisterView()
        {
            mLoginViewObj.SetActive(false);
            mRegisterViewObj.SetActive(true);
        }


        private void OnClick_Register()
        {
            if (string.IsNullOrEmpty(mRegisterAccount.text.Trim()))
            {
                DebugSystem.LogError("注冊账号不能为空");
                return;
            }
            if (string.IsNullOrEmpty(mRegisterPassword.text.Trim()))
            {
                DebugSystem.LogError("注冊密码不能为空");
                return;
            }
            if (mRepeatPassword.text.Trim() != mRegisterPassword.text.Trim())
            {
                DebugSystem.LogError("Register Password no Equal");
                return;
            }
            DebugSystem.Log("Click RegisterBtn");
            mLoginModel.Send_RegisterAccount(mRegisterAccount.text, mRegisterPassword.text, mRepeatPassword.text);
        }

        private void OnClick_ReturnLogin()
        {
            mLoginViewObj.SetActive(true);
            mRegisterViewObj.SetActive(false);
        }

        public void JudgeOrRegisterSuccess(scRegisterAccount mdata)
        {
            if (mdata.Result == 1)
            {
                DebugSystem.LogError("Register Success");
                mAccount.text = mRegisterAccount.text;
                mPassword.text = mRegisterPassword.text;
                mLoginViewObj.SetActive(true);
                mRegisterViewObj.SetActive(false);
            }
            else
            {
                DebugSystem.LogError("Register Account Error: " + mdata.Result);
            }
        }

        public void JudegeOrLoginSuccess(scLoginGame mdata)
        {
            if (mdata.Result==1)
            {
                DebugSystem.Log("Login Success");
                ShowView<SelectServerView>();
                HideView<LoginView>();

                PlayerPrefs.SetString(CacheManager.cache_key_account,mAccount.text);
                PlayerPrefs.SetString(CacheManager.cache_key_password,mPassword.text);
            }
            else
            {
                DebugSystem.Log("登陆失败:"+mdata.Result);
            }
        }
    }
}