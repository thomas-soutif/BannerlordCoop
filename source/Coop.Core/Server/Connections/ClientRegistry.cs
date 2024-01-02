﻿using Common.Messaging;
using Common.Network;
using Coop.Core.Server.Connections.Messages;
using Coop.Core.Server.Connections.States;
using LiteNetLib;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Coop.Core.Server.Connections;

/// <summary>
/// Manages client connections while one or more connections is receiving the game state
/// through a save transfer
/// </summary>
public interface IClientRegistry : IDisposable
{
    bool PlayersLoading { get; }

    bool PlayersOverloaded { get; }

    List<NetPeer> LoadingPeers { get; }

    List<NetPeer> OverloadedPeers { get; }

    /// <summary>
    /// Marks client's overload state.
    /// </summary>
    /// <param name="peer">the client</param>
    /// <param name="val">overload state</param>
    void MarkOverloaded(NetPeer peer, bool val);

    /// <summary>
    /// Checks whether a client is overloaded.
    /// </summary>
    /// <param name="peer">the client</param>
    bool IsOverloaded(NetPeer peer);
}

/// <inheritdoc cref="IClientRegistry"/>
public class ClientRegistry : IClientRegistry
{
    public IDictionary<NetPeer, IConnectionLogic> ConnectionStates { get; private set; } = new Dictionary<NetPeer, IConnectionLogic>();

    private static HashSet<Type> loadingStates = new HashSet<Type>
    {
        typeof(TransferSaveState),
        typeof(LoadingState),
    };

    public bool PlayersLoading => ConnectionStates.Any(state => loadingStates.Contains(state.Value.State.GetType()));
    public bool PlayersOverloaded => ConnectionStates.Any(state => state.Value.IsOverloaded);

    public List<NetPeer> LoadingPeers => ConnectionStates.Where(state => loadingStates.Contains(state.Value.State.GetType())).Select(state => state.Key).ToList();

    public List<NetPeer> OverloadedPeers => ConnectionStates.Where(state => state.Value.IsOverloaded).Select(state => state.Key).ToList();

    private readonly IMessageBroker messageBroker;
    private readonly INetwork network;
    private readonly IConnectionLogicFactory connectionLogicFactory;

    public ClientRegistry(
        IMessageBroker messageBroker,
        INetwork network,
        IConnectionLogicFactory connectionLogicFactory)
    {
        this.messageBroker = messageBroker;
        this.network = network;
        this.connectionLogicFactory = connectionLogicFactory;
        this.messageBroker.Subscribe<PlayerConnected>(PlayerJoiningHandler);
        this.messageBroker.Subscribe<PlayerDisconnected>(PlayerDisconnectedHandler);
    }

    public void Dispose()
    {
        messageBroker.Unsubscribe<PlayerConnected>(PlayerJoiningHandler);
        messageBroker.Unsubscribe<PlayerDisconnected>(PlayerDisconnectedHandler);
    }

    internal void PlayerJoiningHandler(MessagePayload<PlayerConnected> obj)
    {
        var playerPeer = obj.What.PlayerPeer;
        var connectionLogic = connectionLogicFactory.CreateLogic(playerPeer);
        ConnectionStates.Add(playerPeer, connectionLogic);
    }

    internal void PlayerDisconnectedHandler(MessagePayload<PlayerDisconnected> obj)
    {
        var playerId = obj.What.PlayerId;
        
        if(ConnectionStates.TryGetValue(playerId, out IConnectionLogic logic))
        {
            ConnectionStates.Remove(playerId);
            logic.Dispose();
        }
    }

    public void MarkOverloaded(NetPeer peer, bool val)
    {
        ConnectionStates[peer].IsOverloaded = val;
    }

    public bool IsOverloaded(NetPeer peer)
    {
        return ConnectionStates[peer].IsOverloaded;
    }
}
