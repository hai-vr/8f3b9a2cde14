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
            _relayLateInit = IsLocallyOwned ? new H12Wearer(this) : new H12NonWearer(this);
            _relayLateInit.OnNetworkInitialized();
        }

        public override void OnNetworkMessageReceived(ushort RemoteUser, byte[] buffer, DeliveryMethod DeliveryMethod)
        {
            if (_relayLateInit != null) _relayLateInit.OnNetworkMessageReceived(RemoteUser, buffer, DeliveryMethod);
        }

        public override void OnNetworkMessageServerReductionSystem(byte[] buffer)
        {
            if (_relayLateInit != null) _relayLateInit.OnNetworkMessageServerReductionSystem(buffer);
        }

        public void SubmitReliable(byte[] buffer)
        {
            NetworkMessageSend(buffer, MainMessageDeliveryMethod);
        }

        internal object NonWearer_DecodePacket()
        {
            throw new System.NotImplementedException();
        }

        internal object Wearer_DecodePacket(byte[] buffer)
        {
            throw new System.NotImplementedException();
        }

        internal bool CheckThat_IsSelf(ushort remoteUser) => false;
        internal bool CheckThat_IsWearer(ushort remoteUser) => remoteUser == _wearerId;
        internal bool CheckThat_IsNonWearer(ushort remoteUser) => remoteUser != _wearerId;

        internal void ProtocolError(string message)
        {
            H12Debug.LogError(message, H12Debug.LogTag.VixxyNetworking);
        }

        internal void ProtocolWarning(string message)
        {
            H12Debug.LogWarning(message, H12Debug.LogTag.VixxyNetworking);
        }

        internal void ProtocolAssetMismatch(string message)
        {
            H12Debug.LogError(message, H12Debug.LogTag.VixxyNetworking);
        }
    }

    internal interface I12Net
    {
        internal const byte RequestState_NW_to_W = 0x01;
        internal const byte SubmitFullSnapshot_W_to_NW = 0x02;
        internal const byte SubmitIncremental_W_to_NW = 0x03;

        void OnNetworkInitialized();
        void OnNetworkMessageReceived(ushort RemoteUser, byte[] buffer, DeliveryMethod DeliveryMethod);
        void OnNetworkMessageServerReductionSystem(byte[] buffer);
    }
}
