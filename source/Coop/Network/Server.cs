﻿using System;
using System.Collections.Generic;
using System.Threading;
using Coop.Common;
using Coop.Multiplayer;
using Stateless;

namespace Coop.Network
{
    public class Server
    {
        public enum EState
        {
            Inactive,
            Starting,
            Running,
            Stopping
        }

        public readonly UpdateableList Updateables;
        public ServerConfiguration ActiveConfig;
        public EState State => m_State.State;
        public event Action<ConnectionServer> OnClientConnected;

        public void Start(ServerConfiguration config)
        {
            if (m_State.IsInState(EState.Inactive))
            {
                m_State.Fire(
                    new StateMachine<EState, ETrigger>.TriggerWithParameters<ServerConfiguration>(
                        ETrigger.Start),
                    config);
            }
        }

        public void Stop()
        {
            if (State != EState.Inactive)
            {
                m_State.Fire(ETrigger.Stop);
            }
        }

        public void SendToAll(Packet packet)
        {
            foreach (ConnectionServer conn in m_ActiveConnections)
            {
                conn.Send(packet);
            }
        }

        public override string ToString()
        {
            string sDump = string.Join(
                Environment.NewLine,
                $"Server is '{State.ToString()}' with '{m_ActiveConnections.Count}/{ActiveConfig.uiMaxPlayerCount}' players.",
                $"LAN:   {ActiveConfig.lanAddress}:{ActiveConfig.lanPort}",
                $"WAN:   {ActiveConfig.wanAddress}:{ActiveConfig.wanPort}");

            if (m_ActiveConnections.Count > 0)
            {
                sDump += Environment.NewLine + "Connections to clients:";
                sDump += Environment.NewLine + "Latency   " + "Connection State";
                foreach (ConnectionServer conn in m_ActiveConnections)
                {
                    sDump += Environment.NewLine + $"{conn.Latency,-10}" + $"{conn.State}";
                }
            }

            return sDump;
        }

        public virtual void Connected(ConnectionServer con)
        {
            m_ActiveConnections.Add(con);
            OnClientConnected?.Invoke(con);
            Log.Info($"Client connection established: {con}.");
        }

        public virtual void Disconnected(ConnectionServer con, EDisconnectReason eReason)
        {
            Log.Info($"Client connection closed: {con}. {eReason}.");
            con.Disconnect(eReason);
            if (!m_ActiveConnections.Remove(con))
            {
                Log.Error($"Unknown connection: {con}.");
            }
        }

        public virtual bool CanPlayerJoin()
        {
            return State == EState.Running &&
                   m_ActiveConnections.Count < ActiveConfig.uiMaxPlayerCount;
        }

        #region internals
        private enum ETrigger
        {
            Start,
            Initialized,
            Stop,
            Stopped
        }

        public Server()
        {
            Updateables = new UpdateableList();
            m_ActiveConnections = new List<ConnectionServer>();
            m_State = new StateMachine<EState, ETrigger>(EState.Inactive);

            m_State.Configure(EState.Inactive).Permit(ETrigger.Start, EState.Starting);

            StateMachine<EState, ETrigger>.TriggerWithParameters<ServerConfiguration> startTrigger =
                m_State.SetTriggerParameters<ServerConfiguration>(ETrigger.Start);
            m_State.Configure(EState.Starting)
                   .OnEntryFrom(startTrigger, config => load(config))
                   .Permit(ETrigger.Initialized, EState.Running)
                   .Permit(ETrigger.Stop, EState.Stopping);

            m_State.Configure(EState.Running)
                   .OnEntryFrom(ETrigger.Initialized, () => startMainLoop())
                   .OnExit(() => stopMainLoop())
                   .Permit(ETrigger.Stop, EState.Stopping);

            m_State.Configure(EState.Stopping)
                   .OnEntry(() => shutDown())
                   .Permit(ETrigger.Stopped, EState.Inactive);
        }

        ~Server()
        {
            Stop();
        }

        private readonly List<ConnectionServer> m_ActiveConnections;

        private void load(ServerConfiguration config)
        {
            ActiveConfig = config;
            m_State.Fire(ETrigger.Initialized);
        }

        private void shutDown()
        {
            ActiveConfig = null;
            foreach (ConnectionServer conn in m_ActiveConnections)
            {
                conn.Disconnect(EDisconnectReason.ServerShutDown);
            }

            m_ActiveConnections.Clear();
            m_State.Fire(ETrigger.Stopped);
        }

        private readonly StateMachine<EState, ETrigger> m_State;
        private bool m_bStopRequest;
        private readonly object m_StopRequestLock = new object();
        private Thread m_Thread;

        private void startMainLoop()
        {
            m_Thread = new Thread(() => run());
            lock (m_StopRequestLock)
            {
                m_bStopRequest = false;
            }

            m_Thread.Start();
        }

        private void run()
        {
            FrameLimiter frameLimiter = new FrameLimiter(
                ActiveConfig.uiTickRate > 0 ?
                    TimeSpan.FromMilliseconds(1000 / (double) ActiveConfig.uiTickRate) :
                    TimeSpan.Zero);
            bool bRunning = true;
            while (bRunning)
            {
                if (bRunning)
                {
                    Updateables.UpdateAll(frameLimiter.LastFrameTime);
                }

                frameLimiter.Throttle();
                lock (m_StopRequestLock)
                {
                    bRunning = !m_bStopRequest;
                }
            }
        }

        private void stopMainLoop()
        {
            if (m_Thread == null)
            {
                throw new InvalidStateException("Cannot stop: main loop is not running.");
            }

            lock (m_StopRequestLock)
            {
                m_bStopRequest = true;
            }

            m_Thread.Join();
            m_Thread = null;
        }
        #endregion
    }
}
