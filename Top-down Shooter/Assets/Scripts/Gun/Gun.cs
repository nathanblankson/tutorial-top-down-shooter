using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public Transform muzzle;
    public Projectile projectile;
    public float msBetweenShots = 100;
    public float muzzleVelocity = 35f;

    private float _nextShotTime;

    public void Shoot()
    {
        if (Time.time > _nextShotTime) {
            _nextShotTime = Time.time + msBetweenShots / 1000;
            Projectile newProjectile = (Projectile) Instantiate(projectile, muzzle.position, muzzle.rotation);
            newProjectile.SetSpeed(muzzleVelocity);
        }
    }
}
