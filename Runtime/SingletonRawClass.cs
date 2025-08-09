using System;
using UnityEngine;

namespace fefek5.Systems.SingletonSystem.Runtime
{
    public class SingletonRawClass<T> where T : SingletonRawClass<T>
    {
        public event Action onInit;
        public event Action onDeinit;
        
        public static T Instance
        {
            get
            {
                if (_instance != null) return _instance;

                Init();
                
                return _instance;
            }
        }

        private static T _instance;

        static SingletonRawClass() => Init();
        
        private static void Init()
        {
            if (_instance != null) return;
            
            _instance = Activator.CreateInstance<T>();
            
            Application.quitting += Deinit;
            
            _instance.OnInit();
        }
        
        private static void Deinit()
        {
            if (_instance == null) return;
            
            Application.quitting -= Deinit;
            
            _instance.OnDeinit();
            
            _instance = null;
        }

        protected virtual void OnInit() => onInit?.Invoke();
        
        protected virtual void OnDeinit() => onDeinit?.Invoke();
    }
}