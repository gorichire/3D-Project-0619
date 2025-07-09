using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.Core
{
    public class DestroyAfterEffect : MonoBehaviour
    {
        private void Start()
        {

        }

        void Update()
        {
            ParticleSystem[] particleSystems = GetComponentsInChildren<ParticleSystem>();

            foreach (ParticleSystem ps in particleSystems)
            {
                if (ps.IsAlive())
                {
                    return;
                }
            }

            Destroy(gameObject);
        }
    }
}