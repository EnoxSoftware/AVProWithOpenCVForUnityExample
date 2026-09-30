#if UNITY_STANDALONE_WIN

using OpenCVForUnity.CoreModule;
using OpenCVForUnity.Extensions.SourceToMat;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityIntegration;
using OpenCVForUnity.UnityIntegration.Helper.SourceToMat;
using OpenCVForUnity.UnityIntegration.Helper.UI;
using RenderHeads.Media.AVProLiveCamera;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace AVProWithOpenCVForUnityExample
{
    /// <summary>
    /// AVProLiveCamera AsyncGPUReadbackToMatHelper Example
    /// Streams AVPro Live Camera frames into OpenCV <see cref="Mat"/> buffers via <see cref="AsyncGPUReadbackToMatHelper"/>.
    ///
    /// Demonstrates:
    /// - Wiring AVPro Live Camera <see cref="AVProLiveCameraDevice.OutputTexture"/> to <see cref="AsyncGPUReadbackToMatHelper.SourceTexture"/>
    /// - Receiving frames via <see cref="SourceToMatHelperBase.OnFrameMatUpdated"/> (Inspector wiring)
    /// - Calling <see cref="SourceToMatHelperBase.Play"/> in OnInitialized only when not already playing or paused
    /// - Recreating the preview texture on <see cref="SourceToMatHelperBase.OnFrameMatLayoutChanged"/>
    /// - <see cref="SourceToMatControlPanel"/> for Transport / Transform UI
    /// </summary>
    [RequireComponent(typeof(AsyncGPUReadbackToMatHelper))]
    public class AVProLiveCameraAsyncGPUReadbackToMatHelperExample : MonoBehaviour
    {
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

        // Private Fields
        private Texture2D _texture;
        private AsyncGPUReadbackToMatHelper _asyncGpuReadbackToMatHelper;
        private OpenCVForUnity.UnityIntegration.Helper.UI.FpsMonitor _fpsMonitor;
        private SourceToMatControlPanel _controlPanel;
        private AVProLiveCameraDevice _device;

        // Unity Lifecycle Methods
        private void Start()
        {
            _fpsMonitor = GetComponent<OpenCVForUnity.UnityIntegration.Helper.UI.FpsMonitor>();
            _asyncGpuReadbackToMatHelper = GetComponent<AsyncGPUReadbackToMatHelper>();

            _asyncGpuReadbackToMatHelper.OutputColorFormat = SourceToMatColorFormat.RGBA;

            WireSourceToMatControlPanelHooks();
        }

        private void Update()
        {
            if (_camera != null)
            {
                _device = _camera.Device;
            }

            if (_device != null && _device.IsActive && !_device.IsPaused && _device.OutputTexture != null)
            {
                if (_asyncGpuReadbackToMatHelper.SourceTexture != _device.OutputTexture)
                {
                    _asyncGpuReadbackToMatHelper.SourceTexture = _device.OutputTexture;
                    _asyncGpuReadbackToMatHelper.Initialize();
                }
            }
        }

        private void OnDestroy()
        {
            UnwireSourceToMatControlPanelHooks();
        }

        // Public Methods
        /// <summary>
        /// Raises the helper frame mat updated event.
        /// Updates the preview texture when a new frame is available during playback.
        /// </summary>
        public void OnSourceToMatHelperFrameMatUpdated()
        {
            if (!_asyncGpuReadbackToMatHelper.IsPlaying)
            {
                return;
            }

            Mat rgbaMat = _asyncGpuReadbackToMatHelper.FrameMat;
            if (rgbaMat == null || _texture == null)
            {
                return;
            }

            Imgproc.putText(
                rgbaMat,
                "AVPro With OpenCV for Unity Example",
                new Point(50, rgbaMat.rows() / 2),
                Imgproc.FONT_HERSHEY_SIMPLEX,
                2.0,
                new Scalar(255, 0, 0, 255),
                5,
                Imgproc.LINE_AA,
                false);
            Imgproc.putText(
                rgbaMat,
                "W:" + rgbaMat.width() + " H:" + rgbaMat.height() + " SO:" + Screen.orientation,
                new Point(5, rgbaMat.rows() - 10),
                Imgproc.FONT_HERSHEY_SIMPLEX,
                1.0,
                new Scalar(255, 255, 255, 255),
                2,
                Imgproc.LINE_AA,
                false);

            OpenCVMatUnityUtils.MatToTexture2D(rgbaMat, _texture);
        }

        /// <summary>
        /// Raises the helper initialized event.
        /// Recreates the preview texture and starts playback on first initialization.
        /// </summary>
        public void OnSourceToMatHelperInitialized()
        {
            Debug.Log("OnSourceToMatHelperInitialized", this);

            RecreatePreviewTexture();

            if (_fpsMonitor != null)
            {
                _fpsMonitor.Clear();
                UpdateFpsMonitorPlaybackState();
                _fpsMonitor.Add("SourceTexture", _asyncGpuReadbackToMatHelper.SourceTexture != null ? _asyncGpuReadbackToMatHelper.SourceTexture.name : "null");
                _fpsMonitor.Add("Width", _asyncGpuReadbackToMatHelper.Width.ToString());
                _fpsMonitor.Add("Height", _asyncGpuReadbackToMatHelper.Height.ToString());
                _fpsMonitor.Add("MatUpdateFPS", _asyncGpuReadbackToMatHelper.MatUpdateFPS.ToString());
                _fpsMonitor.Add("RequestedUseAsyncGPUReadback", _asyncGpuReadbackToMatHelper.RequestedUseAsyncGPUReadback.ToString());
                _fpsMonitor.Add("EffectiveUseAsyncGPUReadback", _asyncGpuReadbackToMatHelper.EffectiveUseAsyncGPUReadback.ToString());
                _fpsMonitor.Add("Rotate90Degree", _asyncGpuReadbackToMatHelper.Rotate90Degree.ToString());
                _fpsMonitor.Add("FlipVertical", _asyncGpuReadbackToMatHelper.FlipVertical.ToString());
                _fpsMonitor.Add("FlipHorizontal", _asyncGpuReadbackToMatHelper.FlipHorizontal.ToString());
                _fpsMonitor.Add("Orientation", Screen.orientation.ToString());

                if (_device != null)
                {
                    _fpsMonitor.Add("CameraWidth", _device.CurrentWidth.ToString());
                    _fpsMonitor.Add("CameraHeight", _device.CurrentHeight.ToString());
                    _fpsMonitor.Add("CameraFrameRate", _device.CurrentFrameRate.ToString());
                }

                _fpsMonitor.ConsoleText = string.Empty;
            }

            if (!_asyncGpuReadbackToMatHelper.IsPlaying && !_asyncGpuReadbackToMatHelper.IsPaused)
            {
                _asyncGpuReadbackToMatHelper.Play();
                UpdateFpsMonitorPlaybackState();
            }
        }

        /// <summary>
        /// Raises the helper frame mat layout changed event.
        /// </summary>
        public void OnSourceToMatHelperFrameMatLayoutChanged()
        {
            Debug.Log("OnSourceToMatHelperFrameMatLayoutChanged", this);

            RecreatePreviewTexture();

            if (_fpsMonitor != null)
            {
                _fpsMonitor.Add("Width", _asyncGpuReadbackToMatHelper.Width.ToString());
                _fpsMonitor.Add("Height", _asyncGpuReadbackToMatHelper.Height.ToString());
                _fpsMonitor.Add("Orientation", Screen.orientation.ToString());
            }
        }

        /// <summary>
        /// Raises the helper released event.
        /// </summary>
        public void OnSourceToMatHelperReleased()
        {
            Debug.Log("OnSourceToMatHelperReleased", this);

            if (_fpsMonitor != null)
            {
                _fpsMonitor.Clear();
            }

            CleanupPreviewResources();
        }

        /// <summary>
        /// Raises the helper disposed event.
        /// </summary>
        public void OnSourceToMatHelperDisposed()
        {
            Debug.Log("OnSourceToMatHelperDisposed", this);

            CleanupPreviewResources();
        }

        /// <summary>
        /// Raises the helper error occurred event.
        /// </summary>
        /// <param name="errorCode">Error code.</param>
        /// <param name="message">Message.</param>
        public void OnSourceToMatHelperErrorOccurred(SourceToMatErrorCode errorCode, string message)
        {
            Debug.Log("OnSourceToMatHelperErrorOccurred " + errorCode + ":" + message, this);

            if (_fpsMonitor != null)
            {
                _fpsMonitor.ConsoleText = "ErrorCode: " + errorCode + ":" + message;
            }
        }

        /// <summary>
        /// Raises the back button click event.
        /// </summary>
        public async void OnBackButtonClick()
        {
            if (_asyncGpuReadbackToMatHelper.IsPlaying || _asyncGpuReadbackToMatHelper.IsPaused)
            {
                await _asyncGpuReadbackToMatHelper.StopAsync();
            }

            await _asyncGpuReadbackToMatHelper.DisposeAsync();

            SceneManager.LoadScene("AVProWithOpenCVForUnityExample");
        }

        /// <summary>
        /// Invoked by <see cref="SourceToMatControlPanel.OnAfterPlay"/>.
        /// </summary>
        public void OnControlPanelAfterPlay()
        {
            UpdateFpsMonitorPlaybackState();
        }

        /// <summary>
        /// Invoked by <see cref="SourceToMatControlPanel.OnAfterPause"/>.
        /// </summary>
        public void OnControlPanelAfterPause()
        {
            UpdateFpsMonitorPlaybackState();
        }

        /// <summary>
        /// Invoked by <see cref="SourceToMatControlPanel.OnAfterStop"/>.
        /// </summary>
        public void OnControlPanelAfterStop()
        {
            UpdateFpsMonitorPlaybackState();
        }

        /// <summary>
        /// Invoked by <see cref="SourceToMatControlPanel.OnRotate90Changed"/>.
        /// </summary>
        /// <param name="isOn">New Rotate90Degree value applied by the panel.</param>
        public void OnControlPanelRotate90Changed(bool isOn)
        {
            if (_fpsMonitor != null)
            {
                _fpsMonitor.Add("Rotate90Degree", isOn.ToString());
            }
        }

        /// <summary>
        /// Invoked by <see cref="SourceToMatControlPanel.OnFlipVerticalChanged"/>.
        /// </summary>
        /// <param name="isOn">New FlipVertical value applied by the panel.</param>
        public void OnControlPanelFlipVerticalChanged(bool isOn)
        {
            if (_fpsMonitor != null)
            {
                _fpsMonitor.Add("FlipVertical", isOn.ToString());
            }
        }

        /// <summary>
        /// Invoked by <see cref="SourceToMatControlPanel.OnFlipHorizontalChanged"/>.
        /// </summary>
        /// <param name="isOn">New FlipHorizontal value applied by the panel.</param>
        public void OnControlPanelFlipHorizontalChanged(bool isOn)
        {
            if (_fpsMonitor != null)
            {
                _fpsMonitor.Add("FlipHorizontal", isOn.ToString());
            }
        }

        // Private Methods
        private void RecreatePreviewTexture()
        {
            Mat imageMat = _asyncGpuReadbackToMatHelper.FrameMat;
            if (imageMat == null)
            {
                return;
            }

            if (_texture != null)
            {
                Texture2D.Destroy(_texture);
            }

            _texture = new Texture2D(imageMat.cols(), imageMat.rows(), TextureFormat.RGBA32, false);

            OpenCVMatUnityUtils.MatToTexture2D(imageMat, _texture);

            if (ResultPreview != null)
            {
                ResultPreview.texture = _texture;
                AspectRatioFitter aspectRatioFitter = ResultPreview.GetComponent<AspectRatioFitter>();
                if (aspectRatioFitter != null)
                {
                    aspectRatioFitter.aspectRatio = (float)_texture.width / _texture.height;
                }
            }
        }

        private void CleanupPreviewResources()
        {
            DestroyPreviewTexture();
            UpdateFpsMonitorPlaybackState();
        }

        private void DestroyPreviewTexture()
        {
            if (_texture != null)
            {
                Texture2D.Destroy(_texture);
                _texture = null;
            }
        }

        private void UpdateFpsMonitorPlaybackState()
        {
            if (_fpsMonitor == null)
            {
                return;
            }

            _fpsMonitor.Add("PlaybackState", GetPlaybackStateText());
            _fpsMonitor.Add("EffectiveUseAsyncGPUReadback", _asyncGpuReadbackToMatHelper.EffectiveUseAsyncGPUReadback.ToString());
        }

        private string GetPlaybackStateText()
        {
            if (!_asyncGpuReadbackToMatHelper.IsInitialized)
            {
                return "Uninitialized";
            }

            if (_asyncGpuReadbackToMatHelper.IsPlaying)
            {
                return "Playing";
            }

            if (_asyncGpuReadbackToMatHelper.IsPaused)
            {
                return "Paused";
            }

            return "Ready";
        }

        private void WireSourceToMatControlPanelHooks()
        {
            _controlPanel = GetComponent<SourceToMatControlPanel>();
            if (_controlPanel == null)
            {
                return;
            }

            _controlPanel.OnAfterPlay.AddListener(OnControlPanelAfterPlay);
            _controlPanel.OnAfterPause.AddListener(OnControlPanelAfterPause);
            _controlPanel.OnAfterStop.AddListener(OnControlPanelAfterStop);
            _controlPanel.OnRotate90Changed.AddListener(OnControlPanelRotate90Changed);
            _controlPanel.OnFlipVerticalChanged.AddListener(OnControlPanelFlipVerticalChanged);
            _controlPanel.OnFlipHorizontalChanged.AddListener(OnControlPanelFlipHorizontalChanged);
        }

        private void UnwireSourceToMatControlPanelHooks()
        {
            if (_controlPanel == null)
            {
                return;
            }

            _controlPanel.OnAfterPlay.RemoveListener(OnControlPanelAfterPlay);
            _controlPanel.OnAfterPause.RemoveListener(OnControlPanelAfterPause);
            _controlPanel.OnAfterStop.RemoveListener(OnControlPanelAfterStop);
            _controlPanel.OnRotate90Changed.RemoveListener(OnControlPanelRotate90Changed);
            _controlPanel.OnFlipVerticalChanged.RemoveListener(OnControlPanelFlipVerticalChanged);
            _controlPanel.OnFlipHorizontalChanged.RemoveListener(OnControlPanelFlipHorizontalChanged);
            _controlPanel = null;
        }
    }
}

#endif
