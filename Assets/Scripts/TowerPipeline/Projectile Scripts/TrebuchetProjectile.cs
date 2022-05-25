using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class TrebuchetProjectile : Projectile
{
    private float movementSpeed;
    Rigidbody rb;

    float currentTimeToImpact;

    Vector3 currentTargetPosition;
    Vector3 currentTargetVelocity;

    Vector3 lastTargetVelocity = Vector3.zero;
    Vector3 lastFramesVelocity = Vector3.zero;

    Vector3 currentTargetAcceleration = Vector3.zero;

    [SerializeField] private float aoeRadius;


    protected override void Update()
    {

    }

    void Start()
    {
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        /*
        if (target != null)
        {
            currentTargetPosition = target.transform.position;
            currentTargetVelocity = target.nav.velocity;
        }


        if (currentTimeToImpact > 0)
        {
            currentTimeToImpact -= Time.fixedDeltaTime;
        }


        //print(currentTimeToImpact);
        if (lastTargetVelocity == Vector3.zero)
        {
            currentTargetAcceleration = Vector3.zero;
        }
        else
        {
            if (Vector3.Distance(transform.position, currentTargetPosition) > 2f)
            {
                currentTargetAcceleration = (target.nav.velocity - lastTargetVelocity) / Time.fixedDeltaTime;

                Vector3 calculateVelocity = CalculateMomentaryVelocity(currentTimeToImpact);
                calculateVelocity.y = rb.velocity.y;


                float ClampFactor = 1 / Vector3.Distance(transform.position, currentTargetPosition);
                ClampFactor = Mathf.Clamp(ClampFactor, .1f, 2f);
                //print(ClampFactor);
                

                calculateVelocity.x = Mathf.Clamp(calculateVelocity.x, lastFramesVelocity.x - ClampFactor, lastFramesVelocity.x + ClampFactor);
                calculateVelocity.z = Mathf.Clamp(calculateVelocity.z, lastFramesVelocity.z - ClampFactor, lastFramesVelocity.z + ClampFactor);

                //print(Vector3.Magnitude(calculateVelocity));

                rb.velocity = calculateVelocity;


            }
            else
            {
                target.TakeDamage(25f);
                HitNearbyTargets();
                Destroy(gameObject);
            }

        }
        lastTargetVelocity = target.nav.velocity;
        lastFramesVelocity = rb.velocity;

        */
         
    }

    public override void SetTarget(UnitBehavior newTarget, float movementSpeed)
    {
        rb = GetComponent<Rigidbody>();
        target = newTarget;
        this.movementSpeed = movementSpeed;



        currentTimeToImpact = CalculateInitialFlightTime();

        Vector3 projectedImpactPosition = target.GetComponent<UnitNavigation>().GetPositionInSeconds(currentTimeToImpact);

        Instantiate(GameObject.CreatePrimitive(PrimitiveType.Sphere), projectedImpactPosition, Quaternion.identity);

        Vector3 initialVelocity = (projectedImpactPosition - transform.position) / currentTimeToImpact;

        initialVelocity.y = 10;

        rb.velocity = initialVelocity;



    }

    /*
    private Vector3 CalculateMomentaryVelocity()
    {


        return (estimatedTargetPosition - transform.position) / timeToImpact;
    }

    */

    private float CalculateInitialFlightTime()
    {
        return (-2 * 10.0f) / Physics.gravity.y;
    }



    protected void HitNearbyTargets()
    {
        Debug.Log("radius: " + aoeRadius);
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, aoeRadius);
        Debug.Log(hitColliders.Length);
        foreach (var hitCollider in hitColliders)
        {
            Debug.Log("Nearby unit found!");
            UnitBehavior unit = hitCollider.GetComponent<UnitBehavior>();
            unit?.TakeDamage(10);
        }
    }
}
