using UnityEngine;
using UnityEngine.SceneManagement;

namespace AVProWithOpenCVForUnityExample
{
    public class ShowLicense : MonoBehaviour
    {

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void OnBackButton()
        {
            SceneManager.LoadScene("AVProWithOpenCVForUnityExample");
        }
    }
}
