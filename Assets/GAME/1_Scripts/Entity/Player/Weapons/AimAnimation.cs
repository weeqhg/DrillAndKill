using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class AimAnimation : MonoBehaviour
{
    [SerializeField] private Image targetImage;

    public void PlayScaleAnimation()
    {
        if (targetImage == null) targetImage = GetComponent<Image>();

        targetImage.transform.DOKill();
        targetImage.transform.localScale = new Vector3(1.3f, 1.3f, 1f);
        targetImage.transform.DOScale(Vector3.one, 0.1f).SetEase(Ease.OutBack);
    }
}
