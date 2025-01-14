using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public abstract class OffensiveTower : TowerBehavior
{
    [SerializeField]
    protected OffensiveTowerDataSO offensiveTowerData;
    private UnitBehavior currentTarget;
    public UnitBehavior CurrentTarget { get => currentTarget; protected set => currentTarget = value; }


    [SerializeField]
    Transform rotatableTransform;
    [SerializeField]
    float zOffsetForRotatableTransform;

    [SerializeField]
    protected Transform projectileInstantiatePoint;
    // attackCD is assigned manually below using data pulled from the towers themselves
    protected float attackCD;

    protected bool switchedTarget = false;

    protected Animator anim;

    public bool CanFire { get; set; }

    ITowerBuilder build;

    #region Rotation Stuff

    float rotationSpeed = 100;
    private bool animationEventInstantiateProjectileTriggered;


    #endregion

    private void OnEnable()
    {
        build = GetComponent<ITowerBuilder>();
        build.OnBuildComplete += EnableFiring;
    }

    private void OnDisable()
    {
        build.OnBuildComplete -= EnableFiring;
    }

    protected virtual void Awake()
    {
        offensiveTowerData = Instantiate(offensiveTowerData);

        if (anim == null)
        {
            anim = GetComponentInChildren<Animator>();
        }
    }

    public virtual void AcquireTarget()
    {

        float furthestSoFar = 0;
        UnitBehavior closest = null;
        
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, offensiveTowerData.range);
        foreach (Collider curr in hitColliders)
        {
            if (curr.TryGetComponent(out UnitBehavior currentUnit))
            {
                UnitNavigation unitNav = curr.GetComponent<UnitNavigation>();
                if (unitNav.GetDistanceTravelled() > furthestSoFar)
                {
                    furthestSoFar = unitNav.GetDistanceTravelled();
                    closest = currentUnit;
                }
            }
        }

        if (closest != CurrentTarget)
        {
            switchedTarget = true;
        }
        else
        {
            switchedTarget = false;
        }

        CurrentTarget = closest;
    }

    public override void Update()
    {
        AcquireTarget();
        attackCD -= Time.deltaTime;

        if (CurrentTarget)
        {
            RotateTowardsTarget();

            if (attackCD <= 0)
            {
                anim.SetBool("Firing", true);
            }

            if (CanFire && animationEventInstantiateProjectileTriggered)
            {
                Fire();
                animationEventInstantiateProjectileTriggered = false;
            }
        }
        else
        {
            anim.SetBool("Firing", false);
        }

        if (!active)
        {
            active = true;
        }

    }

    public override TowerDataSO GetTowerData()
    {
        return offensiveTowerData;
    }

    protected bool RotateTowardsTarget()
    {
        Quaternion targetRotation = Quaternion.LookRotation(CurrentTarget.transform.position - rotatableTransform.position);
        targetRotation.eulerAngles = new Vector3(rotatableTransform.rotation.eulerAngles.x, targetRotation.eulerAngles.y, zOffsetForRotatableTransform);
        rotatableTransform.rotation = Quaternion.RotateTowards(rotatableTransform.rotation, targetRotation, Time.deltaTime * rotationSpeed);

        if (Math.Abs(targetRotation.eulerAngles.y - rotatableTransform.rotation.eulerAngles.y) < 5)
        {
            return true;
        }
        else
        {
            return false;
        }
         
    }

    protected virtual void Fire()
    {
        anim.SetFloat("Speed", 1 / offensiveTowerData.GetFireRate());
        attackCD = offensiveTowerData.GetFireRate();

        FMOD_PlayOneShot shootSFX;
        if (TryGetComponent<FMOD_PlayOneShot>(out shootSFX))
        {
            shootSFX.Play();

        }

        StartCoroutine(SpawnProjectile());

        // need to implement a callback from the projectile when it hits target, maybe by subscribing the TakeDamage
        // method below directly to the Action on the projectile script
        // currentTarget.TakeDamage(offensiveTowerData.GetDamage());
    }

    
    protected virtual IEnumerator SpawnProjectile()
    {       
        GameObject projectile = Instantiate(offensiveTowerData.ProjectilePrefab.gameObject, projectileInstantiatePoint.position, Quaternion.identity);
        Projectile projectileScript = projectile.GetComponent<Projectile>();

        projectileScript.SetTarget(CurrentTarget, offensiveTowerData.SpeedOfProjectile);
        projectileScript.SetDamage(offensiveTowerData.GetDamage());
        yield return new WaitForSeconds(offensiveTowerData.projectileSpawnOffset * offensiveTowerData.GetFireRate());
    }

    private void EnableFiring()
    {
        CanFire = true;
    }

    public void AnimationEventInstantiateProjectile()
    {
        Debug.Log("AnimationEventInstantiateProjectile");
        animationEventInstantiateProjectileTriggered = true;
    }


}
