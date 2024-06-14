using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // State Machine
    [HideInInspector] public StateMachine stateMachine;
    [HideInInspector] public Idle idleState;
    [HideInInspector] public Walking walkingState;
    [HideInInspector] public Jump jumpState;
    [HideInInspector] public Attack attackState;
    [HideInInspector] public Dead deadState;

    // Internal Properties
    [HideInInspector] public Rigidbody thisRigidbody;
    [HideInInspector] public Animator thisAnimator;
    [HideInInspector] public Collider thisCollider;

    // Movement
    [Header("Movement")]
    public float movementSpeed = 10;
    public float movementSmoothness = 0.5f;
    public float maxSpeed = 10;
    [HideInInspector] public Vector2 movementVector;

    // Jump
    [Header("Jump")]
    public float jumpPower = 10;
    public float jumpMovementFactor = 1f;
    [HideInInspector] public bool hasJumpInput;

    // Slope
    [Header("Slope")]
    public float maxSlopeAngle = 45;
    [HideInInspector] public bool isGrounded;
    [HideInInspector] public bool isOnSlope;
    [HideInInspector] public Vector3 slopeNormal;

    // Attack
    [Header("Attack")]
    public int attackStages;
    public List<float> attackStageDurations;
    public List<float> attackStageMaxIntervals;
    public List<float> attackStageImpulses;
    public GameObject swordHitbox;
    public float swordKnockbackImpulse;

    void Awake() {
        thisRigidbody = GetComponent<Rigidbody>();
        thisAnimator = GetComponent<Animator>();
        thisCollider = GetComponent<Collider>();
    }

    // Start is called before the first frame update
    void Start()
    {
        // StateMachine and its states
        stateMachine = new StateMachine();
        idleState = new Idle(this);
        walkingState = new Walking(this);
        jumpState = new Jump(this);
        attackState = new Attack(this);
        deadState = new Dead(this);
        stateMachine.ChangeState(idleState);

        //Toggle hitbox
       swordHitbox.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        bool isUp = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow);
        bool isDown = Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow);
        bool isLeft = Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow);
        bool isRight = Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow);
        float inputX = isRight ? 1 : isLeft ? -1 : 0;
        float inputY = isUp ? 1 : isDown ? -1 : 0;
        movementVector = new Vector2(inputX, inputY);
        hasJumpInput = Input.GetKey(KeyCode.Space);

        // Update Animator
        float velocity = thisRigidbody.velocity.magnitude;
        float velocityRate = velocity / maxSpeed;
        thisAnimator.SetFloat("fvelocity", velocityRate);

        // Physic updates
        DetectGround();
        DetectSlope();

        // StateMachine
        stateMachine.Update();
    }

    void LateUpdate() {
        stateMachine.LateUpdate();
    }

    void FixedUpdate() {
        // Apply gravity
        Vector3 gravityForce = Physics.gravity * (isOnSlope ? 0.25f : 1f);
        thisRigidbody.AddForce(gravityForce, ForceMode.Acceleration);
        LimitSpeed();

        // Statemachine
        stateMachine.FixedUpdate();
    }

    public void OnSwordCollisionEnter(Collider other) {
        var otherObject = other.gameObject;
        var otherRigidbody = otherObject.GetComponent<Rigidbody>();
        var isTarget = otherObject.layer == LayerMask.NameToLayer("Target");
        if (isTarget && otherRigidbody != null) {
            var positionDiff = otherObject.transform.position - gameObject.transform.position;
            var impulseVector = new Vector3(positionDiff.normalized.x, 0, positionDiff.normalized.z);
            impulseVector *= swordKnockbackImpulse;
            otherRigidbody.AddForce(impulseVector, ForceMode.Impulse);
        }
    }

    public Quaternion GetFoward() {
        Camera camera = Camera.main;
        float eulerY = camera.transform.eulerAngles.y;
        return Quaternion.Euler(0, eulerY, 0);
    }

    public void RotateBodyToFaceInput(float alpha = 0f) {
        if(movementVector.IsZero()) return;

        float beta = alpha == 0f ? movementSmoothness : alpha;

        // Calculate rotation
        Camera camera = Camera.main;
        Vector3 inputVector = new Vector3(movementVector.x, 0, movementVector.y);
        Quaternion q1 = Quaternion.LookRotation(inputVector, Vector3.up);
        Quaternion q2 = Quaternion.Euler(0, camera.transform.eulerAngles.y, 0);
        Quaternion toRotation = q1 * q2;
        Quaternion newRotation = Quaternion.LerpUnclamped(transform.rotation, toRotation, beta);

        //Apply rotation
        thisRigidbody.MoveRotation(newRotation);
    }

    public bool AttemptToAttack() {
        if (Input.GetMouseButtonDown(0)) {
            var isAttacking = stateMachine.currentStateName == attackState.name;
            var canAttack = !isAttacking || attackState.CanSwitchStages();
            if (canAttack) {
                var attackStage = isAttacking ? (attackState.stage + 1) : 1;
                attackState.stage = attackStage;
                stateMachine.ChangeState(attackState);
                return true;
            }
        }
        return false;
    }

    private void DetectGround() {
        // Reset flag
        isGrounded = false;

        // Detect ground
        Vector3 origin = transform.position;
        Vector3 direction = Vector3.down;
        float maxDistance = 0.1f;
        LayerMask groundLayer = GameManager.Instance.groundLayer;
        if (Physics.Raycast(origin, direction, maxDistance, groundLayer)) {
            isGrounded = true;
        }
    }

    private void DetectSlope() {
        // Reset flag
        isOnSlope = false;
        slopeNormal = Vector3.zero;

        // Detect ground
        Vector3 origin = transform.position;
        Vector3 direction = Vector3.down;
        float maxDistance = 0.2f;
        if (Physics.Raycast(origin, direction, out var slopeHitInfo, maxDistance)) {
            float angle = Vector3.Angle(Vector3.up, slopeHitInfo.normal);
            isOnSlope = angle < maxSlopeAngle && angle != 0;
            slopeNormal = isOnSlope ? slopeHitInfo.normal : Vector3.zero;
        }
    }

    private void LimitSpeed() {
        Vector3 flatVelocity = new Vector3(thisRigidbody.velocity.x, 0, thisRigidbody.velocity.z);
        if (flatVelocity.magnitude > maxSpeed) {
            Vector3 limitedVelocity = flatVelocity.normalized * maxSpeed;
            thisRigidbody.velocity = new Vector3(limitedVelocity.x, thisRigidbody.velocity.y, limitedVelocity.z);
        }
    }

    /*void OnGUI() {
        string s= stateMachine.currentStateName + " - " + isGrounded + " - " + transform.position;
        GUI.Label(new Rect(5,5,400,100), s);
    }

    void OnDrawGizmos() {
        if(!thisCollider) return;

        Vector3 origin = transform.position;
        Vector3 direction = Vector3.down;
        float maxDistance = 0.1f;

        // Draw ray
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawLine(origin, direction * maxDistance);
    }*/

    void OnGUI(){
        string s= stateMachine.currentStateName + " - " + isOnSlope;
        GUI.Label(new Rect(5,5,400,100), s);
    }
}
