using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TempShip : MonoBehaviour, IDamageable<int>
{
    //Drag in the Bullet Emitter from the Component Inspector.
    public GameObject Bullet_Emitter;

    //Drag in the Bullet Prefab from the Component Inspector.
    public GameObject Bullet;

    //Enter the Speed of the Bullet from the Component Inspector.
    public float Bullet_Forward_Force;
    //private GameObject _Ship;


    [SerializeField] private Image _healthBarImage;

    [SerializeField]
    private int _Health = 100;
    private int _maxHealth = 100;

    bool _isDead;

    public void TakeDamage(int damageTaken)
    {
        _Health -= damageTaken;

        if (_healthBarImage != null)
            _healthBarImage.fillAmount = (float)_Health / (float)_maxHealth;

        if (_Health <= 0 && !_isDead)
        {
            Die();
        }
    }

    public void Die()
    {
        _isDead = true;
        var rb = gameObject.AddComponent<Rigidbody>();
        rb.velocity = Vector3.down * 5.0f;

        Destroy(gameObject, 3);
    }

    // Start is called before the first frame update
    void Start()
    {


    }

    // Update is called once per frame
    public virtual void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            shoot();
        }

    }


    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Bullet")
        {
            TakeDamage(20);// hit damage
        }
    }

    protected void shoot()
    {
        if (_isDead) return;

        GameObject Temporary_Bullet_Handler;
        Temporary_Bullet_Handler = Instantiate(Bullet, Bullet_Emitter.transform.position, Bullet_Emitter.transform.rotation) as GameObject;

        //Sometimes bullets may appear rotated incorrectly due to the way its pivot was set from the original modeling package.
        //This is EASILY corrected here, you might have to rotate it from a different axis and or angle based on your particular mesh.
        Temporary_Bullet_Handler.transform.Rotate(Vector3.left * 90);

        //Retrieve the Rigidbody component from the instantiated Bullet and control it.
        Rigidbody Temporary_RigidBody;
        Temporary_RigidBody = Temporary_Bullet_Handler.GetComponent<Rigidbody>();

        //Tell the bullet to be "pushed" forward by an amount set by Bullet_Forward_Force. 
        Temporary_RigidBody.AddForce(Bullet_Emitter.transform.up * Bullet_Forward_Force);

        //Basic Clean Up, set the Bullets to self destruct after 10 Seconds, I am being VERY generous here, normally 3 seconds is plenty.
        Destroy(Temporary_Bullet_Handler, 3.0f);



    }
}