using Netick.Unity;
using UnityEngine;
using Object = UnityEngine.Object;

namespace FusionHelpers
{
    /// <summary>
    /// ISparseState represents the networked part of a simple object that require infrequent (network) updates,
    /// but still needs a visual game object rendered every frame on all clients.
    /// For example, a bullet following a straight line can be rendered just knowing its start location and velocity.
    /// </summary>
    /// <typeparam name="TP">The MonoBehaviour that represents this state visually</typeparam>
    public interface ISparseState<in TP> where TP : MonoBehaviour
    {
        public int StartTick { get; set; }
        public int EndTick { get; set; }

        /// <summary>
        /// The extrapolate method should update properties on the state struct using the given local time t
        /// (local time is the offset in seconds from StartTick).
        /// Note that these changes are just local predictions/extrapolations - they will not automatically be networked.
        /// (See the SparseCollection.Process method if you need to update state on State Authority)
        /// </summary>
        /// <param name="t">Local time in seconds to extrapolate to (t is 0 at Runner.Tick==StartTick)</param>
        /// <param name="prefab">Prefab used to create the visual (handy for accessing visual object configuration -
        /// Keep in mind that this is the prefab, it is *NOT* (guaranteed to be) the actual game object used to visualize this state)</param>
        public void Extrapolate(float t, TP prefab);
    }

    /// <summary>
    /// ISparseVisual is implemented by local game objects that "visualize" an ISparseState.
    /// </summary>
    /// <typeparam name="T">The actual type of the sparse state</typeparam>
    /// <typeparam name="TP">The MonoBehaviour used to visualise the sparse state</typeparam>
    public interface ISparseVisual<in T, TP> where T : unmanaged, ISparseState<TP> where TP : MonoBehaviour, ISparseVisual<T, TP>
    {
        /// <summary>
        /// This method is called every frame to update the visual game object to match the current render state.
        /// </summary>
        /// <param name="owner">The NB that owns the collection of sparse states</param>
        /// <param name="state">The current render state of this particular visual</param>
        /// <param name="t">The current local time that the state represents</param>
        /// <param name="isFirstRender">True the first time this is called for a new visual</param>
        /// <param name="isLastRender">True the last time this is called for a given visual</param>
        public void ApplyStateToVisual(NetworkBehaviour owner, T state, float t, bool isFirstRender, bool isLastRender);
    }

    /// <summary>
    /// The sparse collection maps sparse states to sparse visuals and keeps track of the somewhat complex timing involved
    /// </summary>
    /// <typeparam name="T">The actual type of the sparse state</typeparam>
    /// <typeparam name="TP">The MonoBehaviour used to visualise the sparse state</typeparam>
    public class SparseCollection<T, TP> where T : unmanaged, ISparseState<TP> where TP : MonoBehaviour, ISparseVisual<T, TP>
    {
        // Internal struct for keeping track of matching states and visuals
        private struct Entry
        {
            public TP Visual;
            public bool Enabled;
        }

        // To give proxies a chance to disable the local GO before it's re-used, we wait a few additional ticks before re-using a state.
        // ReSharper disable once InconsistentNaming
        private const int REUSE_DELAY_TICKS = 12;

        // Reference to the raw networked state data
        private readonly NetworkArray<T> _states;

        // State to visual map
        private readonly Entry[] _entries;

        // Prefab used for visuals
        private readonly TP _prefab;

        // Last time render was called - used to make sure we don't miss updates if framerate is low
        private float _nextRenderTime;

        /// <summary>
        /// The sparse collection itself is not a networked object, so you need to create its backing data elsewhere (in a NB)
        /// and pass it to the constructor along with a reference to the prefab to use for the associated visuals.
        /// Once created, call
        /// * Render() from the NetworkBehaviour's Render() method, passing in the relevant snapshot
        /// * Process() from the NetworkBehaviour's FixedUpdateNetwork() method if you want to alter state, and
        /// * Add() from Input or State auth to "spawn" a new object.
        /// </summary>
        /// <param name="states">Networked array of sparse state structs</param>
        /// <param name="prefab">Prefab to use for visuals</param>
        public SparseCollection(NetworkArray<T> states, TP prefab)
        {
            _entries = new Entry[states.Length];
            _states = states;
            _prefab = prefab;
        }

        /// <summary>
        /// Call Render() every frame to update visuals to their associated sparse state.
        /// </summary>
        /// <param name="owner">The NB that contains the networked state objects</param>
        /// <param name="states"></param>
        public void Render(NetworkBehaviour owner, NetworkArray<T> states)
        {
            var sandbox = owner.Sandbox;
            
            var renderTime = sandbox.Engine.LocalInterpolation;
            var localRenderTime = renderTime.Time - sandbox.FixedDeltaTime + (double)renderTime.Alpha * sandbox.DeltaTime;

            for (var i = 0; i < _entries.Length; i++)
            {
                var e = _entries[i];
                var state = states[i];

                if (!e.Enabled && e.Visual && e.Visual.gameObject.activeSelf)
                    e.Visual.gameObject.SetActive(false);

                // Note: t may be less than zero if we're rendering across several ticks and StartTick is somewhere in-between.
                // (E.g. from=100, to=102 with start=101 and alpha=0.25 will place us ahead of the start tick)
                var t = localRenderTime - state.StartTick * sandbox.DeltaTime;
                var t1 = (state.EndTick - state.StartTick) * sandbox.DeltaTime;

                var isLastRender = t >= t1 && e.Enabled;
                var isFirstRender = false;

                // We delay disabling of the object one frame since "last render" isn't really a last render if the object is immediately hidden.
                e.Enabled = t >= 0 && t < t1;

                // Make sure we have a valid enabled GameObject if this state represents an active instance
                if (e.Enabled || isLastRender)
                {
                    if (!e.Visual)
                    {
                        e.Visual = Object.Instantiate(_prefab);
                        isFirstRender = true;
                    }

                    if (!e.Visual.gameObject.activeSelf)
                    {
                        e.Visual.gameObject.SetActive(true);
                        isFirstRender = true;
                    }

                    switch (isFirstRender)
                    {
                        case true:
                            ApplyState(state, e, 0, true, false);
                            break;
                        case false when !isLastRender:
                            ApplyState(state, e, (float)t, false, false);
                            break;
                    }

                    if (isLastRender)
                        ApplyState(state, e, t1, false, true);
                }

                // Done modifying the entry struct, copy it back to the array
                _entries[i] = e;
            }
            return;
            void ApplyState(T state, Entry e, float t, bool isFirstRender, bool isLastRender)
            {
                // Update state to t
                state.Extrapolate(t, _prefab);

                // Update visual to match the state
                e.Visual.ApplyStateToVisual(owner, state, t, isFirstRender, isLastRender);
            }
        }

        public delegate bool Processor(ref T state, int tick);

        /// <summary>
        /// Call process every tick if you want to adjust *networked* properties on the sparse state.
        /// As the name suggests, these updates should be infrequent, so if you *do* change the state
        /// you must return true from the delegate to update the backing array.
        /// </summary>
        /// <param name="owner">The NB that owns the sparse state list</param>
        /// <param name="process">A delegate that will process each (active) sparse state</param>
        public void Process(NetworkBehaviour owner, Processor process)
        {
            if (owner.IsProxy)
                return;

            var sandbox = owner.Sandbox;
            for (var i = 0; i < _states.Length; i++)
            {
                var state = _states[i];
                var simTick = sandbox.Tick.TickValue;
                var t = (simTick - state.StartTick) * sandbox.DeltaTime;
                if (simTick > state.EndTick) continue;
                state.Extrapolate(t, _prefab);
                if (!process(ref state, simTick)) continue;
                // Since we're storing the extrapolated state, we must also update the start tick, as this is our new starting point going forward.
                state.StartTick = simTick;
                // Update the networked backing storage so the change is propagated
                _states[i] = state;
            }
        }

        /// <summary>
        /// Call Add to "instantiate" (or rather, "activate") a new sparse state. Note that this will not allocate
        /// but simply select the next in-active sparse state in the array. It will do nothing if none is found.
        /// </summary>
        /// <param name="sandbox"></param>
        /// <param name="state">The initial state to add</param>
        /// <param name="secondsToLive">Initial number of ticks for the sparse state to be alive</param>
        public void Add(NetworkSandbox sandbox, T state, float secondsToLive)
        {
            state.StartTick = sandbox.Tick.TickValue;
            state.EndTick = state.StartTick + Mathf.Max(1, (int)(secondsToLive / sandbox.DeltaTime));

            for (var i = 0; i < _states.Length; i++)
            {
                if (sandbox.Tick.TickValue <= _states[i].EndTick + REUSE_DELAY_TICKS) continue;
                _states[i] = state;
                return;
            }

            Debug.LogWarning("No free slots in state array!");
        }

        /// <summary>
        /// Call Clear to destroy all visuals for this sparse set.
        /// </summary>
        public void Clear()
        {
            for (var i = 0; i < _entries.Length; i++)
            {
                var e = _entries[i];
                if (e.Visual)
                    Object.Destroy(e.Visual.gameObject);
                _entries[i] = default;
            }
        }
    }
}