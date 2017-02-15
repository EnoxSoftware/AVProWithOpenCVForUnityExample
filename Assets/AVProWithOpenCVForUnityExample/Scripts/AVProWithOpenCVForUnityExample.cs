using UnityEngine;
using System.Collections;

#if UNITY_5_3 || UNITY_5_3_OR_NEWER
using UnityEngine.SceneManagement;
#endif

namespace AVProWithOpenCVForUnityExample
{
    public class AVProWithOpenCVForUnityExample : MonoBehaviour
    {

        // Use this for initialization
        void Start ()
        {
    
        }
    
        // Update is called once per frame
        void Update ()
        {
    
        }

        public void OnShowLicenseButton ()
        {
            #if UNITY_5_3 || UNITY_5_3_OR_NEWER
            SceneManager.LoadScene ("ShowLicense");
#else
            Application.LoadLevel ("ShowLicense");
#endif
        }

        public void OnGetReadableTextureExample ()
        {
            #if UNITY_5_3 || UNITY_5_3_OR_NEWER
            SceneManager.LoadScene ("GetReadableTextureExample");
            #else
            Application.LoadLevel ("GetReadableTextureExample");
            #endif
        }

        public void OnExtractFrameExample ()
        {
            #if UNITY_5_3 || UNITY_5_3_OR_NEWER
            SceneManager.LoadScene ("ExtractFrameExample");
            #else
            Application.LoadLevel ("ExtractFrameExample");
            #endif
        }
    }
}
