using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ImageCapturing : MonoBehaviour
{
    private Texture2D cachedTexture;
    public string filePath = string.Empty;
    private RenderTexture renderTexture;
    private Texture2D defaultTexture;
    private Vector2 screenRatio = new Vector2(1f, 1f);
    [SerializeField] Camera captureCamera = null;

    public delegate void OnpostNativeShare();
    public OnpostNativeShare onpostNativeShare;

    private void Awake()
    {
        renderTexture = new RenderTexture(Screen.width, Screen.height, 300);
        InitDefaultTexture();
    }

    private void InitDefaultTexture()
    {
        defaultTexture = new Texture2D(1, 1);
        Color[] pixels = defaultTexture.GetPixels();
        int i = 0;
        for (int num = pixels.Length; i < num; i++)
        {
            pixels[i] = Color.white;
        }
        defaultTexture.SetPixels(pixels);
        defaultTexture.Apply();
    }

    public void TakeLevelScreenshot(Rect boundary)
    {
        //yield return new WaitForEndOfFrame();
        //Debug.LogError(boundary + " " + Screen.width + " " + Screen.height);
        if (!(boundary.width < 64f) && !(boundary.height < 64f))
        {
            captureCamera.targetTexture = renderTexture;
            RenderTexture.active = renderTexture;
            captureCamera.Render();
            if (cachedTexture != null)
            {
                Destroy(cachedTexture);
            }
            cachedTexture = new Texture2D(Mathf.FloorToInt(boundary.width), Mathf.FloorToInt(boundary.height), TextureFormat.RGB24, mipChain: false);
            cachedTexture.ReadPixels(boundary, 0, 0);
            cachedTexture.Apply();

            filePath = Path.Combine(Application.temporaryCachePath, "shared img.png");
            File.WriteAllBytes(filePath, cachedTexture.EncodeToPNG());

            // To avoid memory leaks
            Destroy(cachedTexture);

            RenderTexture.active = null;
            captureCamera.targetTexture = null;

            if (onpostNativeShare != null)
                onpostNativeShare.Invoke();
        }
    }
    public void NativeShare()
    {
        new NativeShare().AddFile(filePath).
                SetSubject("Get for free now!").
                SetText("Da vinci would be proud of you!").
                SetTitle("Stick Puzzle").Share();
    }
    //private IEnumerator TakeSSAndShare()
    //{
    //    yield return new WaitForEndOfFrame();

    //    Texture2D ss = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
    //    ss.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
    //    ss.Apply();

    //    string filePath = Path.Combine(Application.temporaryCachePath, "shared img.png");
    //    File.WriteAllBytes(filePath, ss.EncodeToPNG());

    //    // To avoid memory leaks
    //    Destroy(ss);

    //    new NativeShare().AddFile(filePath).SetSubject("Subject goes here").SetText("Hello world!").Share();

    //    // Share on WhatsApp only, if installed (Android only)
    //    //if( NativeShare.TargetExists( "com.whatsapp" ) )
    //    //	new NativeShare().AddFile( filePath ).SetText( "Hello world!" ).SetTarget( "com.whatsapp" ).Share();
    //}

}
