using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cannon : MonoBehaviour
{
    [SerializeField]
    private float _damage = 1f;
    [SerializeField]
    private float _range = 3f; //for now the range is a general range associated to the ship
    [SerializeField]
    private ParticleSystem fireAnimation;
    [SerializeField]
    private MeshFilter model;
    [SerializeField]
    private AudioSource audioSource;
    Transform transformRelativeToShip;

    public float Damage { get => _damage; }
    public float Range { get => _range; }

    private void Awake()
    {
        /*transformRelativeToShip.position = this.transform.localPosition;
        transformRelativeToShip.rotation = this.transform.localRotation;
        transformRelativeToShip.localScale = this.transform.localScale;*/
    }

    /*public Cannon(float damage, Transform cannonGO)
    {
        _damage = damage;
        transformRelativeToShip.position = cannonGO.localPosition;
        transformRelativeToShip.rotation = cannonGO.localRotation;
        transformRelativeToShip.localScale = cannonGO.localScale;
        *//*fireAnimation = cannonGO.GetComponentInChildren<ParticleSystem>();
        audioSource = cannonGO.GetComponentInChildren<AudioSource>();
        model = cannonGO.GetComponentInChildren<MeshFilter>();*//*
    }*/

    public void UpgradeDamage(int damageAdded)
    {
        _damage += damageAdded;
    }

    public void UpgradeRange(int rangeAdded)
    {
        _range += rangeAdded;
    }

    public void SetCannonSound(AudioClip newClip) //use this function if you want to change the sound produced by a cannon, for instance after an upgrade
    {
        audioSource.clip = newClip;
    }

    public void SetCannonMesh(Mesh newMesh)
    {
        model.mesh = newMesh;
    }
}
