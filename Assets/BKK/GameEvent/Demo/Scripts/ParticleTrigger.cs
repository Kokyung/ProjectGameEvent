using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BKK
{
    public class ParticleTrigger : MonoBehaviour
    {
        public ParticleSystem target;

        public void Boom()
        {
            if (!target) return;

            target.Play(true);
        }
    }
}

