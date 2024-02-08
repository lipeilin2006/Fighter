using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fighter
{
    //这个类不需要改。
    public abstract class Component : IDisposable
    {
        public GameObject gameObject { get; set; } = null;
        public Entity Entity { get; set; } = null;
        public virtual void Init() { }
        public virtual void Start() { }
        public virtual void Update() { }
        public virtual void FixedUpdate() { }
        public virtual void LateUpdate() { }
        public virtual void NetworkUpdate() { }
        public virtual void Dispose() { }
    }
}
