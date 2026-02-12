using UnityEngine;
using Mirror;

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

    private System.Collections.IEnumerator RollRoutine()
    {
        float elapsed = 0;
        
        while (elapsed < rollDuration)
        {
            // Rastgele eksenlerde çılgınca döndür
            transform.Rotate(new Vector3(Random.value, Random.value, Random.value) * spinSpeed * Time.deltaTime);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Döndürme bitti, şimdi "Sihirli" şekilde düzelme evresi
        isRolling = false;
        
        float settleElapsed = 0;
        Quaternion startRot = transform.rotation;

        while (settleElapsed < 0.5f) // Yarım saniyede "çat" diye düzel
        {
            settleElapsed += Time.deltaTime;
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