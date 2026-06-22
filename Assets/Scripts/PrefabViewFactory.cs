using UnityEngine;
using UnityEngine.Scripting;

namespace PlayableAdsShort
{
    [Preserve]
    public sealed class PrefabViewFactory : IViewFactory
    {
        private readonly GameConfig _config;
        private readonly StageView _stage;
        [Preserve]
        public PrefabViewFactory(GameConfig config, StageView stage)
        {
            _config = config;
            _stage = stage;
        }

        public ActorView CreateHero()
        {
            ActorView view = _stage.heroView != null
                ? _stage.heroView
                : Object.Instantiate(_config.heroPrefab, _stage.actorLayer);
            view.Bind(_config.heroStartStrength);
            return view;
        }

        public ChestView CreateChest()
        {
            ChestView view = _stage.chestView != null
                ? _stage.chestView
                : Object.Instantiate(_config.chestPrefab, _stage.targetLayer);
            view.Bind();
            return view;
        }

        public TargetView[] CreateTargets()
        {
            TargetView[] views = _stage.targetViews ?? new TargetView[0];
            if (views.Length == 0 && _stage.targetLayer != null)
            {
                views = _stage.targetLayer.GetComponentsInChildren<TargetView>(includeInactive: false);
            }

            for (int i = 0; i < views.Length; i++)
            {
                if (views[i] != null)
                {
                    views[i].Bind();
                }
            }

            return views;
        }

        public HintRingView CreateHintRing()
        {
            return Object.Instantiate(_config.hintRingPrefab, _stage.hintLayer);
        }

        public PathDotView CreatePathDot()
        {
            return Object.Instantiate(_config.pathDotPrefab, _stage.hintLayer);
        }

        public VfxBurstView CreateSlashVfx()
        {
            VfxBurstView prefab = _config.slashVfxPrefab != null ? _config.slashVfxPrefab : _config.vfxBurstPrefab;
            return Object.Instantiate(prefab, _stage.effectsLayer);
        }

        public VfxBurstView CreateChestBurstVfx()
        {
            VfxBurstView prefab = _config.chestBurstPrefab != null ? _config.chestBurstPrefab : _config.vfxBurstPrefab;
            return Object.Instantiate(prefab, _stage.effectsLayer);
        }

        public VfxBurstView CreateChestRewardFlightVfx()
        {
            VfxBurstView prefab = _config.chestRewardFlightVfxPrefab != null ? _config.chestRewardFlightVfxPrefab : _config.vfxBurstPrefab;
            return Object.Instantiate(prefab, _stage.effectsLayer);
        }

        public VfxBurstView CreateRewardSparkVfx()
        {
            VfxBurstView prefab = _config.rewardSparkPrefab != null ? _config.rewardSparkPrefab : _config.vfxBurstPrefab;
            return Object.Instantiate(prefab, _stage.effectsLayer);
        }

        public VfxBurstView CreateImpactVfx(TargetKind targetKind)
        {
            VfxBurstView prefab = targetKind == TargetKind.WaterEnemy ? _config.waterBurstPrefab : _config.groundBurstPrefab;
            return Object.Instantiate(prefab != null ? prefab : _config.vfxBurstPrefab, _stage.effectsLayer);
        }

        public VfxBurstView CreateDeathVfx(TargetView target)
        {
            if (target == null || target.Id != PlayableConstants.Ids.Octopus || _config.octopusDeathBurstPrefab == null)
            {
                return null;
            }

            return Object.Instantiate(_config.octopusDeathBurstPrefab, _stage.effectsLayer);
        }

        public CtaOverlayView CreateCtaOverlay()
        {
            CtaOverlayView view = Object.Instantiate(_config.ctaOverlayPrefab, _stage.overlayLayer);
            view.Bind();
            return view;
        }
    }
}
