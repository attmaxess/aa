using UnityEngine;
using UnityEngine.UI;

public class ScrollRectExtension
{
    public static void CenterOnItem(ScrollRect mScrollRect, RectTransform target)
    {
        RectTransform mScrollTransform = mScrollRect.GetComponent<RectTransform>();
        RectTransform maskTransform = mScrollRect.viewport;
        RectTransform mContent = mScrollRect.content;
        // Item is here
        var itemCenterPositionInScroll = GetWorldPointInWidget(mScrollTransform, GetWidgetWorldPoint(target));
        //itemCenterPositionInScroll.y -= 275f;
        // But must be here
        var targetPositionInScroll = GetWorldPointInWidget(mScrollTransform, GetWidgetWorldPoint(maskTransform));
        // So it has to move this distance
        var difference = targetPositionInScroll - itemCenterPositionInScroll;
        difference.z = 0f;
        //clear axis data that is not enabled in the scrollrect
        if (!mScrollRect.horizontal)
        {
            difference.x = 0f;
        }
        if (!mScrollRect.vertical)
        {
            difference.y = 0f;
        }

        var normalizedDifference = new Vector2(
                                       difference.x / (mContent.rect.size.x - mScrollTransform.rect.size.x),
                                       difference.y / (mContent.rect.size.y - mScrollTransform.rect.size.y));

        var newNormalizedPosition = mScrollRect.normalizedPosition - normalizedDifference;
        if (mScrollRect.movementType != ScrollRect.MovementType.Unrestricted)
        {
            newNormalizedPosition.x = Mathf.Clamp01(newNormalizedPosition.x);
            newNormalizedPosition.y = Mathf.Clamp01(newNormalizedPosition.y);
        }

        //mScrollRect.DONormalizedPos(newNormalizedPosition, 0.5f);
        mScrollRect.normalizedPosition = newNormalizedPosition;
    }

    private static Vector3 GetWidgetWorldPoint(RectTransform target)
    {
        var pivotOffset = new Vector3(
                              (0.5f - target.pivot.x) * target.rect.size.x,
                              (0.5f - target.pivot.y) * target.rect.size.y,
                              0f);
        var localPosition = target.localPosition + pivotOffset;
        return target.parent.TransformPoint(localPosition);
    }

    private static Vector3 GetWorldPointInWidget(RectTransform target, Vector3 worldPoint)
    {
        return target.InverseTransformPoint(worldPoint);
    }
}
