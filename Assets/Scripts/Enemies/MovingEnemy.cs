using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collisions))]
public class MovingEnemy : Enemy
{
    Collisions col;
    public GameObject center;
    public GameObject player;

    public Cinemachine.CinemachinePathBase m_Path;
    public Cinemachine.CinemachinePathBase.PositionUnits m_PositionUnits = Cinemachine.CinemachinePathBase.PositionUnits.Distance;
    public float m_Position;
    float startM_Position;
    float startYPosition;

    [Header ("Collision bounds")]
    [SerializeField] Vector3 bottomBounds, rightBounds, leftBounds;
    [SerializeField] Vector3 bottomOffset, rightOffset, leftOffset;

    [Space]

    [Header("Collision bools")]
    [SerializeField] bool onGround;
    [SerializeField] bool leftWall;
    [SerializeField] bool rightWall;

    [Space]

    [SerializeField] float speed;
    [SerializeField] bool canMove;
    [SerializeField] float activateDistance = 25;

    // Whether the enemy looks left or right
    public int direction = -1;

    public override void Start()
    {
        // Call the Start method of base class
        base.Start();

        col = GetComponent<Collisions>();
        player = GameObject.FindGameObjectWithTag("Player");
        startM_Position = m_Position;
        startYPosition = transform.position.y;
    }

    public override void Update()
    {
        base.Update();

        CalculateCollisions();

        // If player is within activate range, can move
        if (CheckDistance(activateDistance))
        {
            canMove = true;
        }

        // Enemy can only move while on the ground and not stunned
        if (canMove)
        {
            if (!isFlying)
            {
                if (!stunned && onGround)
                    m_Position += direction * speed * Time.deltaTime;
            }
            else if (!stunned)
            {
                m_Position += direction * speed * Time.deltaTime;
            }
        }

        // Reverse direction when enemy hits wall
        if (leftWall)
            direction = 1;

        if (rightWall)
            direction = -1;

        // Scale model based on direction;
        center.transform.localScale = new Vector3(1, 1, direction);
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        transform.rotation = TrackRotation(m_Position);
        transform.position = GetNewPosition();
    }

    // Looks at the distance between the enemy and the player
    bool CheckDistance(float minDistance)
    {
        return Vector3.Distance(transform.position, player.transform.position) < minDistance;
    }

    Quaternion TrackRotation(float distanceAlongPath)
    {
        m_Position = m_Path.StandardizeUnit(distanceAlongPath, m_PositionUnits);
        Quaternion r = m_Path.EvaluateOrientationAtUnit(m_Position, m_PositionUnits);
        return r;
    }

    Vector3 GetNewPosition()
    {
        Vector3 newLocation = new Vector3(m_Path.EvaluatePositionAtUnit(m_Position, m_PositionUnits).x, transform.position.y, m_Path.EvaluatePositionAtUnit(m_Position, m_PositionUnits).z);
        return newLocation;
    }

    public override void DealDamage(float value, float stunTime)
    {
        base.DealDamage(value, stunTime);
    }

    void CalculateCollisions()
    {
        onGround = col.BottomCollisions(bottomOffset, bottomBounds);

        leftWall = col.LeftCollisions(leftOffset, leftBounds);
        rightWall = col.RightCollisions(rightOffset, rightBounds);
    }

    public override void EnemyDeath()
    {
        GameObject go = Instantiate(DeathParticle, transform.position, transform.rotation); // Spawn Death particle
        if (direction == -1)
        {
            go.transform.Rotate(0, 180, 0);
        }
        gameObject.SetActive(false);
    }

    public override void Reset()
    {
        base.Reset();
        canMove = false;
        m_Position = startM_Position;
        transform.position = new Vector3(transform.position.x, startYPosition, transform.position.z);
    }
}
