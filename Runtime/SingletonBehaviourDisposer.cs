using UnityEngine;

namespace fefek5.Systems.SingletonSystem.Runtime
{
    /// <summary>
    /// Disposes of all singleton behaviours in this game object when it is destroyed.
    /// </summary>
    [DefaultExecutionOrder(9999)]
    internal class SingletonBehaviourDisposer : MonoBehaviour
    {
        private void OnDestroy()
        {
            var singletonBehaviours = GetComponents<SingletonBehaviour>();

            foreach (var singletonBehaviour in singletonBehaviours)
                if (singletonBehaviour.Initialized) singletonBehaviour.Dispose();
        }
    }
}