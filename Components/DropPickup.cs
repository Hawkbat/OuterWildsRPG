using OuterWildsRPG.Enums;
using OuterWildsRPG.Objects.Drops;
using OuterWildsRPG.Utils;
using OWML.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OuterWildsRPG.Components
{
    public class DropPickup : MonoBehaviour
    {
        public static List<DropPickup> All = new();

        static Gradient fadeOutGradient;
        static Gradient fadeInOutGradient;
        static AnimationCurve linearCurve;
        static Material fullParticleMat;
        static Material halfParticleMat;

        DropLocation location;

        ParticleSystem sparks;
        ParticleSystem shafts;
        ParticleSystem shine;
        ParticleSystem sparkles;
        InteractReceiver interactReceiver;
        Light light;
        OWItem owItem;
        Sector sector;
        int sectorOccupantCount = 0;

        bool animatePickUp;
        float pickUpTime;
        Vector3 pickUpPosition;
        bool isHeld;

        public DropLocation GetLocation() => location;

        void Awake()
        {
            enabled = false;
        }

        public void Init(DropLocation location)
        {
            this.location = location;

            var color = Assets.GetRarityColor(location.Drop.Rarity);

            if (fadeOutGradient == null)
            {
                fadeOutGradient = new()
                {
                    alphaKeys = new GradientAlphaKey[] { new(1f, 0f), new(0f, 1f) },
                    colorKeys = new GradientColorKey[] { new(Color.white, 0f), new(Color.white, 1f) },
                };
            }
            if (fadeInOutGradient == null)
            {
                 fadeInOutGradient = new()
                 {
                     alphaKeys = new GradientAlphaKey[] { new(0f, 0f), new(1f, 0.5f), new(0f, 1f) },
                     colorKeys = new GradientColorKey[] { new(Color.white, 0f), new(Color.white, 1f) },
                 };
            }
            if (linearCurve == null)
            {
                linearCurve = new () { keys = new Keyframe[] { new(0f, 0f), new(1f, 1f) } };
            }
            if (fullParticleMat == null)
            {
                fullParticleMat = new(Shader.Find("Outer Wilds/Particles/Additive"))
                {
                    mainTexture = Assets.ParticleTex,
                };
            }
            if (halfParticleMat == null)
            {
                halfParticleMat = new(Shader.Find("Outer Wilds/Particles/Additive"))
                {
                    mainTexture = Assets.ParticleTex,
                    mainTextureScale = new Vector2(0.5f, 1f),
                    mainTextureOffset = new Vector2(0f, 0f),
                };
            }

            sparks = new GameObject("Sparks").AddComponent<ParticleSystem>();
            sparks.transform.parent = transform;
            sparks.transform.localPosition = Vector3.zero;
            sparks.transform.localEulerAngles = new Vector3(-90f, 0f, 0f);
            sparks.transform.localScale = Vector3.one;
            var sparksMain = sparks.main;
            sparksMain.startSpeed = new(0.25f, 0.5f);
            sparksMain.startSize = 0.02f;
            sparksMain.startColor = color;
            sparksMain.startLifetime = 1f;
            sparksMain.scalingMode = ParticleSystemScalingMode.Hierarchy;
            var sparksShape = sparks.shape;
            sparksShape.shapeType = ParticleSystemShapeType.Cone;
            sparksShape.angle = 10f;
            sparksShape.radius = 0.0625f;
            var sparksColorOverLifetime = sparks.colorOverLifetime;
            sparksColorOverLifetime.color = fadeOutGradient;
            var sparksRenderer = sparks.GetComponent<ParticleSystemRenderer>();
            sparksRenderer.renderMode = ParticleSystemRenderMode.Stretch;
            sparksRenderer.lengthScale = 5f;
            sparksRenderer.pivot = new Vector3(0f, 2.5f, 0f);
            sparksRenderer.sharedMaterial = fullParticleMat;

            shafts = new GameObject("Shafts").AddComponent<ParticleSystem>();
            shafts.transform.parent = transform;
            shafts.transform.localPosition = Vector3.zero;
            shafts.transform.localEulerAngles = new Vector3(-90f, 0f, 0f);
            shafts.transform.localScale = Vector3.one;
            var shaftsMain = shafts.main;
            shaftsMain.startSpeed = 0.01f;
            shaftsMain.startSize = new(0.01f, 0.05f);
            shaftsMain.startColor = color;
            shaftsMain.startLifetime = 1f;
            shaftsMain.scalingMode = ParticleSystemScalingMode.Hierarchy;
            var shaftsShape = shafts.shape;
            shaftsShape.shapeType = ParticleSystemShapeType.Cone;
            shaftsShape.angle = 5f;
            shaftsShape.radius = 0.01f;
            var shaftsColorOverLifetime = shafts.colorOverLifetime;
            shaftsColorOverLifetime.color = fadeInOutGradient;
            var shaftsRenderer = shafts.GetComponent<ParticleSystemRenderer>();
            shaftsRenderer.renderMode = ParticleSystemRenderMode.Stretch;
            shaftsRenderer.lengthScale = 16f;
            shaftsRenderer.pivot = new Vector3(0f, 8f, 0f);
            shaftsRenderer.sharedMaterial = halfParticleMat;

            shine = new GameObject("Shine").AddComponent<ParticleSystem>();
            shine.transform.parent = transform;
            shine.transform.localPosition = Vector3.zero;
            shine.transform.localEulerAngles = new Vector3(-90f, 0f, 0f);
            shine.transform.localScale = Vector3.one;
            var shineMain = shine.main;
            shineMain.startSpeed = new(0f, 0.05f);
            shineMain.startSize = 0.25f;
            shineMain.startColor = color;
            shineMain.startLifetime = 1f;
            shineMain.scalingMode = ParticleSystemScalingMode.Hierarchy;
            var shineShape = shine.shape;
            shineShape.shapeType = ParticleSystemShapeType.Sphere;
            shineShape.radius = 0.0001f;
            var shineColorOverLifetime = shine.colorOverLifetime;
            shineColorOverLifetime.color = fadeOutGradient;
            var shineSizeOverLifetime = shine.sizeOverLifetime;
            shineSizeOverLifetime.size = new(1f, linearCurve);
            var shineRenderer = shine.GetComponent<ParticleSystemRenderer>();
            shineRenderer.sharedMaterial = fullParticleMat;

            sparkles = new GameObject("Sparkles").AddComponent<ParticleSystem>();
            sparkles.transform.parent = transform;
            sparkles.transform.localPosition = Vector3.zero;
            sparkles.transform.localEulerAngles = new Vector3(-90f, 0f, 0f);
            sparkles.transform.localScale = Vector3.one;
            var sparklesMain = sparkles.main;
            sparklesMain.startSpeed = new(0f, 0.01f);
            sparklesMain.startSize = new(0.02f, 0.05f);
            sparklesMain.startColor = color;
            sparklesMain.startLifetime = 1f;
            sparklesMain.scalingMode = ParticleSystemScalingMode.Hierarchy;
            var sparklesShape = sparkles.shape;
            sparklesShape.shapeType = ParticleSystemShapeType.Sphere;
            sparklesShape.radius = 0.125f;
            var sparklesColorOverLifetime = sparkles.colorOverLifetime;
            sparklesColorOverLifetime.color = fadeInOutGradient;
            var sparklesRenderer = sparkles.GetComponent<ParticleSystemRenderer>();
            sparklesRenderer.renderMode = ParticleSystemRenderMode.Stretch;
            sparklesRenderer.lengthScale = 8f;
            sparklesRenderer.sharedMaterial = fullParticleMat;

            light = gameObject.AddComponent<Light>();
            light.range = 5f;
            light.intensity = 1f;
            light.color = color;

            owItem = GetComponentInParent<OWItem>();

            var collider = gameObject.AddComponent<SphereCollider>();
            collider.radius = owItem != null ? 0f : 1f;
            collider.isTrigger = true;
            var owCollider = gameObject.AddComponent<OWCollider>();

            interactReceiver = gameObject.AddComponent<InteractReceiver>();
            interactReceiver.SetInteractionEnabled(owItem == null);
            interactReceiver.SetInteractRange(2f);
            interactReceiver.ChangePrompt(Translations.PromptDropPickup(location.Drop));
            interactReceiver.OnPressInteract += OnPressInteract;

            AttachToSector(gameObject.GetComponentInParent<Sector>());

            All.Add(this);
        }

        void OnDestroy()
        {
            DetachFromSector();
            All.Remove(this);
        }

        void AttachToSector(Sector sector)
        {
            if (this.sector != null)
                DetachFromSector();
            this.sector = sector;
            if (sector != null)
            {
                sectorOccupantCount = 0;
                sector.OnOccupantEnterSector.AddListener(OnOccupantEnterSector);
                sector.OnOccupantExitSector.AddListener(OnOccupantExitSector);

                foreach (var occupant in sector.GetOccupants())
                    OnOccupantEnterSector(occupant);
            }
        }

        void DetachFromSector()
        {
            if (sector != null)
            {
                sector.OnOccupantEnterSector.RemoveListener(OnOccupantEnterSector);
                sector.OnOccupantExitSector.RemoveListener(OnOccupantExitSector);
                sector = null;
            }
        }

        void OnOccupantEnterSector(SectorDetector detector)
        {
            sectorOccupantCount++;
            if (sectorOccupantCount > 0 && !light.enabled)
                light.enabled = true;
            if (sectorOccupantCount > 0)
                ToggleVisuals(true);
        }

        void OnOccupantExitSector(SectorDetector detector)
        {
            sectorOccupantCount--;
            if (sectorOccupantCount <= 0 && light.enabled)
                light.enabled = false;
            if (sectorOccupantCount <= 0f)
                ToggleVisuals(false);
        }

        void OnPressInteract()
        {
            if (owItem != null)
            {
                return;
            }

            if (!DropManager.PickUpDropLocation(GetLocation()))
            {
                Locator.GetPlayerAudioController().PlayNegativeUISound();
                interactReceiver.ResetInteraction();
                OuterWildsRPG.DropQueue.Enqueue(Translations.NotificationPickUpDropFailed(location.Drop));
                return;
            }

            animatePickUp = true;
            pickUpTime = Time.time;
            pickUpPosition = transform.position;
            enabled = true;

            interactReceiver.DisableInteraction();

            foreach (var visualPath in location.Visuals)
            {
                var visual = UnityUtils.GetTransformAtPath(visualPath, $"Failed to remove visual for {location.GetUniqueKey()} drop location");
                if (visual != null) visual.gameObject.SetActive(false);
            }

            Locator.GetPlayerAudioController().PlayOneShotInternal(location.Drop.PickUpAudioType);
        }

        public void OnPickedUp(OWItem item)
        {
            DropManager.EquipDrop(location.Drop, EquipSlot.Item);
            interactReceiver.DisableInteraction();
            isHeld = true;
            enabled = true;
            DetachFromSector();
            ToggleVisuals(false);
        }

        public void OnDropped(OWItem item)
        {
            DropManager.UnequipDrop(location.Drop, EquipSlot.Item);
            interactReceiver.EnableInteraction();
            interactReceiver.ResetInteraction();
            isHeld = false;
            AttachToSector(gameObject.GetComponentInParent<Sector>());
            ToggleVisuals(true);
        }

        void ToggleVisuals(bool enable)
        {
            var sparksEmission = sparks.emission;
            sparksEmission.enabled = enable;
            var shaftsEmission = shafts.emission;
            shaftsEmission.enabled = enable;
            var shineEmission = shine.emission;
            shineEmission.enabled = enable;
            var sparklesEmission = sparkles.emission;
            sparklesEmission.enabled = enable;
            light.enabled = enable;
        }

        void Update()
        {
            if (isHeld)
            {
                var heldItem = Locator.GetToolModeSwapper().GetItemCarryTool().GetHeldItem();
                if (heldItem != owItem)
                {
                    OnDropped(owItem);
                }
            }
            if (animatePickUp)
            {
                var t = Mathf.InverseLerp(pickUpTime, pickUpTime + 0.2f, Time.time);

                var cam = Locator.GetPlayerCamera().transform;
                var targetPos = cam.position + cam.up * -0.25f + cam.right * 0.25f;
                transform.position = Vector3.Lerp(pickUpPosition, targetPos, t);
                
                transform.localScale = Vector3.one * (1f - t);
                
                if (t >= 1f)
                {
                    animatePickUp = false;
                    gameObject.SetActive(false);
                }
            }
            if (!animatePickUp && !isHeld)
                enabled = false;
        }
    }
}
