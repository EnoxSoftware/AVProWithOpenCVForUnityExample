using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityIntegration;
using RenderHeads.Media.AVProVideo;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace AVProWithOpenCVForUnityExample
{
    /// <summary>
    /// AVProVideo GetReadableTexture Example
    /// An example of converting an AVProVideo image frame to an OpenCV Mat using GetReadableTexture.
    /// </summary>
    public class AVProVideoGetReadableTextureExample : MonoBehaviour
    {
        // Enums
        public enum ModeType
        {
            original,
            sepia,
            pixelize,
        }

        // Public Fields
        [Header("AVProVideo")]
        /// <summary>
        /// The media player.
        /// </summary>
        public MediaPlayer MediaPlayer;

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
        private int _lastFrame;

        /// <summary>
        /// The source texture for GetReadableTexture.
        /// </summary>
        private Texture2D _sourceTexture;

        /// <summary>
        /// The output texture for display.
        /// </summary>
        private Texture2D _outputTexture;

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

            MediaPlayer.Events.AddListener(OnVideoEvent);
        }

        private void Update()
        {
            if (_sourceTexture != null && _outputTexture != null)
            {
                IMediaControl control = MediaPlayer.Control;

                int lastFrame = control.GetCurrentTimeFrames();

                // Reset _lastFrame at the timing when the video is reset.
                if (lastFrame < _lastFrame)
                    _lastFrame = 0;

                if (lastFrame != _lastFrame)
                {
                    _lastFrame = lastFrame;

                    //Debug.Log("FrameReady " + lastFrame);

                    //Convert AVPro's Texture to Texture2D
                    Helper.GetReadableTexture(MediaPlayer.TextureProducer.GetTexture(), MediaPlayer.TextureProducer.RequiresVerticalFlip(), Helper.GetOrientation(MediaPlayer.Info.GetTextureTransform()), _sourceTexture);

                    //Convert Texture2D to Mat
                    OpenCVMatUtils.Texture2DToMat(_sourceTexture, _rgbaMat);

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
                    OpenCVMatUtils.MatToTexture2D(_rgbaMat, _outputTexture);
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
        /// Callback function to handle events
        /// </summary>
        public void OnVideoEvent(MediaPlayer mp, MediaPlayerEvent.EventType et, ErrorCode errorCode)
        {
            switch (et)
            {
                case MediaPlayerEvent.EventType.ReadyToPlay:
                    MediaPlayer.Control.Play();
                    break;
                case MediaPlayerEvent.EventType.FirstFrameReady:
                    Debug.Log("First frame ready");
                    OnNewMediaReady();
                    break;
                case MediaPlayerEvent.EventType.FinishedPlaying:
                    MediaPlayer.Control.Rewind();
                    break;
            }
            Debug.Log("Event: " + et.ToString());
        }

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

        // Private Methods
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

        /// <summary>
        /// Raises the new media ready event.
        /// </summary>
        private void OnNewMediaReady()
        {
            IMediaInfo info = MediaPlayer.Info;

            Debug.Log("GetVideoWidth " + info.GetVideoWidth() + " GetVideoHeight() " + info.GetVideoHeight());

            // Dispose existing textures and mat
            DisposeTextureToMatConversionResources();

            _sourceTexture = new Texture2D(info.GetVideoWidth(), info.GetVideoHeight(), TextureFormat.RGBA32, false);
            _outputTexture = new Texture2D(info.GetVideoWidth(), info.GetVideoHeight(), TextureFormat.RGBA32, false);

            _rgbaMat = new Mat(_sourceTexture.height, _sourceTexture.width, CvType.CV_8UC4);

            // Set the output Texture2D as the texture of the RawImage for preview.
            ResultPreview.texture = _outputTexture;
            ResultPreview.GetComponent<AspectRatioFitter>().aspectRatio = (float)_outputTexture.width / _outputTexture.height;
        }

        /// <summary>
        /// Disposes the textures and mat used for video to Mat conversion.
        /// </summary>
        private void DisposeTextureToMatConversionResources()
        {
            if (_sourceTexture != null) Texture2D.Destroy(_sourceTexture); _sourceTexture = null;
            if (_outputTexture != null) Texture2D.Destroy(_outputTexture); _outputTexture = null;
            _rgbaMat?.Dispose(); _rgbaMat = null;
        }
    }
}
