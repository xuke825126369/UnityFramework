using UnityEngine;
using System.Collections;
using UnityEngine.UI;
//using game.protobuf.data;
using XkProtobufData;
using xk_System.Debug;
using xk_System.Model;
using System.Collections.Generic;

namespace xk_System.View.Modules
{
    public class RoleSelectView : xk_WindowView
    {
        public Button mEnterGameBtn;
        public Button mGoCreateRoleBtn;

        public Transform mHero2DParent;
        public SelectHeroItem mHero2DItemPrefab;

        private ulong CurrentSelectRoleId = 0;
        private LoginMessage mLoginMessage=null;
        private SelectRoleModel mSelectRoleModel = null;
        protected override void Awake()
        {
            base.Awake();
            mLoginMessage = GetModel<LoginMessage>();
            mSelectRoleModel = GetModel<SelectRoleModel>();
            mEnterGameBtn.onClick.AddListener(Click_EnterGameBtn);
            mGoCreateRoleBtn.onClick.AddListener(Click_CreateRoleBtn);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            mLoginMessage.mSelectRoleResult.addDataBind(GetSelectRoleResult);
            mSelectRoleModel.addDataBind(RefreshView, "mHaveRoleList");
            RefreshView();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            mLoginMessage.mSelectRoleResult.removeDataBind(GetSelectRoleResult);
            mSelectRoleModel.removeDataBind(RefreshView, "mHaveRoleList");
        }

        private void Click_EnterGameBtn()
        {
            if (CurrentSelectRoleId > 0)
            {
                mLoginMessage.send_SelectRole(CurrentSelectRoleId);
            }else
            {
                DebugSystem.LogError("role id is zero");
            }
        }

        private void Click_CreateRoleBtn()
        {
            HideView<RoleSelectView>();
            ShowView<RoleCreateView>();
        }

        private void GetSelectRoleResult(scSelectRole mdata)
        {
            if(mdata.Result==1)
            {
                DebugSystem.Log("进入游戏");
                SceneSystem.Instance.GoToScene(SceneInfo.Scene_2);
            }else
            {
                DebugSystem.LogError("Select Role result is Error:"+mdata.Result);
            }
        }

        private void RefreshView(object data=null)
        {
            List<struct_PlayerDetailInfo> mPlayerList = mSelectRoleModel.mHaveRoleList;
            for(int i=0;i<mPlayerList.Count;i++)
            {
                if(i>=mHero2DParent.childCount)
                {
                    GameObject obj = Instantiate<GameObject>(mHero2DItemPrefab.gameObject);
                    obj.transform.SetParent(mHero2DParent);
                    obj.transform.localPosition = Vector3.zero;
                    obj.transform.localScale = Vector3.one;
                }
                SelectHeroItem mItem = mHero2DParent.GetChild(i).GetComponent<SelectHeroItem>();
                mItem.RefreshItem(mPlayerList[i]);
                mItem.gameObject.SetActive(true);
            }

            for(int i=mPlayerList.Count;i<mHero2DParent.childCount;i++)
            {
                mHero2DParent.GetChild(i).gameObject.SetActive(false);
            }

            if(mSelectRoleModel.LastSelectRoleId==0 && mPlayerList.Count>0)
            {
                CurrentSelectRoleId = mPlayerList[0].Id;
            }
        }
    }
}