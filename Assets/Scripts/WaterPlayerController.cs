using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class AmphibiousAddForceController2D : MonoBehaviour
{
    [Header("Layers")]
    [SerializeField] LayerMask groundMask;
    [SerializeField] LayerMask waterMask;

    [Header("Land/Air (Platformer)")]
    [SerializeField] float maxLandSpeed = 8f;
    [SerializeField] float landAccel = 60f;    // dorongan saat ada input
    [SerializeField] float landBrake = 90f;    // dorongan balik utk rem saat lepas input
    [SerializeField] float landGravityScale = 3f;
    [SerializeField] float jumpForce = 12f;
    [SerializeField] Transform groundCheck;
    [SerializeField] float groundCheckRadius = 0.15f;

    [Header("Water (Swim)")]
    [SerializeField] float maxSwimSpeed = 20f;
    [SerializeField] float swimAccel = 30f;    // akselerasi maksimum
    [SerializeField] float waterDrag = 0f;     // damping biar tidak meluncur terus

    Rigidbody2D rb;
    Vector2 input;
    bool inWater;
    float defaultDrag;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;   // atau Continuous Speculative
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        defaultDrag = rb.linearDamping;
    }

    void Update()
    {
        input = new Vector2(Input.GetAxisRaw("Horizontal"),
                            Input.GetAxisRaw("Vertical"));

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (inWater)
            {
                Debug.Log("Jump f: " + jumpForce);
                // reset vy agar loncat konsisten
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
                rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            }
            else
            {
                Debug.Log("Jump f: " + jumpForce);
                // reset vy agar loncat konsisten
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
                rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            }
           
        }
    }

    void FixedUpdate()
    {
        if (inWater)
        {
            rb.gravityScale = 0;
            rb.linearDamping = 0;
            SwimForces();
        }
        else
        {
            // rb.gravityScale = landGravityScale;
            rb.gravityScale = 0;
            rb.linearDamping = 0;
            // LandAirForces();
            SwimForces();
        }
    }

    // --- DARAT/UDARA: kejar target Vx, biarkan gravitasi urus Vy ---
    void LandAirForces()
    {
        float targetVx = input.x * maxLandSpeed;
        float vxError = targetVx - rb.linearVelocity.x;

        // pilih akselerasi: kalau ada target, pakai landAccel; kalau rem, pakai landBrake
        float maxAx = (Mathf.Abs(targetVx) > 0.01f) ? landAccel : landBrake;

        // konversi kebutuhan percepatan jadi gaya (F = m*a), clamp supaya halus
        float desiredAx = Mathf.Clamp(vxError / Time.fixedDeltaTime, -maxAx, maxAx);
        float forceX = desiredAx * rb.mass;

        rb.AddForce(new Vector2(forceX, 0f), ForceMode2D.Force);
        // opsional: clamp kecepatan horizontal
        float clampedVx = Mathf.Clamp(rb.linearVelocity.x, -maxLandSpeed, maxLandSpeed);
        rb.linearVelocity = new Vector2(clampedVx, rb.linearVelocity.y);
    }

    // --- AIR: thruster 2D, kejar target vektor kecepatan dengan batas akselerasi ---
    void SwimForces()
    {
        Vector2 targetVel = input.normalized * maxSwimSpeed;
        Vector2 velError = targetVel - rb.linearVelocity;

        // percepatan yang diinginkan ke arah error, dibatasi swimAccel
        Vector2 desiredA = Vector2.ClampMagnitude(velError / Time.fixedDeltaTime, swimAccel);
        Vector2 force = desiredA * rb.mass;

        rb.AddForce(force, ForceMode2D.Force);

        // clamp kecepatan maksimum di air
        // if (rb.linearVelocity.sqrMagnitude > maxSwimSpeed * maxSwimSpeed)
        //     rb.linearVelocity = rb.linearVelocity.normalized * maxSwimSpeed;

        // float targetVx = input.x * maxLandSpeed;
        // float vxError = targetVx - rb.linearVelocity.x;

        // // pilih akselerasi: kalau ada target, pakai landAccel; kalau rem, pakai landBrake
        // float maxAx = (Mathf.Abs(targetVx) > 0.01f) ? landAccel : landBrake;

        // // konversi kebutuhan percepatan jadi gaya (F = m*a), clamp supaya halus
        // float desiredAx = Mathf.Clamp(vxError / Time.fixedDeltaTime, -maxAx, maxAx);
        // float forceX = desiredAx * rb.mass;

        // rb.AddForce(new Vector2(forceX, 0f), ForceMode2D.Force);
        // // opsional: clamp kecepatan horizontal
        // float clampedVx = Mathf.Clamp(rb.linearVelocity.x, -maxLandSpeed, maxLandSpeed);
        // rb.linearVelocity = new Vector2(clampedVx, rb.linearVelocity.y);
    }

    bool IsGrounded()
    {
        if (!groundCheck) return rb.IsTouchingLayers(groundMask);
        return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundMask);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & waterMask) != 0)
        {
            inWater = true;
            Debug.Log("In water");
        }
    }
    void OnTriggerExit2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & waterMask) != 0)
        {
            inWater = false;
            Debug.Log("Out water");
        } 
    }
}
