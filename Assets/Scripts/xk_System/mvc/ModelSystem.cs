using System.Collections.Generic;
using System;
using xk_System.Net.Client;
//using game.protobuf.data;
using XkProtobufData;
using xk_System.Debug;
using System.Reflection;
using xk_System.Net;
using Google.Protobuf;

namespace xk_System.Model
{
    public class ModelSystem : Singleton<ModelSystem>
    {
        private Dictionary<Type, xk_Model> mModelDic;

        public ModelSystem()
        {
            mModelDic = new Dictionary<Type, xk_Model>();
        }

        public T GetModel<T>() where T : xk_Model, new()
        {
            Type mType = typeof(T);
            if (mModelDic.ContainsKey(mType))
            {
                return mModelDic[mType] as T;
            }
            else
            {
                return addModel<T>();
            }
        }

        private T addModel<T>() where T : xk_Model, new()
        {
            Type mType = typeof(T);
            if (!mModelDic.ContainsKey(mType))
            {
                xk_Model t = new T();
                t.initModel();
                mModelDic.Add(mType, t);
                return t as T;
            }
            return null;
        }

        public void removeModel<T>() where T : xk_Model
        {
            Type mType = typeof(T);
            if (mModelDic.ContainsKey(mType))
            {
                xk_Model mModel = mModelDic[mType];
                mModel.destroyModel();
                mModelDic.Remove(mType);
            }
        }
    }

    public abstract class xk_Model
    {
        public xk_Model()
        {

        }

        public virtual void initModel()
        {

        }


        public virtual void destroyModel()
        {

        }

        public T GetModel<T>() where T : xk_Model, new()
        {
            return ModelSystem.Instance.GetModel<T>();
        }
    }

    public class DataModel:xk_Model
    {
        private Dictionary<string, List<Action<object>>> m_dicDataBinding = null;
        private Type m_thisType;

        public override void initModel()
        {
            base.initModel();
            m_dicDataBinding = new Dictionary<string, List<Action<object>>>();
            this.m_thisType = base.GetType();
        }

        public override void destroyModel()
        {
            base.destroyModel();
            m_dicDataBinding.Clear();       
        }

        private object _getPropertyValue(string strName)
        {
            FieldInfo field = this.m_thisType.GetField(strName);
            if (field != null)
            {
                return field.GetValue(this);
            }
            return base.GetType().GetProperty(strName).GetValue(this, null);
        }

        public void addDataBind(Action<object> callBack, string propertyName)
        {
            List<Action<object>> list = null;
            if (this.m_dicDataBinding.ContainsKey(propertyName))
            {
                list = this.m_dicDataBinding[propertyName];
            }
            else
            {
                list = new List<Action<object>>();
                this.m_dicDataBinding.Add(propertyName, list);
            }
            list.Add(callBack);
        }

        public void removeDataBind(Action<object> callBack, string propertyName)
        {
            List<Action<object>> list = null;
            if (this.m_dicDataBinding.ContainsKey(propertyName))
            {
                list = this.m_dicDataBinding[propertyName];
                if (list.Contains(callBack))
                {
                    list.Remove(callBack);
                }
            }
        }

        internal void updateBind(string propertyName)
        {
            if (this.m_dicDataBinding.ContainsKey(propertyName))
            {
                object objData = this._getPropertyValue(propertyName);
                List<Action<object>> list = new List<Action<object>>(this.m_dicDataBinding[propertyName]);
                foreach (Action<object> delegate2 in list)
                {
                    delegate2(objData);
                }
            }
        }
    }

    public class NetModel : xk_Model
    {
		protected void addNetListenFun(ProtoCommand command, Action<NetPackage> mFun)
        {
			//NetManager.Instance.addNetListenFun((int)command, mFun);
        }

		protected void removeNetListenFun(ProtoCommand command, Action<NetPackage> mFun)
        {
			//NetManager.Instance.removeNetListenFun((int)command, mFun);
        }

        protected void sendNetData(ProtoCommand command, object data)
        {
			//NetManager.Instance.sendNetData((int)command, data);
        }    
	}
}