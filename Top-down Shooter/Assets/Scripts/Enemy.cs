using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Enemy : LivingEntity
{
    private NavMeshAgent _navMeshAgent;
    private Transform _target;

    protected override void Start()
    {
        base.Start();
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _target = GameObject.FindGameObjectWithTag("Player").transform;

        StartCoroutine(UpdateDestination());
    }

    private void Update()
    {
    }

    IEnumerator UpdateDestination()
    {
        float refreshRate = .2f;

        while (_target != null)
        {
            Vector3 targetPosition = new Vector3(_target.position.x, 0, _target.position.z);
            if (!isDead)
            {
                _navMeshAgent.SetDestination(targetPosition);
            }
            yield return new WaitForSeconds(refreshRate);
        }
    }
}
