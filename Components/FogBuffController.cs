using OuterWildsRPG.Objects.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OuterWildsRPG.Components
{
    public class FogBuffController : MonoBehaviour
    {
        PlanetaryFogController fogController;
        float originalFogDensity;

        void Awake()
        {
            fogController = GetComponent<PlanetaryFogController>();
            originalFogDensity = fogController._fogDensity;
        }

        void Update()
        {
            if (!fogController.enabled) return;
            fogController._fogDensity = originalFogDensity * BuffManager.GetFogDensityMultiplier();
        }
    }
}
