using UnityEngine;
using System.Collections;

public class ObjectRoot :SingleTonMonoBehaviour<ObjectRoot>
{
    public ui_2d ui_2d_root;
    public ui_3d ui_3d_root;
    public scene scene_root;

    [System.Serializable]
    public class ui_2d
    {
        public GameObject root;
        public Camera mCamera;
        public Transform mParent;
    }

    [System.Serializable]
    public class ui_3d
    {
        public GameObject root;
        public ui_3d_obj mObjPrefab;

        [System.Serializable]
        public class ui_3d_obj
        {
            public GameObject root;
            public Camera mCamera;
            public Transform mParent;
        }
    }

    [System.Serializable]
    public class scene
    {
        public GameObject root;
        public Camera mCamera;
    }

    public override void Init()
    {
        base.Init();
        ui_2d_root.root.SetActive(true);
        ui_3d_root.root.SetActive(false);
        scene_root.root.SetActive(false);
    }

}


