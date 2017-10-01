using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using xk_System.Db;
using xk_System.Debug;
using UnityEngine.EventSystems;
using xk_System.View;
//using game.protobuf.data;
using XkProtobufData;

namespace xk_System.View.Modules
{
    public class SelectServerView : xk_WindowView
    {
        public SelectServerScrollView mScrollView;
        public Text mServerText;
        public Button mSureGame;

        private LoginMessage mLoginMessage = null;
        private int CurrentSelectServerId = -1;
        protected override void Awake()
        {
            base.Awake();
            mLoginMessage = GetModel<LoginMessage>();
            mServerText.text = string.Empty;
            mSureGame.onClick.AddListener(OnClick_SureGame);
        }

        public void RefreshText(ServerListDB mdata)
        {
            mServerText.text = mdata.id+"区  "+mdata.serverName;
            CurrentSelectServerId = mdata.id;
        }

        private void OnClick_SureGame()
        {
            mLoginMessage.send_SelectServer((uint)CurrentSelectServerId);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            mLoginMessage.mSeletServerResult.addDataBind(GetSelectServerResult);
            RefreshView();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            mLoginMessage.mSeletServerResult.removeDataBind(GetSelectServerResult);
        }

        private void RefreshView()
        {
            List<ServerListDB> mLsit= DbManager.Instance.GetDb<ServerListDB>();
            mScrollView.InitView(mLsit);
            int LastSelectServerId = -1;
            if (mLoginMessage.mLoginResult.bindData.LastSelecServerId == 0)
            {
                LastSelectServerId = 1;
            }else
            {
                LastSelectServerId = (int)mLoginMessage.mLoginResult.bindData.LastSelecServerId;
            }
            ServerListDB mItem = LoginConfig.Instance.FindServerItem(LastSelectServerId);
            RefreshText(mItem);
        }

        private void GetSelectServerResult(scSelectServer mdata)
        {
            if(mdata.Result==1)
            {
                HideView<SelectServerView>();
                if (mdata.RoleList!=null && mdata.RoleList.Count>0)
                {
                    ShowView<RoleSelectView>();
                }else 
                {
                    if (mdata.RoleList == null)
                    {
                        DebugSystem.LogError("roleList is null");
                    }
                    ShowView<RoleCreateView>();
                }
            }
            else
            {
                DebugSystem.LogError("select server return error:"+mdata.Result);
            }
        }
    }
}