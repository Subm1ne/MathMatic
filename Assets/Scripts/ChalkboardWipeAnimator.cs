using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public struct BoardWipeContent
{
    public string question;
    public string levelInfo;
    public string[] options;
}

public class ChalkboardWipeAnimator : MonoBehaviour
{
    [Header("Referanslar")]
    public RectTransform wipeBar;
    public RectTransform wipeBounds;
    public RectTransform boardWipeBounds;
    public TextMeshProUGUI questionText;
    public TextMeshProUGUI levelInfoText;
    public TextMeshProUGUI[] optionTexts;

    [Header("Silme")]
    public float wipeDuration = 0.55f;
    public Ease wipeEase = Ease.InOutQuad;

    [Header("Bos tahta")]
    public float emptyPauseDuration = 0.4f;

    [Header("Kalemle yazma")]
    public float writeDurationPerChar = 0.035f;
    public float minWriteDuration = 0.18f;
    public float maxWriteDuration = 0.7f;
    public float delayBetweenLines = 0.04f;
    public Ease writeEase = Ease.Linear;

    [Header("Silgi cubugu")]
    [Tooltip("WipeBar RectTransform genisligi 0 ise yedek deger. Boyut/sekil icin WipeBar objesini Inspector'dan duzenle.")]
    public float barWidthFallback = 200f;

    [Header("Ses")]
    public AudioClip eraseSound;
    public AudioClip chalkWriteSound;
    public AudioClip chalkOptionWriteSound;

    [Range(0f, 1f)]
    public float chalkSfxVolume = 1f;

    AudioSource eraseSource;
    AudioSource writeSource;
    AudioSource optionWriteSource;

    RectTransform ActiveBounds => boardWipeBounds != null ? boardWipeBounds : wipeBounds;

    Vector2 posRight;
    Vector2 posLeft;
    float zoneThird;
    float zoneHalfWidth;
    bool isPlaying;
    Sequence activeSequence;
    BoardWipeContent pendingContent;
    bool questionCleared;
    bool levelInfoCleared;
    bool[] optionCleared;

    public bool IsPlaying => isPlaying;

    void Awake()
    {
        if (wipeBounds == null)
            wipeBounds = transform as RectTransform;

        if (boardWipeBounds == null)
        {
            Transform found = transform.root.Find("BoardWipeBounds");
            if (found != null)
                boardWipeBounds = found as RectTransform;
        }

        CachePositions();
        HideBar();
        SetupAudioSources();
    }

    void SetupAudioSources()
    {
        eraseSource = CreateChalkAudioSource("EraseAudio");
        writeSource = CreateChalkAudioSource("WriteAudio");
        optionWriteSource = CreateChalkAudioSource("OptionWriteAudio");
    }

    AudioSource CreateChalkAudioSource(string sourceName)
    {
        Transform existing = transform.Find(sourceName);
        AudioSource source;
        if (existing != null)
        {
            source = existing.GetComponent<AudioSource>();
            if (source != null)
                return source;
        }

        GameObject go = new GameObject(sourceName);
        go.transform.SetParent(transform, false);
        source = go.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = false;
        source.spatialBlend = 0f;
        return source;
    }

    float GetBarWidth()
    {
        if (wipeBar != null && wipeBar.rect.width > 0f)
            return wipeBar.rect.width;
        return barWidthFallback;
    }

    void CachePositions()
    {
        RectTransform bounds = ActiveBounds;
        if (bounds == null)
            return;

        zoneHalfWidth = bounds.rect.width * 0.5f;
        zoneThird = bounds.rect.width / 6f;

        float halfBar = GetBarWidth() * 0.5f;
        posRight = new Vector2(zoneHalfWidth + halfBar, 0f);
        posLeft = new Vector2(-zoneHalfWidth - halfBar, 0f);
    }

    public void PlayWipe(BoardWipeContent content, Action onComplete)
    {
        pendingContent = content;

        if (wipeBar == null)
        {
            PlayWriteOnly(content, onComplete);
            return;
        }

        StopActiveTween();

        RectTransform bounds = ActiveBounds;
        if (bounds == null)
        {
            PlayWriteOnly(content, onComplete);
            return;
        }

        CachePositions();
        ResetWipeClearFlags();
        isPlaying = true;

        wipeBar.gameObject.SetActive(true);
        wipeBar.anchoredPosition = posRight;
        PlayEraseSound();

        activeSequence = DOTween.Sequence();
        activeSequence.Append(
            DOVirtual.Float(0f, 1f, wipeDuration, wipeProgress =>
            {
                wipeBar.anchoredPosition = Vector2.Lerp(posRight, posLeft, wipeProgress);
                UpdateWipeClearing(wipeProgress);
            }).SetEase(wipeEase));
        activeSequence.AppendCallback(() =>
        {
            StopEraseSound();
            ClearAllTexts();
            HideBar();
        });
        activeSequence.AppendInterval(emptyPauseDuration);
        activeSequence.Append(BuildWriteSequence(content));
        activeSequence.OnComplete(() =>
        {
            ShowAllFullText(pendingContent);
            isPlaying = false;
            activeSequence = null;
            onComplete?.Invoke();
        });
    }

    public void SkipWipe(BoardWipeContent content)
    {
        StopActiveTween();
        StopChalkSounds();
        isPlaying = false;
        HideBar();
        ShowAllFullText(content);
    }

    void PlayWriteOnly(BoardWipeContent content, Action onComplete)
    {
        pendingContent = content;
        StopActiveTween();
        isPlaying = true;

        ClearAllTexts();

        activeSequence = DOTween.Sequence();
        activeSequence.AppendInterval(emptyPauseDuration);
        activeSequence.Append(BuildWriteSequence(content));
        activeSequence.OnComplete(() =>
        {
            ShowAllFullText(pendingContent);
            isPlaying = false;
            activeSequence = null;
            onComplete?.Invoke();
        });
    }

    Sequence BuildWriteSequence(BoardWipeContent content)
    {
        Sequence seq = DOTween.Sequence();
        seq.AppendCallback(PlayWriteSound);

        if (levelInfoText != null && !string.IsNullOrEmpty(content.levelInfo))
        {
            seq.Append(WriteTextTween(content.levelInfo, levelInfoText));
            seq.AppendInterval(delayBetweenLines);
        }

        if (questionText != null && !string.IsNullOrEmpty(content.question))
        {
            seq.Append(WriteTextTween(content.question, questionText));
            seq.AppendInterval(delayBetweenLines);
        }

        if (content.options != null && optionTexts != null)
        {
            Sequence optionSeq = DOTween.Sequence();
            bool hasOptionTween = false;
            int count = Mathf.Min(content.options.Length, optionTexts.Length);

            for (int i = 0; i < count; i++)
            {
                if (optionTexts[i] == null || string.IsNullOrEmpty(content.options[i]))
                    continue;

                Tween tween = WriteTextTween(content.options[i], optionTexts[i]);
                if (!hasOptionTween)
                {
                    optionSeq.Append(tween);
                    hasOptionTween = true;
                }
                else
                {
                    optionSeq.Join(tween);
                }
            }

            if (hasOptionTween)
            {
                seq.AppendCallback(SwitchToOptionWriteSound);
                seq.Append(optionSeq);
                seq.AppendCallback(StopOptionWriteSound);
            }
        }

        seq.AppendCallback(StopWriteSound);
        return seq;
    }

    void PlayEraseSound()
    {
        if (eraseSound == null || eraseSource == null)
            return;

        eraseSource.loop = false;
        eraseSource.PlayOneShot(eraseSound, chalkSfxVolume);
    }

    void StopEraseSound()
    {
        if (eraseSource != null && eraseSource.isPlaying)
            eraseSource.Stop();
    }

    void PlayWriteSound()
    {
        if (chalkWriteSound == null || writeSource == null)
            return;

        writeSource.clip = chalkWriteSound;
        writeSource.volume = chalkSfxVolume;
        writeSource.loop = true;
        writeSource.Play();
    }

    void StopWriteSound()
    {
        if (writeSource != null && writeSource.isPlaying)
            writeSource.Stop();
    }

    void SwitchToOptionWriteSound()
    {
        StopWriteSound();
        PlayOptionWriteSound();
    }

    void PlayOptionWriteSound()
    {
        AudioClip clip = chalkOptionWriteSound != null ? chalkOptionWriteSound : chalkWriteSound;
        if (clip == null || optionWriteSource == null)
            return;

        optionWriteSource.clip = clip;
        optionWriteSource.volume = chalkSfxVolume;
        optionWriteSource.loop = true;
        optionWriteSource.Play();
    }

    void StopOptionWriteSound()
    {
        if (optionWriteSource != null && optionWriteSource.isPlaying)
            optionWriteSource.Stop();
    }

    void StopChalkSounds()
    {
        StopEraseSound();
        StopWriteSound();
        StopOptionWriteSound();
    }

    Tween WriteTextTween(string text, TextMeshProUGUI target)
    {
        if (target == null || string.IsNullOrEmpty(text))
            return DOVirtual.DelayedCall(0f, () => { });

        int length = text.Length;

        float duration = Mathf.Clamp(
            length * writeDurationPerChar,
            minWriteDuration,
            maxWriteDuration);

        return DOTween.To(() => 0f, progress =>
            {
                int visible = Mathf.Clamp(Mathf.RoundToInt(progress), 0, length);
                target.text = text.Substring(0, visible);
            },
            length,
            duration)
            .SetEase(writeEase)
            .SetTarget(target)
            .OnComplete(() => ShowFullText(target, text));
    }

    void ResetWipeClearFlags()
    {
        questionCleared = false;
        levelInfoCleared = false;

        int count = optionTexts != null ? optionTexts.Length : 0;
        if (optionCleared == null || optionCleared.Length != count)
            optionCleared = new bool[count];

        for (int i = 0; i < optionCleared.Length; i++)
            optionCleared[i] = false;
    }

    void UpdateWipeClearing(float wipeProgress)
    {
        if (zoneThird <= 0f || wipeProgress <= 0f)
            return;

        TryClearByProgress(questionText, wipeProgress, ref questionCleared);
        TryClearByProgress(levelInfoText, wipeProgress, ref levelInfoCleared);

        if (optionTexts == null)
            return;

        for (int i = 0; i < optionTexts.Length; i++)
        {
            if (i >= optionCleared.Length)
                break;

            bool cleared = optionCleared[i];
            TryClearByProgress(optionTexts[i], wipeProgress, ref cleared);
            optionCleared[i] = cleared;
        }
    }

    void TryClearByProgress(TextMeshProUGUI target, float wipeProgress, ref bool clearedFlag)
    {
        if (clearedFlag || target == null)
            return;

        if (wipeProgress >= GetTextClearThreshold(target))
        {
            ClearText(target);
            clearedFlag = true;
        }
    }

    float GetTextClearThreshold(TextMeshProUGUI target)
    {
        float x = GetTextCenterXInWipeSpace(target);

        if (x > zoneThird)
            return Mathf.Lerp(0.12f, 0.33f, Mathf.InverseLerp(zoneHalfWidth, zoneThird, x));

        if (x < -zoneThird)
            return Mathf.Lerp(0.68f, 0.95f, Mathf.InverseLerp(-zoneThird, -zoneHalfWidth, x));

        return Mathf.Lerp(0.35f, 0.65f, Mathf.InverseLerp(zoneThird, -zoneThird, x));
    }

    float GetTextCenterXInWipeSpace(TextMeshProUGUI target)
    {
        if (target == null || wipeBar == null)
            return 0f;

        RectTransform wipeSpace = wipeBar.parent as RectTransform;
        if (wipeSpace == null)
            return 0f;

        Vector3 worldCenter = target.rectTransform.TransformPoint(target.rectTransform.rect.center);
        return wipeSpace.InverseTransformPoint(worldCenter).x;
    }

    void ClearAllTexts()
    {
        ClearText(questionText);
        ClearText(levelInfoText);

        if (optionTexts == null)
            return;

        foreach (var t in optionTexts)
            ClearText(t);
    }

    void ClearText(TextMeshProUGUI target)
    {
        if (target == null)
            return;

        target.text = "";
        target.maxVisibleCharacters = int.MaxValue;
    }

    void ShowAllFullText(BoardWipeContent content)
    {
        ShowFullText(questionText, content.question);
        ShowFullText(levelInfoText, content.levelInfo);

        if (content.options == null || optionTexts == null)
            return;

        int count = Mathf.Min(content.options.Length, optionTexts.Length);
        for (int i = 0; i < count; i++)
            ShowFullText(optionTexts[i], content.options[i]);
    }

    static void ShowFullText(TextMeshProUGUI target, string text)
    {
        if (target == null)
            return;

        target.text = text ?? "";
        target.maxVisibleCharacters = int.MaxValue;
        target.ForceMeshUpdate();
    }

    void StopActiveTween()
    {
        activeSequence?.Kill();
        activeSequence = null;
        StopChalkSounds();

        if (wipeBar != null)
            wipeBar.DOKill();

        KillTextTween(questionText);
        KillTextTween(levelInfoText);

        if (optionTexts == null)
            return;

        foreach (var t in optionTexts)
            KillTextTween(t);
    }

    static void KillTextTween(TextMeshProUGUI target)
    {
        if (target != null)
            DOTween.Kill(target);
    }

    void HideBar()
    {
        if (wipeBar == null)
            return;

        wipeBar.anchoredPosition = posRight;
        wipeBar.gameObject.SetActive(false);
    }
}
