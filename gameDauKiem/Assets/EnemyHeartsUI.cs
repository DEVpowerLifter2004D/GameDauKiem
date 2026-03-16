using System;
using UnityEngine;
using UnityEngine.UI;
using Unity.VisualScripting;


public class EnemyHeartsUI : MonoBehaviour
{
    [Header("Heart Sprites - Theo thứ tự HP")]
    public Sprite hearts3; // 3 đỏ (HP = 3)
    public Sprite hearts2; // 2 đỏ + 1 xám (HP = 2)
    public Sprite hearts1; // 1 đỏ + 2 xám (HP = 1)
    public Sprite hearts0; // 3 xám (HP = 0)

    [Header("References")]
    public Image heartsImage;  // HeartsImage object
    public EnemyController enemyController;

    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;

        // Tự động tìm
        if (enemyController == null)
        {
            enemyController = GetComponentInParent<EnemyController>();
        }

        if (heartsImage == null)
        {
            heartsImage = GetComponentInChildren<Image>();
        }

        UpdateHearts();
    }

    void Update()
    {
        UpdateHearts();

        // Quay về camera
        if (mainCamera != null)
        {
            transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward,
                            mainCamera.transform.rotation * Vector3.up);
        }
    }

    void UpdateHearts()
    {
        if (enemyController == null || heartsImage == null) return;

        int hp = enemyController.GetCurrentHealth();

        // Đổi sprite theo HP
        switch (hp)
        {
            case 3:
                heartsImage.sprite = hearts3;
                break;
            case 2:
                heartsImage.sprite = hearts2;
                break;
            case 1:
                heartsImage.sprite = hearts1;
                break;
            default:
                heartsImage.sprite = hearts0;
                break;
        }
    }
}
