using System.Collections;
using OpenCVForUnity.UnityIntegration;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace AVProWithOpenCVForUnityExample
{
    public class AVProWithOpenCVForUnityExample : MonoBehaviour
    {
        // Constants
        private static float VERTICAL_NORMALIZED_POSITION = 1f;

        // Public Fields
        [Header("UI")]
        public Text ExampleTitle;
        public Text VersionInfo;
        public ScrollRect ScrollRect;

        // Unity Lifecycle Methods

        private void Start()
        {
            ExampleTitle.text = "AVProWithOpenCVForUnity Example " + Application.version;

            VersionInfo.text = OpenCVForUnity.CoreModule.Core.NATIVE_LIBRARY_NAME + " " + OpenCVEnv.GetVersion() + " (" + OpenCVForUnity.CoreModule.Core.VERSION + ")";
            VersionInfo.text += " / UnityEditor " + Application.unityVersion;
            VersionInfo.text += " / ";
#if UNITY_EDITOR
            VersionInfo.text += "Editor";
#elif UNITY_STANDALONE_WIN
            VersionInfo.text += "Windows";
#elif UNITY_STANDALONE_OSX
            VersionInfo.text += "Mac OSX";
#elif UNITY_STANDALONE_LINUX
            VersionInfo.text += "Linux";
#elif UNITY_ANDROID
            VersionInfo.text += "Android";
#elif UNITY_IOS
            VersionInfo.text += "iOS";
#elif UNITY_VISIONOS
            VersionInfo.text += "VisionOS";
#elif UNITY_WSA
            VersionInfo.text += "WSA";
#elif UNITY_WEBGL
            VersionInfo.text += "WebGL";
#endif
            VersionInfo.text += " ";
#if ENABLE_MONO
            VersionInfo.text += "Mono";
#elif ENABLE_IL2CPP
            VersionInfo.text += "IL2CPP";
#elif ENABLE_DOTNET
            VersionInfo.text += ".NET";
#endif

            ScrollRect.verticalNormalizedPosition = VERTICAL_NORMALIZED_POSITION;

            #if !UNITY_STANDALONE_WIN
            GameObject.Find("Canvas/Panel/SceneList/ScrollView/List/AVProLiveCameraGetFrameAsColor32ExampleButton").GetComponent<Button>().interactable = false;
            GameObject.Find("Canvas/Panel/SceneList/ScrollView/List/AVProLiveCameraAsyncGPUReadback2MatHelperExampleButton").GetComponent<Button>().interactable = false;
            #endif
        }

        // Public Methods
        public void OnScrollRectValueChanged()
        {
            VERTICAL_NORMALIZED_POSITION = ScrollRect.verticalNormalizedPosition;
        }

        public void OnShowLicenseButton()
        {
            SceneManager.LoadScene("ShowLicense");
        }

        public void OnAVProVideoGetReadableTextureExampleButton()
        {
            SceneManager.LoadScene("AVProVideoGetReadableTextureExample");
        }

        public void OnAVProVideoAsyncGPUReadback2MatHelperExampleButton()
        {
            SceneManager.LoadScene("AVProVideoAsyncGPUReadback2MatHelperExample");
        }

        public void OnAVProVideoExtractFrameExampleButton()
        {
            SceneManager.LoadScene("AVProVideoExtractFrameExample");
        }

        public void OnAVProLiveCameraGetFrameAsColor32ExampleButton()
        {
            SceneManager.LoadScene("AVProLiveCameraGetFrameAsColor32Example");
        }

        public void OnAVProLiveCameraAsyncGPUReadback2MatHelperExampleButton()
        {
            SceneManager.LoadScene("AVProLiveCameraAsyncGPUReadback2MatHelperExample");
        }
    }
}
