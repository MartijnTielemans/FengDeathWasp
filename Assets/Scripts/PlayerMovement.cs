using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;

[RequireComponent(typeof(Collisions)), RequireComponent(typeof(PlayerAnimations))]
public class PlayerMovement : MonoBehaviour
{
    GameManager manager;
    SoundManager soundManager;
    public CinemachinePathBase m_Path;

    [SerializeField] GameObject center;
    Rigidbody rb;
    Collisions colScript;
    PlayerAnimations anim;

    [Header("Controls")]
    public InputAction move;
    public InputAction jump;
    public InputAction kick;
    public InputAction respawnButton;

    [Space]

    [Header("Stats")]
    public int health;
    public int maxHealth = 3; // The maximum allowed number of health units
    [SerializeField] float hitPushbackHorizontal = 17;
    [SerializeField] float hitPushbackVertical = 37;
    [SerializeField] float hitStunTime;
    [SerializeField] float invincibilityTime = 2;
    bool invincible = false; // Whether the player can be hit
    public bool canMove; // Whether the player can move using input
    [SerializeField] bool autoMove;
    bool applyPushBack = false;
    bool isHurt = false;
    public bool godMode;

    [Space]

    [Header("Movement")]
    [SerializeField] float moveSpeed; // How fast horizontal movement goes
    [SerializeField] float jumpForce; // How high the player can jump
    [SerializeField] int direction = 1; //Which direction the player is facing
    bool canShortJump = true; // Whther the player can execute a short jump
    bool crouching; // Whether the player is crouching

    [Space]

    [Header("Physics")]
    [SerializeField] float gravity; // How hard the player is pulled downward
    [SerializeField] float maxVelocity = 36;
    [SerializeField] float fallMultiplier; // How fast the player falls down after a jump
    [SerializeField] float lowJumpMultiplier; // How fast the player falls after a short jump (stacks with fallMultiplier)
    [SerializeField] bool hasGravity = true; // Whether the player is affected by gravity

    [Space]

    [Header("Collisions")]
    [SerializeField] bool onGround; // Whether the player is on the ground or not
    BoxCollider playerCollider;
    Vector3 originalColliderSize;
    Vector3 originalColliderOffset;
    [SerializeField] Vector3 smallColliderSize = new Vector3(1, 1, 1);
    [SerializeField] Vector3 smallColliderOffset = new Vector3(0, 0, 0);
    [SerializeField] float originalbottomOffset;
    [SerializeField] float smallbottomOffset;
    bool goingThroughSemiSolid; // Whether the player is currently going through a Semi Solid Platform
    bool leftWall; // Whether the player is touching a wall on their left
    bool rightWall; // Whether the player is touching a wall on their right
    bool inAir;

    [SerializeField] Vector3 bottomBounds, rightBounds, leftBounds;
    [SerializeField] Vector3 bottomOffset, rightOffset, leftOffset;

    public LayerMask attackAffectLayer;

    [Space]

    [Header("Spline")]
    public CinemachinePathBase.PositionUnits m_PositionUnits = CinemachinePathBase.PositionUnits.Distance;
    public float m_Position;
    public Vector2 respawnPosition; // x is the m_position, y is the y position

    [Space]

    [Header("Attack Data")]
    [SerializeField] Attack[] attacks; // An array of all attacks
    List<bool> activeChecks = new List<bool>(); // A list of which attacks are active
    bool doStingMovement;
    [SerializeField] float wingBeatForce = 20;
    [SerializeField] float wingBeatMissRecovery = 1.4f;
    [SerializeField] float airDanceVerticalSpeed = 8;
    [SerializeField] float airDanceHorizontalSpeed = 10;
    float airDanceHorizontal, airDanceVertical;
    [SerializeField] float saltoForce = 30;
    Coroutine AirDanceTimer;
    bool canAirDance = true;

    [Space]

    [Header("Particles")]
    [SerializeField] ParticleSystem airDanceParticle;
    [SerializeField] ParticleSystem dustParticle;
    [SerializeField] ParticleSystem landingDustParticle;
    ParticleSystem.EmissionModule dustEmission;

    [Space]

    [Header("Camera & UI")]
    public GameObject[] vCameras; // Array for Virtual Cameras
    Vector3 defaultFollowOffset; // The default value of the camera's follow offset
    public Animator fadeImage; // A fade UI element

    void OnEnable()
    {
        move.Enable();
        jump.Enable();
        kick.Enable();
        respawnButton.Enable();
    }

    void OnDisable()
    {
        move.Disable();
        jump.Disable();
        kick.Disable();
        respawnButton.Disable();
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        colScript = GetComponent<Collisions>();
        anim = GetComponent<PlayerAnimations>();
        anim.SetFloat("playSpeed", 1);

        // Get the game manager
        manager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        soundManager = GameObject.FindGameObjectWithTag("SoundManager").GetComponent<SoundManager>();

        // Add a bool to the list for each attack
        for (int i = 0; i < attacks.Length; i++)
        {
            activeChecks.Add(false);
        }

        playerCollider = GetComponent<BoxCollider>();
        originalColliderSize = playerCollider.size;
        originalColliderOffset = playerCollider.center;
        originalbottomOffset = bottomOffset.y;

        // Set default respawnPositions
        respawnPosition.x = m_Position;
        respawnPosition.y = transform.position.y;

        dustEmission = dustParticle.emission;
        dustEmission.enabled = false;

        // Spawn in Health UI
        manager.SpawnHealthPoints(health);

        // Set canMove to true after the designated time
        Invoke(nameof(SetCanMove), 1f);
    }

    void Update()
    {
        // Read input
        Vector2 input = move.ReadValue<Vector2>();

        // Update animator parameters
        if (canMove)
        {
            UpdateAnimator();
            anim.SetBool("running", onGround && (!rightWall && input.x > 0 || !leftWall && input.x < 0));
            anim.SetBool("crouching", onGround && input.y < 0 && input.x == 0);
        }

        // For attack animations
        anim.SetBool("stingActive", activeChecks[0]);
        anim.SetBool("wingBeatKickActive", activeChecks[1]);
        anim.SetBool("airDanceKickActive", activeChecks[2]);

        // If any active check is true, set isAttacking to true
        anim.SetBool("isAttacking", activeChecks.Contains(true));

        // Set rigidbody gravity to the correct setting
        rb.useGravity = hasGravity;

        // If autoMove is true, have the player move without input
        if (autoMove)
        {
            m_Position += direction * moveSpeed * Time.deltaTime;
        }

        // If in hitstun, pushback is applied horizontally;
        if (applyPushBack)
        {
            // If the player touches ground, pushback is disabled
            if (onGround && rb.velocity.y < 0)
            {
                applyPushBack = false;
            }

            // If the player inputs while they can move, pushback is disabled
            if (canMove)
            {
                if (input.x != 0)
                {
                    applyPushBack = false;
                }
            }

            // apply pushback force
            if(direction == 1 && !leftWall)
            {
                m_Position += -direction * hitPushbackHorizontal * Time.deltaTime;
            }
            else if(direction == -1 && !rightWall)
            {
                m_Position += -direction * hitPushbackHorizontal * Time.deltaTime;
            }
        }

        // Set the collisions for semi solid platforms
        Physics.IgnoreLayerCollision(9, 8, goingThroughSemiSolid);

        // For fast falling & Short jumps if jump button is not held
        if (hasGravity)
        {
            if (rb.velocity.y < 0)
            {
                rb.velocity += Vector3.up * -gravity * fallMultiplier * Time.deltaTime;
            }
            else if (canShortJump && (rb.velocity.y > 0 && jump.ReadValue<float>() < 0.4f))
            {
                rb.velocity += Vector3.up * -gravity * lowJumpMultiplier * Time.deltaTime;
            }
        }

        // If not on the ground, set inAir to true
        if (!onGround && !inAir)
        {
            inAir = true;
        }
        // Set inAir to false if the ground is hit
        else if (onGround && inAir)
        {
            rb.velocity = new Vector3 (rb.velocity.x, 0, rb.velocity.z);
            inAir = false;

            // Play ground hit sound
            PlaySound(2);
        }

        if (onGround)
        {
            // If moving on the ground, play the dust particle
            if (input.x != 0 && !leftWall && !rightWall)
            {
                dustEmission.enabled = true;
            }
            else
            {
                dustEmission.enabled = false;
            }
        }
        else
        {
            dustEmission.enabled = false;
        }

        if (canMove)
        {
            // If moving, change the position on the track accordingly
            // Left movement
            if (!rightWall && input.x > 0)
            {
                direction = 1;

                if (!rightWall)
                    m_Position += input.x * moveSpeed * Time.deltaTime;
            }

            // Right movement
            if (!leftWall && input.x < 0)
            {
                direction = -1;

                if (!leftWall)
                    m_Position += input.x * moveSpeed * Time.deltaTime;
            }

            // Jump Force upward
            if (onGround && jump.triggered)
            {
                rb.velocity += Vector3.up * jumpForce;
                PlaySound(1); // Plays the jump sound
            }

            if (onGround)
            {
                // You can airdance and short jump again after you hit the ground and can move
                canAirDance = true;
                canShortJump = true;

                // Set crouch collider size
                if (input.y < 0)
                {
                    crouching = true;
                    ChangeColliderSize(smallColliderSize, smallColliderOffset);
                    bottomOffset = new Vector3 (bottomOffset.x, smallbottomOffset, bottomOffset.z);
                }
                else if (crouching)
                {
                    ChangeColliderSize(originalColliderSize, originalColliderOffset);
                    crouching = false;
                    bottomOffset = new Vector3(bottomOffset.x, originalbottomOffset, bottomOffset.z);
                }

                // For performing various attacks
                // Each attack has a different input
                if (input.x == 0 && input.y < 0 && kick.triggered)
                {
                    // Perform WingBeat Kick
                    if (CheckCollision(attacks[1].col))
                    {
                        // Perform attack with no recovery time
                        attacks[1].recoveryTime = 0;
                        StartCoroutine(HandleAttacks(attacks[1].activeTime, attacks[1].recoveryTime, 1));
                    }
                    else
                    {
                        // Perform attack with recovery time
                        attacks[1].recoveryTime = wingBeatMissRecovery;
                        StartCoroutine(HandleAttacks(attacks[1].activeTime, attacks[1].recoveryTime, 1));
                    }
                }
                else if (kick.triggered)
                {
                    if (input.x != 0)
                    {
                        doStingMovement = true;
                    }
                    else
                    {
                        doStingMovement = false;
                    }

                    // Perform Sting
                    StartCoroutine(HandleAttacks(attacks[0].activeTime, attacks[0].recoveryTime, 0));
                }
            }
            else if (!onGround && kick.triggered && canAirDance)
            {
                anim.SetBool("inSaltoState", false);

                // Perform Air Dance Kick
                // kick in input direction unless there is no input direction
                switch (input.y)
                {
                    // up
                    case 1f:
                        HandleAirDance(0, airDanceVerticalSpeed);
                        break;

                    // down
                    case -1f:
                        HandleAirDance(0, -airDanceVerticalSpeed * 2);
                        break;

                    // right
                    case 0f when input.x == 1f:
                        HandleAirDance(airDanceHorizontalSpeed, 0);
                        break;

                    // left
                    case 0f when input.x == -1f:
                        HandleAirDance(airDanceHorizontalSpeed, 0);
                        break;

                    // No directional input = forward
                    default:
                        HandleAirDance(moveSpeed, 0);
                        break;
                }
            }
        }

        // Spawn the hitboxes every frame if the attack is active
        // Sting
        if (activeChecks[0])
        {
            EnableAttackCollider(attacks[0].col, true);
        }
        
        // For the Sting attack movement
        if (doStingMovement && activeChecks[0])
        {
            if (!leftWall && !rightWall)
                m_Position += direction * moveSpeed * Time.deltaTime;
        }

        // Wingbeat Kick
        if (activeChecks[1])
        {
            EnableAttackCollider(attacks[1].col, true);
        }

        // Air Dance Kick
        if (activeChecks[2])
        {
            EnableAttackCollider(attacks[2].col, true);

            // Move in attack direction
            if (direction == -1 && !leftWall)
            {
                m_Position += direction * airDanceHorizontal * Time.deltaTime;
            }
            else if (direction == 1 && !rightWall)
            {
                m_Position += direction * airDanceHorizontal * Time.deltaTime;
            }
            
            rb.velocity = Vector3.up * airDanceVertical;

            if (onGround)
            {
                landingDustParticle.Play();
                AirDanceStopped();
                StopCoroutine(AirDanceTimer);
                canMove = true;
            }
        }

        // Set center z scale to the same as direction
        center.transform.localScale = new Vector3(1, 1, direction);

        // If the respawn button is pressed
        if (respawnButton.triggered)
        {
            StartCoroutine(DeathSequence());
        }
    }

    void FixedUpdate()
    {
        // Calculate Collisions
        CalculateCollisions();

        // Clamp velocity
        if(rb.velocity.y < 0)
            rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxVelocity);

        // Adjust rotation based on dolly track rotation
        transform.rotation = TrackRotation(m_Position);

        // Adjust position based on m_position only on x and z axis
        transform.position = GetNewPosition();

        // Apply gravity
        if (hasGravity)
            rb.velocity += Vector3.down * gravity * Time.deltaTime;
    }

    void CalculateCollisions()
    {
        onGround = colScript.BottomCollisions(bottomOffset, bottomBounds);

        leftWall = colScript.LeftCollisions(leftOffset, leftBounds);
        rightWall = colScript.RightCollisions(rightOffset, rightBounds);
    }

    // Returns a Quaternion based on track rotation
    Quaternion TrackRotation(float distanceAlongPath)
    {
        m_Position = m_Path.StandardizeUnit(distanceAlongPath, m_PositionUnits);
        Quaternion r = m_Path.EvaluateOrientationAtUnit(m_Position, m_PositionUnits);
        return r;
    }

    // Returns a Vector3 based on track position
    Vector3 GetNewPosition()
    {
        Vector3 newLocation = new Vector3(m_Path.EvaluatePositionAtUnit(m_Position, m_PositionUnits).x, transform.position.y, m_Path.EvaluatePositionAtUnit(m_Position, m_PositionUnits).z);
        return newLocation;
    }

    void ChangeColliderSize(Vector3 size, Vector3 center)
    {
        playerCollider.size = size;
        playerCollider.center = center;
    }

    // For checking collision for one frame only
    Collider CheckCollision(Collider col)
    {
        col.enabled = true;
        StartCoroutine(DeSpawnCollider(col));
        return col;
    }

    IEnumerator DeSpawnCollider(Collider col)
    {
        yield return new WaitForEndOfFrame();
        col.enabled = false;
    }

    void EnableAttackCollider(Collider col, bool value)
    {
        col.enabled = value;
    }

    IEnumerator HandleAttacks(float active, float recovery, int i)
    {
        // Spawn hitbox and set canMove to false
        canMove = false;
        activeChecks[i] = true;
        PlaySound(0);

        yield return new WaitForSeconds(active);

        // set active to false / despawn hitbox
        activeChecks[i] = false;

        yield return new WaitForSeconds(recovery);

        // set canMove back to true
        canMove = true;
    }

    void HandleAirDance(float horizontal, float vertical)
    {
        canAirDance = false;

        //rotate model and hitbox up or down if necessary
        // up
        if (vertical > 0)
        {
            if (direction == 1)
            {
                center.transform.localRotation = Quaternion.Euler(-90, 0, 0);
            }
            else if (direction == -1)
            {
                center.transform.localRotation = Quaternion.Euler(90, 0, 0);
            }
        }
        // down
        else if (vertical < 0)
        {
            if (direction == 1)
            {
                center.transform.localRotation = Quaternion.Euler(90, 0, 0);
            }
            else if (direction == -1)
            {
                center.transform.localRotation = Quaternion.Euler(-90, 0, 0);
            }
        }

        // Set collider size to be smaller
        ChangeColliderSize(smallColliderSize, smallColliderOffset);

        // Activate particles & sound
        airDanceParticle.Play();
        PlaySound(4);

        // Set velocity to 0 so there's no previous forces on the player
        rb.velocity = Vector3.zero;

        hasGravity = false;
        canMove = false;
        activeChecks[2] = true;

        airDanceHorizontal = horizontal;
        airDanceVertical = vertical;

        AirDanceTimer = StartCoroutine(AirDanceKickTimer(attacks[2].activeTime, attacks[2].recoveryTime));
    }

    IEnumerator AirDanceKickTimer(float activeTimer, float recoveryTimer)
    {
        yield return new WaitForSeconds(activeTimer);

        AirDanceStopped();

        yield return new WaitForSeconds(recoveryTimer);

        canMove = true;
    }

    void AirDanceSalto()
    {
        // turn off shortjump mulitplier
        canShortJump = false;

        PlaySound(3);

        // Perform salto & animation
        rb.velocity = Vector3.zero;
        rb.velocity += Vector3.up * saltoForce;

        anim.SetBool("inSaltoState", true);

        canAirDance = true;
    }

    void AirDanceStopped()
    {
        // return rotation to normal
        center.transform.localRotation = Quaternion.Euler(Vector3.zero);

        EnableAttackCollider(attacks[2].col, false);
        ChangeColliderSize(originalColliderSize, originalColliderOffset);
        airDanceParticle.Stop();
        StartCoroutine(StopParticle(.2f, landingDustParticle)); // Stops the landning dust particle after a short time
        activeChecks[2] = false;
        hasGravity = true;
    }

    // Causes the player to fly backward
    IEnumerator HandleHitStun(float stunTime, float invincibleTime, float pushback)
    {
        invincible = true;
        canMove = false;
        isHurt = true;

        // Call addPushback, which applies the forces
        AddPushback(pushback);

        yield return new WaitForSeconds(stunTime);
        canMove = true;
        isHurt = false;

        yield return new WaitForSeconds(invincibleTime - stunTime);
        invincible = false;
    }

    void AddPushback(float pushBack)
    {
        // For vertical force
        rb.velocity += Vector3.up * pushBack;

        // For Horizontal force
        applyPushBack = true;
    }

    // Changes the amount of health the player has
    // Used for taking damage
    void ChangeHealth(bool isDamage, int value)
    {
        if (!godMode)
        {
            if (isDamage)
            {
                health -= value;
            }
            else if (!isDamage)
            {
                health += value;
            }
        }
    }

    // After HP < 0, this sequence occurs
    IEnumerator DeathSequence()
    {
        canMove = false;

        // Fade to black
        fadeImage.SetFloat("playSpeed", 1);
        fadeImage.Play("Fade-In");

        yield return new WaitForSeconds(1.6f);

        // Reset player and enemies
        Respawn();

        // Reset enemies
        manager.ResetEnemies();

        yield return new WaitForSeconds(.5f);

        // Fade back in
        fadeImage.Play("Fade-Out");

        // Set canMove back to true
        Invoke(nameof(SetCanMove), 1f);
    }

    // For respawning after death
    void Respawn()
    {
        // Reset position to last checkpoint
        m_Position = respawnPosition.x;
        transform.position = new Vector3 (transform.position.x, respawnPosition.y, transform.position.z);

        // Refill health & set health icons
        health = maxHealth;
        manager.ChangeHealthPoints(health);
    }

    // Changes the active camera
    void ChangeCamera(int index)
    {
        for (int i = 0; i < vCameras.Length; i++)
        {
            if (i == index)
            {
                vCameras[i].SetActive(true);
            }
            else
            {
                vCameras[i].SetActive(false);
            }
        }
    }

    // has the Sound Manager play a sound
    void PlaySound(int index)
    {
        soundManager.PlaySound(soundManager.playerSounds[index]);
    }

    // For updating unity animator parameters
    void UpdateAnimator()
    {
        anim.SetBool("onGround", onGround);

        anim.SetBool("jumping", rb.velocity.y > 0.2f);
        anim.SetBool("falling", rb.velocity.y < -0.2f);

        anim.SetBool("isHurt", isHurt);

        if (onGround || isHurt)
            anim.SetBool("inSaltoState", false);
    }

    // Resets goingThroughSemiSolid to false
    void ResetSemiSolid()
    {
        goingThroughSemiSolid = false;
    }

    // Sets canMove to true
    void SetCanMove()
    {
        canMove = true;
    }

    // Stops a particle system
    IEnumerator StopParticle(float time, ParticleSystem p)
    {
        yield return new WaitForSeconds(time);
        p.Stop();
    }

    private void OnCollisionEnter(Collision col)
    {
        if (!invincible)
        {
            // If the player touches an enemy
            if (col.gameObject.CompareTag("Enemy"))
            {
                // If the enemy's not stunned, deal damage and hitstun to player
                if (!col.gameObject.GetComponent<Hazard>().stunned)
                {
                    for (int i = 0; i < activeChecks.Count; i++)
                    {
                        activeChecks[i] = false;
                    }

                    StartCoroutine(HandleHitStun(hitStunTime, invincibilityTime, hitPushbackVertical));
                    ChangeHealth(true, col.gameObject.GetComponent<Hazard>().damage);
                    manager.ChangeHealthPoints(health);
                    anim.SetBool("isHurt", true);
                    PlaySound(5);
                    Debug.Log("health: " + health);

                    if (health <= 0)
                    {
                        StartCoroutine(DeathSequence());
                    }
                }
            }
        }
    }

    private void OnTriggerEnter(Collider col)
    {
        // For attacks
        if (col.gameObject.CompareTag("Enemy"))
        {
            Enemy enemyScript = col.gameObject.GetComponent<Enemy>();

            if(enemyScript != null)
            {
                // Sting
                if (activeChecks[0])
                {
                    if (!enemyScript.stunned)
                    {
                        enemyScript.DealDamage(attacks[0].damageValue, attacks[0].stunTime);
                    }
                }
                // Wingbeat
                else if (activeChecks[1])
                {
                    if (!enemyScript.stunned)
                    {
                        if (enemyScript.lightWeight)
                            col.gameObject.GetComponent<Rigidbody>().velocity += Vector3.up * wingBeatForce;

                        enemyScript.DealDamage(attacks[1].damageValue, attacks[1].stunTime);
                    }
                }
                // Air dance
                else if (activeChecks[2])
                {
                    AirDanceStopped();
                    StopCoroutine(AirDanceTimer);
                    canMove = true;

                    // Damage enemy and perform salto
                    enemyScript.DealDamage(attacks[1].damageValue, attacks[1].stunTime);
                    AirDanceSalto();
                }
            }
        }

        // If going up, ignore semisolid platform
        if (col.gameObject.CompareTag("SemiSolid") && rb.velocity.y > 0)
        {
            Debug.Log("Semisolid");
            goingThroughSemiSolid = true;
            Invoke("ResetSemiSolid", 0.2f);
        }

        // If a camera trigger is entered, change camera behaviour accordingly
        if (col.gameObject.CompareTag("CamTrigger"))
        {
            // Get trigger script
            CameraTrigger trigger = col.GetComponent<CameraTrigger>();

            // Get trigger values and apply to camera
            ChangeCamera(trigger.newCamera);
        }

        // If trigger enter with a fade trigger
        if (col.gameObject.CompareTag("FadeTrigger"))
        {
            // Make sure player cannot input
            move.Disable();
            jump.Disable();
            kick.Disable();

            autoMove = true;
            fadeImage.SetFloat("playSpeed", .2f);
            fadeImage.Play("Fade-In");

            // fade out music
            soundManager.StartFadeOut(soundManager.Music, 4f);

            // Start Loadscene
            manager.StartLoadScene("LevelEnd", 7f);
        }

        // If come in to contact with a checkpoint, activate it and set respawn location
        if (col.gameObject.CompareTag("Checkpoint"))
        {
            Checkpoint checkpoint = col.gameObject.GetComponent<Checkpoint>();

            if (!checkpoint.active)
            {
                checkpoint.ActivateCkeckpoint();
                soundManager.PlaySound(soundManager.otherSounds[0]);
                respawnPosition.x = m_Position;
                respawnPosition.y = checkpoint.yPos;
            }
        }
    }
}
