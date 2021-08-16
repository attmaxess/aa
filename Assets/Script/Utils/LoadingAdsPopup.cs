using System.Collections;
using UnityEngine;
using Spine.Unity;

public class LoadingAdsPopup : MonoBehaviour
{
    //[SerializeField] SkeletonGraphic buttonHintAnim;
    //[SerializeField] SkeletonGraphic buttonSkipAnim;

    public bool isOpen = false;
    private int countCheck;

    private void CheckVideoAds()
    {
        InvokeRepeating("CheckAvailableVideoAds", 0, 3);
    }
    private void CheckAvailableVideoAds()
    {
        countCheck++;
        if (countCheck >= 3)
        {
            OnNovideoAvailible();
            CancelInvoke();
        }
        else
        {
            if (Bridge.instance.isAvailableVideoAds())
            {
                OnClose();                
            }
        }
    }

    LoadingVideoType currentType;

    public void OnOpen(LoadingVideoType type)
    {        
        if (!Bridge.instance.isAvailableVideoAds())
        {
            this.gameObject.SetActive(true);

            //if (isOpen)// check neu dang chay thi an trang thai cu di
            //{
            //    if (currentType == LoadingVideoType.ButtonHint)
            //    {
            //        buttonHintAnim.AnimationState.SetAnimation(0, "idle", true);
            //    }
            //    else
            //    {
            //        buttonSkipAnim.AnimationState.SetAnimation(0, "idle_video", true);
            //    }
            //}

            isOpen = true;
            countCheck = 0;
            CheckVideoAds();

            Bridge.instance.ReloadVideo();

            currentType = type;
            //if (type == LoadingVideoType.ButtonHint)
            //{
            //    buttonHintAnim.AnimationState.SetAnimation(0, "loading", true);                
            //}
            //else
            //{
            //    buttonSkipAnim.AnimationState.SetAnimation(0, "loading", true);
            //}
        }
    }
    public void OnNovideoAvailible()
    {
        OnClose();
    }
    public void OnClose()
    {
        if (isOpen)
        {
            StartCoroutine(delayClose());

            //if (currentType == LoadingVideoType.ButtonHint)
            //{
            //    buttonHintAnim.AnimationState.SetAnimation(0, "idle", true);
            //}
            //else
            //{
            //    buttonSkipAnim.AnimationState.SetAnimation(0, "idle_video", true);
            //}
        }
    }
    IEnumerator delayClose()
    {
        yield return new WaitForSeconds(0.25f);
        this.gameObject.SetActive(false);
        isOpen = false;
    }

    public enum LoadingVideoType
    {
        ButtonHint,
        ButtonSkip
    }
}
