using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UI;
public enum FacingCamera
{
    Front = 0,
    Back = 1
}

public enum ScreenMode
{
    Full = 0,
    Bottom = 1
}

public enum Aspect
{
    Portrait = 0,
    Landscape = 1
}

public enum Resolution
{
    MinimumRequired = 0,
    Lowest = 1,
    Highest = 2,
    Custom = 3
}

[RequireComponent(typeof(AspectRatioFitter))]
[RequireComponent(typeof(RawImage))]
public class WebcamScript : MonoBehaviour
{
    public RectTransform ScaleRect;
    CanvasScaler canvasScaler;
    /// <summary>
    /// Used for establish the correct scale and aspect ratio for starting a webcam.
    /// </summary>
    public RectTransform CanvasRect;
    private WebCamTexture captureTexture; 
    public WebCamTexture CaptureTexture { get { return captureTexture; } }
    private RawImage display;
    public static Action<WebCamTexture> WebcamStarted;
    public static Action <WebCamTexture> WebcamStopped;
    public WebCamDevice Device;

    public FacingCamera CameraDirection = FacingCamera.Front;
    public ScreenMode Mode = ScreenMode.Full;
    public Resolution CameraResolution = Resolution.MinimumRequired;

    public int FrameRate = 60;

    public Vector2 MinimumRequired = new Vector2(128, 128);
    public Vector2 Lowest = new Vector2();
    public Vector2 Highest = new Vector2();
    public Vector2 CustomResolution = new Vector2();
    private Vector2 resolution = new Vector2(640, 480);
    public LayoutGroup Mask;

    /// <summary>
    /// Used for Camera Capture
    /// </summary>
    Color32[] pix2, pix1;
    bool isCleaning = false;

    public bool IsRunning
    {
        get
        {
            if (CaptureTexture == null) return false;
            return CaptureTexture.isPlaying;
        }
    }

    void Start()
    {
        canvasScaler = GetComponentInParent<CanvasScaler>();
     //   CamCanvas = GetComponentInParent<Canvas>();
        //Get user's device
        //var device = WebCamTexture.devices.FirstOrDefault();
        Device = RearCamera();
    }

    WebCamDevice RearCamera()
    {
        var devices = WebCamTexture.devices;
        var cam = devices.FirstOrDefault(x => x.isFrontFacing == false);
        
        Debug.LogWarning("REARCAM is front facing:" + (cam.isFrontFacing));
        return cam;
    }

    public void Play()
    {
        if (CanvasRect == null) CanvasRect = GetComponentInParent<Canvas>().GetComponent<RectTransform>();

        captureTexture = new WebCamTexture((int)resolution.x, (int)resolution.y, FrameRate);

        display = GetComponent<RawImage>();
        display.texture = captureTexture;

        captureTexture.Play();

        WebcamStarted?.Invoke(captureTexture);
    }
  
    public void Stop()
    {
        if (captureTexture != null) captureTexture.Stop();
        WebcamStopped?.Invoke(captureTexture);
    }

    public void Pause()
    {
        if (captureTexture != null) captureTexture.Pause();
    }

    /// <summary>
    /// Requires testing.
    /// </summary>
    public void ResetCamera()
    {
        // not tested
        if (captureTexture != null)
            captureTexture.Stop();
        Play();
    }

    float elapsedtime;
    void Update()
    {
        if (captureTexture!=null && captureTexture.isPlaying)
        {
#if UNITY_EDITOR
            SetResolution();
            SetRatio();
#else
            ResizeRawImageLayout();
#endif
        }
    }

    /// <summary>
    /// Rotates UI element based on WebCamTexture's Video Rotation Angle property. This fixes the issue with the portrait mode on phone cameras.
    /// </summary>
    private bool SetRotation()
    {
        transform.localRotation = Quaternion.Euler(0, 0, -captureTexture.videoRotationAngle);
#if UNITY_EDITOR
        return captureTexture.videoRotationAngle != 0 ? true : false;
#else
        return captureTexture.videoRotationAngle == 0 ? true : false;
#endif
    }

    /// <summary>
    /// Adjusts the size of the raw image's LayoutElement.
    /// </summary>
    private void ResizeRawImageLayout()
    {
        bool rotated = SetRotation();

        var ratio = resolution.y / resolution.x;

        var layout = display.GetComponent<LayoutElement>();

        layout.minWidth = CanvasRect.rect.height;
        layout.minHeight = layout.minWidth * ratio;

        SetMirror(rotated);
    }

    void SetMirror(bool rotated)
    {
#region MIRROR iOS
#if !UNITY_IOS
        var flipVertically = false;
#else
        var flipVertically = captureTexture.videoVerticallyMirrored;
#endif
        var mirror = flipVertically ? -1 : 1;
        if (rotated)
            display.rectTransform.localScale = new Vector3(mirror, 1, 1);
        else
            display.rectTransform.localScale = new Vector3(1, mirror, 1);
#endregion
    }

    [Tooltip("Disable aspect ratio fitter auto sizing.")]
    public bool debug = false;
    [Obsolete("only works in editor. Use ResizeRawImageLayout() instead.")]
    private void SetRatio()
    {
#if UNITY_EDITOR
        if (debug) return;
#endif
        if (captureTexture != null)
        {
            var rectTransform = GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(1, 0);
            rectTransform.anchorMax = new Vector2(0, 1);

            float screenAspectRatio = (float)Screen.height / (float)Screen.width;

            //If IR isn't working well, try changing Trilinear to Point or Bilinear
            captureTexture.filterMode = FilterMode.Trilinear;

            float webCamAspectRatio = (float)captureTexture.width / (float)captureTexture.height;

            var pivot = screenAspectRatio > 1 ? 1f : 0.5f;
            rectTransform.pivot = new Vector2(0.5f, pivot);

            var UIAspectRatio = GetComponent<AspectRatioFitter>();
            if (UIAspectRatio != null)
            {
                Mask.childAlignment = TextAnchor.UpperCenter;
                UIAspectRatio.enabled = true;
                UIAspectRatio.aspectRatio = webCamAspectRatio;
            }
            else
            {
                Debug.LogError("Webcam does not have an aspect ratio fitter.");
            }
        }
    }

    private void SetResolution()
    {
        switch (CameraResolution)
        {
            case Resolution.MinimumRequired:
                resolution = MinimumRequired;
                break;
            case Resolution.Lowest:
                resolution = Lowest;
                break;
            case Resolution.Highest:
                resolution = Highest;
                break;
            case Resolution.Custom:
                resolution = CustomResolution;
                break;
            default:
                break;
        }
    }

    private void SetCamera()
    {
        switch (CameraDirection)
        {
            case FacingCamera.Front:
                break;
            case FacingCamera.Back:
                break;
            default:
                break;
        }
    }

    private void SetMode()
    {
        switch (Mode)
        {
            case ScreenMode.Full:
                break;
            case ScreenMode.Bottom:
                break;
            default:
                break;
        }
    }

    //public void TakePhoto(ReactUnity.Services.IUserService _userService, PlayerInventoryItemViewModel _model)
    //{
    //    Texture2D snap = GetPhoto();

    //    var path = PhotoChallengeManager.GetFilename(_userService.CurrentUser, _model);
    //    System.IO.File.WriteAllBytes(path, snap.EncodeToPNG());

    //    PhotoChallengeController._path = path;
    //    PhotoChallengeController._captureImage = snap;
    //}

    /// <summary>
    /// Returns Texture that does not have any rotations applied.
    /// </summary>
    /// <returns></returns>
    public Texture2D GetIRPhoto()
    {
        Texture2D photo = new Texture2D(captureTexture.width, captureTexture.height, TextureFormat.RGBA32, false);
        photo.SetPixels32(CaptureTexture.GetPixels32());
        photo.Apply();

        return photo;
    }

    public Texture2D GetPhoto()
    {
        Texture2D photo = new Texture2D(captureTexture.width, captureTexture.height, TextureFormat.RGB24, false);
        photo.SetPixels(CaptureTexture.GetPixels());
        photo.Apply();

#if UNITY_EDITOR
        return photo;
#else
        Texture2D rotatedPhoto = RotateImage(photo, -captureTexture.videoRotationAngle);

        StartCoroutine(Clear());

        return rotatedPhoto;
#endif
    }

    private void OnDestroy()
    {
        StopCoroutine(Clear());
    }

    private void OnApplicationQuit()
    {
        {
            if (captureTexture != null)
            {
                captureTexture.Stop();
            }
        }
    }

    public Texture2D RotateImage(Texture2D originTexture, int angle)
    {
        float pi180 = 0.01745329251f; // Mathf.PI / 180
        Texture2D result;
        if (angle == 90 || angle == -90 || angle == 270 || angle == -270) { 
            result = new Texture2D(originTexture.height, originTexture.width, TextureFormat.RGBA32, false);
        }
        else
        {
            result = new Texture2D(originTexture.width, originTexture.height, TextureFormat.RGBA32, false);
        }
        pix1 = result.GetPixels32();
        pix2 = originTexture.GetPixels32();
        Color32[] pix3 = rotateSquare(pix2, (pi180 * (double)angle), originTexture, angle);

        result.SetPixels32(pix3);
        result.Apply();
        return result;
    }

    Color32[] rotateSquare(Color32[] arr, double phi, Texture2D originTexture, int angle)
    {
        int row;
        int column;
        int index;
        pix2 = originTexture.GetPixels32();
        int width = originTexture.width;
        int height = originTexture.height;
        Color32[] tempPix = new Color32[width * height];

        Debug.LogError("Angle: " + angle);

        if (angle == -90 || angle == 270)
        {
            for (column = 0; column < width; column++)
            {
                for (row = 0; row < height; row++)
                {
                    index = row * width + (width - 1 - column);
                    tempPix[column * height + row] = pix2[index];
                }
            }
        }
        else if (angle == 90 || angle == -270)
        {
            for (column = 0; column < width; column++)
            {
                for (row = 0; row < height; row++)
                {
                    index = (width * (height - 1 - row)) + column;
                    tempPix[column * height + row] = pix2[index];
                }
            }
        }
        else
        {
            return pix2;
        }

        return tempPix;
    }

    IEnumerator Clear()
    {
        isCleaning = true;
        AsyncOperation op = Resources.UnloadUnusedAssets();

        while (!op.isDone) yield return new WaitForSecondsRealtime(3);

        GarbageCollector.CollectIncremental(30000000);
        //GC.Collect();
        isCleaning = false;
    }
}

