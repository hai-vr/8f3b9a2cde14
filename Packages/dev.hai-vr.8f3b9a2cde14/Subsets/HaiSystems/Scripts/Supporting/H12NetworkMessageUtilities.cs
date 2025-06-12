using System;

namespace Hai.Project12.HaiSystems.Supporting
{
    public static class H12NetworkMessageUtilities
    {
        public static ArraySegment<byte> SubBuffer(byte[] unsafeBuffer)
        {
            return new ArraySegment<byte>(unsafeBuffer, 1, unsafeBuffer.Length - 1);
        }

        //

        /// Decoding issues that are not supposed to happen ever, such as sending a message to self.
        public static void StateError(string message)
        {
            H12Debug.LogError($"State error: {message}", H12Debug.LogTag.VixxyNetworking);
        }

        /// Decoding issues that may be triggered by tampering messages, or possibly by using incompatible client or module versions.
        /// Either way, they are not supposed to happen when all clients are using the same version of the protocol.
        public static void ProtocolError(string message)
        {
            H12Debug.LogError($"Protocol error: {message}", H12Debug.LogTag.VixxyNetworking);
        }

        /// Decoding issues that has a chance to be triggered accidentally, for instance when packets are received
        /// in the incorrect order, before an initialization step is complete.
        public static void ProtocolAccident(string message)
        {
            H12Debug.LogError($"Protocol accident: {message}", H12Debug.LogTag.VixxyNetworking);
        }

        //

        public static void ProtocolError_IncorrectFixedBufferLength(byte[] unsafeBuffer, int expected)
        {
            ProtocolError($"Buffer has incorrect length (expected {expected}, was {unsafeBuffer.Length}.");
        }
    }
}
