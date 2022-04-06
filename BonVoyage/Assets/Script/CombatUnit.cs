using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatUnit : MonoBehaviour
{
    // Start is called before the first frame update

    private ParticleSystem[] _leftSideParticleSystem;
    private ParticleSystem[] _rightSideParticleSystem;

    void Start()
    {
        _leftSideParticleSystem = transform.Find("Left").gameObject.GetComponentsInChildren<ParticleSystem>();
        _rightSideParticleSystem = transform.Find("Right").gameObject.GetComponentsInChildren<ParticleSystem>();

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            StartCoroutine(_playRollingBroadSide(0.2f, 0));
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            StartCoroutine(_playRollingBroadSide(0.2f, 1));
        }
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
