﻿﻿#nullable enable
namespace SuperGamblino.Core.GamesObjects
{
    public class BlackjackHelper
    {
        public bool IsGameDone { get; set; }
        public string? UserHand { get; set; }
        public string? DealerHand { get; set; }
        public int? Bet { get; set; }
    }
}