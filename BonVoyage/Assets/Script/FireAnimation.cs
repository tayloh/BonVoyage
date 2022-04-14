using System.Collections;
using UnityEngine;

public class FireAnimation : MonoBehaviour
{
    public float ShootingInterval = 0.5f;

    private ParticleSystem[] _leftSideParticleSystem;
    private ParticleSystem[] _rightSideParticleSystem;

    void Start()
    {
        _leftSideParticleSystem = transform.Find("Left").gameObject.GetComponentsInChildren<ParticleSystem>();
        _rightSideParticleSystem = transform.Find("Right").gameObject.GetComponentsInChildren<ParticleSystem>();
    }


    public void PlayFireAnimation(int broadside)
    {
        StartCoroutine(_playRollingBroadSide(ShootingInterval, broadside));
    }


    private IEnumerator _playRollingBroadSide(float interval, int side)
    {
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
