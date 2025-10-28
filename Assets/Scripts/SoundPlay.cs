using UnityEngine;

public class SoundPlay : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip soundClipGrab;
    public AudioClip soundClipRelease;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }
    
    public void PlayGrabSound()
    {
        if (audioSource != null && soundClipGrab != null)
        {
            audioSource.PlayOneShot(soundClipGrab);
        }
    }   
    
    public void PlayReleaseSound()
    {
        if (audioSource != null && soundClipRelease != null)
        {
            audioSource.PlayOneShot(soundClipRelease);
        }
    }
}
