using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cannon : MonoBehaviour
{
    [SerializeField]
    private float _damage = 1f;
    [SerializeField]
    private float _range = 3f;
    [SerializeField]
    private ParticleSystem fireAnimation;
    [SerializeField]
    private MeshFilter model;
    [SerializeField]
    private AudioSource audioSource;
    Transform transformRelativeToShip;


    public float Damage { get => _damage; }
    public float Range { get => _range; }

    public Cannon(float damage, Transform cannonGO)
    {
        _damage = damage;
        transformRelativeToShip.position = cannonGO.localPosition;
        transformRelativeToShip.rotation = cannonGO.localRotation;
        transformRelativeToShip.localScale = cannonGO.localScale;
        /*fireAnimation = cannonGO.GetComponentInChildren<ParticleSystem>();
        audioSource = cannonGO.GetComponentInChildren<AudioSource>();
        model = cannonGO.GetComponentInChildren<MeshFilter>();*/
    }

    public void UpgradeDamage(int DamageAdded)
    {
        _damage += DamageAdded;
    }
}
