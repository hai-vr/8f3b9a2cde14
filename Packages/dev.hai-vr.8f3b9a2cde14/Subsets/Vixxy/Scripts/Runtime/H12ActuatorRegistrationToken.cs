using HVR.Basis.Comms;

namespace Hai.Project12.Vixxy.Runtime
{
    public struct H12ActuatorRegistrationToken
    {
        public string registeredAddress;
        public AcquisitionService.AddressUpdated registeredCallback;
        public I12VixxyActuator registeredActuator;
    }
}
