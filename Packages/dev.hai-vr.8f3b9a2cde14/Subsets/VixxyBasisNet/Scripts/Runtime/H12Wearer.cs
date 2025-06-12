using LiteNetLib;

namespace Hai.Project12.VixxyBasisNet.Runtime
{
    internal class H12Wearer : I12Net
    {
        private readonly P12VixxyBasisNetworking _vixxyNet;

        public H12Wearer(P12VixxyBasisNetworking vixxyNet)
        {
            _vixxyNet = vixxyNet;
        }

        public void OnNetworkInitialized()
        {
            _vixxyNet.Wearer_SubmitFullSnapshot_ToAllNonWearers();
        }

        public void OnNetworkMessageReceived(ushort RemoteUser, byte[] buffer, DeliveryMethod DeliveryMethod)
        {
            if (_vixxyNet.CheckThat_IsNonWearer(RemoteUser))
            {
                ProceedWithDecoding(buffer);
            }
            else
            {
                _vixxyNet.ProtocolError("Protocol error: Cannot receive a message from self.");
            }
        }

        public void OnNetworkMessageServerReductionSystem(byte[] buffer)
        {
            _vixxyNet.ProtocolError("Protocol error: Server reduction system cannot be received by wearer.");
        }

        private void ProceedWithDecoding(byte[] buffer)
        {
            var decodedPacket = _vixxyNet.Wearer_DecodePacket(buffer);
            // TODO: Do stuff with that packet
        }
    }
}
