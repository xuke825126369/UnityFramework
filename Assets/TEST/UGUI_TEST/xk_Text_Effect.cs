using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public class xk_Text_Effect : BaseMeshEffect
{
    public override void ModifyMesh(VertexHelper vh)
    {
        Debug.LogError("ModifyMesh");
        List<UIVertex> vertices = new List<UIVertex>();
        vh.GetUIVertexStream(vertices);
        Debug.Log("定點的shuliangd的："+vertices.Count);
        Debug.Log("定點的shuliangd的：" + vh.currentVertCount);
        UIVertex mV=new UIVertex();
        for(int i=0;i<vh.currentVertCount;i++)
        {
            vh.PopulateUIVertex(ref mV, i);
           mV.position +=new Vector3(100, 0, 0);
            vh.SetUIVertex(mV,i);
            Debug.Log(i+"位置："+mV.position);
            // Debug.Log("矩形：" + v.tangent);
            //Debug.Log("uv0：" + v.uv0);
            // Debug.Log("uv1：" + v.uv1);
        }
        

    }
}
