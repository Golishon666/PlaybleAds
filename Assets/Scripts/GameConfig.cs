using UnityEngine;

namespace PlayableAdsShort
{
    public enum TargetKind
    {
        Chest,
        GroundEnemy,
        WaterEnemy
    }

    [CreateAssetMenu(menuName = "Playable Ads Short/Game Config")]
    public sealed class GameConfig : ScriptableObject
    {
        [Header("Prefabs")]
        public ActorView heroPrefab;
        public ChestView chestPrefab;
        public HintRingView hintRingPrefab;
        public PathDotView pathDotPrefab;
        public VfxBurstView vfxBurstPrefab;
        public VfxBurstView groundBurstPrefab;
        public VfxBurstView waterBurstPrefab;
        public VfxBurstView octopusDeathBurstPrefab;
        public VfxBurstView rewardSparkPrefab;
        public VfxBurstView chestRewardFlightVfxPrefab;
        public GameObject weaponProjectilePrefab;
        public CtaOverlayView ctaOverlayPrefab;

        [Header("Audio")]
        public AudioClip clickClip;
        public AudioClip chestClip;
        public AudioClip hitClip;
        public AudioClip invalidClip;
        public AudioClip powerClip;
        public AudioClip waterHitClip;
        public AudioClip ctaClip;
        public AudioClip footstepClip;
        [Range(0f, 1f)] public float footstepVolume = 0.45f;

        [Header("Gameplay")]
        public int heroStartStrength = PlayableConstants.Gameplay.HeroStartStrength;

        [Header("Motion")]
        public float moveSpeed = 3.8f;
        public float pathDotDelay = PlayableConstants.Motion.PathDotDelay;
        public float invalidShakeDuration = PlayableConstants.Motion.InvalidShakeDuration;
        public float ctaDelay = PlayableConstants.Motion.CtaDelay;
        public float chestImpactDelay = PlayableConstants.Motion.ChestRewardSpawnDelay;
        [Range(0f, 1f)] public float chestRewardReleaseNormalizedTime = PlayableConstants.Motion.ChestRewardReleaseNormalizedTime;
        public float selectionPulseDuration = PlayableConstants.Motion.SelectionPulseDuration;
        public float attackImpactDelay = 0.34f;
        public float secondAttackImpactDelay = PlayableConstants.Motion.SecondAttackImpactDelay;
        public float attackRecoveryDuration = 0.58f;
    }
}
