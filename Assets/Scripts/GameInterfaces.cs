using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace PlayableAdsShort
{
    public interface IGameInput
    {
        event Action<string> TargetSelected;
        void Publish(string targetId);
        void SetEnabled(bool enabled);
    }

    public interface IGameSequence
    {
        UniTask PlayChestAsync(ActorView hero, ChestView chest, GameState state, CancellationToken token);
        UniTask PlayAttackAsync(ActorView hero, TargetView target, GameState state, CancellationToken token);
        UniTask PlayInvalidAsync(TargetView target, CancellationToken token, bool showMarker = true);
        UniTask PlayInvalidAsync(ChestView chest, CancellationToken token);
        UniTask PlayCtaAsync(CancellationToken token);
        void ShowHint(ActorView hero, ChestView chest);
        void ShowHint(ActorView hero, TargetView target);
        void ClearHint();
    }

    public interface IViewFactory
    {
        ActorView CreateHero();
        ChestView CreateChest();
        TargetView[] CreateTargets();
        HintRingView CreateHintRing();
        PathDotView CreatePathDot();
        VfxBurstView CreateChestRewardFlightVfx();
        VfxBurstView CreateRewardSparkVfx();
        VfxBurstView CreateImpactVfx(TargetKind targetKind);
        VfxBurstView CreateDeathVfx(TargetView target);
        CtaOverlayView CreateCtaOverlay();
    }
}
