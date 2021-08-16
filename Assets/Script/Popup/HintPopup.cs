using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class HintPopup : MonoBehaviour
{
    [SerializeField] GameObject container = null;
    [SerializeField] Text hintText = null;

    public void OnOpen(string text)
    {
        gameObject.SetActive(true);
        container.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
        UIController.instance.FadeInPanel();

        hintText.text = text;
    }

    public void OnClose()
    {
        UIController.instance.FadeOutPanel();
        container.transform.localScale = Vector3.zero;
        gameObject.SetActive(false);
    }
}
