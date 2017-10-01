using System;
using System.Collections.Generic;
using System.Reflection;
namespace xk_System.Reflection
{
    public class ReflectionSystem :Singleton<ReflectionSystem>
    {
        public object GetFieldValue(System.Type type, string value)
        {
            if (type == typeof(int))
            {
                int nGetValue = 0;
                int.TryParse(value, out nGetValue);
                return nGetValue;
            }
            else if (type == typeof(string))
            {
                return value;
            }
            else if (type == typeof(short))
            {
                short nGetValue = 0;
                short.TryParse(value, out nGetValue);
                return nGetValue;
            }
            else if (type == typeof(float))
            {
                float nGetValue = 0;
                float.TryParse(value, out nGetValue);
                return nGetValue;
            }
            else if (type == typeof(double))
            {
                double nGetValue = 0;
                double.TryParse(value, out nGetValue);
                return nGetValue;
            }
            else if (type == typeof(byte))
            {
                byte nGetValue = 0;
                byte.TryParse(value, out nGetValue);
                return nGetValue;
            }
            else if (type == typeof(bool))
            {
                bool nGetValue = false;
                bool.TryParse(value, out nGetValue);
                return nGetValue;
            }
            else if (type.BaseType == typeof(Enum))
            {
                return Enum.Parse(type, value);
            }
            return null;
        }

        public void SetClassValue(object mObject, Dictionary<string, string> list)
        {
            FieldInfo[] mFieldInfo = mObject.GetType().GetFields();
            for (int i = 0; i < mFieldInfo.Length; i++)
            {
                FieldInfo mField = mFieldInfo[i];
                string key = mField.Name;
                string value = list[key];
                if (mFieldInfo[i].FieldType.IsArray)
                {
                    Array mArray = (Array)mField.GetValue(this);
                    string[] valueArray2 = value.Split(',');
                    string[] valueArray = new string[mArray.Length];
                    for (int k = 0; k < valueArray2.Length; k++)
                    {
                        valueArray[k] = valueArray2[k];
                    }
                    for (int j = 0; j < mArray.Length; j++)
                    {
                        mArray.SetValue(GetFieldValue(mField.FieldType.GetElementType(), valueArray[j]), j);
                    }
                }
                else
                {
                    mField.SetValue(mObject,GetFieldValue(mField.FieldType, value));
                }
            }
        }


    }
}