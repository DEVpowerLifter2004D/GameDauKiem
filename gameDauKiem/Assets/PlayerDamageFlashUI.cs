using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerDamageFlashUI : MonoBehaviour
{
    [Header("Overlay")]
    public Image damageOverlay;

    [Header("Flash Settings")]
    [Range(0f, 1f)] public float maxAlpha = 0.25f;
    public float fadeInTime = 0.04f;
    public float fadeOutTime = 0.2f;
    public Color flashColor = new Color(1f, 0f, 0f, 1f);

    private Coroutine flashRoutine;

    void Awake()
    {
        if (damageOverlay == null)
        {
            damageOverlay = GetComponent<Image>();
        }

        SetOverlayAlpha(0f);
    }

    public void PlayFlash()
    {
        if (damageOverlay == null || !isActiveAndEnabled) return;

        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
        }

        flashRoutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        yield return FadeTo(maxAlpha, fadeInTime);
        yield return FadeTo(0f, fadeOutTime);
        flashRoutine = null;
    }

    private IEnumerator FadeTo(float targetAlpha, float duration)
    {
        Color c = flashColor;
        float startAlpha = damageOverlay.color.a;

        if (duration <= 0f)
        {
            c.a = targetAlpha;
            damageOverlay.color = c;
            yield break;
        }

        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            c.a = Mathf.Lerp(startAlpha, targetAlpha, t / duration);
            damageOverlay.color = c;
            yield return null;
        }

        c.a = targetAlpha;
        damageOverlay.color = c;
    }

    private void SetOverlayAlpha(float alpha)
    {
        Color c = flashColor;
        c.a = alpha;
        damageOverlay.color = c;
    }
}
