using UnityEngine;
using System.Collections;
//using game.protobuf.data;
using XkProtobufData;
using xk_System.Net;
using System.Collections.Generic;

namespace xk_System.Model.Modules
{
    public class ChatItemData
    {
        public int ChannelId;
        public ulong playrId;
        public string name;
        public string content;
        public ulong time;
    }

    public class ChatModel : DataModel
    {
        public List<ChatItemData> mChatDataList = new List<ChatItemData>();
        /// <summary>
        /// 聊天频道ID
        /// </summary>
        public const int channel_Id_System = 1;
        public const int channel_Id_World = 2;
        public const int channel_Id_Guild = 3;
        public const int channel_Id_Team = 4;
        public const int channel_Id_Private = 5;
        public const int channel_Id_Nearby = 6;

        public override void initModel()
        {
            base.initModel();
            ModelSystem.Instance.GetModel<ChatMessage>().mBindServerSendData.addDataBind(GetServerData);
        }

        public override void destroyModel()
        {
            base.destroyModel();
            ModelSystem.Instance.GetModel<ChatMessage>().mBindServerSendData.removeDataBind(GetServerData);
        }

        private void GetServerData(struct_ChatInfo mdata)
        {
            ReceiveData(GetCLientData(mdata));
        }

        private ChatItemData GetCLientData(struct_ChatInfo mdata)
        {
            ChatItemData mClientData = new ChatItemData();
            return mClientData;
        }

        public void ReceiveData(ChatItemData mdata)
        {
            mChatDataList.Insert(0,mdata);
            updateBind("mChatDataList");
        }
    }

   
}