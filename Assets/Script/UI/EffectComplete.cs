using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectComplete : MonoBehaviour
{
    [SerializeField] List<ParticleSystem> listParticles = null;

    public void PlayEffect()
    {
        listParticles.ForEach(item => item.Play());
    }
}
