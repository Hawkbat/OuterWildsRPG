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

        bool animatePickUp;
        float pickUpTime;

        public DropLocation GetLocation() => location;

        void Awake()
        {
            enabled = false;
        }

        public void Init(DropLocation location)
        {
            this.location = location;

            var drop = location.Drop;
            var color = Assets.GetRarityColor(drop.Rarity);

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

            var collider = gameObject.AddComponent<SphereCollider>();
            collider.radius = 1f;
            var owCollider = gameObject.AddComponent<OWCollider>();

            interactReceiver = gameObject.AddComponent<InteractReceiver>();
            interactReceiver.SetInteractionEnabled(true);
            interactReceiver.SetInteractRange(2f);
            interactReceiver.SetPromptText(UITextType.ItemPickUpPrompt);
            interactReceiver.OnPressInteract += OnPressInteract;

            var sector = gameObject.GetComponentInParent<Sector>();
            if (sector != null)
            {
                sector.OnOccupantEnterSector.AddListener(OnOccupantEnterSector);
                sector.OnOccupantExitSector.AddListener(OnOccupantExitSector);

                foreach (var occupant in sector.GetOccupants())
                    OnOccupantEnterSector(occupant);
            }

            All.Add(this);
        }

        void OnDestroy()
        {
            var sector = gameObject.GetComponentInParent<Sector>();
            if (sector != null)
            {
                sector.OnOccupantEnterSector.RemoveListener(OnOccupantEnterSector);
                sector.OnOccupantExitSector.RemoveListener(OnOccupantExitSector);
            }

            All.Remove(this);
        }

        int occupantCount = 0;

        void OnOccupantEnterSector(SectorDetector detector)
        {
            occupantCount++;
            if (occupantCount > 0 && !light.enabled)
                light.enabled = true;
        }

        void OnOccupantExitSector(SectorDetector detector)
        {
            occupantCount--;
            if (occupantCount <= 0 && light.enabled)
                light.enabled = false;
        }

        void OnPressInteract()
        {
            var drop = location.Drop;
            if (!DropManager.PickUpDrop(drop))
            {
                Locator.GetPlayerAudioController().PlayNegativeUISound();
                interactReceiver.ResetInteraction();
                return;
            }

            animatePickUp = true;
            pickUpTime = Time.time;
            enabled = true;

            interactReceiver.DisableInteraction();

            var sparksEmission = sparks.emission;
            sparksEmission.enabled = false;
            var shaftsEmission = shafts.emission;
            shaftsEmission.enabled = false;
            var shineEmission = shine.emission;
            shineEmission.enabled = false;
            var sparklesEmission = sparkles.emission;
            sparklesEmission.enabled = false;

            foreach (var visualPath in location.Visuals)
            {
                try
                {
                    var visual = UnityUtils.GetTransformAtPath(visualPath);
                    visual.gameObject.SetActive(false);
                } catch (Exception ex)
                {
                    OuterWildsRPG.Instance.ModHelper.Console.WriteLine($"Failed to remove visual for {drop.Name} drop location", MessageType.Warning);
                    OuterWildsRPG.Instance.ModHelper.Console.WriteLine(ex.Message, MessageType.Error);
                }
            }

            Locator.GetPlayerAudioController().PlayPickUpItem(ItemType.WarpCore);
        }

        void Update()
        {
            if (animatePickUp)
            {
                var t = Mathf.InverseLerp(pickUpTime, pickUpTime + 0.2f, Time.time);
                transform.localScale = Vector3.one * (1f - t);
                if (t >= 1f)
                {
                    gameObject.SetActive(false);
                    enabled = false;
                }
            }
        }
    }
}
