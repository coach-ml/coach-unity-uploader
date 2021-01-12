using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WebCamPanel : MonoBehaviour
{

    public RawImage image;
    private WebCamTexture _webCamTexture;

    // Use this for initialization
    void Start()
    {
        StartCoroutine(SetupCoroutine());
    }

    private IEnumerator SetupCoroutine()
    {
        while (_webCamTexture == null)
        {
            WebCamDevice[] devices = WebCamTexture.devices;
            if (devices.Length > 0)
            {
                _webCamTexture = new WebCamTexture(devices[0].name);
                _webCamTexture.Play();

                yield return new WaitUntil(() => _webCamTexture.width > 10);

                image.texture = _webCamTexture;
                image.material.mainTexture = _webCamTexture;
                image.color = Color.white;
                bool rotated = TransformImage();
                SetImageSize(rotated);
            }
            yield return 0;
        }
    }

    private void SetImageSize(bool rotated)
    {
        var rotTexSize = rotated ? new Vector2(_webCamTexture.height, _webCamTexture.width) : new Vector2(_webCamTexture.width, _webCamTexture.height);
        float ratio = 1f;
        if (rotTexSize.x < Screen.width && rotTexSize.y > Screen.height)
        {
            ratio = Screen.width / rotTexSize.x;
        }
        else if (rotTexSize.x > Screen.width && rotTexSize.y < Screen.height)
        {
            ratio = Screen.height / rotTexSize.y;
        }
        else if (rotTexSize.x < Screen.width && rotTexSize.y < Screen.height)
        {
            var widthRatio = Screen.width / rotTexSize.x;
            var heightRatio = Screen.height / rotTexSize.y;
            ratio = widthRatio < heightRatio ? heightRatio : widthRatio;
        }
        else if (rotTexSize.x > Screen.width && rotTexSize.y > Screen.height)
        {
            var widthRatio = Screen.width / rotTexSize.x;
            var heightRatio = Screen.height / rotTexSize.y;
            ratio = widthRatio < heightRatio ? heightRatio : widthRatio;
        }

        var rect = image.gameObject.GetComponent<RectTransform>();
        var texSize = new Vector2(_webCamTexture.width, _webCamTexture.height);
        rect.sizeDelta = texSize * ratio;
    }

    private bool TransformImage()
    {
#if UNITY_IOS
        // iOS cam is mirrored
        image.gameObject.transform.localScale = new Vector3(-1, 1, 1);
#endif
        image.gameObject.transform.Rotate(0.0f, 0, -_webCamTexture.videoRotationAngle);
        return _webCamTexture.videoRotationAngle != 0;
    }

    private void OnDestroy()
    {
        _webCamTexture.Stop();
        Destroy(_webCamTexture);
    }
}
