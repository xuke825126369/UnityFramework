using xk_System.Model;
using XkProtobufData;
using xk_System.Net.Client;
using xk_System.Debug;
using xk_System.Net;
using Google.Protobuf;
using xk_System.Net.Protocol;

public class ChatMessage : NetModel
{
	public DataBind<struct_ChatInfo> mBindServerSendData = new DataBind<struct_ChatInfo> ();
    public override void initModel()
    {
        base.initModel();
        addNetListenFun(ProtoCommand.ProtoChat, Receive_ServerSenddata);
        addNetListenFun(ProtoCommand.ProtoPushChatinfo, Receive_Push_ChatInfo);
    }

    public override void destroyModel()
	{
		base.destroyModel ();
		removeNetListenFun (ProtoCommand.ProtoChat, Receive_ServerSenddata);
		removeNetListenFun (ProtoCommand.ProtoPushChatinfo, Receive_Push_ChatInfo);
	}

    public void request_ClientSendData(uint channelId, string sendName, string content)
    {
        csChatData mClientSendData = new csChatData();
        mClientSendData.ChannelId = channelId;
        mClientSendData.TalkMsg = content;
        sendNetData(ProtoCommand.ProtoChat, mClientSendData);
    }

	private void Receive_ServerSenddata(NetPackage mPackage)
    {
		scChatData mServerSendData = Protocol3Utility.getData<scChatData> (mPackage.buffer);
    }

	private void Receive_Push_ChatInfo(NetPackage mPackage)
	{
		pushChatInfo mPushChatInfo = Protocol3Utility.getData<pushChatInfo> (mPackage.buffer);
	}
}
