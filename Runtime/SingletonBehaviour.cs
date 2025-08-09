using System;
using UnityEngine;

namespace fefek5.Systems.SingletonSystem.Runtime
{
    /// <summary>
    /// Base class for all singletons.
    /// </summary>
    [DefaultExecutionOrder(10000), RequireComponent(typeof(SingletonBehaviourInitializer), typeof(SingletonBehaviourDisposer))]
    public abstract class SingletonBehaviour : MonoBehaviour
    {
        #region Events

        /// <summary>
        /// Invoked when the singleton is initialized.
        /// </summary>
        public event Action onInitialize;
        
        /// <summary>
        /// Invoked when the singleton is disposed.
        /// </summary>
        public event Action onDispose;

        /// <summary>
        /// Invoked when the singleton is initialized.
        /// </summary>
        protected virtual void OnInitialize() => onInitialize?.Invoke();
        
        /// <summary>
        /// Invoked when the singleton is disposed.
        /// </summary>
        protected virtual void OnDispose() => onDispose?.Invoke();
        
        #endregion
        
        /// <summary>
        /// If true, the singleton will be added to the DontDestroyOnLoad list.
        /// </summary>
        [field: SerializeField, Tooltip("If true, the singleton will be added to the DontDestroyOnLoad list.")]
        public bool SetDontDestroyOnLoad { get; protected set; } = true;
        
        /// <summary>
        /// Indicates whether the singleton is initialized.
        /// </summary>
        public bool Initialized { get; private set; }

        /// <summary>
        /// Initializes the singleton.
        /// </summary>
        internal virtual void Initialize()
        {
            if (Initialized)
            {
                Debug.LogWarning($"[SingletonSystem] Singleton of type {GetType().Name} is already initialized.");
                return;
            }

            if (SetDontDestroyOnLoad)
                DontDestroyOnLoad(gameObject);
            
            SingletonsStorage.Register(this);
            
            Initialized = true;
            
            OnInitialize();
        }

        /// <summary>
        /// Disposes of the singleton.
        /// </summary>
        internal virtual void Dispose()
        {
            if (!Initialized)
            {
                Debug.LogWarning($"[SingletonSystem] Singleton of type {GetType().Name} is not initialized.");
                return;
            }

            SingletonsStorage.UnRegister(this);
            
            Initialized = false;
            
            OnDispose();
        }
    }
    
    /// <summary>
    /// Singleton class that can be inherited to create a singleton.
    /// </summary>
    /// <typeparam name="T">The type of the singleton.</typeparam>
    public class SingletonBehaviour<T> : SingletonBehaviour where T : Component
    {
        /// <summary>
        /// The instance of the singleton.
        /// </summary>
        public static T Instance
        {
            get
            {
                if (_instance) return _instance;

                _instance = SingletonsStorage.Get<T>();

                return _instance;
            }
        }
        
        private static T _instance;

        /// <summary>
        /// Initializes the singleton
        /// </summary>
        internal sealed override void Initialize()
        {
            if (_instance && _instance != this)
            {
                Debug.LogWarning($"[SingletonSystem] Singleton of type {typeof(T).Name} already exists" +
                                 $"in '{_instance.gameObject.scene.name}' scene. " +
                                 $"Destroying this instance on {gameObject.scene.name}.");
                
                Destroy(gameObject);
            }
            else if (!_instance) _instance = GetComponent<T>();

            base.Initialize();
        }
    }
}