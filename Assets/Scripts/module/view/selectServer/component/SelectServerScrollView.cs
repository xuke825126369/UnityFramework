using UnityEngine;
using System.Collections;
using System;
using xk_System.Db;
namespace xk_System.View.Modules
{
    public class SelectServerScrollView : xInfiniteScrollingView<ServerListDB>
    {
        public ServerItem mItemPrefab;
        public override xScrollViewItem<ServerListDB> GetItemPrefab()
        {
            return mItemPrefab;
        }
    }
}