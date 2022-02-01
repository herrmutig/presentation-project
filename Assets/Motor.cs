using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class Motor : MonoBehaviour
{
    // SerializeField makes the attribute visible in the Editor. From there you or any Designer could change the Value
    // without editing the class.

    [Header("Settings")]
    [SerializeField] private float capsuleHeight = 1.8f;
    [SerializeField] private float capsuleRadius = 0.5f;
    [SerializeField] private float stepHeight = 0.1f;

    [Tooltip("ExtendedRayLength should be at least twice as big as stepHeight (for proper stepdown behaviour)")]
    [SerializeField] private float extendedRayLength = 0.2f;
    [SerializeField] private bool isExtended = false;
    [SerializeField] private LayerMask detectionLayers;


    private Rigidbody rb;
    private CapsuleCollider capsule;
    private Vector3 groundNormal;
    

// Debugging Stuff. Is not included, when building the game for release e.g.
#if UNITY_EDITOR
    void OnValidate()
    {
        Awake();
    }

    void OnDrawGizmos()
    {
        Ray r = getGroundDetectionRay();
        Gizmos.color = Color.red;
        Gizmos.DrawRay(r.origin, r.direction * getGroundDetectionRayDistance());
    }
#endif

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        capsule = GetComponent<CapsuleCollider>();
        recalculateColliderBounds();
    }

    public void Detect()
    {
        var groundRay = getGroundDetectionRay();
        var raycastDist = getGroundDetectionRayDistance();

        RaycastHit hit;
        if (Physics.Raycast(groundRay, out hit, raycastDist, detectionLayers, QueryTriggerInteraction.Ignore))
        {
            // Ground hit
            groundNormal = hit.normal;

            // Check if ray penetrates the ground (We need to move up in that case)
            float moveUpDistance = getUnextendedRayDistance() - hit.distance;
            rb.position = new Vector3(rb.position.x, moveUpDistance + rb.position.y, rb.position.z);
        }
        else
        {
            groundNormal = Vector3.zero;
        }
    }

    public Vector3 GroundNormal()
    {
        return groundNormal;
    }

    public bool IsGrounded()
    {
        return groundNormal != Vector3.zero;
    }

    public void SetVelocity(Vector3 velocity, bool extendGroundRays)
    {
        if (IsGrounded() && extendGroundRays)
            velocity = Vector3.ProjectOnPlane(velocity, groundNormal);
        
        isExtended = extendGroundRays;
        rb.velocity = velocity;
    }

    public Vector3 GetVelocity()
    {
        return rb.velocity;
    }

    /// <summary>
    /// We need two Distances, depending on if we are grounded or not.
    /// Extended Distance:
    /// Is normally a longer ray casted because only touching the surface might
    /// lose some collision detections. Also when moving down a slope, it is preferrable to have a bit of a longer
    /// ray, just to ensure that the ground contact is not lost. Good Value for extended ray is "2 * stepheight".

    /// Unextended Ditance:
    /// This distance is used when falling mid-air and for adjusting the Capsule Collider's Position on the surface it is standing on.
    /// The value is determined by the stepHeight.
    /// </summary>
    /// <returns></returns>
    private float getGroundDetectionRayDistance()
    { 
        return isExtended ? getExtendedRayDistance() : getUnextendedRayDistance();
    }

    private float getExtendedRayDistance()
    {
        return (capsule.height * 0.5f) + Mathf.Abs(extendedRayLength);
    }

    private float getUnextendedRayDistance()
    {
        return (capsule.height * 0.5f) + Mathf.Abs(stepHeight);
    }

    private void recalculateColliderBounds()
    {
        // Calculate new collider bounds considereing the Stepheight.
        // This does normally not change during gameplay. So it is a good candidate to initialize it on Awake.
        capsule.radius = capsuleRadius;
        capsule.height = capsuleHeight;

        // Considering StepHeight.
        capsule.center = new Vector3(0f, stepHeight, 0f);
        capsule.height -= stepHeight;
    }

    // Creates a Ray that points in the down Direction of the Capsule.
    private Ray getGroundDetectionRay()
    {
        Vector3 o = capsule.transform.position + capsule.center;
        Vector3 d = -capsule.transform.up;
        return new Ray(o, d);
    }
}
