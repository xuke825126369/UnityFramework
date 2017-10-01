using UnityEngine;
using System.Collections;

public class CommonScrollView : xScrollView<object>
{
    public CommonScrollViewItem mItemPrefab;
    public override xScrollViewItem<object> GetItemPrefab()
    {
        return mItemPrefab;
    }
}
