using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class BaseButton : MonoBehaviour
{
    public UnityEvent completedAnimCallback;

    [SerializeField, Range(10f, 200f)] float percentScale = 110f;

    bool isAnimRun = false;
    [SerializeField]
    private bool isSound = true;
    public void ButtonClick()
    {
        if (isAnimRun)
        {
            return;
        }
        if (isSound)
        {
            SoundManager.instance.PlayAudioClip(SoundManager.instance.mainButton);
        }

        isAnimRun = true;
        Vector3 target = transform.localScale * percentScale / 100f;
        Vector3 originalScale = transform.localScale;
        target.z = 0f;
        transform.DOScale(target, 0.05f).OnComplete(delegate
        {
            transform.DOScale(originalScale, 0.05f).OnComplete(delegate
            {
                isAnimRun = false;
                if (this.completedAnimCallback != null)
                {
                    this.completedAnimCallback.Invoke();
                }
            });
        });
    }
}
