using UnityEngine;

public class OssicleSound : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Table")) return;
        var audio = AudioManager.Instance;
        if (audio != null && audio.SfxSource.clip != audio.SfxOssicle)
            audio.SfxSource.PlayOneShot(audio.SfxOssicle, 1f);
    }
}
