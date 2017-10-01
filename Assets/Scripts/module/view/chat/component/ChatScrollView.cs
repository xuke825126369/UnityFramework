using UnityEngine;
using System.Collections;
using xk_System.Model.Modules;
using System;

public class ChatScrollView : xInfiniteScrollingView<ChatItemData>
{
    public xk_chatItem mChatItem;
    public override xScrollViewItem<ChatItemData> GetItemPrefab()
    {
        return mChatItem;
    }
}
