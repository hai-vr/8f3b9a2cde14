namespace Hai.Project12.Vixxy.Runtime
{
    public interface I12VixxyNetworkable
    {
        void RequireNetworked(string address, float defaultValue, P12VixxyNetDataUsage netDataUsage);
    }

    public enum P12VixxyNetDataUsage
    {
        Bit,
        Analog8Bits
    }
}
