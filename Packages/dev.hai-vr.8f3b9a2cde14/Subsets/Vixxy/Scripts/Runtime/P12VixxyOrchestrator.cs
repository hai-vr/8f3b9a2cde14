using System.Collections.Generic;
using System.Linq;
using Hai.Project12.HaiSystems.Supporting;
using HVR.Basis.Comms;
using UnityEngine;

namespace Hai.Project12.Vixxy.Runtime
{
    /// There is one instance of this **per avatar** or **per world object**.
    [DefaultExecutionOrder(-10)] // FIXME: acquisitionService can be null if the dependents become awake before this
    public class P12VixxyOrchestrator : MonoBehaviour
    {
        // TODO:
        // - Collect arriving data.
        // - When data arrives, we mark the aggregators and the actuators of that data.
        // - When all data arrived, and we're starting the update cycle, we wake up all aggregators of that data.

        [SerializeField] private AcquisitionService acquisitionService;
        [SerializeField] private Transform context; // Can be null. If it is null, the orchestrator *is* the context.

        private readonly HashSet<I12VixxyAggregator> _aggregatorsToUpdateThisTick = new();
        private readonly HashSet<I12VixxyActuator> _actuatorsToUpdateThisTick = new();
        private bool _anythingNeedsUpdating;

        private readonly Dictionary<int, HashSet<I12VixxyAggregator>> _iddressToAggregators = new();
        private readonly Dictionary<int, HashSet<I12VixxyActuator>> _iddressToActuators = new();
        private readonly Dictionary<GameObject, MaterialPropertyBlock> _objectToMaterialPropertyBlock = new();
        private readonly Dictionary<GameObject, Renderer> _objectToRenderer_mayContainNullObjects = new();
        private readonly HashSet<GameObject> _stagedBlocks = new(); // FIXME: We should really just be binding tuples into _objectToMaterialPropertyBlock

        private readonly HashSet<I12VixxyAggregator> _workAggregators = new();

        private void Awake()
        {
            if (!acquisitionService) acquisitionService = AcquisitionService.SceneInstance;
        }

        public Transform Context()
        {
            return context != null ? context : transform;
        }

        public void PassAddressUpdated(string address)
        {
            PassAddressUpdated(H12VixxyAddress.AddressToId(address));
        }

        public void PassAddressUpdated(int iddress)
        {
            // TODO: Store received addresses and value

            // This cannot be cached outside of this lambda (unless we're smart about it),
            // as new aggregators and actuators may be added.
            // Might need to add a baking phase so that we don't do a string lookup every time
            // (consider switching to an int lookup).

            var aggregators = AggregatorsOf(iddress);
            var actuators = ActuatorsOf(iddress);

            // In AcquisitionService, acquisition events are raised as soon as the data arrives.
            // We don't want to process that new data when it arrives, instead we want to process
            // only after all data has arrived for that frame, all at once.

            // FIXME: AcquisitionService "OnAddressUpdated" fires when ANY data is received on that line.
            // The value may have not changed. We need to track it so that we don't send unnecessarily update actuators,
            // like that of face tracking.
            // OR, modify AcquisitionService to have OnAddressValueChanged.
            _aggregatorsToUpdateThisTick.UnionWith(aggregators);
            _actuatorsToUpdateThisTick.UnionWith(actuators);
            _anythingNeedsUpdating = true;
        }

        private IEnumerable<I12VixxyAggregator> AggregatorsOf(int iddress)
        {
            if (_iddressToAggregators.TryGetValue(iddress, out var results)) return results;
            return Enumerable.Empty<I12VixxyAggregator>();
        }

        private IEnumerable<I12VixxyActuator> ActuatorsOf(int iddress)
        {
            if (_iddressToActuators.TryGetValue(iddress, out var results)) return results;
            return Enumerable.Empty<I12VixxyActuator>();
        }

        public void ProvideValue(int iddress, float result)
        {
            // FIXME: This bleeds the value type to the orchestrator. It would be nice to avoid that.
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

            if (_stagedBlocks.Count > 0)
            {
                foreach (var stagedBlock in _stagedBlocks)
                {
                    // No ContainsKey checks: The objects should always exist in the dictionaries. If they don't, it's a design error.
                    var stagedRenderer = _objectToRenderer_mayContainNullObjects[stagedBlock];
                    if (stagedRenderer != null)
                    {
                        stagedRenderer.SetPropertyBlock(_objectToMaterialPropertyBlock[stagedBlock]);
                    }
                }
                _stagedBlocks.Clear();
            }
        }

        public H12ActuatorRegistrationToken RegisterActuator(string address, I12VixxyActuator actuator, AcquisitionService.AddressUpdated addressUpdatedFn)
        {
            return RegisterActuator(H12VixxyAddress.AddressToId(address), actuator, addressUpdatedFn);
        }

        public H12ActuatorRegistrationToken RegisterActuator(int iddress, I12VixxyActuator actuator, AcquisitionService.AddressUpdated addressUpdatedFn)
        {
            if (_iddressToActuators.TryGetValue(iddress, out var existingActuators))
            {
                existingActuators.Add(actuator);
            }
            else
            {
                var newActuators = new HashSet<I12VixxyActuator> { actuator };
                _iddressToActuators.Add(iddress, newActuators);
            }

            // When an actuator is added, it is scheduled to be updated for initialization purposes.
            _anythingNeedsUpdating = true;
            _actuatorsToUpdateThisTick.Add(actuator);

            var address = H12VixxyAddress.ResolveKnownAddressFromId(iddress);
            acquisitionService.RegisterAddresses(new [] { address }, addressUpdatedFn);

            return new H12ActuatorRegistrationToken
            {
                registeredAddress = address,
                registeredIddress = iddress,
                registeredCallback = addressUpdatedFn,
                registeredActuator = actuator
            };
        }

        public void UnregisterActuator(H12ActuatorRegistrationToken actuatorRegistrationToken)
        {
            if (_iddressToActuators.TryGetValue(actuatorRegistrationToken.registeredIddress, out var existingActuator))
            {
                existingActuator.Remove(actuatorRegistrationToken.registeredActuator);
            }
            acquisitionService.UnregisterAddresses(new []{ actuatorRegistrationToken.registeredAddress }, actuatorRegistrationToken.registeredCallback);
        }

        public void RegisterAggregator(string address, I12VixxyAggregator actuator)
        {
            RegisterAggregator(H12VixxyAddress.AddressToId(address), actuator);
        }

        public void RegisterAggregator(int iddress, I12VixxyAggregator actuator)
        {
            if (_iddressToAggregators.TryGetValue(iddress, out var existingAggregators))
            {
                existingAggregators.Add(actuator);
            }
            else
            {
                var newAggregators = new HashSet<I12VixxyAggregator> { actuator };
                _iddressToAggregators.Add(iddress, newAggregators);
            }

            // When an aggregator is added, it is scheduled to be updated for initialization purposes.
            _anythingNeedsUpdating = true;
            _aggregatorsToUpdateThisTick.Add(actuator);
        }

        public void UnregisterAggregator(string address, I12VixxyAggregator aggregator)
        {
            UnregisterAggregator(H12VixxyAddress.AddressToId(address), aggregator);
        }

        public void UnregisterAggregator(int iddress, I12VixxyAggregator aggregator)
        {
            if (_iddressToAggregators.TryGetValue(iddress, out var existingActuator)) existingActuator.Remove(aggregator);
        }

        public void RequireMaterialPropertyBlock(GameObject bakedObject)
        {
            if (!_objectToMaterialPropertyBlock.ContainsKey(bakedObject))
            {
                _objectToMaterialPropertyBlock.Add(bakedObject, new MaterialPropertyBlock());
                _objectToRenderer_mayContainNullObjects.Add(bakedObject, bakedObject.GetComponent<Renderer>());
            }
        }

        public MaterialPropertyBlock GetMaterialPropertyBlockForBakedObject(GameObject bakedObject)
        {
            // If the key doesn't exist, it is a design error. Callers should only call GetMaterialPropertyBlockFor
            // if that subject is guaranteed to have a MaterialPropertyBlock declared, as it is required by Awake.
            // (Live edits not currently supported)
            if (!_objectToMaterialPropertyBlock.ContainsKey(bakedObject))
            {
                // DEFENSIVE for live edits only. This condition should not be entered by design.
                H12Debug.LogWarning("A MaterialPropertyBlock object was not found. This is either a design error, or the user is currently doing a live edit," +
                                    " and MaterialPropertyBlock are not normally cached if the control did not previously make use of materials.");
                _objectToMaterialPropertyBlock.Add(bakedObject, new MaterialPropertyBlock());
                _objectToRenderer_mayContainNullObjects.Add(bakedObject, bakedObject.GetComponent<Renderer>());
            }

            return _objectToMaterialPropertyBlock[bakedObject];
        }

        public void StagePropertyBlock(GameObject bakedObject)
        {
            _stagedBlocks.Add(bakedObject);
        }
    }
}
