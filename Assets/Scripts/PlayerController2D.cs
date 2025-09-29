using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController2D : MonoBehaviour
{
    public float jumpForce = 19f;
    public float coyoteTime = 0.15f;
    public float jumpBuffer = 0.15f;

    public float moveSpeed = 5f;

    public float rotateStepDegrees = 90f;
    public float rotateCooldown = 0.08f;
    public ColorType bottomColor = ColorType.Green;
    public ColorType zeroRotationBottom = ColorType.Green;
    public bool invertOrderDirection = true;

    public float baseGravity = 3f;
    public float apexGravityMult = 0.6f;
    public float fallGravityMult = 1.1f;
    public float apexThreshold = 1.0f;

    Rigidbody2D rb;
    bool grounded;
    float lastGroundedTime, lastJumpPressedTime, lastRotateTime;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = baseGravity;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        SyncBottomFromRotation();
    }

    void OnValidate()
    {
        SyncBottomFromRotation();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
            lastJumpPressedTime = Time.time;

        if (Time.time - lastRotateTime > rotateCooldown)
        {
            if (Input.GetKeyDown(KeyCode.A)) Rotate(-1);
            if (Input.GetKeyDown(KeyCode.D)) Rotate(+1);
        }

        if ((Time.time - lastGroundedTime) < coyoteTime &&
            (Time.time - lastJumpPressedTime) < jumpBuffer)
        {
            Jump();
            lastJumpPressedTime = -999f;
        }
    }

    void FixedUpdate()
    {
        float move = 0f;
        if (Input.GetKey(KeyCode.LeftArrow))  move = -1f;
        if (Input.GetKey(KeyCode.RightArrow)) move = +1f;

        Vector2 v = rb.velocity;
        v.x = move * moveSpeed;
        rb.velocity = v;

        var hit = Physics2D.Raycast(transform.position, Vector2.down, 0.6f, LayerMask.GetMask("Platform"));
        grounded = hit.collider != null;
        if (grounded) lastGroundedTime = Time.time;

        float vy = rb.velocity.y;
        if (Mathf.Abs(vy) < apexThreshold)
            rb.gravityScale = baseGravity * apexGravityMult;
        else if (vy < 0f)
            rb.gravityScale = baseGravity * fallGravityMult;
        else
            rb.gravityScale = baseGravity;
    }

    void Jump()
    {
        var v = rb.velocity; v.y = 0f;
        rb.velocity = v;
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }

    void Rotate(int dir)
    {
        lastRotateTime = Time.time;
        transform.Rotate(0, 0, -dir * rotateStepDegrees);
        ColorType[] cw = { ColorType.Green, ColorType.Blue, ColorType.Red, ColorType.Yellow };
        int idx = System.Array.IndexOf(cw, bottomColor);
        bool clockwise = dir > 0;
        if (invertOrderDirection) clockwise = !clockwise;
        bottomColor = clockwise ? cw[(idx + 1) % 4] : cw[(idx + 3) % 4];
    }

    void SyncBottomFromRotation()
    {
        float z = transform.eulerAngles.z;
        float step = rotateStepDegrees <= 0 ? 90f : rotateStepDegrees;
        int stepsCCW = Mathf.RoundToInt(z / step);
        int stepsCW = invertOrderDirection
            ? ((stepsCCW % 4) + 4) % 4
            : (((-stepsCCW) % 4) + 4) % 4;

        ColorType[] cw = { ColorType.Green, ColorType.Blue, ColorType.Red, ColorType.Yellow };
        int baseIndex = System.Array.IndexOf(cw, zeroRotationBottom);
        if (baseIndex < 0) baseIndex = 0;
        bottomColor = cw[(baseIndex + stepsCW) % 4];
    }
}
