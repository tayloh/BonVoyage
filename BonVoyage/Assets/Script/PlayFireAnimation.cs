using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayFireAnimation : MonoBehaviour
{

    public float ShootingInterval = 0.5f;

    private ParticleSystem[] _leftSideParticleSystem;
    private ParticleSystem[] _rightSideParticleSystem;

    void Start()
    {
        _leftSideParticleSystem = transform.Find("Left").gameObject.GetComponentsInChildren<ParticleSystem>();
        _rightSideParticleSystem = transform.Find("Right").gameObject.GetComponentsInChildren<ParticleSystem>();

    }
    

    public void FireAnimation(int broadside)
    {
        _playRollingBroadSide(ShootingInterval, broadside);
    }


    private IEnumerator _playRollingBroadSide(float interval, int side)
    {
        if (side == 0)
        {
            foreach (var item in _leftSideParticleSystem)
            {
                item.Play();
                yield return new WaitForSeconds(interval);
            }
        }
        else if (side == 1)
        {
            foreach (var item in _rightSideParticleSystem)
            {
                item.Play();
                yield return new WaitForSeconds(interval);
            }
        }

    }
}
