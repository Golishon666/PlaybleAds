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
        public TargetView targetPrefab;
        public ChestView chestPrefab;
        public HintRingView hintRingPrefab;
        public PathDotView pathDotPrefab;
        public VfxBurstView vfxBurstPrefab;
        public VfxBurstView slashVfxPrefab;
        public VfxBurstView chestBurstPrefab;
        public VfxBurstView groundBurstPrefab;
        public VfxBurstView waterBurstPrefab;
        public VfxBurstView octopusDeathBurstPrefab;
        public GameObject weaponProjectilePrefab;
        public CtaOverlayView ctaOverlayPrefab;

        [Header("Audio")]
        public AudioClip clickClip;
        public AudioClip chestClip;
        public AudioClip hitClip;
        public AudioClip invalidClip;
        public AudioClip powerClip;
        public AudioClip ctaClip;

        [Header("Gameplay")]
        public int heroStartStrength = PlayableConstants.Gameplay.HeroStartStrength;

        [Header("Motion")]
        public float moveDuration = PlayableConstants.Motion.MoveDuration;
        public float moveSpeed = 3.8f;
        public float attackDuration = PlayableConstants.Motion.AttackDuration;
        public float hintPulseDuration = PlayableConstants.Motion.HintPulseDuration;
        public float pathDotDelay = PlayableConstants.Motion.PathDotDelay;
        public float invalidShakeDuration = PlayableConstants.Motion.InvalidShakeDuration;
        public float ctaDelay = PlayableConstants.Motion.CtaDelay;
        public float chestImpactDelay = PlayableConstants.Motion.ChestRewardSpawnDelay;
        public float selectionPulseDuration = PlayableConstants.Motion.SelectionPulseDuration;
        public float attackImpactDelay = 0.34f;
        public float attackRecoveryDuration = 0.58f;
        public float defeatedHoldDuration = 0.55f;
    }
}
