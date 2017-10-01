using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

/// <summary>
/// 聊天表情
/// </summary>
public class ExpressionText: MonoBehaviour
{
    public ExpressionItem mSpritePrefab;
    public Transform Parent;

    public void Init(List<SpriteInfo> mMatchExpressList)
    {
        for (int i = 0; i < mMatchExpressList.Count; i++)
        {
            if (i >= Parent.childCount)
            {
                GameObject obj = Instantiate(mSpritePrefab.gameObject) as GameObject;
                obj.transform.SetParent(Parent);
                obj.transform.localScale = Vector3.one;
                obj.transform.localPosition = Vector3.zero;
                obj.SetActive(true);
            }

            Parent.GetChild(i).GetComponent<RectTransform>().anchoredPosition = new Vector2(mMatchExpressList[i].pos, 0);
            Parent.GetChild(i).GetComponent<ExpressionItem>().RefreshItem(mMatchExpressList[i]);
            Parent.GetChild(i).gameObject.SetActive(true);
        }
        for (int i = mMatchExpressList.Count; i < Parent.childCount; i++)
        {
            Parent.GetChild(i).gameObject.SetActive(false);
        }
    }
}
