using UnityEngine;
using System.Collections;
using xk_System.Model;
using System.Collections.Generic;
using XkProtobufData;

public class SelectRoleModel : DataModel
{
    public List<struct_PlayerDetailInfo> mHaveRoleList = new List<struct_PlayerDetailInfo>();
    public ulong LastSelectRoleId;
    private LoginMessage mLoginMessage = null;
    public override void initModel()
    {
        base.initModel();
        mLoginMessage = GetModel<LoginMessage>();
        mLoginMessage.mCreateRoleResult.addDataBind(Receive_CreateRoleListInfo);

        scSelectServer mdata = mLoginMessage.mSeletServerResult.bindData;
        mHaveRoleList.AddRange(mdata.RoleList);
        LastSelectRoleId = mdata.LastSelectRoleId;
    }

    public override void destroyModel()
    {
        base.destroyModel();
        mHaveRoleList.Clear();
        mHaveRoleList = null;
        mLoginMessage = null;
        mLoginMessage.mCreateRoleResult.removeDataBind(Receive_CreateRoleListInfo);
    }

    private void Receive_CreateRoleListInfo(scCreateRole mdata)
    {
        if (mdata.Result == 1)
        {
            if (mdata.Role != null)
            {
                mHaveRoleList.Add(mdata.Role);
                updateBind("mHaveRoleList");
            }
        }
    }
}
