using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Enemy : LivingEntity
{
    public enum State
    {
        Idle,
        Chasing,
        Attacking
    }

    public float attackDamage = 1f;
    public float attackSpeed = 3f;
    public float attackDistanceThreshold = .5f;
    public float timeBetweenAttacks = 1f;

    private NavMeshAgent _navMeshAgent;
    private Transform _target;
    private LivingEntity _targetEntity;
    private Material _material;

    private Color _originalColor;
    private Color _attackColor = Color.red;

    private float _nextAttackTime;
    private float _collisionRadius;
    private bool _hasTarget;
    private float _targetCollisionRadius;

    private State _currentState;

    protected override void Start()
    {
        base.Start();
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _material = GetComponent<Renderer>().material;
        _originalColor = _material.color;

        if (GameObject.FindGameObjectWithTag("Player").transform != null)
        {
            _currentState = State.Chasing;
            _hasTarget = true;
            _target = GameObject.FindGameObjectWithTag("Player").transform;
            _targetEntity = _target.GetComponent<LivingEntity>();
            _targetEntity.OnDeath += OnTargetDeath;

            _collisionRadius = GetComponent<CapsuleCollider>().radius;
            _targetCollisionRadius = _target.GetComponent<CapsuleCollider>().radius;
        }

        StartCoroutine(UpdateDestination());
    }

    private void Update()
    {
        if (_hasTarget)
        {
            if (Time.time > _nextAttackTime)
            {
                // Use sqr instead of sqr root as it is less expensive to use Vector3.Distance
                float sqrDistanceToTarget = (_target.position - transform.position).sqrMagnitude;
                if (sqrDistanceToTarget < Mathf.Pow(attackDistanceThreshold + _collisionRadius + _targetCollisionRadius, 2))
                {
                    _nextAttackTime = Time.time + timeBetweenAttacks;
                    StartCoroutine(Attack());
                }
            }
        }
    }

    private void OnTargetDeath()
    {
        _hasTarget = false;
        _currentState = State.Idle;
    }

    IEnumerator UpdateDestination()
    {
        float refreshRate = .2f;

        while (_hasTarget)
        {
            if (_currentState == State.Chasing)
            {
                if (!isDead)
                {
                    // Stop outside target's bounds + some distance - don't want to clip through/into
                    Vector3 directionToTarget = (_target.position - transform.position).normalized;
                    Vector3 targetPosition = _target.position - directionToTarget * (_collisionRadius + _targetCollisionRadius + attackDistanceThreshold/2);
                    _navMeshAgent.SetDestination(targetPosition);
                }
            }
            yield return new WaitForSeconds(refreshRate);
        }
    }

    IEnumerator Attack()
    {
        _currentState = State.Attacking;
        _navMeshAgent.enabled = false;

        Vector3 startingPosition = transform.position;

        // Stop outside target's bounds + some distance - don't want to clip through/into
        Vector3 directionToTarget = (_target.position - transform.position).normalized;
        Vector3 targetPosition = _target.position - directionToTarget * (_collisionRadius);

        float percent = 0f;
        float attackAnimationSpeed = 3f;

        _material.color = _attackColor;
        bool hasAppliedDamage = false;

        while (percent <= 1)
        {
            if (percent >= .5f && !hasAppliedDamage)
            {
                hasAppliedDamage = true;
                _targetEntity.TakeDamage(attackDamage);
            }
            percent += Time.deltaTime * attackAnimationSpeed;
            float interpolation = (-Mathf.Pow(percent, 2) + percent) * 4; // parabola - go from 0 to 1 and forth
            transform.position = Vector3.Lerp(startingPosition, targetPosition, interpolation);

            yield return null;
        }

        _material.color = _originalColor;
        _currentState = State.Chasing;
        _navMeshAgent.enabled = true;
    }
}
