using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WebCamController : MonoBehaviour
{
    public RectTransform ScaleRect;
    public RectTransform CanvasRect;
    public GameObject WebcamPrefab;
    private static List<GameObject> cams = new List<GameObject>();
    public static WebCamController Background;
    public WebCamTexture activeCameraTexture
    {
        get
        {
            var current = GetCurrent();
            if(current!=null)
            return current.CaptureTexture;
            return null;
        }
    }

    public bool IsBackground = false;
    public void Start()
    {
        if(IsBackground)
            DontDestroyOnLoad(gameObject.GetComponentInParent<Canvas>().gameObject);
        if(IsBackground)
            Background = this;
        // clear webcams if they exist;
        foreach (Transform child in transform)
            Destroy(child.gameObject);
    }

    public static void StartBackgroundCam()
    {
        Background.CreateBackground().Play();
    }

    public static void StopBackgroundCam()
    {
        var cam = Background.GetComponentInChildren<WebcamScript>();
        if (cam != null) cam.Stop();
    }

    WebcamScript CreateBackground()
    {
        DestroyWebCam();
        var cam = Instantiate(WebcamPrefab, transform, false);
        cam.transform.SetParent(transform, false);
        var rect = Background.GetComponent<RectTransform>();
        SetAnchors(rect);
        var webcam = cam.GetComponentInChildren<WebcamScript>();
        webcam.CanvasRect = CanvasRect;
        webcam.ScaleRect = ScaleRect;
        cams.Add(cam);

        return webcam;
    }

    private static WebcamScript currentWebCamScript;
    private static GameObject currentCam;
    public static WebcamScript GetCurrent()
    {
        if(currentCam == null)
            currentCam = cams.FirstOrDefault(x => x != null);
        if (currentCam != null)
            if(currentWebCamScript == null)
            currentWebCamScript = currentCam.GetComponentInChildren<WebcamScript>();
        return currentWebCamScript;
    }

    /// <summary>
    /// Stretch to fill.
    /// </summary>
    /// <param name="rect"></param>
    void SetAnchors(RectTransform rect)
    {
        rect.anchorMin = new Vector2(0, 0);
        rect.anchorMax = new Vector2(1, 1);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = new Vector2(0, 0);
        rect.offsetMax = new Vector2(0, 0);
    }

    /// <summary>
    /// Instantiates the camera and starts it.
    /// </summary>
    public void Create()
    {
        DestroyWebCam();
        var cam = Instantiate(WebcamPrefab, transform, false);
        cam.transform.SetParent(transform, false);
        var rect = cam.GetComponent<RectTransform>();
        SetAnchors(rect);

        cams.Add(cam);
        var webCamScript = cam.GetComponentInChildren<WebcamScript>();
        webCamScript.CanvasRect = CanvasRect;
        webCamScript.ScaleRect = ScaleRect;
        cam.GetComponentInChildren<WebcamScript>().Play();
    }

    public void DestroyWebCam()
    {
        if (cams != null) cams.ForEach(x => Destroy(x));
        cams = new List<GameObject>();
        if (transform != null)
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
                var cam = child.GetComponentInChildren<WebcamScript>();
                if (cam != null) cam.Stop();
            }
    }
}
