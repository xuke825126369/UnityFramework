using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
namespace xk_System.Model.Modules
{
    public class xk_chatItem : xScrollViewItem<ChatItemData>
    {
        public xk_chat_LineItem mItemPrefab;
        public Text mText;

        private int LineCout = 0;
        public override void RefreshItem(ChatItemData data)
        {
            base.RefreshItem(data);
            ChatItemData mdata = data as ChatItemData;
            string content = mdata.name + ":" + mdata.content;

            List<ModifyStrResult> LineStrList = TextUtility.Instance.ModifyStrResult(mText, content);
            LineCout = LineStrList.Count;

            for (int i = 0; i < LineStrList.Count; i++)
            {
                if (i >= transform.childCount)
                {
                    GameObject obj = Instantiate(mItemPrefab.gameObject) as GameObject;
                    obj.transform.SetParent(transform);
                    obj.transform.localScale = Vector3.one;
                    obj.transform.localPosition = Vector3.zero;
                    obj.SetActive(true);
                }
                transform.GetChild(i).GetComponent<xk_chat_LineItem>().RefreshItem(LineStrList[i], mdata);
                transform.GetChild(i).GetComponent<RectTransform>().anchoredPosition = new Vector2(0, (LineStrList.Count - i - 1) * 34);
                transform.GetChild(i).gameObject.SetActive(true);
            }
            for (int i = LineStrList.Count; i < transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.SetActive(false);
            }
        }

        public override Vector2 GetCellSize()
        {
            return new Vector2(0, LineCout * 34 + 10);
        }

        private string getChannelStr(int id, string content)
        {
            string head = "";
            switch (id)
            {
                 case ChatModel.channel_Id_System:
                     head = "<color=#ff4614>" + content + "</color>";
                     break;
                 case ChatModel.channel_Id_World:
                     head = "<color=#9e5efe>" + content + "</color>";
                     break;
                 case ChatModel.channel_Id_Guild:
                     head = "<color=#13a338>" + content + "</color>";
                     break;
                 case ChatModel.channel_Id_Team:
                     head = "<color=#247ee4>" + content + "</color>";
                     break;
                 case ChatModel.channel_Id_Private:
                     head = "<color=#f53ca4>" + content + "</color>";
                     break;
                 case ChatModel.channel_Id_Nearby:
                     head = "<color=#f6ff00>" + content + "</color>";
                     break;
            }
            return head;
        }
    }
}