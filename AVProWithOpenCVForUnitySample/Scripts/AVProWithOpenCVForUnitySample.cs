using UnityEngine;
using System.Collections;

#if UNITY_5_3 || UNITY_5_3_OR_NEWER
using UnityEngine.SceneManagement;
#endif

namespace AVProWithOpenCVForUnitySample
{
    public class AVProWithOpenCVForUnitySample : MonoBehaviour
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

        public void OnGetReadableTextureSample ()
        {
            #if UNITY_5_3 || UNITY_5_3_OR_NEWER
            SceneManager.LoadScene ("GetReadableTextureSample");
            #else
            Application.LoadLevel ("GetReadableTextureSample");
            #endif
        }

        public void OnExtractFrameSample ()
        {
            #if UNITY_5_3 || UNITY_5_3_OR_NEWER
            SceneManager.LoadScene ("ExtractFrameSample");
            #else
            Application.LoadLevel ("ExtractFrameSample");
            #endif
        }
    }
}
