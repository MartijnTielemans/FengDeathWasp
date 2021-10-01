using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : Hazard
{
    SoundManager soundManager;
    Rigidbody rb;
    public float health = 100;
    public float maxHealth;
    public float gravity = 20;

    public bool isFlying = false;
    public bool lightWeight = false;
    public bool canDie = true;

    public GameObject HitParticle;
    public GameObject DeathParticle;

    public virtual void Start()
    {
        soundManager = GameObject.FindGameObjectWithTag("SoundManager").GetComponent<SoundManager>();
        rb = GetComponent<Rigidbody>();
        maxHealth = health;

        if (isFlying)
        {
            rb.useGravity = false;
        }
    }

    public virtual void Update()
    {
        if (health <= 0 && canDie)
        {
            EnemyDeath();
        }
    }

    public virtual void FixedUpdate()
    {
        if (!isFlying)
            rb.velocity += Vector3.down * gravity * Time.deltaTime;
    }

    public virtual void DealDamage(float value, float stunTime)
    {
        Instantiate(HitParticle, transform.position, Quaternion.identity); // Spawn Hit Particle
        health -= value;
        soundManager.PlaySound(soundManager.enemySounds[0]); // Play hit sound
        StartCoroutine(SetStunned(stunTime));
    }

    public IEnumerator SetStunned(float timer)
    {
        stunned = true;

        yield return new WaitForSeconds(timer);

        stunned = false;
    }

    // When an enemy dies
    public virtual void EnemyDeath()
    {
        Instantiate(DeathParticle, transform.position, transform.rotation); // Spawn Death particle
        gameObject.SetActive(false);
    }

    public override void Reset()
    {
        base.Reset();
        health = maxHealth;
    }
}
