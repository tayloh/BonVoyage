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

    private Ship _ship;

    private Ship _target;

    private int _numCannonsFired = 4; //default value

    public float GetFireAnimationTime()
    {
        var timeInSeconds = 0.0f;
        var timings = _ship.GetCannonWaitFireDurations();

        for (int i = 0; i < _numCannonsFired; i++)
        {
            // Timings can be less than cannons fired
            // It's just a pre set list.
            timeInSeconds += timings[i % timings.Count];
        }

        // Add a little time after the animation is finished to not instantly skip to next ship
        timeInSeconds += 0.75f;

        return timeInSeconds;
        
    }

    void Start()
    {
        /*_leftSideParticleSystem = _leftBroadSide.GetComponentsInChildren<ParticleSystem>();
        _rightSideParticleSystem = _rightBroadSide.GetComponentsInChildren<ParticleSystem>();*/
        _leftSideParticleSystem = transform.GetChild(0).GetComponentsInChildren<ParticleSystem>();
        _rightSideParticleSystem = transform.GetChild(1).GetComponentsInChildren<ParticleSystem>();
        _leftCannons = transform.GetChild(0).GetComponentsInChildren<Cannon>();
        _rightCannons = transform.GetChild(1).GetComponentsInChildren<Cannon>();

        _numCannonsFired = GetComponent<Ship>().GetNumberOfCannons() / 2;

        //AnimationDuration = _numCannons * ShootingInterval;
        audioSource = GetComponent<AudioSource>();

        _ship = GetComponent<Ship>();
    }


    public void PlayFireAnimation(int broadside, int numCannons)
    {
        _numCannonsFired = numCannons;
        StartCoroutine(_playRollingBroadSide(ShootingInterval, broadside));
    }

    public void PlayFireSound()
    {
        audioSource.Play();
    }

    public void SetTargetShip(Ship ship)
    {
        _target = ship;
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

                if (i >= _numCannonsFired)
                {
                    yield break;
                }

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

                if (i >= _numCannonsFired)
                {
                    yield break;
                }

                yield return new WaitForSeconds(interval);
            }
        }
    }
}
