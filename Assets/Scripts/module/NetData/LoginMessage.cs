using UnityEngine;
using System.Collections;
using xk_System.Model;
//using game.protobuf.data;
using XkProtobufData;
using xk_System.Debug;
using xk_System.Net.Client;
using xk_System.Net;

public class LoginMessage : NetModel
{
    public DataBind<scRegisterAccount> mRegisterResult = new DataBind<scRegisterAccount>();
    public DataBind<scLoginGame> mLoginResult = new DataBind<scLoginGame>();
    public DataBind<scSelectServer> mSeletServerResult = new DataBind<scSelectServer>();
    public DataBind<scCreateRole> mCreateRoleResult = new DataBind<scCreateRole>();
    public DataBind<scSelectRole> mSelectRoleResult = new DataBind<scSelectRole>();

    public override void initModel()
    {
        base.initModel();
        addNetListenFun(ProtoCommand.ProtoRegisteraccount, Receive_RegisterAccountResult);
        addNetListenFun(ProtoCommand.ProtoLogin, receive_LoginGame);
        addNetListenFun(ProtoCommand.ProtoSelectserver, receive_SelectServer);
        addNetListenFun(ProtoCommand.ProtoCreaterole, receive_CreateRole);
        addNetListenFun(ProtoCommand.ProtoSelectrole, receive_SelectRole);


    }

    public override void destroyModel()
    {
        base.destroyModel();
        removeNetListenFun(ProtoCommand.ProtoRegisteraccount, Receive_RegisterAccountResult);
        removeNetListenFun(ProtoCommand.ProtoLogin, receive_LoginGame);
        removeNetListenFun(ProtoCommand.ProtoSelectserver, receive_SelectServer);
        removeNetListenFun(ProtoCommand.ProtoCreaterole, receive_CreateRole);
        removeNetListenFun(ProtoCommand.ProtoSelectrole, receive_SelectRole);
    }
    /// <summary>
    /// 发送账户注册信息
    /// </summary>
    /// <param name="aN"></param>
    /// <param name="ps"></param>
    /// <param name="reps"></param>
    public void Send_RegisterAccount(string aN, string ps, string reps)
    {
        csRegisterAccount mdata = new csRegisterAccount();
        mdata.AccountName = aN;
        mdata.Password = ps;
        mdata.RepeatPassword = reps;
        sendNetData(ProtoCommand.ProtoRegisteraccount, mdata);
    }

    private void Receive_RegisterAccountResult(Package mProtobuf)
    {
        scRegisterAccount mscRegisterAccountdata=mProtobuf.getData<scRegisterAccount>();
        mRegisterResult.HandleData(mscRegisterAccountdata);
    }
    /// <summary>
    /// 发送账户登录信息
    /// </summary>
    /// <param name="ac"></param>
    /// <param name="ps"></param>
    public void send_LoginGame(string ac, string ps)
    {
        csLoginGame mdata = new csLoginGame();
        mdata.AccountName= ac;
        mdata.Password = ps;
        sendNetData(ProtoCommand.ProtoLogin, mdata);
    }

    private void receive_LoginGame(Package mPackage)
    {
        scLoginGame mscLoginGame=mPackage.getData<scLoginGame>();
        mLoginResult.HandleData(mscLoginGame);
    }

    /// <summary>
    /// 发送选择大区信息
    /// </summary>
    /// <param name="serverId"></param>
    public void send_SelectServer(uint serverId)
    {
        csSelectServer mdata = new csSelectServer();
        mdata.Id = serverId;
        sendNetData(ProtoCommand.ProtoSelectserver, mdata);
    }

    private void receive_SelectServer(Package mProtobuf)
    {
        scSelectServer mdata=mProtobuf.getData<scSelectServer>();
        mSeletServerResult.HandleData(mdata);
    }

    public void send_CreateRole(string name,uint sex,uint profession)
    {
        csCreateRole mdata = new csCreateRole();
        mdata.Name= name;
        mdata.Sex = sex;
        mdata.Profession = profession;
        sendNetData(ProtoCommand.ProtoCreaterole, mdata);
    }

    private void receive_CreateRole(Package mPackage)
    {
        scCreateRole mdata = mPackage.getData<scCreateRole>();
        mCreateRoleResult.HandleData(mdata);
    }

    public void send_SelectRole(ulong roleId)
    {
        csSelectRole mdata = new csSelectRole();
        mdata.RoleId = roleId;
        sendNetData(ProtoCommand.ProtoSelectrole, mdata);
    }

    private void receive_SelectRole(Package mPackage)
    {
        scSelectRole mdata = mPackage.getData<scSelectRole>();
        mSelectRoleResult.HandleData(mdata);
    }

}
