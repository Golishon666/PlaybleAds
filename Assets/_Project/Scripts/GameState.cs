using System.Collections.Generic;
using UnityEngine.Scripting;

namespace PlayableAdsShort
{
    [Preserve]
    public sealed class GameState
    {
        private readonly HashSet<string> _defeatedTargets = new HashSet<string>();

        public int HeroStrength { get; private set; }
        public bool ChestOpened { get; private set; }
        public bool IsBusy { get; set; }
        public int CompletedActions { get; private set; }

        [Preserve]
        public GameState()
        {
        }

        public void Reset(int heroStrength)
        {
            HeroStrength = heroStrength;
            ChestOpened = false;
            IsBusy = false;
            CompletedActions = 0;
            _defeatedTargets.Clear();
        }

        public void OpenChest(int reward)
        {
            ChestOpened = true;
            HeroStrength += reward;
            CompletedActions++;
        }

        public void Defeat(string targetId, int reward)
        {
            _defeatedTargets.Add(targetId);
            HeroStrength += reward;
            CompletedActions++;
        }

        public bool IsDefeated(string targetId)
        {
            return _defeatedTargets.Contains(targetId);
        }
    }
}
