using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

[RequireComponent(typeof(Button))]
public class ButtonPressScale : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    [Range(0.5f, 1f)]
    public float pressedScale = 0.92f;
    public float pressDuration = 0.08f;
    public float releaseDuration = 0.12f;
    public Ease pressEase = Ease.OutQuad;
    public Ease releaseEase = Ease.OutBack;

    RectTransform rect;
    Vector3 normalScale;
    Button button;
    bool isPressed;

    void Awake()
    {
        rect = transform as RectTransform;
        button = GetComponent<Button>();
        normalScale = rect.localScale;
    }

    void OnEnable()
    {
        ResetInstant();
    }

    void OnDisable()
    {
        rect.DOKill();
        isPressed = false;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!CanAnimate()) return;

        isPressed = true;
        rect.DOKill();
        rect.DOScale(normalScale * pressedScale, pressDuration).SetEase(pressEase);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Release();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Release();
    }

    void Release()
    {
        if (!isPressed) return;
        isPressed = false;

        if (rect == null) return;

        rect.DOKill();
        rect.DOScale(normalScale, releaseDuration).SetEase(releaseEase);
    }

    bool CanAnimate()
    {
        return button != null && button.interactable && button.enabled;
    }

    void ResetInstant()
    {
        if (rect == null) return;
        rect.DOKill();
        rect.localScale = normalScale;
        isPressed = false;
    }
}
