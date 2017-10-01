using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using xk_System.Debug;

namespace xk_System.Db
{
    public class ItemData
    {
        public int MaxLayer;
        public ItemDB mItemDB;
        public List<SubItem> mSubItemList = new List<SubItem>();
    }

    public class SubItem
    {
        /// <summary>
        /// 表示第几层
        /// </summary>
        public int Layer;
        /// <summary>
        /// 表示子2茶树的索引（从左到右）
        /// </summary>
        public int index;
        /// <summary>
        /// 我兄弟的个数
        /// </summary>
        public int cout;
        /// <summary>
        /// 父节点
        /// </summary>
        public SubItem mItemDBParent;
        /// <summary>
        /// 我
        /// </summary>
        public ItemDB mItemDB;
    }

    public class ItemConfig : Singleton<ItemConfig>
    {
        public readonly List<ItemDB> mItemConfig = null;
        public ItemConfig()
        {
            mItemConfig = DbManager.Instance.GetDb<ItemDB>();
        }

        public ItemDB FindItem(int id)
        {
            return mItemConfig.Find((x) => x.id == id);
        }

        public List<ItemDB> GetCanCompoundItemList(int id)
        {
            List<ItemDB> mlist = new List<ItemDB>();
            foreach (var v in mItemConfig)
            {
                if (v.SubItemArray.Contains(id))
                {
                    mlist.Add(v);
                }
            }
            return mlist;
        }

        public List<ItemData> GetItemDataList(List<int> mItemIdList)
        {
            List<ItemData> mItemDataList = new List<ItemData>();
            foreach (var v in mItemIdList)
            {
                mItemDataList.Add(GetItemData(v));
            }
            return mItemDataList;
        }

        public ItemData GetItemData(int id)
        {
            ItemData mItemData = new ItemData();
            ItemDB mItemDB = mItemConfig.Find((x) => x.id == id);
            mItemData.mItemDB = mItemDB;

            SubItem mSubItem = new SubItem();
            mSubItem.Layer = 0;
            mSubItem.index = 0;
            mSubItem.mItemDBParent = null;
            mSubItem.cout = 1;
            mSubItem.mItemDB = mItemDB;
            mItemData.mSubItemList.Add(mSubItem);

            if (mItemDB.SubItemArray != null && mItemDB.SubItemArray.Count > 0)
            {
                GetSubItemList(mSubItem, 0, mItemData);
            }
            return mItemData;
        }

        private void GetSubItemList(SubItem mSubItemDBParent, int Layer, ItemData mItemData)
        {
            Layer++;
            if (Layer + 1 > mItemData.MaxLayer)
            {
                mItemData.MaxLayer = Layer + 1;
            }
            int cout = mSubItemDBParent.mItemDB.SubItemArray.Count;
            for (int i = 0; i < mSubItemDBParent.mItemDB.SubItemArray.Count; i++)
            {
                ItemDB mItemDB1 = mItemConfig.Find((x) => x.id == mSubItemDBParent.mItemDB.SubItemArray[i]);
                if (mItemDB1 == null)
                {
                    DebugSystem.LogError("物品找不到：" + mSubItemDBParent.mItemDB.SubItemArray[i]);
                    continue;
                }
                SubItem mSubItem = new SubItem();
                mSubItem.Layer = Layer;
                mSubItem.index = i;
                mSubItem.mItemDBParent = mSubItemDBParent;
                mSubItem.cout = cout;
                mSubItem.mItemDB = mItemDB1;
                mItemData.mSubItemList.Add(mSubItem);

                if (mItemDB1.SubItemArray != null && mItemDB1.SubItemArray.Count > 0)
                {
                    GetSubItemList(mSubItem, Layer, mItemData);
                }
            }
        }

        public string GetItemAttDes(int id)
        {
            string des = "";
            ItemDB mItemAtt = FindItem(id);
            for (int i = 0; i < mItemAtt.ItemAttGroup.Count; i += 2)
            {
                des += GetAttTypeDes(mItemAtt.ItemAttGroup[i], mItemAtt.ItemAttGroup[i + 1]) + "\n";
            }
            return des;
        }

        private string GetAttTypeDes(int attType, int value)
        {
            string des = "";
            switch (attType)
            {
                case 1:
                    des = "+" + value + "生命值";
                    break;
                case 2:
                    des = "+" + value + "法力值";
                    break;
                case 3:
                    des = "+" + value + "攻击力";
                    break;
                case 4:
                    des = "+" + value + "法强";
                    break;
                case 5:
                    des = "+" + value + "护甲";
                    break;
                case 6:
                    des = "+" + value + "魔抗";
                    break;
                case 7:
                    des = "+" + value + "移动速度";
                    break;
                case 10:
                    des = "+" + value + "护甲穿透";
                    break;
                case 11:
                    des = "+" + value + "法术穿透";
                    break;
                case 12:
                    des = "+" + value + "金币/10秒";
                    break;
                case 20:
                    des = "+%" + value + "移动速度";
                    break;
                case 21:
                    des = "+%" + value + "攻击速度";
                    break;
                case 22:
                    des = "+%" + value + "冷却缩减";
                    break;
                case 23:
                    des = "+%" + value + "生命偷取";
                    break;
                case 24:
                    des = "+%" + value + "法术吸血";
                    break;
                case 25:
                    des = "+%" + value + "暴击几率";
                    break;
                case 26:
                    des = "+%" + value + "暴击伤害";
                    break;
                case 27:
                    des = "+%" + value + "护甲穿透";
                    break;
                case 28:
                    des = "+%" + value + "法术穿透";
                    break;
                case 29:
                    des = "+%" + value + "韧性";
                    break;
                case 30:
                    des = "+%" + value + "基础生命回复";
                    break;
                case 31:
                    des = "+%" + value + "基础法力回复";
                    break;
                case 32:
                    des = "-%" + value + "降低暴击伤害";
                    break;
                case 33:
                    des = "+%" + value + "基础攻击力";
                    break;
                case 50:
                    des = "对野怪+%" + value + "生命偷取";
                    break;
                case 51:
                    des = "对野怪+%" + value + "基础法力回复";
                    break;

            }
            return des;
        }

        public string GetItemOtherAttDes(int id)
        {
            string des = "";
            ItemDB mItemDB = FindItem(id);
            for (int i = 0; i < mItemDB.skillGroup.Count; i++)
            {
                SkillDB mSkillDB = SkillConfig.Instance.FindItem(mItemDB.skillGroup[i]);
                des += mSkillDB.skillName+":"+mSkillDB.skillDes + "\n";
            }
            return des;
        }
    }
}