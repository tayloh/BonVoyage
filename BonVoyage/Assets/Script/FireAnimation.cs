using System.Collections;
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
        //PlayFireSound();
        if (side == 1)
        {
            /*foreach (var item in _leftSideParticleSystem)
            {
                item.Play();
                yield return new WaitForSeconds(interval);
            }*/
            foreach(Cannon cannon in _leftCannons)
            {
                cannon.PlayFiringAnimation();
                cannon.PlaySound();
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
            foreach (Cannon cannon in _rightCannons)
            {
                cannon.PlayFiringAnimation();
                cannon.PlaySound();
                yield return new WaitForSeconds(interval);
            }
        }
    }
}
