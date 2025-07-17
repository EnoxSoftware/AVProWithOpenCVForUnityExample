#if UNITY_STANDALONE_WIN
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityIntegration;
using RenderHeads.Media.AVProLiveCamera;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace AVProWithOpenCVForUnityExample
{
    /// <summary>
    /// AVProLiveCamera GetFrameAsColor32 Example
    /// An example of converting an AVProLiveCamera image frame to an OpenCV Mat using GetFrameAsColor32.
    /// </summary>
    public class AVProLiveCameraGetFrameAsColor32Example : MonoBehaviour
    {
        // Enums
        public enum ModeType
        {
            original,
            sepia,
            pixelize,
        }

        // Public Fields
        [Header("AVProLiveCamera")]
        public AVProLiveCamera Camera;

        [Header("Output")]
        /// <summary>
        /// The RawImage for previewing the result.
        /// </summary>
        public RawImage ResultPreview;

        [Space(10)]

        /// <summary>
        /// The original mode toggle.
        /// </summary>
        public Toggle OriginalModeToggle;

        /// <summary>
        /// The sepia mode toggle.
        /// </summary>
        public Toggle SepiaModeToggle;

        /// <summary>
        /// The pixelize mode toggle.
        /// </summary>
        public Toggle PixelizeModeToggle;

        // Private Fields
        private AVProLiveCameraDevice _device;

        private Color32[] _frameData;
        private int _frameWidth;
        private int _frameHeight;
        private GCHandle _frameHandle;
        private System.IntPtr _framePointer;
        private uint _lastFrame;

        /// <summary>
        /// The texture.
        /// </summary>
        private Texture2D _texture;

        /// <summary>
        /// The rgba mat.
        /// </summary>
        private Mat _rgbaMat;

        /// <summary>
        /// The m sepia kernel.
        /// </summary>
        private Mat _mSepiaKernel;

        /// <summary>
        /// The m size0.
        /// </summary>
        private Size _mSize0;

        /// <summary>
        /// The m intermediate mat.
        /// </summary>
        private Mat _mIntermediateMat;

        /// <summary>
        /// The mode.
        /// </summary>
        private ModeType _mode;

        // Unity Lifecycle Methods
        private void Start()
        {
            if (OriginalModeToggle.isOn)
            {
                _mode = ModeType.original;
            }
            if (SepiaModeToggle.isOn)
            {
                _mode = ModeType.sepia;
            }
            if (PixelizeModeToggle.isOn)
            {
                _mode = ModeType.pixelize;
            }

            InitializeImageProcessingResources();
        }

        private void Update()
        {
            if (Camera != null)
                _device = Camera.Device;

            if (_device != null && _device.IsActive && !_device.IsPaused)
            {
                if (_device.CurrentWidth != _frameWidth || _device.CurrentHeight != _frameHeight)
                {
                    InitializeTextureToMatConversionResources(_device.CurrentWidth, _device.CurrentHeight);
                }

                uint lastFrame = AVProLiveCameraPlugin.GetLastFrame(_device.DeviceIndex);

                // Reset _lastFrame at the timing when the camera is reset.
                if (lastFrame < _lastFrame)
                    _lastFrame = 0;

                if (lastFrame != _lastFrame)
                {
                    _lastFrame = lastFrame;

                    // Get frame data as Color32 array using GetFrameAsColor32
                    // Note: When AVProLiveCameraManager.SupportInternalFormatConversion = true,
                    // GetFrameAsColor32() may fail to retrieve data or return incorrect data,
                    // so it is recommended to set it to false.
                    bool result = AVProLiveCameraPlugin.GetFrameAsColor32(_device.DeviceIndex, _framePointer, _frameWidth, _frameHeight);

                    if (result)
                    {
                        OpenCVMatUtils.CopyToMat<Color32>(_frameData, _rgbaMat);

                        if (_mode == ModeType.original)
                        {

                        }
                        else if (_mode == ModeType.sepia)
                        {
                            Core.transform(_rgbaMat, _rgbaMat, _mSepiaKernel);
                        }
                        else if (_mode == ModeType.pixelize)
                        {
                            Imgproc.resize(_rgbaMat, _mIntermediateMat, _mSize0, 0.1, 0.1, Imgproc.INTER_NEAREST);
                            Imgproc.resize(_mIntermediateMat, _rgbaMat, _rgbaMat.size(), 0.0, 0.0, Imgproc.INTER_NEAREST);
                        }

                        Imgproc.putText(_rgbaMat, "AVPro With OpenCV for Unity Example", new Point(50, _rgbaMat.rows() / 2), Imgproc.FONT_HERSHEY_SIMPLEX, 2.0, new Scalar(255, 0, 0, 255), 5, Imgproc.LINE_AA, false);
                        Imgproc.putText(_rgbaMat, "W:" + _rgbaMat.width() + " H:" + _rgbaMat.height() + " SO:" + Screen.orientation, new Point(5, _rgbaMat.rows() - 10), Imgproc.FONT_HERSHEY_SIMPLEX, 1.0, new Scalar(255, 255, 255, 255), 2, Imgproc.LINE_AA, false);

                        //Convert Mat to Texture2D
                        OpenCVMatUtils.MatToTexture2D(_rgbaMat, _texture);
                    }
                }
            }
        }

        private void OnDestroy()
        {
            DisposeTextureToMatConversionResources();
            DisposeImageProcessingResources();
        }

        // Public Methods
        /// <summary>
        /// Raises the back button event.
        /// </summary>
        public void OnBackButton()
        {
            SceneManager.LoadScene("AVProWithOpenCVForUnityExample");
        }

        /// <summary>
        /// Raises the original mode toggle event.
        /// </summary>
        public void OnOriginalModeToggle()
        {
            if (OriginalModeToggle.isOn)
            {
                _mode = ModeType.original;
            }
        }

        /// <summary>
        /// Raises the sepia mode toggle event.
        /// </summary>
        public void OnSepiaModeToggle()
        {
            if (SepiaModeToggle.isOn)
            {
                _mode = ModeType.sepia;
            }
        }

        /// <summary>
        /// Raises the pixelize mode toggle event.
        /// </summary>
        public void OnPixelizeModeToggle()
        {
            if (PixelizeModeToggle.isOn)
            {
                _mode = ModeType.pixelize;
            }
        }

        /// <summary>
        /// Initializes the image processing resources (sepia kernel, intermediate mat, size).
        /// </summary>
        private void InitializeImageProcessingResources()
        {
            // sepia
            _mSepiaKernel = new Mat(4, 4, CvType.CV_32F);
            _mSepiaKernel.put(0, 0, /* R */0.189f, 0.769f, 0.393f, 0f);
            _mSepiaKernel.put(1, 0, /* G */0.168f, 0.686f, 0.349f, 0f);
            _mSepiaKernel.put(2, 0, /* B */0.131f, 0.534f, 0.272f, 0f);
            _mSepiaKernel.put(3, 0, /* A */0.000f, 0.000f, 0.000f, 1f);

            // pixelize
            _mIntermediateMat = new Mat();
            _mSize0 = new Size();
        }

        /// <summary>
        /// Disposes the image processing resources (sepia kernel, intermediate mat, size).
        /// </summary>
        private void DisposeImageProcessingResources()
        {
            _mSepiaKernel?.Dispose(); _mSepiaKernel = null;
            _mIntermediateMat?.Dispose(); _mIntermediateMat = null;
            _mSize0 = null;
        }

        // Private Methods
        /// <summary>
        /// Initializes the textures and mat used for texture to Mat conversion.
        /// </summary>
        private void InitializeTextureToMatConversionResources(int width, int height)
        {
            // Free buffer if it's diffarent size
            if (_frameHandle.IsAllocated && _frameData != null)
            {
                if (_frameData.Length != width * height)
                {
                    DisposeTextureToMatConversionResources();
                }
            }

            if (_frameData == null)
            {
                _frameWidth = width;
                _frameHeight = height;
                _frameData = new Color32[_frameWidth * _frameHeight];
                _frameHandle = GCHandle.Alloc(_frameData, GCHandleType.Pinned);
                _framePointer = _frameHandle.AddrOfPinnedObject();

                _texture = new Texture2D(_frameWidth, _frameHeight, TextureFormat.RGBA32, false, false);

                _rgbaMat = new Mat(_texture.height, _texture.width, CvType.CV_8UC4);

                // Set the Texture2D as the texture of the RawImage for preview.
                ResultPreview.texture = _texture;
                ResultPreview.GetComponent<AspectRatioFitter>().aspectRatio = (float)_texture.width / _texture.height;
            }
        }

        /// <summary>
        /// Disposes the textures and mat used for texture to Mat conversion.
        /// </summary>
        private void DisposeTextureToMatConversionResources()
        {
            if (_frameHandle.IsAllocated)
            {
                _framePointer = System.IntPtr.Zero;
                _frameHandle.Free();
                _frameData = null;
            }

            if (_texture)
            {
                Texture2D.Destroy(_texture);
                _texture = null;
            }

            _rgbaMat?.Dispose(); _rgbaMat = null;
        }
    }
}
#endif
