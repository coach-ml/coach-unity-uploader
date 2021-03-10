//using UnityEngine;
//using System.Collections;
//using UnityEngine.UI;
//using System.Linq;
//using System;
//using System.IO;

//namespace Util
//{
//    /// <summary>
//    /// Controller script for a toggalable WebCam/Device Camera view. 
//    /// </summary>
//    public class DeviceCameraObjectController : MonoBehaviour
//    {

//        public RawImage image;
//        public RectTransform imageParent;

//        // Device cameras
//        WebCamDevice frontCameraDevice;
//        WebCamDevice backCameraDevice;
//        public WebCamDevice activeCameraDevice { get; private set; }

//        WebCamTexture frontCameraTexture;
//        WebCamTexture backCameraTexture;
//        public WebCamTexture activeCameraTexture { get; private set; }

//        // Image rotation
//        Vector3 rotationVector = new Vector3(0f, 0f, 0f);

//        // Image uvRect
//        Rect defaultRect = new Rect(0f, 0f, 1f, 1f);
//        Rect fixedRect = new Rect(0f, 1f, 1f, -1f);

//        // Image Parent's scale
//        Vector3 defaultScale = new Vector3(1f, 1f, 1f);
//        Vector3 fixedScale = new Vector3(-1f, 1f, 1f);


//        protected virtual void Awake()
//        {
//            SetupCameraTexture();
//        }



//        private void SetupCameraTexture()
//        {
//            if (Application.RequestUserAuthorization(UserAuthorization.WebCam))
//            {
//                // Check for device cameras
//                if (WebCamTexture.devices.Length == 0)
//                {
//                    Debug.LogWarning("No devices cameras found");
//                    return;
//                }

//                // Get the device's cameras and create WebCamTextures with them
//#if UNITY_EDITOR
//                if (WebCamTexture.devices.Any(d => d.name.Contains("Remote")))
//                {
//                    frontCameraDevice = WebCamTexture.devices.Last(d => d.name.Contains("Remote"));
//                    backCameraDevice = WebCamTexture.devices.First(d => d.name.Contains("Remote"));
//                }
//                else
//                {
//                    frontCameraDevice = WebCamTexture.devices.Last();
//                    backCameraDevice = WebCamTexture.devices.First();
//                }
//#else
//                        frontCameraDevice = WebCamTexture.devices.Last();
//                        backCameraDevice = WebCamTexture.devices.First();
//#endif

//                frontCameraTexture = new WebCamTexture(frontCameraDevice.name);
//                backCameraTexture = new WebCamTexture(backCameraDevice.name);

//                // Set camera filter modes for a smoother looking image
//                frontCameraTexture.filterMode = FilterMode.Trilinear;
//                backCameraTexture.filterMode = FilterMode.Trilinear;

//                // Set the camera to use by default
//                SetActiveCamera(backCameraTexture);
//            }
//        }

//        public bool IsRunning { get; private set; }

//        public void Play()
//        {
//            image.gameObject.SetActive(true);

//            if (activeCameraTexture == null)
//            {
//                //Attempt to setup camera texture again (won't setup if camera permission is not given)
//                Debug.Log("activeCameraTexture is null - attempting to setup and play...");
//                SetupCameraTexture();
//            }

//            if (activeCameraTexture != null)
//            {
//                activeCameraTexture.Play();
//                IsRunning = true;
//            }
//        }

//        public void Pause()
//        {
//            if (activeCameraTexture != null)
//            {
//                activeCameraTexture.Pause();
//                IsRunning = false;
//            }
//        }

//        public void Stop(bool setActive = false)
//        {
//            image.gameObject.SetActive(setActive);
//            if (activeCameraTexture != null)
//            {
//                activeCameraTexture.Stop();
//                IsRunning = false;
//            }
//        }

//        public Texture2D GetPhoto()
//        {
//            Texture2D photo = new Texture2D(activeCameraTexture.width, activeCameraTexture.height);
//            photo.SetPixels(activeCameraTexture.GetPixels());
//            photo.Apply();

//            return photo;
//        }

//        // Set the device camera to use and start it
//        public void SetActiveCamera(WebCamTexture cameraToUse)
//        {
//            if (DevicePermissions.instance.CheckPermission(Permission.CAMERA))
//            {
//                if (activeCameraTexture != null)
//                {
//                    activeCameraTexture.Stop();
//                }

//                activeCameraTexture = cameraToUse;
//                activeCameraDevice = WebCamTexture.devices.FirstOrDefault(device =>
//                    device.name == cameraToUse.deviceName);

//                image.texture = activeCameraTexture;
//                UpdateSize();
//            }
//        }

//        // Switch between the device's front and back camera
//        public void SwitchCamera()
//        {
//            SetActiveCamera(activeCameraTexture.Equals(frontCameraTexture) ?
//                backCameraTexture : frontCameraTexture);
//        }

//        protected virtual void Update()
//        {
//            UpdateSize();
//        }

//        // Make adjustments to image every frame to be safe, since Unity isn't 
//        // guaranteed to report correct data as soon as device camera is started
//        private void UpdateSize()
//        {
//            // Rotate image to show correct orientation 
//            if (activeCameraTexture == null)
//            {
//                SetupCameraTexture();
//            }

//            rotationVector.z = -activeCameraTexture.videoRotationAngle;
//            image.rectTransform.localEulerAngles = rotationVector;

//            if (activeCameraTexture.width >= 100)
//            {
//                float videoRatio =
//                    (float)activeCameraTexture.width / (float)activeCameraTexture.height;
//                var parentHeight = imageParent.rect.height;
//                var parentWidth = imageParent.rect.width;
//                var parentRatio = parentWidth / parentHeight;

//                if (activeCameraTexture.videoRotationAngle == 90 || activeCameraTexture.videoRotationAngle == 270)
//                {
//                    videoRatio = 1f / videoRatio;
//                }

//                float targetWidth, targetHeight;

//                if (parentRatio > videoRatio)
//                {
//                    targetWidth = parentWidth;
//                    targetHeight = parentWidth / videoRatio;
//                }
//                else
//                {
//                    targetWidth = parentHeight * videoRatio;
//                    targetHeight = parentHeight;
//                }

//                if (activeCameraTexture.videoRotationAngle == 90 || activeCameraTexture.videoRotationAngle == 270)
//                {
//                    image.rectTransform.sizeDelta = new Vector2(targetHeight, targetWidth);
//                }
//                else
//                {
//                    image.rectTransform.sizeDelta = new Vector2(targetWidth, targetHeight);
//                }
//            }
//            else
//            {
//                if (activeCameraTexture.videoRotationAngle == 90 || activeCameraTexture.videoRotationAngle == 270)
//                {
//                    image.rectTransform.sizeDelta = new Vector2(Screen.height, Screen.width);
//                }
//                else
//                {
//                    image.rectTransform.sizeDelta = new Vector2(Screen.height, Screen.width);
//                }
//            }

//            // Unflip if vertically flipped
//            image.uvRect =
//                activeCameraTexture.videoVerticallyMirrored ? fixedRect : defaultRect;

//            // Mirror front-facing camera's image horizontally to look more natural
//            imageParent.localScale =
//                activeCameraDevice.isFrontFacing ? fixedScale : defaultScale;
//        }

//        private void OnDestroy()
//        {
//            activeCameraTexture?.Stop();
//            Destroy(activeCameraTexture);
//        }
//    }
//}
