using System.Collections;
using UnityEngine;

public class FireAnimation : MonoBehaviour
{
    public float ShootingInterval = 0.2f;

    [SerializeField]
    private GameObject _leftBroadSide;
    [SerializeField]
    private GameObject _rightBroadSide;

    private ParticleSystem[] _leftSideParticleSystem;
    private ParticleSystem[] _rightSideParticleSystem;
    private AudioSource audioSource;

    private int _numCannons = 4; //default value

    public float GetFireAnimationTime()
    {
        return _numCannons * ShootingInterval;
    }

    void Start()
    {
        _leftSideParticleSystem = _leftBroadSide.GetComponentsInChildren<ParticleSystem>();
        _rightSideParticleSystem = _rightBroadSide.GetComponentsInChildren<ParticleSystem>();

        _numCannons = GetComponent<Ship>().GetNumberOfCannons() / 2;

        //AnimationDuration = _numCannons * ShootingInterval;
        audioSource = GetComponent<AudioSource>();     
    }


    public void PlayFireAnimation(int broadside)
    {
        StartCoroutine(_playRollingBroadSide(ShootingInterval, broadside));
    }

    public void PlayFireSound()
    {
        audioSource.Play();
    }

    private IEnumerator _playRollingBroadSide(float interval, int side)
    {
        PlayFireSound();
        if (side == 1)
        {
            foreach (var item in _leftSideParticleSystem)
            {
                item.Play();
                yield return new WaitForSeconds(interval);
            }
        }
        else if (side == 0)
        {
            foreach (var item in _rightSideParticleSystem)
            {
                item.Play();
                yield return new WaitForSeconds(interval);
            }
        }
    }
}
