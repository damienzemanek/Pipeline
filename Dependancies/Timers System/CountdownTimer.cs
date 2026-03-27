using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace EMILtools.Timers
{
    public interface ICountdownTimer
    {
        public bool IsFinished();
        public void Reset();
        public void Restart();
        public void Reset(float newInitialTime);
    }
    
    [Serializable]
    public class CountdownTimer : Timer, ICountdownTimer
    { 
        public CountdownTimer(float _initialTime) : base(_initialTime) { }
        public CountdownTimer(Ref<float> _initialTime) : base(_initialTime) { }

        
        public CountdownTimer(float _initialTime, bool isRef, 
            Action[] OnTimerStartCbs = null, Action[] OnTimerTickCbs = null, Action[] OnTimerStopCbs = null)
        : base (_initialTime, OnTimerStartCbs, OnTimerTickCbs, OnTimerStopCbs) { }
        
        public override void TickImplementation(float deltaTime)
        {

            if (Time > 0) { Time -= deltaTime; }
            if (Time <= 0) { Time = 0; Stop(); }
        }
        
        public bool IsFinished() => Time <= 0;
        public void Reset() => Time = initialTime;
        public void Restart() { Reset(); StartAndReset(); }
        public void Reset(float newInitialTime) => Time = newInitialTime;
        
        [Button] void SetTimeTo(float val) => Time = val;
        [Button] void StartAgain() => StartAndReset();

    }
}

