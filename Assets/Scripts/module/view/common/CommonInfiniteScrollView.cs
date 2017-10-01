using UnityEngine;
using System.Collections;

public class CommonInfiniteScrollView : xInfiniteScrollingView<object>
{
    public CommonScrollViewItem mItemPrefab;
    public override xScrollViewItem<object> GetItemPrefab()
    {
        return mItemPrefab;
    }
}
