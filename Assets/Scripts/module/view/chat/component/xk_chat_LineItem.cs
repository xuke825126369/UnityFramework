using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
namespace xk_System.Model.Modules
{
    public class xk_chat_LineItem : MonoBehaviour
    {
        public Text mText;
        public ExpressionText mExpression;
        public HyperLinkText mHyperLink;

        public void RefreshItem(ModifyStrResult mdata, ChatItemData mdata1)
        {
            mText.text = mdata.resultStr;
            mText.color = getChannelStr(mdata1.ChannelId);
            mExpression.Init(mdata.mMatchExpressList);
            mHyperLink.Init(mdata.mMatchHyperLinkList);
        }

        private Color getChannelStr(int id)
        {
            switch (id)
            {
                /* case ChatModel.channel_Id_System:
                     return TextUtility.Instance.GetTextColor(TextUtility.TextType_Color_channel_System);
                 case ChatModel.channel_Id_World:
                     return TextUtility.Instance.GetTextColor(TextUtility.TextType_Color_channel_World);
                 case ChatModel.channel_Id_Guild:
                     return TextUtility.Instance.GetTextColor(TextUtility.TextType_Color_channel_Guild);
                 case ChatModel.channel_Id_Team:
                     return TextUtility.Instance.GetTextColor(TextUtility.TextType_Color_channel_Team);
                 case ChatModel.channel_Id_Private:
                     return TextUtility.Instance.GetTextColor(TextUtility.TextType_Color_channel_Private);
                 case ChatModel.channel_Id_Nearby:
                     return TextUtility.Instance.GetTextColor(TextUtility.TextType_Color_channel_Nearby);*/
            }
            return Color.white;
        }
    }
}