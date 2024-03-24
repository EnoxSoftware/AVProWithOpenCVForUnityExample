using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace AVProWithOpenCVForUnityExample
{
    public class AVProWithOpenCVForUnityExample : MonoBehaviour
    {

        [Header("UI")]
        public Text exampleTitle;
        public Text versionInfo;
        public ScrollRect scrollRect;
        private static float verticalNormalizedPosition = 1f;

        void Awake()
        {
            //QualitySettings.vSyncCount = 0;
            //Application.targetFrameRate = 60;
        }

        IEnumerator Start()
        {

            exampleTitle.text = "AVProWithOpenCVForUnity Example " + Application.version;

            versionInfo.text = OpenCVForUnity.CoreModule.Core.NATIVE_LIBRARY_NAME + " " + OpenCVForUnity.UnityUtils.Utils.getVersion() + " (" + OpenCVForUnity.CoreModule.Core.VERSION + ")";
            versionInfo.text += " / UnityEditor " + Application.unityVersion;
            versionInfo.text += " / ";
#if UNITY_EDITOR
            versionInfo.text += "Editor";
#elif UNITY_STANDALONE_WIN
            versionInfo.text += "Windows";
#elif UNITY_STANDALONE_OSX
            versionInfo.text += "Mac OSX";
#elif UNITY_STANDALONE_LINUX
            versionInfo.text += "Linux";
#elif UNITY_ANDROID
            versionInfo.text += "Android";
#elif UNITY_IOS
            versionInfo.text += "iOS";
#elif UNITY_WSA
            versionInfo.text += "WSA";
#elif UNITY_WEBGL
            versionInfo.text += "WebGL";
#endif
            versionInfo.text += " ";
#if ENABLE_MONO
            versionInfo.text += "Mono";
#elif ENABLE_IL2CPP
            versionInfo.text += "IL2CPP";
#elif ENABLE_DOTNET
            versionInfo.text += ".NET";
#endif

            scrollRect.verticalNormalizedPosition = verticalNormalizedPosition;

            yield break;
        }

        public void OnScrollRectValueChanged()
        {
            verticalNormalizedPosition = scrollRect.verticalNormalizedPosition;
        }

        public void OnShowLicenseButton()
        {
            SceneManager.LoadScene("ShowLicense");
        }

        public void OnAVProVideoGetReadableTextureExampleButton()
        {
            SceneManager.LoadScene("AVProVideoGetReadableTextureExample");
        }

        public void OnAVProVideoAsyncGPUReadbackExampleButton()
        {
            SceneManager.LoadScene("AVProVideoAsyncGPUReadbackExample");
        }

        public void OnAVProVideoExtractFrameExampleButton()
        {
            SceneManager.LoadScene("AVProVideoExtractFrameExample");
        }

        public void OnAVProLiveCameraGetFrameAsColor32ExampleButton()
        {
            SceneManager.LoadScene("AVProLiveCameraGetFrameAsColor32Example");
        }

        public void OnAVProLiveCameraAsyncGPUReadbackExampleButton()
        {
            SceneManager.LoadScene("AVProLiveCameraAsyncGPUReadbackExample");
        }
    }
}
