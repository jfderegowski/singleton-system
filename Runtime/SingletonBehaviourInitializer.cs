using UnityEngine;

namespace fefek5.Systems.SingletonSystem.Runtime
{
    /// <summary>
    /// Initializes all singleton behaviours in this game object.
    /// </summary>
    [DefaultExecutionOrder(-10000)]
    internal class SingletonBehaviourInitializer : MonoBehaviour
    {
        private void Awake()
        {
            var singletonBehaviours = GetComponents<SingletonBehaviour>();

            foreach (var singletonBehaviour in singletonBehaviours)
                if (!singletonBehaviour.Initialized) singletonBehaviour.Initialize();
        }
    }
}