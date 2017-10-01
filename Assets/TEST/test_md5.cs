using UnityEngine;
using System.Collections;
using xk_System.Debug;
using System.Security.Cryptography;
using xk_System.Crypto;
using System.Text;
using System;

public class test_md5 : MonoBehaviour {

	// Use this for initialization
	void Start ()
    {

        string data = "123456789asdffasfdasdfasdfasdfasdfasdfsdfasfasdfasdfsdfasdfasfasdfasdfsadfsfsdfsdfsdfsfsdfsdfsdfsdfsdfsdfsdfsdfsdf";
        string key = "1234567891234567";
        string iv = "1234567891234567";
        byte[] data_byte = Encoding.UTF8.GetBytes(data);
        byte[] aaaa=EncryptionSystem.getSingle<Encryption_AES>().Encryption(data_byte,key,iv);
        if(aaaa==null)
        {
            DebugSystem.LogError("aaaa错误");
        }
        DebugSystem.Log("aaaachangdu:"+aaaa.Length);
        string aaStr="";
        foreach(byte b in aaaa)
        {
            aaStr += b+" | ";
        }
        DebugSystem.Log(aaStr);
        byte[] bbb= EncryptionSystem.getSingle<Encryption_AES>().Decryption(aaaa, key, iv);
        if (bbb == null)
        {
            DebugSystem.LogError("bbb错误");
        }
        DebugSystem.Log("bbb:" + Encoding.UTF8.GetString(bbb));
    }

	void Update ()
    {

       
    }
}
