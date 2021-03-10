using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections;

public class DeviceCameraController : MonoBehaviour
{
    public RawImage image;
    public RectTransform imageParent;
    public AspectRatioFitter imageFitter;
    public Slider cameraSelector;

    // Device cameras
    WebCamDevice activeCameraDevice;
    WebCamTexture activeCameraTexture;

    // Image rotation
    Vector3 rotationVector = new Vector3(0f, 0f, 0f);

    // Image uvRect
    Rect defaultRect = new Rect(0f, 0f, 1f, 1f);
    Rect fixedRect = new Rect(0f, 1f, 1f, -1f);

    // Image Parent's scale
    Vector3 defaultScale = new Vector3(1f, 1f, 1f);
    Vector3 fixedScale = new Vector3(-1f, 1f, 1f);

    IEnumerator Start()
    {
        yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);

        // Check for device cameras
        if (WebCamTexture.devices.Length == 0)
        {
            Debug.Log("No devices cameras found");
            yield break;
        }

        // Get the device's cameras and create WebCamTextures with them
        for (var i = 0; i < WebCamTexture.devices.Length; i++)
        {
            var d = WebCamTexture.devices[i];
            Debug.Log("CAMERA: " + i + " || " + d.name);
        }

        if (cameraSelector != null)
            cameraSelector.maxValue = WebCamTexture.devices.Length - 1;

        //activeCameraDevice = WebCamTexture.devices[];
        //activeCameraDevice = WebCamTexture.devices.Where(d => !d.isFrontFacing).First();
 

#if UNITY_EDITOR
        if (WebCamTexture.devices.Any(d => d.name.Contains("Remote")))
        {
            activeCameraDevice = WebCamTexture.devices.Last(d => d.name.Contains("Remote"));
        }
        else
        {
            activeCameraDevice = WebCamTexture.devices.Last();
        }
#else
       activeCameraDevice = WebCamTexture.devices.Where(d => !d.isFrontFacing).First();
#endif

        

        var texture = new WebCamTexture(activeCameraDevice.name);
        texture.requestedWidth = 1080;
        texture.requestedHeight = 1920;

        // Set camera filter modes for a smoother looking image
        texture.filterMode = FilterMode.Trilinear;

        // Set the camera to use by default
        SetActiveCamera(texture);
    }

    public void OnCameraSliderChange()
    {
        Debug.Log((int)cameraSelector.value);
        ChangeCamera(WebCamTexture.devices[(int)cameraSelector.value]);
    }

    public Texture2D GetWebcamPhoto()
    {
       

        if (activeCameraTexture != null)
        {
            Texture2D photo = new Texture2D(activeCameraTexture.width, activeCameraTexture.height);
            photo.SetPixels(activeCameraTexture.GetPixels());
            photo.Apply();

            return photo;
        }

        return null;
    }

    public void Play()
    {
        if (activeCameraTexture != null)
        {
            activeCameraTexture.Play();
        }
    }

    public void Stop()
    {
        if (activeCameraTexture != null)
        {
            activeCameraTexture.Stop();
        }
    }

    public void ChangeCamera(WebCamDevice device)
    {
        var texture = new WebCamTexture(device.name);
        SetActiveCamera(texture);
    }

    // Set the device camera to use and start it
    public void SetActiveCamera(WebCamTexture cameraToUse)
    {
        if (activeCameraTexture != null)
        {
            activeCameraTexture.Stop();
        }

        activeCameraTexture = cameraToUse;
        activeCameraDevice = WebCamTexture.devices.FirstOrDefault(device =>
            device.name == cameraToUse.deviceName);

        image.texture = activeCameraTexture;
        //image.material.SetTextureScale("_Texture", new Vector2(1f, 1f));
        UpdateSize();

        activeCameraTexture.Play();
    }

    // Make adjustments to image every frame to be safe, since Unity isn't 
    // guaranteed to report correct data as soon as device camera is started

    private void FixedUpdate()
    {
        //// Skip making adjustment for incorrect camera data
        //if (activeCameraTexture.width < 100)
        //{
        //    Debug.Log("Still waiting another frame for correct info...");
        //    return;
        //}

        //// Rotate image to show correct orientation 
        //rotationVector.z = -activeCameraTexture.videoRotationAngle;
        //image.rectTransform.localEulerAngles = rotationVector;

        //// Set AspectRatioFitter's ratio
        //float videoRatio =
        //    (float)activeCameraTexture.width / (float)activeCameraTexture.height;
        //imageFitter.aspectRatio = videoRatio;

        //// Unflip if vertically flipped
        //image.uvRect =
        //    activeCameraTexture.videoVerticallyMirrored ? fixedRect : defaultRect;

        //// Mirror front-facing camera's image horizontally to look more natural
        //imageParent.localScale =
            //activeCameraDevice.isFrontFacing ? fixedScale : defaultScale;

        UpdateSize();
    }

    // Make adjustments to image every frame to be safe, since Unity isn't 
    // guaranteed to report correct data as soon as device camera is started
    private void UpdateSize()
    {
        // Rotate image to show correct orientation 
        if (activeCameraTexture == null)
        {
            Debug.Log("No active camera texture");
            return;
            //SetupCameraTexture();
        }

        rotationVector.z = -activeCameraTexture.videoRotationAngle;
        image.rectTransform.localEulerAngles = rotationVector;

        if (activeCameraTexture.width >= 100)
        {
            float videoRatio =
                (float)activeCameraTexture.width / (float)activeCameraTexture.height;
            var parentHeight = imageParent.rect.height;
            var parentWidth = imageParent.rect.width;
            var parentRatio = parentWidth / parentHeight;

            if (activeCameraTexture.videoRotationAngle == 90 || activeCameraTexture.videoRotationAngle == 270)
            {
                videoRatio = 1f / videoRatio;
            }

            float targetWidth, targetHeight;

            if (parentRatio > videoRatio)
            {
                targetWidth = parentWidth;
                targetHeight = parentWidth / videoRatio;
            }
            else
            {
                targetWidth = parentHeight * videoRatio;
                targetHeight = parentHeight;
            }

            if (activeCameraTexture.videoRotationAngle == 90 || activeCameraTexture.videoRotationAngle == 270)
            {
                image.rectTransform.sizeDelta = new Vector2(targetHeight, targetWidth);
            }
            else
            {
                image.rectTransform.sizeDelta = new Vector2(targetWidth, targetHeight);
            }
        }
        else
        {
            if (activeCameraTexture.videoRotationAngle == 90 || activeCameraTexture.videoRotationAngle == 270)
            {
                image.rectTransform.sizeDelta = new Vector2(Screen.height, Screen.width);
            }
            else
            {
                image.rectTransform.sizeDelta = new Vector2(Screen.height, Screen.width);
            }
        }

        // Unflip if vertically flipped
        image.uvRect =
            activeCameraTexture.videoVerticallyMirrored ? fixedRect : defaultRect;

        // Mirror front-facing camera's image horizontally to look more natural
        imageParent.localScale =
            activeCameraDevice.isFrontFacing ? fixedScale : defaultScale;
    }


    public void Dispose()
    {
        Stop();
        Texture2D.Destroy(activeCameraTexture);
        Destroy(this);
    }
}