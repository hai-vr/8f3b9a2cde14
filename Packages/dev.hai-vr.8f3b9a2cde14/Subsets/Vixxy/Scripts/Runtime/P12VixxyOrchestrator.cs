using System.Collections.Generic;
using System.Linq;
using Hai.Project12.HaiSystems.Supporting;
using HVR.Basis.Comms;
using UnityEngine;

namespace Hai.Project12.Vixxy.Runtime
{
    /// There is one instance of this **per avatar** or **per world object**.
    public class P12VixxyOrchestrator : MonoBehaviour
    {
        // TODO:
        // - Collect arriving data.
        // - When data arrives, we mark the aggregators and the actuators of that data.
        // - When all data arrived, and we're starting the update cycle, we wake up all aggregators of that data.

        [LateInjectable] [SerializeField] private AcquisitionService acquisitionService;
        private readonly HashSet<I12VixxyAggregator> _aggregatorsToUpdateThisTick = new();
        private readonly HashSet<I12VixxyActuator> _actuatorsToUpdateThisTick = new();
        private bool _anythingNeedsUpdating;

        // TODO: Don't do string lookup tables!
        private readonly Dictionary<string, HashSet<I12VixxyAggregator>> _addressToAggregators = new();
        private readonly Dictionary<string, HashSet<I12VixxyActuator>> _addressToActuators = new();

        private readonly HashSet<I12VixxyAggregator> _workAggregators = new();
        // private readonly List<(string, AcquisitionService.AddressUpdated)> _acquisitionServiceRegistrationTracker = new List<(string, AcquisitionService.AddressUpdated)>();

        private void Awake()
        {
            if (!acquisitionService) acquisitionService = AcquisitionService.SceneInstance;

            // In AcquisitionService, acquisition events are raised as soon as the data arrives.
            // We don't want to process that new data when it arrives, instead we want to process
            // only after all data has arrived for that frame, all at once.

            // TODO: Register addresses to listen to based on the aggregators and actuators we have registered.
            // TEMP__RegisterAddressesToAcquisition(P12VixxySubmitSettableToAcquisition.TestAddress);
        }

        public void PassAddressUpdated(string address)
        {
            // TODO: Store received addresses and value

            // This cannot be cached outside of this lambda (unless we're smart about it),
            // as new aggregators and actuators may be added.
            // Might need to add a baking phase so that we don't do the lookup every time.

            var aggregators = AggregatorsOf(address);
            var actuators = ActuatorsOf(address);

            _aggregatorsToUpdateThisTick.UnionWith(aggregators);
            _actuatorsToUpdateThisTick.UnionWith(actuators);
            _anythingNeedsUpdating = true;
        }

        private IEnumerable<I12VixxyAggregator> AggregatorsOf(string address)
        {
            if (_addressToAggregators.TryGetValue(address, out var results)) return results;
            return Enumerable.Empty<I12VixxyAggregator>();
        }

        private IEnumerable<I12VixxyActuator> ActuatorsOf(string address)
        {
            if (_addressToActuators.TryGetValue(address, out var results)) return results;
            return Enumerable.Empty<I12VixxyActuator>();
        }

        // TODO: This update loop must only run after the services that submit to AcquisitionService have run.
        // Execution order may need tweaking.
        private void Update()
        {
            if (!_anythingNeedsUpdating) return;

            // Randomness in the number of iteration cycles is an attempt to ensure we don't get implementation-specific
            // behaviour that expects a specific number of cycles to happen.
            var randomIterations = UnityEngine.Random.Range(5, 10);
            while (randomIterations > 0 && _aggregatorsToUpdateThisTick.Count > 0)
            {
                randomIterations--;
                // Starting a new cycle.
                _workAggregators.Clear();
                _workAggregators.UnionWith(_aggregatorsToUpdateThisTick);
                _aggregatorsToUpdateThisTick.Clear();

                foreach (var aggregator in _workAggregators)
                {
                    if (aggregator.TryAggregate(out var newAggregators, out var newActuators))
                    {
                        _aggregatorsToUpdateThisTick.UnionWith(newAggregators);
                        _actuatorsToUpdateThisTick.UnionWith(newActuators);
                    }
                }
            }

            // Deck remaining aggregations for next frame. We already gave it a bunch of chances.
            _anythingNeedsUpdating = _aggregatorsToUpdateThisTick.Count > 0;

            // TODO: It may be possible to do a reverse graph traversal, where we deny listening to addresses
            // or processing aggregators if there are no actuators that listen to that data in the first place.
            if (_actuatorsToUpdateThisTick.Count > 0)
            {
                foreach (var actuator in _actuatorsToUpdateThisTick)
                {
                    actuator.Actuate();
                }

                _actuatorsToUpdateThisTick.Clear();
            }
        }

        public void RegisterActuator(string address, I12VixxyActuator actuator)
        {
            if (_addressToActuators.TryGetValue(address, out var existingActuators))
            {
                existingActuators.Add(actuator);
            }
            else
            {
                var newActuators = new HashSet<I12VixxyActuator> { actuator };
                _addressToActuators.Add(address, newActuators);
            }

            // When an actuator is added, it is scheduled to be updated for initialization purposes.
            _anythingNeedsUpdating = true;
            _actuatorsToUpdateThisTick.Add(actuator);
        }

        public void UnregisterActuator(string address, I12VixxyActuator actuator)
        {
            if (_addressToActuators.TryGetValue(address, out var existingActuator)) existingActuator.Remove(actuator);
        }

        public void RegisterAggregator(string address, I12VixxyAggregator actuator)
        {
            if (_addressToAggregators.TryGetValue(address, out var existingAggregators))
            {
                existingAggregators.Add(actuator);
            }
            else
            {
                var newAggregators = new HashSet<I12VixxyAggregator> { actuator };
                _addressToAggregators.Add(address, newAggregators);
            }

            // When an aggregator is added, it is scheduled to be updated for initialization purposes.
            _anythingNeedsUpdating = true;
            _aggregatorsToUpdateThisTick.Add(actuator);
        }

        public void UnregisterAggregator(string address, I12VixxyAggregator aggregator)
        {
            if (_addressToAggregators.TryGetValue(address, out var existingActuator)) existingActuator.Remove(aggregator);
        }
    }
}
