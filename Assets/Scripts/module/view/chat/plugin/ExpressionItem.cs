
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using xk_System.AssetPackage;

public class ExpressionItem : MonoBehaviour
{
    public Image mExpressionIcon;
    private void Awake()
    {

    }

    public void RefreshItem(SpriteInfo mSpriteInfo)
    {
        mExpressionIcon.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0f);
        mExpressionIcon.GetComponent<RectTransform>().sizeDelta = new Vector2(mSpriteInfo.width, mSpriteInfo.height);
        Sprite mSprite= AtlasManager.Instance.GetSprite("emoj",mSpriteInfo.mSpriteText);
        mExpressionIcon.sprite = mSprite;
    }
	
}
