using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Runtime
{
    /// <summary>
    /// Collection of all singletons.
    /// </summary>
    public class SingletonsStorage : ResourcesScriptableObject<SingletonsStorage>
    {
        /// <summary>
        /// The list of all singleton prefabs in the Resources folder.
        /// </summary>
        public static List<SingletonBehaviour> ResourcesSingletonBehaviours
        {
            get
            {
                if (_resourcesSingletonBehaviours == null)
                {
                    var allPrefabs = Resources.LoadAll<GameObject>("");

                    var singletonPrefabs = allPrefabs
                        .Select(prefab => prefab.GetComponent<SingletonBehaviour>())
                        .Where(component => component != null)
                        .ToList();

                    singletonPrefabs = singletonPrefabs.Except(Instance.SingletonBehaviours).ToList();
                    
                    var duplicates = (from singleton in singletonPrefabs
                        let count = singletonPrefabs.Count(other => singleton == other)
                        where count > 1
                        select singleton).ToList();
                    
                    foreach (var duplicate in duplicates)
                        Debug.LogWarning($"[SingletonSystem] Duplicate singleton prefab in Resources: {duplicate}", duplicate);
                    
                    _resourcesSingletonBehaviours = singletonPrefabs.Except(duplicates).ToList();
                }

                return _resourcesSingletonBehaviours;
            }
        }
        
        /// <summary>
        /// Backing field for ResourcesSingletonBehaviours.
        /// </summary>
        private static List<SingletonBehaviour> _resourcesSingletonBehaviours;
        
        /// <summary>
        /// The list of all singletons.
        /// </summary>
        [field: SerializeField, Tooltip("Add a singleton to the list and it will be initialized on startup.")]
        public List<SingletonBehaviour> SingletonBehaviours { get; private set; } = new();
        
        /// <summary>
        /// The list of all scriptable object singletons.
        /// </summary>
        [field: SerializeField, Tooltip("Add a scriptable object singleton to the list and it will be get from there.")]
        public List<SingletonObject> SingletonObjects { get; private set; } = new();
        
        /// <summary>
        /// Call base OnValidate method to ensure that the singletons are valid.
        /// </summary>
        protected override void OnValidate()
        {
            base.OnValidate();
            
            CheckForDuplicates();
        }

        /// <summary>
        /// Validates the singletons before initializing them.
        /// </summary>
        private void ValidateBeforeInitialize()
        {
            CheckForNulls();
            CheckForDuplicates();
        }

        /// <summary>
        /// Checks for null singletons.
        /// </summary>
        private void CheckForNulls()
        {
            var nulls = (from singleton in SingletonBehaviours
                where !singleton
                select singleton).ToList();

            foreach (var nullSingleton in nulls)
            {
                Debug.Log("[SingletonSystem] Removing null singleton", this);
                SingletonBehaviours.Remove(nullSingleton);
            }
        }

        /// <summary>
        /// Checks for duplicate singletons.
        /// </summary>
        private void CheckForDuplicates()
        {
            var singletonBehavioursDuplicates = (from singletonBehaviour in SingletonBehaviours
                where singletonBehaviour
                let count = SingletonBehaviours.Count(otherSingleton => singletonBehaviour == otherSingleton)
                where count > 1
                select singletonBehaviour).ToList();

            foreach (var duplicate in singletonBehavioursDuplicates) 
                Debug.Log($"[SingletonSystem] Duplicate singleton: {duplicate}", this);
            
            var singletonObjectsDuplicates = (from singletonObject in SingletonObjects
                where singletonObject
                let count = SingletonObjects.Count(otherSoSingleton => singletonObject == otherSoSingleton)
                where count > 1
                select singletonObject).ToList();
            
            foreach (var duplicate in singletonObjectsDuplicates)
                Debug.Log($"[SingletonSystem] Duplicate scriptable object singleton: {duplicate}", this);
        }

        #region Initialization

        /// <summary>
        /// The list of all spawned singletons.
        /// This list is used to get the singleton of the specified type.
        /// This list is also used to check if the singleton is already spawned.
        /// </summary>
        private static readonly List<SingletonBehaviour> _spawnedSingletons = new();
        
        /// <summary>
        /// The list of all singletons to enable.
        /// </summary>
        private static readonly Dictionary<SingletonBehaviour, bool> _singletonsToEnable = new();

        /// <summary>
        /// Initializes the singleton collection.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            Instance.ValidateBeforeInitialize();

            _spawnedSingletons.Clear();
            _singletonsToEnable.Clear();
            
            var singletonsToSpawn = new List<SingletonBehaviour>();
            singletonsToSpawn.AddRange(Instance.SingletonBehaviours);
            singletonsToSpawn.AddRange(ResourcesSingletonBehaviours);
            
            foreach (var singletonPrefab in singletonsToSpawn)
            {
                var prevEnabled = singletonPrefab.gameObject.activeSelf;

                singletonPrefab.gameObject.SetActive(false);

                var singletonClone = Instantiate(singletonPrefab);

                singletonClone.Initialize();
                
                _singletonsToEnable.Add(singletonClone, prevEnabled);
            }
            
            foreach (var (singleton, prevEnabled) in _singletonsToEnable) 
                singleton.gameObject.SetActive(prevEnabled);
        }

        /// <summary>
        /// Gets the singleton of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the singleton.</typeparam>
        /// <returns></returns>
        internal static T Get<T>() where T : Component
        {
            if (Application.isPlaying)
            {
                foreach (var singleton in _spawnedSingletons)
                    if (singleton is T prefab) return prefab;
            }
            else
            {
                foreach (var singleton in Instance.SingletonBehaviours)
                    if (singleton is T prefab) return prefab;
                
                foreach (var singleton in ResourcesSingletonBehaviours)
                    if (singleton is T prefab) return prefab;
            }

            Debug.Log($"[SingletonSystem] No found singleton of type {typeof(T).Name}. " +
                      "Calling FindFirstObjectByType<T>()");
            
            var findObject = FindFirstObjectByType<T>();

            return findObject ? findObject : new GameObject(typeof(T).Name).AddComponent<T>();
        }

        /// <summary>
        /// Registers a singleton.
        /// </summary>
        /// <param name="singletonBehaviour">The singleton to register.</param>
        internal static void Register(SingletonBehaviour singletonBehaviour) => _spawnedSingletons.Add(singletonBehaviour);

        /// <summary>
        /// Unregisters a singleton.
        /// </summary>
        /// <param name="singletonBehaviour">The singleton to unregister.</param>
        internal static void UnRegister(SingletonBehaviour singletonBehaviour) => _spawnedSingletons.Remove(singletonBehaviour);

        #endregion
    }
}