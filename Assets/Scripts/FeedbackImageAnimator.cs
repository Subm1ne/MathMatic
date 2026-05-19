using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class FeedbackImageAnimator : MonoBehaviour
{
    enum AnimType
    {
        Bounce = 0,
        LeftSlot = 1,
        RightSlot = 2
    }

    [Header("Fotograflar")]
    public Sprite[] correctSprites;
    public Sprite[] wrongSprites;

    [Header("UI Image")]
    public Image leftImage;
    public Image rightImage;

    [Header("1 - Ziplama (Image ve Image 1)")]
    public float bounceRiseDistance = 400f;
    public float bounceUpDuration = 0.4f;
    public float bounceHoldDuration = 0.65f;
    public float bounceDownDuration = 0.35f;
    public Ease bounceUpEase = Ease.OutQuad;
    public Ease bounceDownEase = Ease.InQuad;

    [Header("2 - Image: soldan gir, sola don")]
    public float leftTravelDistance = 500f;
    public float leftEnterDuration = 0.4f;
    public float leftHoldDuration = 0.65f;
    public float leftExitDuration = 0.4f;
    public Ease leftMoveEase = Ease.OutQuad;

    [Header("3 - Image (1): sagdan gir, saga don")]
    public float rightTravelDistance = 500f;
    public float rightEnterDuration = 0.4f;
    public float rightHoldDuration = 0.65f;
    public float rightExitDuration = 0.4f;
    public Ease rightMoveEase = Ease.OutQuad;

    [Header("Genel")]
    public bool avoidRepeat = true;
    public bool logWarnings = true;

    Sequence activeSequence;
    Sprite lastSprite;
    Vector2 leftRestPos;
    Vector2 rightRestPos;

    public float TotalDuration =>
        Mathf.Max(
            bounceUpDuration + bounceHoldDuration + bounceDownDuration,
            leftEnterDuration + leftHoldDuration + leftExitDuration,
            rightEnterDuration + rightHoldDuration + rightExitDuration);

    void Awake()
    {
        CacheRestPositions();
        HideAll();
    }

    public void Play(bool isCorrect)
    {
        if (leftImage == null && rightImage == null)
        {
            Log("Left Image ve Right Image bos.");
            return;
        }

        Sprite[] pool = isCorrect ? correctSprites : wrongSprites;
        if (pool == null || pool.Length == 0)
        {
            Log("Sprite listesi bos.");
            return;
        }

        Stop();

        Sprite pick = avoidRepeat ? PickRandom(pool, lastSprite) : pool[Random.Range(0, pool.Length)];
        lastSprite = pick;

        AnimType anim = PickAnimType();
        activeSequence = PlayAnim(anim, pick);
    }

    AnimType PickAnimType()
    {
        bool hasLeft = leftImage != null;
        bool hasRight = rightImage != null;

        if (hasLeft && hasRight)
            return (AnimType)Random.Range(0, 3);

        if (hasLeft)
            return Random.value > 0.5f ? AnimType.Bounce : AnimType.LeftSlot;

        return Random.value > 0.5f ? AnimType.Bounce : AnimType.RightSlot;
    }

    Sequence PlayAnim(AnimType anim, Sprite sprite)
    {
        switch (anim)
        {
            case AnimType.Bounce:
                Image bounceTarget = PickBounceSlot(out Vector2 bounceRest);
                ApplySprite(bounceTarget, sprite);
                return BuildBounce(bounceTarget, bounceRest);

            case AnimType.LeftSlot:
                if (leftImage == null)
                {
                    ApplySprite(rightImage, sprite);
                    return BuildRightExit(rightImage, rightRestPos);
                }
                ApplySprite(leftImage, sprite);
                return BuildLeftExit(leftImage, leftRestPos);

            default:
                if (rightImage == null)
                {
                    ApplySprite(leftImage, sprite);
                    return BuildLeftExit(leftImage, leftRestPos);
                }
                ApplySprite(rightImage, sprite);
                return BuildRightExit(rightImage, rightRestPos);
        }
    }

    Image PickBounceSlot(out Vector2 rest)
    {
        if (leftImage != null && rightImage != null)
        {
            bool useLeft = Random.value > 0.5f;
            Image t = useLeft ? leftImage : rightImage;
            rest = useLeft ? leftRestPos : rightRestPos;
            HideExcept(t);
            return t;
        }

        Image only = leftImage != null ? leftImage : rightImage;
        rest = only == leftImage ? leftRestPos : rightRestPos;
        HideExcept(only);
        return only;
    }

    Sequence BuildBounce(Image img, Vector2 rest)
    {
        RectTransform rect = img.rectTransform;
        PrepareVisible(img);

        Vector2 below = rest + Vector2.down * bounceRiseDistance;
        rect.anchoredPosition = below;

        Sequence seq = DOTween.Sequence();
        seq.Append(rect.DOAnchorPos(rest, bounceUpDuration).SetEase(bounceUpEase));
        seq.AppendInterval(bounceHoldDuration);
        seq.Append(rect.DOAnchorPos(below, bounceDownDuration).SetEase(bounceDownEase));
        seq.OnComplete(HideAll);
        return seq;
    }

    Sequence BuildLeftExit(Image img, Vector2 rest)
    {
        RectTransform rect = img.rectTransform;
        PrepareVisible(img);

        Vector2 offLeft = rest + Vector2.left * leftTravelDistance;
        rect.anchoredPosition = offLeft;

        Sequence seq = DOTween.Sequence();
        seq.Append(rect.DOAnchorPos(rest, leftEnterDuration).SetEase(leftMoveEase));
        seq.AppendInterval(leftHoldDuration);
        seq.Append(rect.DOAnchorPos(offLeft, leftExitDuration).SetEase(leftMoveEase));
        seq.OnComplete(HideAll);
        return seq;
    }

    Sequence BuildRightExit(Image img, Vector2 rest)
    {
        RectTransform rect = img.rectTransform;
        PrepareVisible(img);

        Vector2 offRight = rest + Vector2.right * rightTravelDistance;
        rect.anchoredPosition = offRight;

        Sequence seq = DOTween.Sequence();
        seq.Append(rect.DOAnchorPos(rest, rightEnterDuration).SetEase(rightMoveEase));
        seq.AppendInterval(rightHoldDuration);
        seq.Append(rect.DOAnchorPos(offRight, rightExitDuration).SetEase(rightMoveEase));
        seq.OnComplete(HideAll);
        return seq;
    }

    void ApplySprite(Image img, Sprite sprite)
    {
        img.sprite = sprite;
        img.preserveAspect = true;
    }

    public void Stop()
    {
        if (activeSequence != null && activeSequence.IsActive())
            activeSequence.Kill();

        if (leftImage != null) KillTweens(leftImage);
        if (rightImage != null) KillTweens(rightImage);
        HideAll();
    }

    void PrepareVisible(Image img)
    {
        HideExcept(img);
        EnsureOpaque(img);
        img.gameObject.SetActive(true);
    }

    void HideExcept(Image active)
    {
        if (leftImage != null && leftImage != active)
            HideImage(leftImage, leftRestPos);
        if (rightImage != null && rightImage != active)
            HideImage(rightImage, rightRestPos);
    }

    void HideAll()
    {
        if (leftImage != null) HideImage(leftImage, leftRestPos);
        if (rightImage != null) HideImage(rightImage, rightRestPos);
    }

    void HideImage(Image img, Vector2 rest)
    {
        KillTweens(img);
        img.rectTransform.anchoredPosition = rest;
        img.rectTransform.localScale = Vector3.one;
        img.gameObject.SetActive(false);
    }

    void CacheRestPositions()
    {
        if (leftImage != null) leftRestPos = leftImage.rectTransform.anchoredPosition;
        if (rightImage != null) rightRestPos = rightImage.rectTransform.anchoredPosition;
    }

    static void EnsureOpaque(Image img)
    {
        Color c = img.color;
        c.a = 1f;
        img.color = c;
    }

    static void KillTweens(Image img)
    {
        img.rectTransform.DOKill();
        img.DOKill();
    }

    void Log(string msg)
    {
        if (logWarnings) Debug.LogWarning("[FeedbackImageAnimator] " + msg, this);
    }

    static Sprite PickRandom(Sprite[] pool, Sprite avoid)
    {
        if (pool.Length == 1) return pool[0];

        Sprite pick = pool[Random.Range(0, pool.Length)];
        int safety = 0;
        while (pick == avoid && safety < 10)
        {
            pick = pool[Random.Range(0, pool.Length)];
            safety++;
        }
        return pick;
    }
}
