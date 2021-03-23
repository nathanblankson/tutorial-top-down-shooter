using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public LayerMask collisionMask;

    private float _speed = 10f;
    private float _damage = 1f;

    public void SetSpeed(float speed)
    {
        _speed = speed;
    }

    public void SetDamage(float damage)
    {
        _damage = damage;
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

        if (Physics.Raycast(ray, out hit, distance, collisionMask, QueryTriggerInteraction.Collide))
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
}
