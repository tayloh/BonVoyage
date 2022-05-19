using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{

    public static float DefaultSoundtrackVolume = 0.25f;
    public static float DefaultCannonShotVolume = 0.55f;

    public static void AdjustVolumeOfSoundtrack(float volume)
    {
        Camera.main.GetComponent<AudioSource>().volume = volume;
    }
    

    public static IEnumerator FadeDownSoundtrackCoroutine(float ms, float toVolume)
    {
        var startVolume = Camera.main.GetComponent<AudioSource>().volume;
        var currVolume = startVolume;

        while (currVolume > toVolume)
        {
            currVolume -= (startVolume - toVolume) / ms;
            Camera.main.GetComponent<AudioSource>().volume = currVolume;

            yield return new WaitForSecondsRealtime(0.001f);
        }
    }

    public static IEnumerator FadeUpSoundtrackCoroutine(float ms, float toVolume)
    {
        var startVolume = Camera.main.GetComponent<AudioSource>().volume;
        var currVolume = startVolume;

        while (currVolume < toVolume)
        {
            currVolume += (toVolume - startVolume) / ms;
            Camera.main.GetComponent<AudioSource>().volume = currVolume;

            yield return new WaitForSecondsRealtime(0.001f);
        }
    }

}
