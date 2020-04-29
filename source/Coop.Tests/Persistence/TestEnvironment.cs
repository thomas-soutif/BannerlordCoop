﻿using System.Collections.Generic;
using Coop.Game.Persistence;
using TaleWorlds.CampaignSystem;

namespace Coop.Tests
{
    internal class TestEnvironment : IEnvironment
    {
        public List<CampaignTimeControlMode> Values = new List<CampaignTimeControlMode>() { CampaignTimeControlMode.Stop };
        public CampaignTimeControlMode TimeControlMode
        {
            get => Values[^1];
            set => Values.Add(value);
        }
    }
}