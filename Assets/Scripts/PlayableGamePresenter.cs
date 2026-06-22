using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Scripting;
using VContainer.Unity;

namespace PlayableAdsShort
{
    [Preserve]
    public sealed class PlayableGamePresenter : IStartable, IDisposable
    {
        private readonly GameConfig _config;
        private readonly StageView _stage;
        private readonly GameState _state;
        private readonly IGameInput _input;
        private readonly IViewFactory _factory;
        private readonly IGameSequence _sequence;
        private readonly Dictionary<string, TargetView> _targetViews = new Dictionary<string, TargetView>();
        private readonly CancellationTokenSource _destroyCts = new CancellationTokenSource();

        private ActorView _hero;
        private ChestView _chest;
        private bool _ctaShown;

        [Preserve]
        public PlayableGamePresenter(
            GameConfig config,
            StageView stage,
            GameState state,
            IGameInput input,
            IViewFactory factory,
            IGameSequence sequence)
        {
            _config = config;
            _stage = stage;
            _state = state;
            _input = input;
            _factory = factory;
            _sequence = sequence;
        }

        public void Start()
        {
            _stage.Apply(_config);
            _state.Reset(_config.heroStartStrength);

            _hero = _factory.CreateHero();
            _chest = _factory.CreateChest();

            foreach (TargetView view in _factory.CreateTargets())
            {
                if (view == null)
                {
                    continue;
                }

                if (_targetViews.ContainsKey(view.Id))
                {
                    Debug.LogWarning($"Duplicate target id '{view.Id}' on {view.name}. The duplicate was ignored.");
                    continue;
                }

                _targetViews[view.Id] = view;
            }

            if (_targetViews.Count == 0)
            {
                Debug.LogError("Playable scene has no TargetView instances. Add target prefabs under StageView.targetLayer.");
            }

            _input.TargetSelected += OnTargetSelected;
            UpdateAvailability();
            RunHintLoopAsync(_destroyCts.Token).Forget();
        }

        public void Dispose()
        {
            _input.TargetSelected -= OnTargetSelected;
            _destroyCts.Cancel();
            _destroyCts.Dispose();
        }

        private void OnTargetSelected(string targetId)
        {
            if (_state.IsBusy || _ctaShown)
            {
                return;
            }

            HandleTargetSelectedAsync(targetId, _destroyCts.Token).Forget();
        }

        private async UniTaskVoid HandleTargetSelectedAsync(string targetId, CancellationToken token)
        {
            _state.IsBusy = true;
            _input.SetEnabled(false);

            try
            {
                if (_chest != null && targetId == _chest.Id)
                {
                    if (_state.ChestOpened)
                    {
                        await _sequence.PlayInvalidAsync(_chest, token);
                    }
                    else
                    {
                        await _sequence.PlayChestAsync(_hero, _chest, _state, token);
                    }
                }
                else if (_targetViews.TryGetValue(targetId, out TargetView targetView))
                {
                    bool hasEnoughStrength = _state.HeroStrength >= targetView.Strength;
                    if (_state.ChestOpened &&
                        !_state.IsDefeated(targetId) &&
                        hasEnoughStrength)
                    {
                        await _sequence.PlayAttackAsync(_hero, targetView, _state, token);
                    }
                    else
                    {
                        await _sequence.PlayInvalidAsync(targetView, token, showMarker: hasEnoughStrength);
                    }
                }

                UpdateAvailability();

                if (AreAllTargetsDefeated())
                {
                    _hero.PlayShopUpdate();
                }

                if (!_ctaShown && ShouldShowCta())
                {
                    _ctaShown = true;
                    await _sequence.PlayCtaAsync(token);
                }
            }
            finally
            {
                _input.SetEnabled(true);
                _state.IsBusy = false;
            }
        }

        private void UpdateAvailability()
        {
            foreach (KeyValuePair<string, TargetView> pair in _targetViews)
            {
                string id = pair.Key;
                TargetView view = pair.Value;
                bool available = _state.ChestOpened && !_state.IsDefeated(id) && _state.HeroStrength >= view.Strength;
                view.SetAvailable(available);
            }
        }

        private bool ShouldShowCta()
        {
            return _targetViews.Values.Any(target => target.EndsGame && _state.IsDefeated(target.Id)) ||
                   AreAllTargetsDefeated();
        }

        private bool AreAllTargetsDefeated()
        {
            return _targetViews.Count > 0 && _targetViews.Keys.All(id => _state.IsDefeated(id));
        }

        private async UniTaskVoid RunHintLoopAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested && !_ctaShown)
            {
                if (!_state.IsBusy)
                {
                    if (!_state.ChestOpened)
                    {
                        _sequence.ShowHint(_hero, _chest);
                    }
                    else
                    {
                        TargetView next = _targetViews.Values
                            .Where(target => !_state.IsDefeated(target.Id))
                            .OrderBy(target => target.Strength)
                            .ThenBy(target => target.HintOrder)
                            .FirstOrDefault(target => _state.HeroStrength >= target.Strength);

                        if (next != null)
                        {
                            _sequence.ShowHint(_hero, next);
                        }
                    }
                }

                await UniTask.Delay(1500, cancellationToken: token);
            }
        }
    }
}
