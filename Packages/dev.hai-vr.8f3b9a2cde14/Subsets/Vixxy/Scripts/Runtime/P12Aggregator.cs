using System.Collections.Generic;
using UnityEngine;

namespace Hai.Project12.Vixxy.Runtime
{
    public class P12Aggregator : MonoBehaviour
    {
        private readonly HashSet<P12Aggregator> _transformerResult = new HashSet<P12Aggregator>();
        private readonly HashSet<P12Actuator> _actuatorResult = new HashSet<P12Actuator>();

        private float _activeResult = float.MinValue;
        private bool _hasNeverBeenTransformed = true;

        /// Transforms the data. If the result is different, or if this was never transformed before, this returns true.
        public bool TryAggregate(out IEnumerable<P12Aggregator> transformers, out IEnumerable<P12Actuator> actuators)
        {
            float result = 0f;

            transformers = _transformerResult;
            actuators = _actuatorResult;

            if (_hasNeverBeenTransformed)
            {
                _activeResult = result;
                return true;
            }

            if (_activeResult != result)
            {
                _activeResult = result;
                return true;
            }

            // Even if an input changes, if the output doesn't change, then it will not result in a change on the actuators.
            return false;
        }
    }
}
