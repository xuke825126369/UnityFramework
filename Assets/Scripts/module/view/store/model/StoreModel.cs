using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using xk_System.Debug;

namespace xk_System.Model.Modules
{
    public class StoreModel : xk_Model
    {
        public Dictionary<int[], Vector3> mPosDic = new Dictionary<int[], Vector3>();
        public override void initModel()
        {
            base.initModel();
            InitItemPos();
        }

        private void InitItemPos()
        {
            int[] aaa = new int[3];
            aaa[0] = 0;///第0层
            aaa[1] = 1;///有1个弟兄
            aaa[2] = 0;//弟兄中的索引（从左到右）
            Vector3 mVector = new Vector3(0, 90, 0);
            mPosDic.Add(aaa, mVector);

            aaa = new int[3];
            aaa[0] = 1;///第一层
            aaa[1] = 1;///有1个弟兄
            aaa[2] = 0;//弟兄中的索引（从左到右）
            mVector = new Vector3(0,-70,0);
            mPosDic.Add(aaa,mVector);

            aaa = new int[3];
            aaa[0] = 1;///第一层
            aaa[1] = 2;///有2个弟兄
            aaa[2] = 0;//弟兄中的索引（从左到右）
            mVector = new Vector3(-80, -70, 0);
            mPosDic.Add(aaa, mVector);

            aaa = new int[3];
            aaa[0] = 1;///第一层
            aaa[1] = 2;///有2个弟兄
            aaa[2] = 1;//弟兄中的索引（从左到右）
            mVector = new Vector3(80, -70, 0);
            mPosDic.Add(aaa, mVector);

            aaa = new int[3];
            aaa[0] = 1;///第一层
            aaa[1] = 3;///有3个弟兄
            aaa[2] = 0;//弟兄中的索引（从左到右）
            mVector = new Vector3(-120, -70, 0);
            mPosDic.Add(aaa, mVector);

            aaa = new int[3];
            aaa[0] = 1;///第一层
            aaa[1] = 3;///有3个弟兄
            aaa[2] = 1;//弟兄中的索引（从左到右）
            mVector = new Vector3(0, -70, 0);
            mPosDic.Add(aaa, mVector);

            aaa = new int[3];
            aaa[0] = 1;///第一层
            aaa[1] = 3;///有3个弟兄
            aaa[2] = 2;//弟兄中的索引（从左到右）
            mVector = new Vector3(120, -70, 0);
            mPosDic.Add(aaa, mVector);

            aaa = new int[3];
            aaa[0] = 2;///第2层
            aaa[1] = 1;///有1个弟兄
            aaa[2] = 0;//弟兄中的索引（从左到右）
            mVector = new Vector3(0, -70, 0);
            mPosDic.Add(aaa, mVector);


            aaa = new int[3];
            aaa[0] = 2;///第2层
            aaa[1] = 2;///有2个弟兄
            aaa[2] = 0;//弟兄中的索引（从左到右）
            mVector = new Vector3(-40, -70, 0);
            mPosDic.Add(aaa, mVector);


            aaa = new int[3];
            aaa[0] = 2;///第2层
            aaa[1] = 2;///有2个弟兄
            aaa[2] = 1;//弟兄中的索引（从左到右）
            mVector = new Vector3(40, -70, 0);
            mPosDic.Add(aaa, mVector);

            aaa = new int[3];
            aaa[0] = 2;///第2层
            aaa[1] = 3;///有1个弟兄
            aaa[2] = 0;//弟兄中的索引（从左到右）
            mVector = new Vector3(-40, -70, 0);
            mPosDic.Add(aaa, mVector);


            aaa = new int[3];
            aaa[0] = 2;///第2层
            aaa[1] = 3;///有2个弟兄
            aaa[2] = 1;//弟兄中的索引（从左到右）
            mVector = new Vector3(0, -70, 0);
            mPosDic.Add(aaa, mVector);


            aaa = new int[3];
            aaa[0] = 2;///第2层
            aaa[1] = 3;///有2个弟兄
            aaa[2] = 2;//弟兄中的索引（从左到右）
            mVector = new Vector3(40, -70, 0);
            mPosDic.Add(aaa, mVector);


            aaa = new int[3];
            aaa[0] = 3;///第3层
            aaa[1] = 1;///有1个弟兄
            aaa[2] = 0;//弟兄中的索引（从左到右）
            mVector = new Vector3(0, -70, 0);
            mPosDic.Add(aaa, mVector);


            aaa = new int[3];
            aaa[0] = 3;///第3层
            aaa[1] = 2;///有2个弟兄
            aaa[2] = 0;//弟兄中的索引（从左到右）
            mVector = new Vector3(-40, -70, 0);
            mPosDic.Add(aaa, mVector);


            aaa = new int[3];
            aaa[0] = 3;///第2层
            aaa[1] = 2;///有2个弟兄
            aaa[2] = 1;//弟兄中的索引（从左到右）
            mVector = new Vector3(40, -70, 0);
            mPosDic.Add(aaa, mVector);

            aaa = new int[3];
            aaa[0] = 3;///第2层
            aaa[1] = 3;///有1个弟兄
            aaa[2] = 0;//弟兄中的索引（从左到右）
            mVector = new Vector3(-40, -70, 0);
            mPosDic.Add(aaa, mVector);


            aaa = new int[3];
            aaa[0] = 3;///第2层
            aaa[1] = 3;///有2个弟兄
            aaa[2] = 1;//弟兄中的索引（从左到右）
            mVector = new Vector3(0, -70, 0);
            mPosDic.Add(aaa, mVector);


            aaa = new int[3];
            aaa[0] = 3;///第2层
            aaa[1] = 3;///有2个弟兄
            aaa[2] = 2;//弟兄中的索引（从左到右）
            mVector = new Vector3(40, -70, 0);
            mPosDic.Add(aaa, mVector);

        }


        public Vector3 GetLocalPos(int layer,int cout,int index)
        {
            foreach(var v in mPosDic)
            {
                if (v.Key[0] == layer && v.Key[1] == cout && v.Key[2]==index)
                {
                    return v.Value;
                }
            }
            DebugSystem.LogError("没有找到相关位置信息");
            return Vector3.zero;
        }
    }

}