using UnityEngine;
using Mirror;
using System.Collections;

public class DiceVisual : MonoBehaviour
{
    private Quaternion targetRotation;
    private bool isRolling = false;
    public bool isStopped = true;
    
    [Header("Ayarlar")]
    public float spinSpeed = 1500f; // Havada ne kadar hızlı dönecek?
    public float rollDuration = 2f; // Kaç saniye havada kalacak?

    public void Roll(int result, Vector3 spawnPos)
    {
        // Null hatasını önlemek için Rigidbody'i artık pasif kullanabiliriz 
        // veya yerçekimini tamamen kapatabiliriz.
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null) {
            rb.useGravity = false;
            rb.isKinematic = true; // Fizik motoru zarı itip kakmasın
        }

        transform.position = spawnPos;
        targetRotation = GetRotationFromValue(result);
        
        isRolling = true;
        isStopped = false;
        
        // Zamanlayıcıyı başlat (Coroutine yerine Update içinde de yönetebiliriz)
        StopAllCoroutines();
        StartCoroutine(RollRoutine());
    }

    private IEnumerator RollRoutine()
    {
        float elapsed = 0;
        
        // 1. ADIM: Her karede değişmeyecek, sabit ama rastgele bir dönme yönü belirle
        // Her eksene 0.5f ile 1.0f arası değer vererek 'cılız' dönmeleri engelle
        Vector3 torqueAxis = new Vector3(
            Random.Range(0.5f, 1f) * (Random.value > 0.5f ? 1 : -1),
            Random.Range(0.5f, 1f) * (Random.value > 0.5f ? 1 : -1),
            Random.Range(0.5f, 1f) * (Random.value > 0.5f ? 1 : -1)
        );

        while (elapsed < rollDuration)
        {
            // 2. ADIM: Artık her karede Random hesaplamıyoruz.
            // Belirlediğimiz sabit eksende spinSpeed hızıyla döndürüyoruz.
            transform.Rotate(torqueAxis * spinSpeed * Time.deltaTime);
            
            elapsed += Time.deltaTime;
            yield return null;
        }

        // 3. ADIM: "Sihirli" düzelme (Settle) evresi
        isRolling = false;
        float settleElapsed = 0;
        Quaternion startRot = transform.rotation;

        while (settleElapsed < 0.5f)
        {
            settleElapsed += Time.deltaTime;
            // Pürüzsüz geçiş
            transform.rotation = Quaternion.Slerp(startRot, targetRotation, settleElapsed / 0.5f);
            yield return null;
        }

        transform.rotation = targetRotation;
        isStopped = true;
    }

    private Quaternion GetRotationFromValue(int value)
    {
        switch (value)
        {
            case 1: return Quaternion.Euler(0, 0, 0);
            case 2: return Quaternion.Euler(0, 0, 90);
            case 3: return Quaternion.Euler(-90, 0, 0);
            case 4: return Quaternion.Euler(90, 0, 0);
            case 5: return Quaternion.Euler(0, 0, -90);
            case 6: return Quaternion.Euler(-180, 0, 0);
            default: return Quaternion.identity;
        }
    }
}