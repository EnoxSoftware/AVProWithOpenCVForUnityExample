
#if UNITY_STANDALONE_WIN
using System;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityIntegration;
using OpenCVForUnity.UnityIntegration.Helper.Source2Mat;
using RenderHeads.Media.AVProLiveCamera;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace AVProWithOpenCVForUnityExample
{
    /// <summary>
    /// AVProLiveCamera AsyncGPUReadback2MatHelper Example
    /// </summary>
    [RequireComponent(typeof(AsyncGPUReadback2MatHelper))]
    public class AVProLiveCameraAsyncGPUReadback2MatHelperExample : MonoBehaviour
    {
        // Enums
        public enum FPSPreset : int
        {
            _0 = 0,
            _1 = 1,
            _5 = 5,
            _10 = 10,
            _15 = 15,
            _30 = 30,
            _60 = 60,
        }

        // Constants

        // Public Fields
        [Header("AVProLiveCamera")]
        /// <summary>
        /// The AVProLiveCamera.
        /// </summary>
        public AVProLiveCamera _camera;

        [Header("Output")]
        /// <summary>
        /// The RawImage for previewing the result.
        /// </summary>
        public RawImage ResultPreview;

        [Space(10)]

        /// <summary>
        /// The requested mat update FPS dropdown.
        /// </summary>
        public Dropdown RequestedMatUpdateFPSDropdown;

        /// <summary>
        /// The requested mat update FPS.
        /// </summary>
        public FPSPreset RequestedMatUpdateFPS = FPSPreset._30;

        /// <summary>
        /// The rotate 90 degree toggle.
        /// </summary>
        public Toggle Rotate90DegreeToggle;

        /// <summary>
        /// The flip vertical toggle.
        /// </summary>
        public Toggle FlipVerticalToggle;

        /// <summary>
        /// The flip horizontal toggle.
        /// </summary>
        public Toggle FlipHorizontalToggle;

        // Private Fields
        /// <summary>
        /// The texture.
        /// </summary>
        private Texture2D _texture;

        /// <summary>
        /// The async GPU readback to mat helper.
        /// </summary>
        private AsyncGPUReadback2MatHelper _asyncGPUReadback2MatHelper;

        /// <summary>
        /// The FPS monitor.
        /// </summary>
        private FpsMonitor _fpsMonitor;

        /// <summary>
        /// The AVProLiveCamera device.
        /// </summary>
        private AVProLiveCameraDevice _device;

        // Unity Lifecycle Methods
        private void Start()
        {
            _fpsMonitor = GetComponent<FpsMonitor>();

            // Get the AsyncGPUReadback2MatHelper component attached to the current game object
            _asyncGPUReadback2MatHelper = gameObject.GetComponent<AsyncGPUReadback2MatHelper>();

            // Update GUI state
            string[] enumNames = System.Enum.GetNames(typeof(FPSPreset));
            int index = Array.IndexOf(enumNames, RequestedMatUpdateFPS.ToString());
            RequestedMatUpdateFPSDropdown.value = index;
            Rotate90DegreeToggle.isOn = _asyncGPUReadback2MatHelper.Rotate90Degree;
            FlipVerticalToggle.isOn = _asyncGPUReadback2MatHelper.FlipVertical;
            FlipHorizontalToggle.isOn = _asyncGPUReadback2MatHelper.FlipHorizontal;
        }

        private void Update()
        {
            if (_camera != null)
                _device = _camera.Device;

            if (_device != null && _device.IsActive && !_device.IsPaused && _device.OutputTexture != null)
            {
                // Update source texture if it has changed
                if (_asyncGPUReadback2MatHelper.SourceTexture != _device.OutputTexture)
                {
                    _asyncGPUReadback2MatHelper.SourceTexture = _device.OutputTexture;
                    _asyncGPUReadback2MatHelper.Initialize();
                }
            }

            // Check if the async GPU readback is playing and if a new frame was updated
            if (_asyncGPUReadback2MatHelper.IsPlaying() && _asyncGPUReadback2MatHelper.DidUpdateThisFrame())
            {
                // Retrieve the current frame as a Mat object
                Mat rgbaMat = _asyncGPUReadback2MatHelper.GetMat();

                // Add text overlay on the frame
                Imgproc.putText(rgbaMat, "AVPro With OpenCV for Unity Example", new Point(50, rgbaMat.rows() / 2), Imgproc.FONT_HERSHEY_SIMPLEX, 2.0, new Scalar(255, 0, 0, 255), 5, Imgproc.LINE_AA, false);
                Imgproc.putText(rgbaMat, "W:" + rgbaMat.width() + " H:" + rgbaMat.height() + " SO:" + Screen.orientation, new Point(5, rgbaMat.rows() - 10), Imgproc.FONT_HERSHEY_SIMPLEX, 1.0, new Scalar(255, 255, 255, 255), 2, Imgproc.LINE_AA, false);

                // Convert the Mat to a Texture2D to display it on a texture
                OpenCVMatUtils.MatToTexture2D(rgbaMat, _texture);
            }
        }

        private void OnDestroy()
        {
            // Dispose of the asyncGPUReadback2MatHelper object and release any resources held by it.
            _asyncGPUReadback2MatHelper?.Dispose();
        }

        // Public Methods
        /// <summary>
        /// Raises the async GPU readback to mat helper initialized event.
        /// </summary>
        public void OnAsyncGPUReadback2MatHelperInitialized()
        {
            Debug.Log("OnAsyncGPUReadback2MatHelperInitialized");

            // Retrieve the current frame from the AsyncGPUReadback2MatHelper as a Mat object
            Mat asyncGPUReadbackMat = _asyncGPUReadback2MatHelper.GetMat();
            asyncGPUReadbackMat.setTo(new Scalar(0, 0, 0, 255));

            // Create a new Texture2D with the same dimensions as the Mat and RGBA32 color format
            _texture = new Texture2D(asyncGPUReadbackMat.cols(), asyncGPUReadbackMat.rows(), TextureFormat.RGBA32, false);

            // Convert the Mat to a Texture2D, effectively transferring the image data
            OpenCVMatUtils.MatToTexture2D(asyncGPUReadbackMat, _texture);

            // Set the Texture2D as the texture of the RawImage for preview.
            ResultPreview.texture = _texture;
            ResultPreview.GetComponent<AspectRatioFitter>().aspectRatio = (float)_texture.width / _texture.height;

            if (_fpsMonitor != null)
            {
                _fpsMonitor.Add("SourceTexture", _asyncGPUReadback2MatHelper.SourceTexture != null ? _asyncGPUReadback2MatHelper.SourceTexture.name : "null");
                _fpsMonitor.Add("Width", _asyncGPUReadback2MatHelper.GetWidth().ToString());
                _fpsMonitor.Add("Height", _asyncGPUReadback2MatHelper.GetHeight().ToString());
                _fpsMonitor.Add("Rotate90Degree", _asyncGPUReadback2MatHelper.Rotate90Degree.ToString());
                _fpsMonitor.Add("FlipVertical", _asyncGPUReadback2MatHelper.FlipVertical.ToString());
                _fpsMonitor.Add("FlipHorizontal", _asyncGPUReadback2MatHelper.FlipHorizontal.ToString());
                _fpsMonitor.Add("Orientation", Screen.orientation.ToString());

                // Add AVProLiveCamera information
                if (_device != null)
                {
                    _fpsMonitor.Add("CameraWidth", _device.CurrentWidth.ToString());
                    _fpsMonitor.Add("CameraHeight", _device.CurrentHeight.ToString());
                    _fpsMonitor.Add("CameraFrameRate", _device.CurrentFrameRate.ToString());
                }
            }
        }

        /// <summary>
        /// Raises the async GPU readback to mat helper disposed event.
        /// </summary>
        public void OnAsyncGPUReadback2MatHelperDisposed()
        {
            Debug.Log("OnAsyncGPUReadback2MatHelperDisposed");

            // Destroy the texture and set it to null
            if (_texture != null) Texture2D.Destroy(_texture); _texture = null;
        }

        /// <summary>
        /// Raises the async GPU readback to mat helper error occurred event.
        /// </summary>
        /// <param name="errorCode">Error code.</param>
        /// <param name="message">Message.</param>
        public void OnAsyncGPUReadback2MatHelperErrorOccurred(Source2MatHelperErrorCode errorCode, string message)
        {
            Debug.Log("OnAsyncGPUReadback2MatHelperErrorOccurred " + errorCode + ":" + message);

            // if (_fpsMonitor != null)
            // {
            //     _fpsMonitor.consoleText = "ErrorCode: " + errorCode + ":" + message;
            // }
        }

        /// <summary>
        /// Raises the back button click event.
        /// </summary>
        public void OnBackButtonClick()
        {
            // Load the specified scene when the back button is clicked
            SceneManager.LoadScene("AVProWithOpenCVForUnityExample");
        }

        /// <summary>
        /// Raises the play button click event.
        /// </summary>
        public void OnPlayButtonClick()
        {
            _asyncGPUReadback2MatHelper.Play();
        }

        /// <summary>
        /// Raises the pause button click event.
        /// </summary>
        public void OnPauseButtonClick()
        {
            _asyncGPUReadback2MatHelper.Pause();
        }

        /// <summary>
        /// Raises the stop button click event.
        /// </summary>
        public void OnStopButtonClick()
        {
            _asyncGPUReadback2MatHelper.Stop();
        }

        /// <summary>
        /// Raises the requested mat update FPS dropdown value changed event.
        /// </summary>
        public void OnRequestedMatUpdateFPSDropdownValueChanged(int result)
        {
            string[] enumNames = Enum.GetNames(typeof(FPSPreset));
            int value = (int)System.Enum.Parse(typeof(FPSPreset), enumNames[result], true);

            if ((int)RequestedMatUpdateFPS != value)
            {
                RequestedMatUpdateFPS = (FPSPreset)value;

                _asyncGPUReadback2MatHelper.RequestedMatUpdateFPS = (int)RequestedMatUpdateFPS;
            }
        }

        /// <summary>
        /// Raises the rotate 90 degree toggle value changed event.
        /// </summary>
        public void OnRotate90DegreeToggleValueChanged()
        {
            if (Rotate90DegreeToggle.isOn != _asyncGPUReadback2MatHelper.Rotate90Degree)
            {
                _asyncGPUReadback2MatHelper.Rotate90Degree = Rotate90DegreeToggle.isOn;

                if (_fpsMonitor != null)
                    _fpsMonitor.Add("Rotate90Degree", _asyncGPUReadback2MatHelper.Rotate90Degree.ToString());
            }
        }

        /// <summary>
        /// Raises the flip vertical toggle value changed event.
        /// </summary>
        public void OnFlipVerticalToggleValueChanged()
        {
            if (FlipVerticalToggle.isOn != _asyncGPUReadback2MatHelper.FlipVertical)
            {
                _asyncGPUReadback2MatHelper.FlipVertical = FlipVerticalToggle.isOn;

                if (_fpsMonitor != null)
                    _fpsMonitor.Add("FlipVertical", _asyncGPUReadback2MatHelper.FlipVertical.ToString());
            }
        }

        /// <summary>
        /// Raises the flip horizontal toggle value changed event.
        /// </summary>
        public void OnFlipHorizontalToggleValueChanged()
        {
            if (FlipHorizontalToggle.isOn != _asyncGPUReadback2MatHelper.FlipHorizontal)
            {
                _asyncGPUReadback2MatHelper.FlipHorizontal = FlipHorizontalToggle.isOn;

                if (_fpsMonitor != null)
                    _fpsMonitor.Add("FlipHorizontal", _asyncGPUReadback2MatHelper.FlipHorizontal.ToString());
            }
        }

    }
}
#endif

