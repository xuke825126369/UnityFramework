using System.Collections;
using System.Reflection;
using xk_System.Debug;
using System;
namespace xk_System.Db
{
    public sealed class TestDb : DbBase
    {
        /// <summary>
        /// 
        /// </summary>
        public readonly int aaa;
        public readonly string eee;
        public readonly int[] fff;

        public TestDb(ArrayList list)
        {
            FieldInfo[] mFieldInfo = this.GetType().GetFields();
            for (int i = 0; i < mFieldInfo.Length; i++)
            {
                DebugSystem.Log(mFieldInfo[i].Name);
                mFieldInfo[i].SetValue(this,list[i]);

            }
        }

        public TestDb(int aaa,string eee,int[] fff)
        {
            this.aaa = aaa;
            this.eee = eee;
            this.fff = fff;
        }
    }
}