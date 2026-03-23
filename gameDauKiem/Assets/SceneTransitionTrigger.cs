using Cainos.PixelArtPlatformer_VillageProps;
using NUnit.Framework;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class SceneTransitionTrigger : MonoBehaviour
{
    [Header("Transition Settings")]
    public Color targetColor = new Color(0.2f, 0.15f, 0.1f, 1f); // Màu underground
    public float fadeDuration = 2f; // 2 giây

    [Header("Overlay Settings")]
    public UnityEngine.UI.Image darkOverlay; // UI Image (optional)
    public float targetOverlayAlpha = 0.4f;

    private Camera mainCamera;
    private Color originalColor;
    private bool hasTransitioned = false;

    void Start()
    {
        mainCamera = Camera.main;
        originalColor = mainCamera.backgroundColor;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !hasTransitioned)
        {
            Debug.Log("🚪 Player entered underground!");
            StartCoroutine(FadeToUnderground());
            hasTransitioned = true;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && hasTransitioned)
        {
            Debug.Log("🚪 Player returned to surface!");
            StartCoroutine(FadeToSurface());
            hasTransitioned = false;
        }
    }

    IEnumerator FadeToUnderground()
    {
        float elapsed = 0f;
        Color startColor = mainCamera.backgroundColor;
        float startAlpha = darkOverlay != null ? darkOverlay.color.a : 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;

            // Fade camera color
            mainCamera.backgroundColor = Color.Lerp(startColor, targetColor, t);

            // Fade overlay
            if (darkOverlay != null)
            {
                Color c = darkOverlay.color;
                c.a = Mathf.Lerp(startAlpha, targetOverlayAlpha, t);
                darkOverlay.color = c;
            }

            yield return null;
        }

        // Đảm bảo kết thúc chính xác
        mainCamera.backgroundColor = targetColor;
        if (darkOverlay != null)
        {
            Color c = darkOverlay.color;
            c.a = targetOverlayAlpha;
            darkOverlay.color = c;
        }
    }

    IEnumerator FadeToSurface()
    {
        float elapsed = 0f;
        Color startColor = mainCamera.backgroundColor;
        float startAlpha = darkOverlay != null ? darkOverlay.color.a : 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;

            // Fade back
            mainCamera.backgroundColor = Color.Lerp(startColor, originalColor, t);

            // Fade overlay back
            if (darkOverlay != null)
            {
                Color c = darkOverlay.color;
                c.a = Mathf.Lerp(startAlpha, 0f, t);
                darkOverlay.color = c;
            }

            yield return null;
        }

        mainCamera.backgroundColor = originalColor;
        if (darkOverlay != null)
        {
            Color c = darkOverlay.color;
            c.a = 0f;
            darkOverlay.color = c;
        }
    }
}
