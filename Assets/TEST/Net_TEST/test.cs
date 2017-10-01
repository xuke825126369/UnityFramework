using UnityEngine;
using game.protobuf.data;
using xk_System.Debug;
using xk_System.Net.Client;
using System.Collections;
using xk_System.Net;

namespace Test
{
   public class test : MonoBehaviour
    {
        private static int ID = 0;
        NetSystem mNetSystem;
        void Start()
        {
			mNetSystem = new NetSystem();
            mNetSystem.init("192.168.1.109",7878);
           // mNetSystem.addListenFun((int)ProtoCommand.PROTO_CHAT, ReceiveFun);
            ID++;

            StartCoroutine(Run());
        }

        IEnumerator Run()
        {           
            while (true)
            {
                for (int i = 0; i < 10; i++)
                {
                    //SendFun();
                }
                yield return new WaitForSeconds(2f);
                //break;
            }
        }

        void Update()
        {
            mNetSystem.ReceiveData();
        }

        /*void SendFun()
        {
            ClientSendData mClient = new ClientSendData();
            mClient.SenderName = "client" + ID;
            mClient.TalkMsg = "hellosssssssssssssssssssssssssssssssssssssssss";
            mNetSystem.SendData((int)ProtoCommand.Chat, mClient);
        }

        private ServerSendData mdata = new ServerSendData();
        void ReceiveFun(Package package)
        {
            package.getData<ServerSendData>(mdata);
            DebugSystem.Log("发送者：" + mdata.NickName);
            DebugSystem.Log("接收信息：" + mdata.TalkMsg);
        }
        void OnDestroy()
        {
            StopAllCoroutines();
            mNetSystem.CloseNet();
        }*/
    }
}
