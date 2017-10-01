using UnityEngine;
using System.Collections;

namespace Test
{
    public class Net_Test : MonoBehaviour {

        public int Cout;
        public test mTest;
        // Use this for initialization
        void Start()
        {
            StartCoroutine(Run());
        }

        IEnumerator Run()
        {
            int i = 0;
            while (i<50)
            {
                Instantiate(mTest.gameObject);
                i++;
                yield return 0;
            }
        }

        // Update is called once per frame
        void Update() {

        }
    }
}