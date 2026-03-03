using UnityEngine;

public class OssicleSound : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Ossicle collided with: " + collision.gameObject.name);
        if (collision.gameObject.CompareTag("Table"))
        {
            AudioManager audioManager = GameObject.Find("AudioManager").GetComponent<AudioManager>();
            Debug.Log("Ossicle collided with table, playing sound.");
            if (audioManager != null && audioManager.SfxSource.clip != audioManager._sfxOssicle)
            {
                Debug.Log("Playing ossicle sound.");
                audioManager.SfxSource.PlayOneShot(audioManager._sfxOssicle, 1f);
            }
        }
    }
}
