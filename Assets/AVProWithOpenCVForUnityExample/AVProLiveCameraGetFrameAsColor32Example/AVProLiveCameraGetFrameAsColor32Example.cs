using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityUtils;
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
        public AVProLiveCamera _camera;
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
        }

        void Update()
        {
            if (_camera != null)
                _device = _camera.Device;

            if (_device != null && _device.IsActive && !_device.IsPaused)
            {
                if (_device.CurrentWidth != _frameWidth || _device.CurrentHeight != _frameHeight)
                {
                    CreateBuffer(_device.CurrentWidth, _device.CurrentHeight);
                }

                uint lastFrame = AVProLiveCameraPlugin.GetLastFrame(_device.DeviceIndex);

                // Reset _lastFrame at the timing when the camera is reset.
                if (lastFrame < _lastFrame)
                    _lastFrame = 0;

                if (lastFrame != _lastFrame)
                {
                    _lastFrame = lastFrame;

                    bool result = AVProLiveCameraPlugin.GetFrameAsColor32(_device.DeviceIndex, _framePointer, _frameWidth, _frameHeight);
                    
                    if (result)
                    {
                        
                        MatUtils.copyToMat<Color32>(_frameData, rgbaMat);

                        
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
                        Imgproc.putText(rgbaMat, "W:" + rgbaMat.width() + " H:" + rgbaMat.height() + " SO:" + Screen.orientation, new Point(5, rgbaMat.rows() - 10), Imgproc.FONT_HERSHEY_SIMPLEX, 1.0, new Scalar(255, 255, 255, 255), 2, Imgproc.LINE_AA, false);

                        //Convert Mat to Texture2D
                        Utils.matToTexture2D(rgbaMat, texture);
                        
                    }
                }
            }
        }

        private void CreateBuffer(int width, int height)
        {
            // Free buffer if it's diffarent size
            if (_frameHandle.IsAllocated && _frameData != null)
            {
                if (_frameData.Length != width * height)
                {
                    FreeBuffer();
                }
            }

            if (_frameData == null)
            {
                _frameWidth = width;
                _frameHeight = height;
                _frameData = new Color32[_frameWidth * _frameHeight];
                _frameHandle = GCHandle.Alloc(_frameData, GCHandleType.Pinned);
                _framePointer = _frameHandle.AddrOfPinnedObject();

                texture = new Texture2D(_frameWidth, _frameHeight, TextureFormat.RGBA32, false, false);

                rgbaMat = new Mat(texture.height, texture.width, CvType.CV_8UC4);

                
                gameObject.GetComponent<Renderer>().material.mainTexture = texture;

                gameObject.transform.localScale = new Vector3(texture.width, texture.height, 1);
                Debug.Log("Screen.width " + Screen.width + " Screen.height " + Screen.height + " Screen.orientation " + Screen.orientation);

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
                
            }
        }

        private void FreeBuffer()
        {
            if (_frameHandle.IsAllocated)
            {
                _framePointer = System.IntPtr.Zero;
                _frameHandle.Free();
                _frameData = null;
            }

            if (texture)
            {
                Texture2D.DestroyImmediate(texture);
                texture = null;
            }

            if (rgbaMat != null)
                rgbaMat.Dispose();
        }

        void OnDestroy()
        {
            FreeBuffer();

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