using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using xk_System.Debug;
//using game.protobuf.data;
using XkProtobufData;

namespace xk_System.View.Modules
{
    public class RoleCreateView : xk_WindowView
    {
        public InputField mName;
        public Toggle mSex_Boy;
        public Toggle mSex_Girl;

        public Toggle mProfression_warrior;
        public Toggle mProgression_enchanter;
        public Toggle mProgression_troistpriest;

        public Button mRandomNameBtn;
        public Button mCreateRoleBtn;
        public Button mGoBackBtn;


        private int CurrentSexId = -1;
        private int CurrentProfessionId = -1;
        private string CurrentName = string.Empty;

        private LoginMessage mLoginMessage=null;
        protected override void Awake()
        {
            base.Awake();
            mLoginMessage = GetModel<LoginMessage>();
            mSex_Boy.onValueChanged.AddListener(OnValueChange_Sex);
            mSex_Girl.onValueChanged.AddListener(OnValueChange_Sex);
            mProfression_warrior.onValueChanged.AddListener(OnValueChange_Profession);
            mProgression_enchanter.onValueChanged.AddListener(OnValueChange_Profession);
            mProgression_troistpriest.onValueChanged.AddListener(OnValueChange_Profession);
            mName.onValueChanged.AddListener(OnValueChanged_Name);
            mRandomNameBtn.onClick.AddListener(Click_RandomName);
            mCreateRoleBtn.onClick.AddListener(Click_CreateRole);
            mGoBackBtn.onClick.AddListener(Click_GoBackBtn);
        }

        private void Click_GoBackBtn()
        {
            HideView<RoleCreateView>();
            ShowView<RoleSelectView>();
        }

        private void Click_RandomName()
        {

        }

        private void Click_CreateRole()
        {
            if(string.IsNullOrEmpty(CurrentName))
            {
                DebugSystem.LogError("创建名不能为空");
                return;
            }
            if(CurrentSexId<=0)
            {
                DebugSystem.LogError("请选择角色性别");
                return;
            }
            if(CurrentProfessionId<=0)
            {
                DebugSystem.LogError("请选择角色职业");
                return;
            }
            mLoginMessage.send_CreateRole(CurrentName,(uint)CurrentSexId,(uint)CurrentProfessionId);
        }

        private void OnValueChange_Sex(bool orValueChanged)
        {
            if(orValueChanged==true)
            {
                GameObject obj= EventSystem.current.currentSelectedGameObject;
                DebugSystem.Log("当前Toggle："+obj.name);
                if(obj==mSex_Boy.gameObject)
                {
                    CurrentSexId = 1;
                }else
                {
                    CurrentSexId = 2;
                }
            }
        }
        private void OnValueChange_Profession(bool orValueChanged)
        {
            if (orValueChanged == true)
            {
                GameObject obj = EventSystem.current.currentSelectedGameObject;
                DebugSystem.Log("当前Toggle：" + obj.name);
                if (obj == mProfression_warrior.gameObject)
                {
                    CurrentProfessionId = 1;
                }else if(obj==mProgression_enchanter.gameObject)
                {
                    CurrentProfessionId = 2;
                }else if(obj==mProgression_troistpriest.gameObject)
                {
                    CurrentProfessionId = 3;
                }
            }
        }

        private void OnValueChanged_Name(string text)
        {
            CurrentName = text.Trim();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            mLoginMessage.mCreateRoleResult.addDataBind(Receive_CreateRoleResult);

            mSex_Boy.isOn = true;
            CurrentSexId = 1;
            mProfression_warrior.isOn = true;
            CurrentProfessionId = 1;
            mName.text = string.Empty;
            CurrentName = string.Empty;
        }

        private void Receive_CreateRoleResult(scCreateRole mdata)
        {
            if(mdata.Result==1)
            {
                DebugSystem.Log("创建角色成功");
            }else
            {
                DebugSystem.Log("创建角色失败："+mdata.Result);
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            mLoginMessage.mCreateRoleResult.removeDataBind(Receive_CreateRoleResult);
        }
    }
}