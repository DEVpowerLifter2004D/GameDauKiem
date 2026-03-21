using Cainos.PixelArtPlatformer_VillageProps;
using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.UIElements;

public class FakeFlag : MonoBehaviour
{
    [Header("Fake Flag Settings")]
    [Tooltip("Hiệu ứng khi biến mất")]
    public bool playDisappearEffect = true;

    [Header("Text References")]
    public GameObject fakeFlagText; // Text "ĐÍCH ĐẾN NÈ!"

    void Start()
    {
        // Tự động tìm text con nếu chưa gán
        if (fakeFlagText == null)
        {
            fakeFlagText = GetComponentInChildren<TMPro.TextMeshProUGUI>()?.gameObject;

            // Nếu dùng Text legacy thay vì TMP
            if (fakeFlagText == null)
            {
                var textComp = GetComponentInChildren<UnityEngine.UI.Text>();
                if (textComp != null)
                    fakeFlagText = textComp.gameObject;
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("🎭 FAKE FLAG! Player got trolled! 😂");

            // Biến mất!
            StartCoroutine(DisappearEffect());
        }
    }

    System.Collections.IEnumerator DisappearEffect()
    {
        // Flash nhanh trước khi biến mất
        SpriteRenderer sr = GetComponent<SpriteRenderer>();

        if (sr != null && playDisappearEffect)
        {
            for (int i = 0; i < 3; i++)
            {
                sr.enabled = false;
                if (fakeFlagText != null) fakeFlagText.SetActive(false);

                yield return new WaitForSeconds(0.1f);

                sr.enabled = true;
                if (fakeFlagText != null) fakeFlagText.SetActive(true);

                yield return new WaitForSeconds(0.1f);
            }
        }

        // Biến mất hoàn toàn!
        Debug.Log("💨 Fake flag disappeared!");
        Destroy(gameObject);
    }
}
