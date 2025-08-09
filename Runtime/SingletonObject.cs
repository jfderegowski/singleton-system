using System.Linq;
using UnityEngine;

namespace fefek5.Systems.SingletonSystem.Runtime
{
    /// <summary>
    /// Base class for scriptable object singletons.
    /// </summary>
    public abstract class SingletonObject : ScriptableObject { }

    /// <summary>
    /// Adds singleton functionality to a scriptable object.
    /// </summary>
    /// <typeparam name="T">The type of the scriptable object singleton.</typeparam>
    public abstract class SingletonObject<T> : SingletonObject where T : SingletonObject<T>
    {
        /// <summary>
        /// The instance of the scriptable object singleton.
        /// </summary>
        public static T Instance
        {
            get
            {
                if (_instance) return _instance;

                if (SingletonsStorage.Instance)
                {
                    foreach (var soSingleton in SingletonsStorage.Instance.SingletonObjects.Where(singletonObject =>
                                 singletonObject.GetType() == typeof(T)))
                        return _instance = soSingleton as T;
                }

                _instance = Resources.Load<T>(typeof(T).Name);

                if (_instance) return _instance;

#if UNITY_EDITOR
                _instance = CreateScriptableObject();
                
                static T CreateScriptableObject()
                {
                    var singletonObject = CreateInstance<T>();
                    singletonObject.name = typeof(T).Name;

                    if (!UnityEditor.AssetDatabase.IsValidFolder("Assets/Resources"))
                        UnityEditor.AssetDatabase.CreateFolder("Assets", "Resources");

                    UnityEditor.AssetDatabase.CreateAsset(singletonObject, $"Assets/Resources/{typeof(T).Name}.asset");
                    
                    UnityEditor.AssetDatabase.SaveAssets();

                    return singletonObject;
                }
#endif

                return _instance;
            }
        }

        private static T _instance;
    }
}