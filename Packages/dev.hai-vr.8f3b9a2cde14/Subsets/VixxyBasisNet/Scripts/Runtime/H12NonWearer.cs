using LiteNetLib;

namespace Hai.Project12.VixxyBasisNet.Runtime
{
    internal class H12NonWearer : I12Net
    {
        private readonly P12VixxyBasisNetworking _vixxyNet;
        private readonly byte[] _premade_RequestState_NW_to_W = { I12Net.RequestState_NW_to_W };

        public H12NonWearer(P12VixxyBasisNetworking vixxyNet)
        {
            _vixxyNet = vixxyNet;
        }

        public void OnNetworkInitialized()
        {
            _vixxyNet.SubmitReliable(_premade_RequestState_NW_to_W);
        }

        public void OnNetworkMessageReceived(ushort RemoteUser, byte[] buffer, DeliveryMethod DeliveryMethod)
        {
            // TODO: Handle packets:
            // - From Wearer: SubmitFullSnapshot => ()
            // - (Packets from self must be considered as programming errors)
            // - (Packets from other non-Wearers must be considered as network tampering with non-standard components)
            if (_vixxyNet.CheckThat_IsWearer(RemoteUser))
            {
                ProceedWithDecoding(buffer);
            }
            else if (_vixxyNet.CheckThat_IsSelf(RemoteUser))
            {
                _vixxyNet.ProtocolError("Protocol error: Cannot receive a message from self.");
            }
            else
            {
                _vixxyNet.ProtocolError("Protocol error: Non-wearers cannot receive this message from other non-wearers.");
            }
        }

        public void OnNetworkMessageServerReductionSystem(byte[] buffer)
        {
            ProceedWithDecoding(buffer);
        }

        private void ProceedWithDecoding(byte[] buffer)
        {
            var decodedPacket = _vixxyNet.NonWearer_DecodePacket();
            // TODO: Do stuff with that packet
        }
    }
}
