﻿using Common.Messaging;
using GameInterface.Services.Heroes.Data;
using LiteNetLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.MountAndBlade.Diamond;

namespace GameInterface.Services.Heroes.Messages;

public record RegisterNewPlayerHero : ICommand
{
    public NetPeer SendingPeer { get; }
    public byte[] Bytes { get; }

    public RegisterNewPlayerHero(NetPeer sendingPeer, byte[] bytes)
    {
        SendingPeer = sendingPeer;
        Bytes = bytes;
    }
}

public record NewPlayerHeroRegistered : IResponse
{
    public NetPeer SendingPeer { get; }
    public NewPlayerData NewPlayerData { get; }

    public NewPlayerHeroRegistered(NetPeer sendingPeer, NewPlayerData playerData)
    {
        SendingPeer = sendingPeer;

        NewPlayerData = playerData;
    }
}