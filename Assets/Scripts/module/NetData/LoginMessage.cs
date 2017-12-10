using UnityEngine;
using System.Collections;
using xk_System.Model;
using XkProtobufData;
using xk_System.Debug;
using xk_System.Net.Client;
using xk_System.Net;
using Google.Protobuf;
using xk_System.Net.Protocol;

public class LoginMessage : NetModel
{
    public DataBind<scRegisterAccount> mRegisterResult = new DataBind<scRegisterAccount>();
    public DataBind<scLoginGame> mLoginResult = new DataBind<scLoginGame>();
    public DataBind<scSelectServer> mSeletServerResult = new DataBind<scSelectServer>();
    public DataBind<scCreateRole> mCreateRoleResult = new DataBind<scCreateRole>();
    public DataBind<scSelectRole> mSelectRoleResult = new DataBind<scSelectRole>();

	private scRegisterAccount mRegisterAccount = null;

    public override void initModel()
	{
		base.initModel ();
		addNetListenFun (ProtoCommand.ProtoRegisteraccount, Receive_RegisterAccountResult);
		addNetListenFun (ProtoCommand.ProtoLogin, receive_LoginGame);
		addNetListenFun (ProtoCommand.ProtoSelectserver, receive_SelectServer);
		addNetListenFun (ProtoCommand.ProtoCreaterole, receive_CreateRole);
		addNetListenFun (ProtoCommand.ProtoSelectrole, receive_SelectRole);
	}

    public override void destroyModel()
	{
		base.destroyModel ();
		removeNetListenFun (ProtoCommand.ProtoRegisteraccount, Receive_RegisterAccountResult);
		removeNetListenFun (ProtoCommand.ProtoLogin, receive_LoginGame);
		removeNetListenFun (ProtoCommand.ProtoSelectserver, receive_SelectServer);
		removeNetListenFun (ProtoCommand.ProtoCreaterole, receive_CreateRole);
		removeNetListenFun (ProtoCommand.ProtoSelectrole, receive_SelectRole);
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

	private void Receive_RegisterAccountResult(NetPackage package)
    {
		scRegisterAccount mscRegisterAccountdata = Protocol3Utility.getData<scRegisterAccount>(package.buffer);
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

	private void receive_LoginGame(NetPackage mPackage)
    {
		scLoginGame mscLoginGame = Protocol3Utility.getData<scLoginGame>(mPackage.buffer);
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

	private void receive_SelectServer(NetPackage mPackage)
    {
		scSelectServer mdata = Protocol3Utility.getData<scSelectServer>(mPackage.buffer);
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

	private void receive_CreateRole(NetPackage mPackage)
    {
		scCreateRole mdata = Protocol3Utility.getData<scCreateRole>(mPackage.buffer);
        mCreateRoleResult.HandleData(mdata);
    }

    public void send_SelectRole(ulong roleId)
    {
        csSelectRole mdata = new csSelectRole();
        mdata.RoleId = roleId;
        sendNetData(ProtoCommand.ProtoSelectrole, mdata);
    }

	private void receive_SelectRole(NetPackage mPackage)
    {
		scSelectRole mdata = Protocol3Utility.getData<scSelectRole>(mPackage.buffer);
        mSelectRoleResult.HandleData(mdata);
    }
}
