namespace EpinelPS.LobbyServer.Event
{
    public static class EventConstants 
    {
		//bc enum needs casting in each entry in list events and get joined event
		//and casting for each single entry would be more tedious that placing a variable here
		//if there is a way to plainly write event type without casting to int or this tower of variables please do so 
        public const int None = 0;
        public const int DailyMissionEvent = 1;
        public const int LoginEvent = 2;
        public const int ViewShortCut = 3;
        public const int CooperationEvent = 4;
        public const int StoryEvent = 5;
        public const int PickupGachaEvent = 6;
        public const int PollEvent = 7;
        public const int ComeBackUserEvent = 8;
        public const int EventPass = 9;
        public const int FieldHubEvent = 10;
        public const int ShopEvent = 11;
        public const int MissionEvent = 12;
        public const int ChargeGachaEvent = 13;
        public const int MiniGameSortOut = 14;
        public const int CharacterSkillResetEvent = 15;
        public const int EventQuest = 16;
        public const int RewardUpEvent = 17;
        public const int SDBattleEvent = 18;
        public const int TextAdventure = 19;
        public const int ChallengeModeEvent = 20;
        public const int DailyFreeGachaEvent = 21;
        public const int BoxGachaEvent = 22;
        public const int DiceEvent = 23;
        public const int BBQTycoon = 24;
        public const int CE002MiniGame = 25;
        public const int TriggerMissionEventReward = 26;
        public const int ArenaRookieGroupShuffle = 27;
        public const int ArenaSpecialGroupShuffle = 28;
        public const int NKSMiniGame = 29;
        public const int DatingSimulator = 30;
        public const int DessertRush = 31;
        public const int CE003MiniGame = 32;
        public const int TowerDefense = 33;
        public const int EventPlaySoda = 34;
        public const int IslandAdventure = 35;
        public const int MiniGameDD = 36;
        public const int CE004MiniGame = 37;
        public const int MVGMiniGame = 38;
		public const int DragonDungeonRunMiniGame = 39;
        public const int NewPlayerLottery = 40;
        public const int PirateCafe = 41;
        public const int CEEvaMiniGame = 42;
        public const int BubbleMarchMiniGame = 43;
        public const int CE006BossChallengeMiniGame = 44;
        public const int SupportCharacterEvent = 45;
        public const int GachaBoard = 46;
        public const int FreeRewardPass = 47;
        public const int GachaPayback = 48;
        public const int FieldCollectEvent = 49;
        public const int MiniGameBTG = 50;
    }
}
