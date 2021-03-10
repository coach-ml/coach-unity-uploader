using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Used by AR type challenges that need a Camera for either a background or inset view. Ensures that there is only 1 cam being used at a time.
/// Also disables/enables other views that conflict with the camera. Acts as a container for the AR camera which is used for AR interaction.
/// </summary>
public class ARManager : MonoBehaviour
{
    //TODO: manage non-AR background

    static ARManager singleton;

    /// <summary>
    /// Reference to controller that instantiates, plays, and stops the live feed views.
    /// </summary>
    public WebCamController CamController;

    /// <summary>
    /// Used for rendering the live feed.
    /// </summary>
    public Camera WebCam;

    /// <summary>
    /// Used for rendering the 3D gameobject elements and 3D UI elements.
    /// </summary>
    public Camera ARCamera;

    public static Action<bool> CameraStateChanged;

    public Camera CartoonBackgroundCamera;
    public static void EnableCartoonBackground(bool enable)
    {
        if (singleton != null)
        if (singleton.CartoonBackgroundCamera != null)
        {
            singleton.CartoonBackgroundCamera.enabled = enable;
            var audio = singleton.CartoonBackgroundCamera.GetComponent<AudioListener>();
            if (audio != null)
            {
                //audio.enabled = enable;
                audio.enabled = false;
            }
        }
        else Debug.Log("cartoon null");
    }
    public RawImage BackgroundImage;
    public List<Material> Backgrounds = new List<Material>();
    private Material DefaultBackground;
    public static bool IsCamEnabled
    {
        get
        {
            if(singleton != null)
                return singleton.IsCam;
            return false;
        }
    }
    private bool isCam;
    /// <summary>
    /// Enable or disable the AR view. 
    /// Disables map camera and overlay map UI.
    /// Does not manage non-AR background.
    /// </summary>
    public bool IsCam
    {
        get { return isCam; }
        set
        {
            if (value)
            {
                EnableCartoonBackground(false);
                WebCamController.StartBackgroundCam();           
            }
            else
            {
                WebCamController.StopBackgroundCam();            
            }
  // TODO Optimization: consider Challenge is still active but using cartoon background. Need to refactor map and mission UI to reduce draw calls.
            if (WebCam != null) WebCam.enabled = value;
            if (ARCamera != null)
            {
                ARCamera.enabled = value;
                var audio = ARCamera.GetComponent<AudioListener>();
                if (audio != null)
                {
                  //  audio.enabled = value;        //todo: refactor audio to be used by AR correctly. Disabled currently due to AudioManager having AudioListener component
                    audio.enabled = false;
                }
            }
            MapUI(false);
            isCam = value;
            CameraStateChanged?.Invoke(isCam);
            Debug.Log("<color=blue> AR enabled: " + value);
        }
    }

    /// <summary>
    /// Set map and UI state.
    /// </summary>
    /// <param name="enable"></param>
    public static void MapUI(bool enable )
    {
        var map = singleton.GetMapCamera();
        if (map == null) Debug.LogError("no map camera found");

        if (map != null)
        {
            map.enabled = enable;
        }
    }

    /// <summary>
    /// Use the webcam feed.
    /// </summary>
    public static void UseCamera()
    {
        singleton.IsCam = true;
    }

    /// <summary>
    /// Used to render 3D objects on top of a cartoon background. Disables, webcam feed.
    /// </summary>
    public static void ActivateARWithBackground()
    {
        var WebCam = singleton.WebCam;
        var ARCamera = singleton.ARCamera;
        var map = singleton.GetMapCamera();
        if (map == null) Debug.LogError("no map camera found");
        if (WebCam != null) WebCam.enabled = false;
        if (ARCamera != null) ARCamera.enabled = true;
        if (map != null)
        {
            map.enabled = false;
        }

        EnableCartoonBackground(true);
        WebCamController.StopBackgroundCam();

        singleton.isCam = false;
    }

    /// <summary>
    /// Disables the AR cam, webcam, cartoon background and enables the MAP and UI.
    /// </summary>
    public static void DeactivateAR()
    {
        singleton.IsCam = false;
        EnableCartoonBackground(false);
        MapUI(true);
    }
    /// <summary>
    /// Disables webcam AR mode.
    /// </summary>
    /// <param name="background">Show cartoon background.</param>
    public static void ActivateRegular(bool background = true)
    {
        singleton.IsCam = false;
        EnableCartoonBackground(background);
    }

    Camera GetMapCamera()
    {
        return null;
    }


    /// <summary>
    /// Enables the cartoon background. Pass true to select a new background from an array of available images.
    /// </summary>
    /// <param name="newImage"></param>
    public static void SetBackground(bool newImage = false)
    {
        if (newImage)
        {
            int random = UnityEngine.Random.Range(0, singleton.Backgrounds.Count);
            if (random >= singleton.Backgrounds.Count) random = singleton.Backgrounds.Count - 1;
            singleton.DefaultBackground = singleton.Backgrounds[random];
        }
        singleton.BackgroundImage.material = singleton.DefaultBackground;
    }

    public void Start()
    {
        if (singleton == null || singleton == this)
        {
            singleton = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Debug.LogError("An instance of the AR manager already exists. Destroying duplicate...");
            Destroy(gameObject);
        }

        SetBackground(true);
    }
}
