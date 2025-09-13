using System;
using Runtime;
using UnityEngine;

namespace Tests
{
    public class TestSingletonMonoBehaviour : SingletonBehaviour<TestSingletonMonoBehaviour>
    {
        [SerializeField] private int _value;
        
        [ContextMenu("Add")]
        private void Add() => Instance._value++;
        
        [ContextMenu("Subtract")]
        private void Subtract() => Instance._value--;

        private void OnEnable()
        {
            Debug.Log($"OnEnable: {_value}", this);
        }
        
        private void OnDisable()
        {
            Debug.Log($"OnDisable: {_value}", this);
        }
    }
}