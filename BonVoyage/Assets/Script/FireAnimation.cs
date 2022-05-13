using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireAnimation : MonoBehaviour
{
    public float ShootingInterval = 0.2f;

    [SerializeField]
    private GameObject _leftBroadSide;
    [SerializeField]
    private GameObject _rightBroadSide;
    private Cannon[] _leftCannons;
    private Cannon[] _rightCannons;

    private ParticleSystem[] _leftSideParticleSystem;
    private ParticleSystem[] _rightSideParticleSystem;
    private AudioSource audioSource;

    private List<float> _cannonWaitFireDurations = new List<float> { 0.15f, 0.2f, 0.2f, 0.25f, 0.25f, 0.3f, 0.3f, 0.35f, 0.4f, 0.45f };
    private Ship _ship;

    private int _numCannons = 4; //default value

    public float GetFireAnimationTime()
    {
        return _numCannons * ShootingInterval;
        
    }

    void Start()
    {
        /*_leftSideParticleSystem = _leftBroadSide.GetComponentsInChildren<ParticleSystem>();
        _rightSideParticleSystem = _rightBroadSide.GetComponentsInChildren<ParticleSystem>();*/
        _leftSideParticleSystem = transform.GetChild(0).GetComponentsInChildren<ParticleSystem>();
        _rightSideParticleSystem = transform.GetChild(1).GetComponentsInChildren<ParticleSystem>();
        _leftCannons = transform.GetChild(0).GetComponentsInChildren<Cannon>();
        _rightCannons = transform.GetChild(1).GetComponentsInChildren<Cannon>();

        _numCannons = GetComponent<Ship>().GetNumberOfCannons() / 2;

        //AnimationDuration = _numCannons * ShootingInterval;
        audioSource = GetComponent<AudioSource>();

        _ship = GetComponent<Ship>();
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
        var intervalDurations = _ship.GetCannonWaitFireDurations();

        //PlayFireSound();
        if (side == 1)
        {
            /*foreach (var item in _leftSideParticleSystem)
            {
                item.Play();
                yield return new WaitForSeconds(interval);
            }*/
            var i = 0;
            foreach(Cannon cannon in _leftCannons)
            {
                interval = intervalDurations[i % intervalDurations.Count];
                cannon.PlayFiringAnimation();
                cannon.PlaySound();
                i++;
                yield return new WaitForSeconds(interval);
            }
        }
        else if (side == 0)
        {
            /*foreach (var item in _rightSideParticleSystem)
            {
                item.Play();
                yield return new WaitForSeconds(interval);
            }*/

            var i = 0;
            foreach (Cannon cannon in _rightCannons)
            {
                interval = intervalDurations[i % intervalDurations.Count];
                cannon.PlayFiringAnimation();
                cannon.PlaySound();
                i++;
                yield return new WaitForSeconds(interval);
            }
        }
    }
}
