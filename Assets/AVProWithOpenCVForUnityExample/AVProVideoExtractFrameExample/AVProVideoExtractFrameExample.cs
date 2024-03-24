using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityUtils;
using RenderHeads.Media.AVProVideo;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace AVProWithOpenCVForUnityExample
{
    /// <summary>
    /// AVProVideo ExtractFrame Example
    /// An example of converting an AVProVideo image frame to an OpenCV Mat using ExtractFrame.
    /// </summary>
    public class AVProVideoExtractFrameExample : MonoBehaviour
    {

        private const int TARGET_FRAME_NUMBER = 100;
        private float _timeSeconds = 0f;
        public bool _accurateSeek = false;
        public int _timeoutMs = 250;

        /// <summary>
        /// The media player.
        /// </summary>
        public MediaPlayer mediaPlayer;

        /// <summary>
        /// The texture.
        /// </summary>
        Texture2D texture;

        /// <summary>
        /// The rgba mat.
        /// </summary>
        Mat rgbaMat;

        /// <summary>
        /// The m sepia kernel.
        /// </summary>
        Mat mSepiaKernel;

        /// <summary>
        /// The m size0.
        /// </summary>
        Size mSize0;

        /// <summary>
        /// The m intermediate mat.
        /// </summary>
        Mat mIntermediateMat;

        public enum modeType
        {
            original,
            sepia,
            pixelize,
        }

        /// <summary>
        /// The mode.
        /// </summary>
        modeType mode;


        /// <summary>
        /// The original mode toggle.
        /// </summary>
        public Toggle originalModeToggle;


        /// <summary>
        /// The sepia mode toggle.
        /// </summary>
        public Toggle sepiaModeToggle;


        /// <summary>
        /// The pixelize mode toggle.
        /// </summary>
        public Toggle pixelizeModeToggle;


        // Use this for initialization
        void Start()
        {
            if (originalModeToggle.isOn)
            {
                mode = modeType.original;
            }
            if (sepiaModeToggle.isOn)
            {
                mode = modeType.sepia;
            }
            if (pixelizeModeToggle.isOn)
            {
                mode = modeType.pixelize;
            }

            // sepia
            mSepiaKernel = new Mat(4, 4, CvType.CV_32F);
            mSepiaKernel.put(0, 0, /* R */0.189f, 0.769f, 0.393f, 0f);
            mSepiaKernel.put(1, 0, /* G */0.168f, 0.686f, 0.349f, 0f);
            mSepiaKernel.put(2, 0, /* B */0.131f, 0.534f, 0.272f, 0f);
            mSepiaKernel.put(3, 0, /* A */0.000f, 0.000f, 0.000f, 1f);


            // pixelize
            mIntermediateMat = new Mat();
            mSize0 = new Size();


            mediaPlayer.Events.AddListener(OnVideoEvent);
        }

        // Update is called once per frame
        void Update()
        {

            if (texture != null)
            {

                //Convert AVPro's Texture to Texture2D
                mediaPlayer.ExtractFrame(texture, _timeSeconds, _accurateSeek, _timeoutMs);

                //Convert Texture2D to Mat
                Utils.texture2DToMat(texture, rgbaMat);


                if (mode == modeType.original)
                {

                }
                else if (mode == modeType.sepia)
                {

                    Core.transform(rgbaMat, rgbaMat, mSepiaKernel);

                }
                else if (mode == modeType.pixelize)
                {

                    Imgproc.resize(rgbaMat, mIntermediateMat, mSize0, 0.1, 0.1, Imgproc.INTER_NEAREST);
                    Imgproc.resize(mIntermediateMat, rgbaMat, rgbaMat.size(), 0.0, 0.0, Imgproc.INTER_NEAREST);

                }


                Imgproc.putText(rgbaMat, "AVPro With OpenCV for Unity Example", new Point(50, rgbaMat.rows() / 2), Imgproc.FONT_HERSHEY_SIMPLEX, 2.0, new Scalar(255, 0, 0, 255), 5, Imgproc.LINE_AA, false);
                Imgproc.putText(rgbaMat, "ExtractFrame: " + TARGET_FRAME_NUMBER + "( " + _timeSeconds + "sec )", new Point(50, rgbaMat.rows() / 2 + 60), Imgproc.FONT_HERSHEY_SIMPLEX, 2.0, new Scalar(0, 0, 255, 255), 5, Imgproc.LINE_AA, false);
                Imgproc.putText(rgbaMat, "W:" + rgbaMat.width() + " H:" + rgbaMat.height() + " SO:" + Screen.orientation, new Point(5, rgbaMat.rows() - 10), Imgproc.FONT_HERSHEY_SIMPLEX, 1.0, new Scalar(255, 255, 255, 255), 2, Imgproc.LINE_AA, false);

                //Convert Mat to Texture2D
                Utils.matToTexture2D(rgbaMat, texture);

            }

        }

        // Callback function to handle events
        public void OnVideoEvent(MediaPlayer mp, MediaPlayerEvent.EventType et,
                                  ErrorCode errorCode)
        {
            switch (et)
            {
                case MediaPlayerEvent.EventType.ReadyToPlay:
                    mediaPlayer.Control.Play();
                    mediaPlayer.Control.Pause();
                    break;
                case MediaPlayerEvent.EventType.FirstFrameReady:
                    Debug.Log("First frame ready");
                    OnNewMediaReady();
                    break;
                case MediaPlayerEvent.EventType.FinishedPlaying:
                    mediaPlayer.Control.Rewind();
                    break;
            }
            Debug.Log("Event: " + et.ToString());
        }

        /// <summary>
        /// Raises the new media ready event.
        /// </summary>
        private void OnNewMediaReady()
        {
            IMediaInfo info = mediaPlayer.Info;

            Debug.Log("GetVideoWidth " + info.GetVideoWidth() + " GetVideoHeight() " + info.GetVideoHeight());

            // Create a texture the same resolution as our video
            if (texture != null)
            {
                Texture2D.Destroy(texture);
                texture = null;
            }

            texture = new Texture2D(info.GetVideoWidth(), info.GetVideoHeight(), TextureFormat.RGBA32, false);

            rgbaMat = new Mat(texture.height, texture.width, CvType.CV_8UC4);


            gameObject.GetComponent<Renderer>().material.mainTexture = texture;

            gameObject.transform.localScale = new Vector3(texture.width, texture.height, 1);
            Debug.Log("Screen.width " + Screen.width + " Screen.height " + Screen.height + " Screen.orientation " + Screen.orientation);

            float width = texture.width;
            float height = texture.height;

            float widthScale = (float)Screen.width / width;
            float heightScale = (float)Screen.height / height;
            if (widthScale < heightScale)
            {
                Camera.main.orthographicSize = (width * (float)Screen.height / (float)Screen.width) / 2;
            }
            else
            {
                Camera.main.orthographicSize = height / 2;
            }


            _timeSeconds = 1f / info.GetVideoFrameRate() * TARGET_FRAME_NUMBER;
        }

        /// <summary>
        /// Raises the destroy event.
        /// </summary>
        void OnDestroy()
        {
            if (texture != null)
            {
                Texture2D.Destroy(texture);
                texture = null;
            }

            if (mSepiaKernel != null)
            {
                mSepiaKernel.Dispose();
                mSepiaKernel = null;
            }

            if (mIntermediateMat != null)
            {
                mIntermediateMat.Dispose();
                mIntermediateMat = null;
            }
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

            if (originalModeToggle.isOn)
            {
                mode = modeType.original;
            }
        }

        /// <summary>
        /// Raises the sepia mode toggle event.
        /// </summary>
        public void OnSepiaModeToggle()
        {

            if (sepiaModeToggle.isOn)
            {
                mode = modeType.sepia;
            }
        }

        /// <summary>
        /// Raises the pixelize mode toggle event.
        /// </summary>
        public void OnPixelizeModeToggle()
        {

            if (pixelizeModeToggle.isOn)
            {
                mode = modeType.pixelize;
            }
        }
    }
}
