using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using xk_System.Db;
using xk_System.Model;

public abstract class xScrollViewBase<T> : MonoBehaviour
{
    protected enum Direction
    {
        DownToUp = 1,
        UpToDwon = 2,
        RightToLeft = 3,
        LeftToRight = 4,
    }
    public bool orSizeConstant = false;
    public int RowColumnCout = 1;
    public ScrollRect mScrollRect;
    public int ShowCout;
    public int LastSelectDataIndex = -1;

    public List<Vector2> mDataPosList = new List<Vector2>();
    public List<Vector2> mDataSizeList = new List<Vector2>();
    protected List<xScrollViewItem<T>> mItemList = new List<xScrollViewItem<T>>();
    protected List<T> mdataList = new List<T>();
    protected RectTransform mScrollRectTransform;
    protected Direction mDirection = Direction.DownToUp;
    protected void Awake()
    {
        GetItemPrefab().gameObject.SetActive(false);
        mScrollRect.onValueChanged.AddListener(OnValueChange);

        if (RowColumnCout > 1 && !orSizeConstant)
        {
            Debug.LogError("尺寸一定要恒定");
        }
    }
    protected void OnDestroy()
    {
        mItemList.Clear();
        mdataList.Clear();
        mScrollRect = null;
    }

    protected virtual void OnValueChange(Vector2 vt2)
    {
        if (mDirection == Direction.DownToUp || mDirection == Direction.UpToDwon)
        {
            if (mScrollRect.content.rect.height < mScrollRect.GetComponent<RectTransform>().rect.size.y)
            {
                mScrollRect.content.anchoredPosition = Vector2.zero;
                return;
            }
        }
        else
        {
            if (mScrollRect.content.rect.width < mScrollRect.GetComponent<RectTransform>().rect.size.x)
            {
                mScrollRect.content.anchoredPosition = Vector2.zero;
                return;
            }
        }
        OnScroll(vt2);
    }

    private void SetDirection()
    {
        if (mScrollRect.content.anchorMin.y != mScrollRect.content.anchorMax.y || mScrollRect.content.anchorMin.x != mScrollRect.content.anchorMax.x)
        {
            Debug.LogError("ScrollRect  Content设置不正确");
        }
        if (mScrollRect.content.anchorMin.y == 0f && mScrollRect.content.anchorMin.x == 0.5f && mScrollRect.content.pivot.y == 0f && mScrollRect.content.pivot.x == 0.5f)
        {
            mDirection = Direction.DownToUp;
        }
        else if (mScrollRect.content.anchorMin.y == 1f && mScrollRect.content.anchorMin.x == 0.5f && mScrollRect.content.pivot.y == 1f && mScrollRect.content.pivot.x == 0.5f)
        {
            mDirection = Direction.UpToDwon;
        }
        else if (mScrollRect.content.anchorMin.x == 0f && mScrollRect.content.anchorMin.y == 0.5f && mScrollRect.content.pivot.x == 0f && mScrollRect.content.pivot.y == 0.5f)
        {
            mDirection = Direction.LeftToRight;
        }
        else if (mScrollRect.content.anchorMin.x == 1f && mScrollRect.content.anchorMin.y == 0.5f && mScrollRect.content.pivot.x == 1f && mScrollRect.content.pivot.y == 0.5f)
        {
            mDirection = Direction.RightToLeft;
        }
        else
        {
            if (mScrollRect.content.anchorMin.y == 0f)
            {
                mScrollRect.content.anchorMin = new Vector2(0.5f, 0f);
                mScrollRect.content.anchorMax = new Vector2(0.5f, 0f);
                mScrollRect.content.pivot = new Vector2(0.5f, 0f);
                mDirection = Direction.DownToUp;
            }
            else if (mScrollRect.content.anchorMin.y == 1f)
            {
                mScrollRect.content.anchorMin = new Vector2(0.5f, 1f);
                mScrollRect.content.anchorMax = new Vector2(0.5f, 1f);
                mScrollRect.content.pivot = new Vector2(0.5f, 1f);
                mDirection = Direction.UpToDwon;
            }
            else if (mScrollRect.content.anchorMin.x == 0f)
            {
                mScrollRect.content.anchorMin = new Vector2(0f, 0.5f);
                mScrollRect.content.anchorMax = new Vector2(0f, 0.5f);
                mScrollRect.content.pivot = new Vector2(0f, 0.5f);
                mDirection = Direction.LeftToRight;
            }
            else if (mScrollRect.content.anchorMin.x == 1f)
            {
                mScrollRect.content.anchorMin = new Vector2(1f, 0.5f);
                mScrollRect.content.anchorMax = new Vector2(1f, 0.5f);
                mScrollRect.content.pivot = new Vector2(1f, 0.5f);
                mDirection = Direction.RightToLeft;
            }
            Debug.LogError("ScrollRect  Content设置不正确:修正结果：" + mDirection);
        }
        RectTransform mPrefabTransform = GetItemPrefab().GetComponent<RectTransform>();
        if (mDirection == Direction.DownToUp)
        {
            Vector2 mv1 = mPrefabTransform.anchorMin;
            mPrefabTransform.anchorMin = new Vector2(mv1.x, 0f);
            Vector2 mv2 = mPrefabTransform.anchorMax;
            mPrefabTransform.anchorMax = new Vector2(mv2.x, 0f);
            Vector2 mv3 = mPrefabTransform.pivot;
            mPrefabTransform.pivot = new Vector2(mv3.x, 0f);
        }
        else if (mDirection == Direction.UpToDwon)
        {
            Vector2 mv1 = mPrefabTransform.anchorMin;
            mPrefabTransform.anchorMin = new Vector2(mv1.x, 1f);
            Vector2 mv2 = mPrefabTransform.anchorMax;
            mPrefabTransform.anchorMax = new Vector2(mv2.x, 1f);
            Vector2 mv3 = mPrefabTransform.pivot;
            mPrefabTransform.pivot = new Vector2(mv3.x, 1f);
        }
        else if (mDirection == Direction.LeftToRight)
        {
            Vector2 mv1 = mPrefabTransform.anchorMin;
            mPrefabTransform.anchorMin = new Vector2(0f, mv1.y);
            Vector2 mv2 = mPrefabTransform.anchorMax;
            mPrefabTransform.anchorMax = new Vector2(0f, mv1.y);
            Vector2 mv3 = mPrefabTransform.pivot;
            mPrefabTransform.pivot = new Vector2(0f, mv1.y);
        }
        else if (mDirection == Direction.RightToLeft)
        {
            Vector2 mv1 = mPrefabTransform.anchorMin;
            mPrefabTransform.anchorMin = new Vector2(1f, mv1.y);
            Vector2 mv2 = mPrefabTransform.anchorMax;
            mPrefabTransform.anchorMax = new Vector2(1f, mv1.y);
            Vector2 mv3 = mPrefabTransform.pivot;
            mPrefabTransform.pivot = new Vector2(1f, mv1.y);
        }

        if ((mScrollRect.vertical == true && mScrollRect.horizontal == true)
    || (mScrollRect.vertical == false && mScrollRect.horizontal == false)
    || (mScrollRect.vertical == true && ((mDirection != Direction.DownToUp) && (mDirection != Direction.UpToDwon)))
    || (mScrollRect.horizontal == true && ((mDirection != Direction.LeftToRight) && (mDirection != Direction.RightToLeft)))
    )
        {
            Debug.LogError("ScrollRect 设置不正确");
        }
    }

    /// <summary>
    /// 判断此数据是否该隐藏
    /// </summary>
    /// <param name="dataIndex"></param>
    /// <returns></returns>
    protected bool JudgeDataInView(int dataIndex)
    {
        if (dataIndex>=0 && dataIndex < mDataPosList.Count)
        {
            Vector2 mVec = mScrollRect.GetComponent<RectTransform>().rect.size;
            if (mDirection == Direction.DownToUp || mDirection == Direction.UpToDwon)
            {
                if (mScrollRect.content.anchoredPosition.y * mDataPosList[RowColumnCout].y <= 0)
                {
                    float size1 = Mathf.Abs(mDataPosList[dataIndex].y) + mDataSizeList[dataIndex].y - Mathf.Abs(mScrollRect.content.anchoredPosition.y);
                    float size2 = Mathf.Abs(mDataPosList[dataIndex].y) - Mathf.Abs(mScrollRect.content.anchoredPosition.y);
                    if (size2 <= mVec.y && size1 >= 0f)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }else
                {
                    float size1 = Mathf.Abs(mDataPosList[dataIndex].y) + Mathf.Abs(mScrollRect.content.anchoredPosition.y);
                    if (size1 <= mVec.y)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                    
            }
            else
            {

                if (mScrollRect.content.anchoredPosition.x * mDataPosList[RowColumnCout].x <= 0)
                {
                    float size1 = Mathf.Abs(mDataPosList[dataIndex].x) + mDataSizeList[dataIndex].x - Mathf.Abs(mScrollRect.content.anchoredPosition.x);
                    float size2 = Mathf.Abs(mDataPosList[dataIndex].x) - Mathf.Abs(mScrollRect.content.anchoredPosition.x);
                    if (size2 <= mVec.x && size1 >= 0f)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    float size1 = Mathf.Abs(mDataPosList[dataIndex].x) + Mathf.Abs(mScrollRect.content.anchoredPosition.x);
                    if (size1 <= mVec.x)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }
        else
        {
            return false;
        }
    }

    protected void SetItemdata(xScrollViewItem<T> mItem, int dataIndex)
    {
        mItem.InitItem(mdataList[dataIndex], dataIndex);
        if (dataIndex > mDataPosList.Count - 1)
        {
            SetPosAndSize(dataIndex, mItem.GetCellSize());
            AdjustContentSize();
        }
        SetItemPosition(mItem, dataIndex);
    }

    protected void SetAllPosAndPos()
    {
        if (orSizeConstant)
        {
            int startIndex = mDataPosList.Count;
            for (int i = startIndex; i < mdataList.Count; i++)
            {
                SetPosAndSize(i, GetItemPrefab().GetCellSize());
            }
            AdjustContentSize();
        }
    }

    protected virtual void SetPosAndSize(int dataIndex, Vector2 mSize)
    {
        if (orSizeConstant)
        {
            mDataSizeList.Insert(dataIndex,mSize);
            Vector2 mPos = Vector2.zero;
            if (mDirection == Direction.DownToUp)
            {
                float x = dataIndex % RowColumnCout * mSize.x;
                float y = dataIndex / RowColumnCout * mSize.y;
                mPos = new Vector2(x, y);
                mDataPosList.Insert(dataIndex, mPos);
            }
            else if (mDirection == Direction.UpToDwon)
            {
                float x = dataIndex % RowColumnCout * mSize.x;
                float y = -dataIndex / RowColumnCout * mSize.y;
                mPos = new Vector2(x, y);
                mDataPosList.Insert(dataIndex, mPos);
            }
            else if (mDirection == Direction.LeftToRight)
            {
                float y = dataIndex % RowColumnCout * mSize.y;
                float x = dataIndex / RowColumnCout * mSize.x;
                mPos = new Vector2(x, y);
                mDataPosList.Insert(dataIndex, mPos);
            }
            else if (mDirection == Direction.RightToLeft)
            {
                float y = dataIndex % RowColumnCout * mSize.y;
                float x = -dataIndex / RowColumnCout * mSize.x;
                mPos = new Vector2(x, y);
                mDataPosList.Insert(dataIndex, mPos);
            }
        }
        else
        {
            Vector2 mPos = Vector2.zero;
            mDataSizeList.Insert(dataIndex, mSize);
            if (dataIndex == 0)
            {
                mPos = Vector2.zero;
            }
            else if (mDirection == Direction.DownToUp)
            {
                mPos = new Vector2(0, mDataPosList[dataIndex - 1].y + mDataSizeList[dataIndex - 1].y);
            }
            else if (mDirection == Direction.LeftToRight)
            {
                mPos = new Vector2(mDataPosList[dataIndex - 1].x + mDataSizeList[dataIndex - 1].x, 0);
            }
            else if (mDirection == Direction.UpToDwon)
            {
                mPos = new Vector2(0, mDataPosList[dataIndex - 1].y - mDataSizeList[dataIndex - 1].y);
            }
            else if (mDirection == Direction.RightToLeft)
            {
                mPos = new Vector2(mDataPosList[dataIndex - 1].x - mDataSizeList[dataIndex - 1].x, 0);
            }
            mDataPosList.Insert(dataIndex, mPos);
        }
    }

    protected virtual void SetItemPosition(xScrollViewItem<T> mItem, int dataIndex)
    {
        mItem.GetComponent<RectTransform>().anchoredPosition = new Vector2(mDataPosList[dataIndex].x, mDataPosList[dataIndex].y);
    }

    protected virtual void AdjustContentSize()
    {
        if (mDataPosList.Count > 0)
        {
            if (!orSizeConstant)
            {
                if (mDirection == Direction.DownToUp || mDirection == Direction.UpToDwon)
                {
                    float mSize = Mathf.Abs(mDataPosList[mDataPosList.Count - 1].y) + mDataSizeList[mDataSizeList.Count - 1].y;
                    mSize += (mdataList.Count - mDataPosList.Count) * GetItemPrefab().GetCellSize().y;
                    mScrollRect.content.sizeDelta = new Vector2(mScrollRect.content.sizeDelta.x, mSize);
                }
                else
                {
                    float mSize = Mathf.Abs(mDataPosList[mDataPosList.Count - 1].x) + mDataSizeList[mDataSizeList.Count - 1].x;
                    mSize += (mdataList.Count - mDataPosList.Count) * GetItemPrefab().GetCellSize().x;
                    mScrollRect.content.sizeDelta = new Vector2(mSize, mScrollRect.content.sizeDelta.y);
                }

            }
            else
            {
                if (mDirection == Direction.DownToUp || mDirection == Direction.UpToDwon)
                {
                    float mCellSize = GetItemPrefab().GetCellSize().y;
                    float mSize = Mathf.Abs(mDataPosList[mDataPosList.Count - 1].y) + mCellSize;
                    mScrollRect.content.sizeDelta = new Vector2(mScrollRect.content.sizeDelta.x, mSize);
                }
                else
                {
                    float mCellSize = GetItemPrefab().GetCellSize().x;
                    float mSize = Mathf.Abs(mDataPosList[mDataPosList.Count - 1].x) + mCellSize;
                    mScrollRect.content.sizeDelta = new Vector2(mSize, mScrollRect.content.sizeDelta.y);
                }
            }
        }
        else
        {
            if (mDirection == Direction.DownToUp || mDirection == Direction.UpToDwon)
            {
                mScrollRect.content.sizeDelta = new Vector2(mScrollRect.content.sizeDelta.x, 0f);
            }
            else
            {
                mScrollRect.content.sizeDelta = new Vector2(0f, mScrollRect.content.sizeDelta.y);
            }
        }
    }
    public virtual void InitView(List<T> data)
    {
        SetDirection();
        mDataPosList.Clear();
        mDataSizeList.Clear();
        mScrollRectTransform = mScrollRect.GetComponent<RectTransform>();
        mScrollRect.content.anchoredPosition = Vector3.zero;
        if (mDirection == Direction.DownToUp || mDirection == Direction.UpToDwon)
        {
            mScrollRect.content.sizeDelta = new Vector2(mScrollRect.content.sizeDelta.x, 0);
        }
        else
        {
            mScrollRect.content.sizeDelta = new Vector2(0, mScrollRect.content.sizeDelta.y);
        }

        mdataList = data;
        SetAllPosAndPos();
    }
    protected abstract void OnScroll(Vector2 vt2);

    public abstract xScrollViewItem<T> GetItemPrefab();

    public abstract void AddData(int dataIndex, T data);

    public abstract void DeleteData(int dataIndex);

    protected void addChild(GameObject goPrefab, Transform parent)
    {
        GameObject goChild = GameObject.Instantiate(goPrefab) as GameObject;
        goChild.transform.SetParent(parent, false);
        goChild.transform.localScale = Vector3.one;
        SetItemhierarchy(goChild.transform);
        xScrollViewItem<T> mItem = goChild.GetComponent<xScrollViewItem<T>>();
        mItemList.Add(mItem);
    }

    /// <summary>
    /// 设置Item的层级
    /// </summary>
    /// <param name="mItem"></param>
    protected virtual void SetItemhierarchy(Transform mItem)
    {
        mItem.SetAsLastSibling();
    }
}

public abstract class xInfiniteScrollingView<T> : xScrollViewBase<T>
{
    public int CurrentShowFirstLine = 0;
    public int CurrentShowLastLine = 0;

    public override void InitView(List<T> data)
    {
        base.InitView(data);
        InitShowCout();
        CurrentShowFirstLine = 0;
        InitView(CurrentShowFirstLine);
    }

    protected override void OnScroll(Vector2 vt2)
    {
        int _curScrollPerLineIndex = getShowFirstLine();
        if (_curScrollPerLineIndex != CurrentShowFirstLine)
        {
            Debug.LogError("当前显示的行：" + _curScrollPerLineIndex);
            RefreshView1(_curScrollPerLineIndex);
        }
        JudgeItemOrShow();
    }

    public void JudgeItemOrShow()
    {
        foreach (var v in mItemList)
        {
            if (JudgeDataInView(v.dataIndex))
            {
                v.gameObject.SetActive(true);
            }
            else
            {
                v.gameObject.SetActive(false);
            }
        }
    }

    private int getShowFirstLine()
    {
        if (mDirection == Direction.DownToUp && mScrollRect.content.anchoredPosition.y >= 0f)
        {
            return 0;
        }
        else if (mDirection == Direction.UpToDwon && mScrollRect.content.anchoredPosition.y <= 0f)
        {
            return 0;
        }
        else if (mDirection == Direction.LeftToRight && mScrollRect.content.anchoredPosition.x >= 0f)
        {
            return 0;
        }
        else if (mDirection == Direction.RightToLeft && mScrollRect.content.anchoredPosition.x <= 0f)
        {
            return 0;
        }
        else
        {
            float mContentMoveHeight = 0f;
            if (mDirection == Direction.DownToUp || mDirection == Direction.UpToDwon)
            {
                mContentMoveHeight = Mathf.Abs(mScrollRect.content.anchoredPosition.y);
                if (orSizeConstant == false)
                {
                    int current = CurrentShowFirstLine;
                    if (Mathf.Abs(mDataPosList[current].y) + mDataSizeList[current].y > mContentMoveHeight)
                    {
                        while (current > 0 && current < mdataList.Count)
                        {
                            int aaa = current - 1;
                            if (aaa >= 0 && Mathf.Abs(mDataPosList[aaa].y) + mDataSizeList[aaa].y > mContentMoveHeight)
                            {
                                current = aaa;
                            }
                            else
                            {
                                break;
                            }
                        }
                        return current;
                    }
                    else
                    {
                        while (true)
                        {
                            current++;
                            if (Mathf.Abs(mDataPosList[current].y) + mDataSizeList[current].y > mContentMoveHeight)
                            {
                                break;
                            }
                        }
                        return current;
                    }
                }
                else
                {
                    return (int)(mContentMoveHeight / GetItemPrefab().GetCellSize().y);
                }
            }
            else
            {
                mContentMoveHeight = Mathf.Abs(mScrollRect.content.anchoredPosition.x);
                if (mContentMoveHeight <= mScrollRect.content.sizeDelta.y - mScrollRectTransform.rect.width)
                {
                    if (orSizeConstant == false)
                    {
                        int current = CurrentShowFirstLine;
                        if (Mathf.Abs(mDataPosList[current].x) + mDataSizeList[current].x > mContentMoveHeight)
                        {
                            while (current > 0 && current < mdataList.Count)
                            {
                                int aaa = current - 1;
                                if (aaa >= 0 && Mathf.Abs(mDataPosList[aaa].x) + mDataSizeList[aaa].x > mContentMoveHeight)
                                {
                                    current = aaa;
                                }
                                else
                                {
                                    break;
                                }
                            }
                            return current;
                        }
                        else
                        {
                            while (true)
                            {
                                current++;
                                if (Mathf.Abs(mDataPosList[current].x) + mDataSizeList[current].x > mContentMoveHeight)
                                {
                                    break;
                                }
                            }
                            return current;
                        }
                    }
                    else
                    {
                        return (int)(mContentMoveHeight / GetItemPrefab().GetCellSize().x);
                    }
                }
                else
                {
                    return CurrentShowFirstLine;
                }
            }
        }
    }
    /// <summary>
    /// 急性跳转，一般不要使用
    /// </summary>
    /// <param name="dataIndex"></param>
    public void GoToItem(int dataIndex)
    {
        LastSelectDataIndex = dataIndex;
        if (orSizeConstant)
        {
            float mHeight = mDataPosList[dataIndex].y;
            mScrollRect.content.anchoredPosition = new Vector2(0, -mHeight);

            OnValueChange(Vector2.zero);
        }
        else
        {
            Debug.LogError("立即跳转有隐形Bug，必须为恒定尺寸，否则只能滑动寻找（非及时性）");
        }
    }

    private void InitShowCout()
    {
        int sumCout = ShowCout * RowColumnCout;
        if (mItemList.Count >= sumCout)
        {
            return;
        }

        for (int i = 0; i < sumCout; i++)
        {
            addChild(GetItemPrefab().gameObject, mScrollRect.content);
        }
    }

    public void RefreshView1(int FirstShowLine)
    {
        int cout = 0;
        if (RowColumnCout > 1)
        {
            if (CurrentShowFirstLine > FirstShowLine)
            {
                int Length = CurrentShowFirstLine - FirstShowLine;
                CurrentShowLastLine = CurrentShowLastLine - Length;

                int HidelIndex = (CurrentShowLastLine + 1) * RowColumnCout - 1;
                foreach (var v in mItemList)
                {
                    if (v.dataIndex > HidelIndex)
                    {
                        v.gameObject.SetActive(false);
                        v.RefreshDataIndex(-1);
                    }
                }
                int showLength = Length * RowColumnCout;
                int StartIndex = CurrentShowFirstLine * RowColumnCout - 1;
                foreach (var v in mItemList)
                {
                    if (v.dataIndex < 0)
                    {
                        if (cout < showLength)
                        {
                            int dataIndex = StartIndex - cout;
                            SetItemdata(v, dataIndex);
                            v.gameObject.SetActive(true);
                            cout++;
                        }
                        else
                        {
                            break;
                        }
                    }
                    else
                    {
                        v.gameObject.SetActive(true);
                    }
                }
                CurrentShowFirstLine = FirstShowLine;
            }
            else
            {
                int Length = -CurrentShowFirstLine + FirstShowLine;
                int HidelIndex = FirstShowLine * RowColumnCout;
                foreach (var v in mItemList)
                {
                    if (v.dataIndex < HidelIndex)
                    {
                        v.gameObject.SetActive(false);
                        v.RefreshDataIndex(-1);
                    }
                }
                int showLength = Length * RowColumnCout;
                int StartIndex = (CurrentShowLastLine + 1) * RowColumnCout;
                foreach (var v in mItemList)
                {
                    if (v.dataIndex<0)
                    {
                        if (cout < showLength)
                        {
                            int dataIndex = StartIndex + cout;
                            if (dataIndex < mdataList.Count)
                            {
                                SetItemdata(v, dataIndex);
                                v.gameObject.SetActive(true);
                                cout++;
                            }
                            else
                            {
                                break;
                            }
                        }
                        else
                        {
                            break;
                        }
                    }else
                    {
                        v.gameObject.SetActive(true);
                    }
                }
                CurrentShowLastLine = CurrentShowLastLine + Length;
                CurrentShowFirstLine = FirstShowLine;
            }
        }
        else
        {
            if (CurrentShowFirstLine > FirstShowLine)
            {
                int Length = CurrentShowFirstLine - FirstShowLine;
                CurrentShowLastLine = CurrentShowLastLine - Length;
                foreach (var v in mItemList)
                {
                    if (v.dataIndex > CurrentShowLastLine)
                    {
                        v.gameObject.SetActive(false);
                        v.RefreshDataIndex(-1);
                    }
                }
                foreach (var v in mItemList)
                {
                    if (v.dataIndex < 0)
                    {
                        int index = CurrentShowFirstLine - 1 - cout;
                        SetItemdata(v, index);
                        v.gameObject.SetActive(true);
                        cout++;
                        if (cout >= Length)
                        {
                            break;
                        }
                    }
                    else
                    {
                        if (!v.gameObject.activeSelf)
                        {
                            v.gameObject.SetActive(true);
                        }
                    }
                }
                CurrentShowFirstLine = FirstShowLine;
            }
            else
            {
                int Length = -CurrentShowFirstLine + FirstShowLine;
                foreach (var v in mItemList)
                {
                    if (v.dataIndex < FirstShowLine)
                    {
                        v.gameObject.SetActive(false);
                        v.RefreshDataIndex(-1);
                    }
                }
                foreach (var v in mItemList)
                {
                    if (v.dataIndex < 0)
                    {
                        if (cout < Length)
                        {
                            int index = CurrentShowLastLine + 1 + cout;
                            if (index < mdataList.Count)
                            {
                                SetItemdata(v, index);
                                v.gameObject.SetActive(true);
                                cout++;
                            }
                            else
                            {
                                break;
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                    else
                    {
                        if (!v.gameObject.activeSelf)
                        {
                            v.gameObject.SetActive(true);
                        }
                    }
                }
                CurrentShowLastLine = CurrentShowLastLine + Length;
                CurrentShowFirstLine = FirstShowLine;
            }
        }
        Debug.LogError("一次刷新：" + cout);
    }

    public void RefreshView2(int FirstShowLine)
    {
        int dataIndex = FirstShowLine * RowColumnCout;
        int cout = 0;
        for (int i = 0; i < mItemList.Count; i++)
        {
            if (dataIndex < mdataList.Count)
            {
                SetItemdata(mItemList[i], dataIndex);
                mItemList[i].gameObject.SetActive(true);
                dataIndex++;
                cout++;
            }
            else
            {
                mItemList[i].gameObject.SetActive(false);
            }
        }
        CurrentShowFirstLine = FirstShowLine;
        CurrentShowLastLine = FirstShowLine + (cout - 1) / RowColumnCout;
        Debug.LogError("一次刷新：" + cout);
    }

    public void InitView(int FirstShowLine)
    {
        int dataIndex = FirstShowLine * RowColumnCout;
        int cout = 0;
        for (int i = 0; i < mItemList.Count; i++)
        {
            if (dataIndex < mdataList.Count)
            {
                SetItemdata(mItemList[i], dataIndex);
                mItemList[i].gameObject.SetActive(true);
                dataIndex++;
                cout++;
            }
            else
            {
                mItemList[i].gameObject.SetActive(false);
            }
        }
        CurrentShowFirstLine = FirstShowLine;
        CurrentShowLastLine = FirstShowLine + (cout - 1) / RowColumnCout;
        Debug.LogError("一次刷新：" + (CurrentShowLastLine + 1));
    }
    // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~对数据进行增删~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    public override void AddData(int dataIndex,T data)
    {
        mdataList.Insert(dataIndex,data);
        if (!orSizeConstant)
        {
            RefreshView2(CurrentShowFirstLine);
        }
        else
        {
            SetAllPosAndPos();
            RefreshView2(CurrentShowFirstLine);
        }

        Debug.LogError("当前插入索引："+dataIndex+"当前数据个数：" + mdataList.Count);
    }

    public override void DeleteData(int dataIndex)
    {
        Debug.LogError("当前删除：" + dataIndex + " | 当前数据个数：" + mdataList.Count);
        foreach (var v in mItemList)
        {
            if (v.dataIndex == dataIndex)
            {
                if (CurrentShowLastLine + 1 < mdataList.Count)
                {
                    int NewDataIndex = CurrentShowLastLine + 1;
                    SetItemdata(v, NewDataIndex);
                    v.gameObject.SetActive(true);
                }
                else
                {
                    v.gameObject.SetActive(false);
                }
                break;
            }
        }

        mdataList.RemoveAt(dataIndex);
        if (mDataPosList.Count > dataIndex)
        {
            if (!orSizeConstant)
            {
                mDataSizeList.RemoveAt(dataIndex);
                mDataPosList.RemoveAt(dataIndex);
                for (int i = dataIndex; i < mDataPosList.Count; i++)
                {
                    SetPosAndSize(i, mDataSizeList[i]);
                }
            }
            else
            {
                mDataPosList.RemoveAt(mDataPosList.Count - 1);
            }
        }

        foreach (var v in mItemList)
        {
            if (v.gameObject.activeSelf && v.dataIndex > dataIndex)
            {
                int NewdataIndex = v.dataIndex - 1;
                v.RefreshDataIndex(NewdataIndex);
                SetItemPosition(v, NewdataIndex);
            }
        }

        AdjustContentSize();
    }

}

public abstract class xScrollView<T> : xScrollViewBase<T>
{
    public bool orAsync = false;
    protected override void OnScroll(Vector2 vt2)
    {
        JudgeItemOrShow();
    }

    public void JudgeItemOrShow()
    {
        foreach (var v in mItemList)
        {
            if (JudgeDataInView(v.dataIndex))
            {
                v.gameObject.SetActive(true);
            }
            else
            {
                v.gameObject.SetActive(false);
            }
        }
    }

    public override void InitView(List<T> mdataList)
    {
        base.InitView(mdataList);
        if (!orAsync)
        {
            for (int i = 0; i < mdataList.Count; i++)
            {
                if (i >= mItemList.Count)
                {
                    addChild(GetItemPrefab().gameObject, mScrollRect.content);
                }
                SetItemdata(mItemList[i], i);
                mItemList[i].gameObject.SetActive(true);
            }
            for (int i = mdataList.Count; i < mItemList.Count; i++)
            {
                mItemList[i].gameObject.SetActive(false);
            }
        }
        else
        {
           UpdateManager.Instance.xStartCoroutine(AsyncRefreshView());
        }
    }

    private IEnumerator AsyncRefreshView()
    {
        for (int i = 0,Length=mdataList.Count; i < Length; i++)
        {
            if (i >= mItemList.Count)
            {
                addChild(GetItemPrefab().gameObject, mScrollRect.content);              
            }
            SetItemdata(mItemList[i],i);
            mItemList[i].gameObject.SetActive(true);
            if (i >= 5)
            {
                yield return 0;
            }
        }
        for(int i=mdataList.Count;i<mItemList.Count;i++)
        {
            mItemList[i].gameObject.SetActive(false);
        }
    }

    public override void AddData(int dataIndex, T data)
    {
        mdataList.Insert(dataIndex, data);

        Debug.LogError("当前插入索引：" + dataIndex + "当前数据个数：" + mdataList.Count);
    }

    public override void DeleteData(int dataIndex)
    {
        Debug.LogError("当前删除：" + dataIndex + " | 当前数据个数：" + mdataList.Count);    
    }
}

public abstract class xScrollViewItem<T> : MonoBehaviour
{
    protected T mSaveData= default(T);
    protected xScrollViewBase<T> mDataListParent;
    public int dataIndex = -1;
    protected DataBind<T> mDataBind=new DataBind<T>();

    protected virtual void Awake()
    {
        mDataListParent = transform.GetComponentInParent<xScrollViewBase<T>>();
    }

    protected virtual void OnDisable()
    {
        
    }

    public virtual void InitItem(T data, int dataIndex)
    {
        RefreshItem(data);
        RefreshDataIndex(dataIndex);
    }

    public virtual void RefreshDataIndex(int dataIndex)
    {
        this.dataIndex = dataIndex;
        this.gameObject.name = dataIndex.ToString();
    }

    public virtual void RefreshItem(T data)
    {
        mSaveData = data;
    }


    /// <summary>
    /// 返回Item的尺寸
    /// </summary>
    /// <returns></returns>
    public abstract Vector2 GetCellSize();
}

public abstract class CommonScrollViewItem : xScrollViewItem<object>
{
    

}