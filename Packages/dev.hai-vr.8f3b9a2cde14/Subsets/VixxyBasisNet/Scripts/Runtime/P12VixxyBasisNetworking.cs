using System;
using Basis.Scripts.BasisSdk;
using Basis.Scripts.Behaviour;
using Hai.Project12.HaiSystems.Supporting;
using Hai.Project12.Vixxy.Runtime;
using LiteNetLib;
using UnityEngine;

namespace Hai.Project12.VixxyBasisNet.Runtime
{
    public class P12VixxyBasisNetworking : BasisAvatarMonoBehaviour
    {
        private const DeliveryMethod MainMessageDeliveryMethod = DeliveryMethod.Sequenced;

        [SerializeField] private P12VixxyOrchestrator orchestrator;
        [SerializeField] private BasisAvatar avatar;

        private ushort _wearerId;
        private bool _isNetworkInitialized;
        private I12Net _relayLateInit;

        private void Awake()
        {
            orchestrator.OnNetworkDataUpdateRequired += OnNetworkDataUpdateRequired;

            if (avatar == null) avatar = GetComponentInParent<BasisAvatar>(true);
            avatar.OnAvatarNetworkReady -= OnAvatarNetworkReady;
            avatar.OnAvatarNetworkReady += OnAvatarNetworkReady;
        }

        private void OnDestroy()
        {
            avatar.OnAvatarNetworkReady -= OnAvatarNetworkReady;
        }

        private void OnNetworkDataUpdateRequired()
        {
            if (!_isNetworkInitialized) return;

        }

        private void OnAvatarNetworkReady(bool IsOwner)
        {
            _wearerId = avatar.LinkedPlayerID;
        }

        internal void Wearer_SubmitFullSnapshot_ToAllNonWearers()
        {
        }

        public override void OnNetworkChange(byte messageIndex, bool IsLocallyOwned)
        {
            if (_relayLateInit != null)
            {
                H12NetworkMessageUtilities.ProtocolAccident("Received OnNetworkChange more than once in this object's lifetime, this is not normal.");
                return;
            }
            _relayLateInit = IsLocallyOwned ? new H12Wearer(this) : new H12NonWearer(this);
            _relayLateInit.OnNetworkInitialized();
        }

        public override void OnNetworkMessageReceived(ushort RemoteUser, byte[] unsafeBuffer, DeliveryMethod DeliveryMethod)
        {
            if (_relayLateInit != null) _relayLateInit.OnNetworkMessageReceived(User(RemoteUser), unsafeBuffer, DeliveryMethod);
            else H12NetworkMessageUtilities.ProtocolAccident("Received OnNetworkMessageReceived before any OnNetworkChange was received.");
        }

        public override void OnNetworkMessageServerReductionSystem(byte[] unsafeBuffer)
        {
            if (_relayLateInit != null) _relayLateInit.OnNetworkMessageServerReductionSystem(unsafeBuffer);
            else H12NetworkMessageUtilities.ProtocolAccident("Received OnNetworkMessageServerReductionSystem before any OnNetworkChange was received.");
        }

        public void SubmitReliable(byte[] buffer)
        {
            NetworkMessageSend(buffer, MainMessageDeliveryMethod);
        }

        private H12AvatarContextualUser User(ushort user)
        {
            return new H12AvatarContextualUser
            {
                User = user,
                IsWearer = user == _wearerId
            };
        }

        public void Wearer_SubmitFullSnapshotTo(H12AvatarContextualUser remoteUser)
        {
            throw new System.NotImplementedException();
        }

        public void NonWearer_ProcessFullSnapshot(object subBuffer)
        {
            throw new NotImplementedException();
        }
    }

    public struct H12AvatarContextualUser
    {
        public ushort User;
        public bool IsWearer;
    }

    internal interface I12Net
    {
        internal const byte RequestState_NW_to_W = 0x01;
        internal const byte SubmitFullSnapshot_W_to_NW = 0x02;
        internal const byte SubmitIncremental_W_to_NW = 0x03;

        void OnNetworkInitialized();
        void OnNetworkMessageReceived(H12AvatarContextualUser RemoteUser, byte[] unsafeBuffer, DeliveryMethod DeliveryMethod);
        void OnNetworkMessageServerReductionSystem(byte[] unsafeBuffer);
    }
}
