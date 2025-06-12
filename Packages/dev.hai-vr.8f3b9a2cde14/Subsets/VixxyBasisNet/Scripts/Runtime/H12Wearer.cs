using Hai.Project12.HaiSystems.Supporting;
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

        public void OnNetworkMessageReceived(H12AvatarContextualUser RemoteUser, byte[] unsafeBuffer, DeliveryMethod DeliveryMethod)
        {
            if (!RemoteUser.IsWearer)
            {
                ProceedWithDecoding(unsafeBuffer, RemoteUser);
            }
            else
            {
                H12NetworkMessageUtilities.StateError("Wearer can't receive a message from wearer.");
            }
        }

        public void OnNetworkMessageServerReductionSystem(byte[] unsafeBuffer)
        {
            H12NetworkMessageUtilities.StateError("Server reduction system message cannot be received by wearer.");
        }

        private void ProceedWithDecoding(byte[] unsafeBuffer, H12AvatarContextualUser remoteUser)
        {
            if (unsafeBuffer.Length == 0)
            {
                H12NetworkMessageUtilities.ProtocolError("Buffer cannot be empty.");
                return;
            }

            byte packetId = unsafeBuffer[0];
            switch (packetId)
            {
                case I12Net.RequestState_NW_to_W:
                    if (unsafeBuffer.Length != 1)
                    {
                        H12NetworkMessageUtilities.ProtocolError($"Buffer has incorrect length (expected 1, was {unsafeBuffer.Length}.");
                        return;
                    }

                    _vixxyNet.Wearer_SubmitFullSnapshotTo(remoteUser);
                    break;

                default:
                    H12NetworkMessageUtilities.ProtocolError("Unknown packet ID.");
                    break;
            }
        }
    }
}
