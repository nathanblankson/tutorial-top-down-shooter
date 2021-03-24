using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public LayerMask collisionMask;

    private float _speed = 10f;
    private float _damage = 1f;

    private float _lifetime;
    private float _skinWidth = .1f; // increase if enemy moving too fast

    public void SetSpeed(float speed)
    {
        _speed = speed;
    }

    public void SetDamage(float damage)
    {
        _damage = damage;
    }

    private void Start()
    {
        Destroy(gameObject, _lifetime);

        // Check for collisions - if instantiated inside object
        Collider[] initialCollisions = Physics.OverlapSphere(transform.position, .1f, collisionMask);
        if (initialCollisions.Length > 0)
        {
            OnHit(initialCollisions[0]);
        }
    }

    private void Update()
    {
        float moveDistance = _speed * Time.deltaTime;
        CheckCollisions(moveDistance);
        transform.Translate(Vector3.forward * moveDistance);
    }

    private void CheckCollisions(float distance)
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, distance + _skinWidth, collisionMask, QueryTriggerInteraction.Collide))
        {
            OnHit(hit);
        }
    }

    private void OnHit(RaycastHit hit)
    {
        IDamageable damageable = hit.collider.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeHit(_damage, hit);
        }
        Destroy(gameObject);
    }

    private void OnHit(Collider other)
    {
        IDamageable damageable = other.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(_damage);
        }
        Destroy(gameObject);
    }
}
