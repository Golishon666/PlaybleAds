using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace PlayableAdsShort
{
    public sealed class GameLifetimeScope : LifetimeScope
    {
        [SerializeField] private GameConfig config;
        [SerializeField] private StageView stageView;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(config);
            builder.RegisterInstance(stageView);
            builder.RegisterInstance(stageView.worldInput).As<IGameInput>();
            builder.Register<GameState>(Lifetime.Singleton);
            builder.Register<PrefabViewFactory>(Lifetime.Singleton).As<IViewFactory>();
            builder.Register<GameSequence>(Lifetime.Singleton).As<IGameSequence>();
            builder.RegisterEntryPoint<PlayableGamePresenter>();
        }
    }
}
