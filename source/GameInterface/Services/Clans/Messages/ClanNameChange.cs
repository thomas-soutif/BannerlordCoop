﻿using Common.Messaging;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;
using TaleWorlds.CampaignSystem;

namespace GameInterface.Services.Clans.Messages
{
    public record ClanNameChange : IEvent
    {
        public Clan Clan { get; }
        public string Name { get; }
        public string InformalName { get; }

        public ClanNameChange(Clan clan, string name, string informalName)
        {
            Clan = clan;
            Name = name;
            InformalName = informalName;
        }
    }
}
