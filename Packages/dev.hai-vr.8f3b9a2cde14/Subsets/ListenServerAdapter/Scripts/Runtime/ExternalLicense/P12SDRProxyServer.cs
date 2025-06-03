using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using Basis.Network.Core;
using LiteNetLib;
using LiteNetLib.Utils;
using Steamworks;
using Steamworks.Data;
using UnityEngine;

namespace Hai.Project12.ListenServer.Runtime.ExternalLicense
{
    /// SDR Relay socket acting as a proxy server.
    public class P12SDRProxyServer : MonoBehaviour, IDisposable
    {
        public const uint RVRAppId = 2_212_290;

        private SocketManager _socket;

        private void OnEnable()
        {
            var appId = RVRAppId;
            if (!SteamClient.IsValid) SteamClient.Init(appId);
            BasisDebug.Log($"Steam: Initialized SteamClient with appID {appId}.", BasisDebug.LogTag.Networking);

            _socket = SteamNetworkingSockets.CreateRelaySocket<SocketManager>();
            _socket.Interface = new P12SDRProxyServerManager(P12ListenServer.Port);
            BasisDebug.Log("Steam: Created a relay socket.", BasisDebug.LogTag.Networking);
        }

        private void OnDisable()
        {
            if (_socket != null)
            {
                BasisDebug.Log("Steam: Closing relay socket.", BasisDebug.LogTag.Networking);
                _socket.Close();
                BasisDebug.Log("Steam: Closed relay socket.", BasisDebug.LogTag.Networking);
                _socket = null;
            }
        }

        public void Dispose()
        {
            SteamClient.Shutdown();
        }
    }

    internal class P12SDRProxyServerManager : ISocketManager
    {
        private const int MaximumMessageLength = 1024 * 1024;
        private static byte[] _maximumMessageBuffer = new byte[MaximumMessageLength];
        private static int _port;

        private Dictionary<Connection, NetManager> _connectionToNetmgr = new Dictionary<Connection, NetManager>();
        private Dictionary<Connection, NetPeer> _connectionToPeer = new Dictionary<Connection, NetPeer>();

        internal P12SDRProxyServerManager(int port)
        {
            _port = port;
        }

        public void OnConnecting(Connection connection, ConnectionInfo info)
        {
        }

        public void OnConnected(Connection connection, ConnectionInfo info)
        {
            var client = new NetManager(new OurNetListener(this, connection))
            {
                AutoRecycle = false,
                UnconnectedMessagesEnabled = false,
                NatPunchEnabled = true,
                AllowPeerAddressChange = true,
                BroadcastReceiveEnabled = false,
                UseNativeSockets = false,
                ChannelsCount = BasisNetworkCommons.TotalChannels,
                EnableStatistics = true,
                UpdateTime = BasisNetworkCommons.NetworkIntervalPoll,
                PingInterval = 1500,
                UnsyncedEvents = true,
            };
            client.Start();
            NetDataWriter Writer = new NetDataWriter(true,12);
            //this is the only time we dont put key!
            /*
            Writer.Put(BasisNetworkVersion.ServerVersion);
            Basis.Network.Core.Serializable.SerializableBasis.BytesMessage AuthBytes = new Basis.Network.Core.Serializable.SerializableBasis.BytesMessage();
            AuthBytes.Serialize(Writer, AuthenticationMessage);
            SerializableBasis.ReadyMessage.Serialize(Writer);
            */
            NetPeer peer = client.Connect(IPAddress.Loopback.ToString(), _port, Writer);

            _connectionToNetmgr[connection] = client;
            _connectionToPeer[connection] = peer;
        }

        public void OnDisconnected(Connection connection, ConnectionInfo info)
        {
            _connectionToPeer[connection].Disconnect();
            _connectionToPeer.Remove(connection);
            _connectionToNetmgr.Remove(connection);
        }

        public void OnMessage(Connection connection, NetIdentity identity, IntPtr data, int size, long messageNum, long recvTime, int channel)
        {
            // FIXME: Yeah uh, there's no chance this is gonna work.

            // Incoming SDR --> Listen server
            if (channel > 255)
            {
                BasisDebug.LogError($"Networking error, channel too big {channel}. Closing connection.");
                _connectionToPeer[connection].Disconnect();
                return;
            }
            if (size > MaximumMessageLength)
            {
                BasisDebug.LogError("Networking error, message length received through SDR is longer than what we can handle. Closing connection.");
                _connectionToPeer[connection].Disconnect();
                return;
            }

            Marshal.Copy(data, _maximumMessageBuffer, 0, size);
            _connectionToPeer[connection].Send(_maximumMessageBuffer, 0, size, (byte)channel, DeliveryMethod.ReliableOrdered);
        }
    }

    internal class OurNetListener : INetEventListener
    {
        private readonly P12SDRProxyServerManager _proxyServerManager;
        private readonly Connection _sdrConnection;

        public OurNetListener(P12SDRProxyServerManager proxyServerManager, Connection sdrConnection)
        {
            _proxyServerManager = proxyServerManager;
            _sdrConnection = sdrConnection;
        }

        public void OnPeerConnected(NetPeer peer)
        {
        }

        public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
        }

        public void OnNetworkError(IPEndPoint endPoint, SocketError socketError)
        {
        }

        public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod)
        {
            // Outbound to SDR <-- Listen server
            // _sdrConnection.SendMessage(reader.re)
            var isUnreliable = deliveryMethod == DeliveryMethod.Unreliable;
            // _sdrConnection.SendMessage(, isUnreliable ? SendType.Unreliable : SendType.Reliable);
        }

        public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
        {
        }

        public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
        {
        }

        public void OnConnectionRequest(ConnectionRequest request)
        {
        }
    }
}
