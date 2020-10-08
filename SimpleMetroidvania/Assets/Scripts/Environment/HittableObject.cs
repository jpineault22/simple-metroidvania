using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Defines any object that can be hit with a normal attack (enemies, destructible grounds, etc.)
public abstract class HittableObject : MonoBehaviour
{
    public abstract void GetHit(Direction pAttackDirection, int pDamage);
}
