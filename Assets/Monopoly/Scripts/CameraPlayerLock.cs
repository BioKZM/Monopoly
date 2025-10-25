using UnityEngine;

public class CameraPlayerLock : MonoBehaviour
{
    public Transform target;               // Takip edilecek oyuncu
    public Vector3 offset = new Vector3(6f, 0f, 0f); // İzometrik pozisyon
    public float followSpeed = 10f;         // Kameranın pozisyon geçiş hızı
    public float rotationSmooth = 10f;      // Kameranın rotasyon geçiş hızı

    private Quaternion targetRotation;     // Kameranın geçeceği hedef rotasyon

    void Start()
    {
        if (target == null) return;

        // Başlangıçta izometrik bir rotasyon belirle (45 derece yukarıdan + çapraz)
        targetRotation = Quaternion.Euler(45f, 45f, 0f);
        transform.rotation = targetRotation;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Pozisyonu izometrik offset ile takip et
        Vector3 desiredPosition = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);

        // Eğer oyuncu bir köşeye döndüyse, yeni hedef rotasyonu belirle
        UpdateTargetRotationBasedOnPlayerDirection();

        // Kamerayı hedef rotasyona yumuşak geçişle çevir
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSmooth * Time.deltaTime);
    }

    void UpdateTargetRotationBasedOnPlayerDirection()
    {
        // Oyuncunun yönünü kullanarak hedef açıyı belirle
        Vector3 forward = target.forward;
        // forward.y = 0; // yukarı yönü yok say
        forward.Normalize();

        // Oyuncu +X yönüne bakıyorsa (sağa) → kamera arkadan çapraz bakmalı
        // Oyuncu -Z yönüne bakıyorsa (aşağı) → vs.
        float angle = Mathf.Atan2(forward.x, forward.z) * Mathf.Rad2Deg;

        // Kamera açısını izometrik olacak şekilde ayarla (45° yukarıdan)
        targetRotation = Quaternion.Euler(45f, angle + 45f, 0f);
    }
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}
