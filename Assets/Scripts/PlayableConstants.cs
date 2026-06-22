using UnityEngine;

namespace PlayableAdsShort
{
    public static class PlayableConstants
    {
        public static class Ids
        {
            public const string Chest = "chest";
            public const string RightEnemy = "right_enemy";
            public const string LeftEnemy = "left_enemy";
            public const string Stingray = "stingray";
            public const string Octopus = "octopus";
            public const string Shark = "shark";
        }

        public static class Names
        {
            public const string DockGuard = "Dock Guard";
            public const string PierGuard = "Pier Guard";
            public const string Stingray = "Stingray";
            public const string Octopus = "Octopus";
            public const string Shark = "Shark";
        }

        public static class Cta
        {
            public const string Title = "RAID & RUSH";
            public const string Subtitle = "Choose stronger targets. Win the island.";
            public const string Button = "PLAY NOW";
            public const string Url = "https://example.com/play";
        }

        public static class Gameplay
        {
            public const int HeroStartStrength = 2;
            public const int ChestStrength = 3;
            public const int ChestReward = 3;
            public const int RightEnemyStrength = 3;
            public const int RightEnemyReward = 3;
            public const int LeftEnemyStrength = 4;
            public const int LeftEnemyReward = 5;
            public const int StingrayStrength = 13;
            public const int StingrayReward = 13;
            public const int OctopusStrength = 21;
            public const int OctopusReward = 21;
            public const int SharkStrength = 37;
            public const int SharkReward = 37;
        }

        public static class Motion
        {
            public const float MoveDuration = 0.68f;
            public const float AttackDuration = 0.4f;
            public const float HintPulseDuration = 0.62f;
            public const float SelectionPulseDuration = 0.34f;
            public const float PathDotDelay = 0.045f;
            public const float InvalidShakeDuration = 0.28f;
            public const float CtaDelay = 0.45f;
            public const float ChestRewardSpawnDelay = 0.75f;
            public const float ChestRewardReleaseNormalizedTime = 0.8f;
            public const float ChestRewardFlightDuration = 0.78f;
            public const float ChestRewardArcHeight = 1.15f;
            public const int PathDotCount = 5;
            public const int PathPreviewDelayMs = 260;
        }

        public static class Sway
        {
            public const float CycleDuration = 2.25f;
            public const float PositionAmplitudeX = 0.025f;
            public const float PositionAmplitudeY = 0.04f;
            public const float RotationAmplitude = 1.8f;
        }

        public static class Effects
        {
            public const float BurstDuration = 0.42f;
        }

        public static class Animation
        {
            public const string MovingBool = "Moving";
            public const string AttackTrigger = "Attack";
            public const string SecondAttackTrigger = "SecondAttack";
            public const string HitTrigger = "Hit";
            public const string InvalidTrigger = "Invalid";
            public const string PowerTrigger = "Power";
            public const string ShopUpdateTrigger = "ShopUpdate";
            public const string DeathTrigger = "Death";
            public const string OpenTrigger = "Open";
            public const string OpenState = "Open";
            public const int DefaultLayer = 0;
            public const float CompleteNormalizedTime = 1f;
            public const float AnimatorWaitTimeout = 3f;

            public static readonly int MovingBoolHash = Animator.StringToHash(MovingBool);
            public static readonly int AttackTriggerHash = Animator.StringToHash(AttackTrigger);
            public static readonly int SecondAttackTriggerHash = Animator.StringToHash(SecondAttackTrigger);
            public static readonly int HitTriggerHash = Animator.StringToHash(HitTrigger);
            public static readonly int InvalidTriggerHash = Animator.StringToHash(InvalidTrigger);
            public static readonly int PowerTriggerHash = Animator.StringToHash(PowerTrigger);
            public static readonly int ShopUpdateTriggerHash = Animator.StringToHash(ShopUpdateTrigger);
            public static readonly int DeathTriggerHash = Animator.StringToHash(DeathTrigger);
            public static readonly int OpenTriggerHash = Animator.StringToHash(OpenTrigger);
            public static readonly int OpenStateHash = Animator.StringToHash(OpenState);
        }
    }
}
