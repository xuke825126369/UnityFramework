using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using xk_System.Crypto;
namespace TEST
{
    public class CSharp_Main : MonoBehaviour
    {
        void Start()
        {
            Encryption_RSA m = new Encryption_RSA();

            string pubkey = @"MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDvpBDYeWK1VGZCwETz5MHSzmZag8zgDDH89JqgJ1EUDThBrHYlbksR3ACORfvDh6J+jSKWoXIQJcS12ElyqzwUaTpTthnUh0jU8mJgOzJuTwtk2xZKNEk71M098h4tmriJCPMWEHMn+0U/qCoHSdzbftNwXTpjRrf6L4PyFAeePQIDAQAB";
            string data1 = @"appid=1283&attach=ggg&channel=2&createat=2016-09-09 14:14:09&orderid=egsdfvvsd1473401644.625039&ordername=屠龙刀&payat=2016-09-09 14:14:36&price=0.01&sandbox=1&startat=2016-09-09 14:14:15&status=5&transid=20160909141404613077331813616493&userid=12414141&username=2352tgsdvzcas";
            string sign = @"t+htvZ4Y7IcZajN3iVcYZdULl8m57vd51Rh1ewnt29VUUQDnJ9DFA5egdXxa7o/0w/h7w1PGKE4jYKo9pFbnetYKsbIfeCo/DZDFYILO2CHwVeJXU9AehbZ4CyqPnDqoMo6PpqvDlocfGufne5u10wxClFSSnrZyWE9kTIk9IUY=";
            bool result= m.Verify(pubkey,data1,sign);
            if(result==true)
            {
                Debug.Log("验证签名成功");
            }else
            {
                Debug.Log("验证签名失败");
            }



            
        }

    }
}