using System.Collections.Generic;
using UnityEngine;

namespace Unity2DGameTemplate.StateMachines
{
    /// <summary>
    /// ステートマシンのを表すクラスです。
    /// </summary>
    /// <typeparam name="TEStateName">ステートを判別するための列挙型です。</typeparam>
    /// <typeparam name="TStateMachineMessage">ステートマシンから各ステートに渡す情報です。</typeparam>
    public abstract class StateMachineBase<TEStateName, TStateMachine, TStateMachineMessage> : MonoBehaviour
        where TEStateName : System.Enum
        where TStateMachine : StateMachineBase<TEStateName, TStateMachine, TStateMachineMessage>
    {
        [SerializeField] protected List< StateBase<TEStateName, TStateMachine, TStateMachineMessage> > stateList;
        protected Dictionary<TEStateName, StateBase<TEStateName, TStateMachine, TStateMachineMessage>> States
            = new Dictionary<TEStateName, StateBase<TEStateName, TStateMachine, TStateMachineMessage>>();

        public TEStateName CurrentStateName { get; protected set; }

        protected TStateMachineMessage StateMachineMessage;

        protected abstract TStateMachine StateMachine { get; }

        public void RegisterState(StateBase<TEStateName, TStateMachine, TStateMachineMessage> state)
        {
            States.Add(state.StateName, state);
        }

        public void InitState(TEStateName stateName)
        {
            CurrentStateName = stateName;
            States[CurrentStateName].OnEnter(StateMachine, StateMachineMessage);
        }

        public void ChangeState(TEStateName stateName)
        {
            States[CurrentStateName].OnExit(StateMachine, StateMachineMessage);
            CurrentStateName = stateName;
            States[CurrentStateName].OnEnter(StateMachine, StateMachineMessage);
        }

        protected virtual void Update()
        {
            States[CurrentStateName].Update(StateMachine, StateMachineMessage);
        }
    }

    /// <summary>
    /// 1つのステートを表すクラスです。
    /// </summary>
    /// <typeparam name="TStateNameEnum">ステートを判別するための列挙型です。</typeparam>
    /// <typeparam name="TStateMachine">ステートマシンを表すクラスです。</typeparam>
    /// <typeparam name="TStateMessage">ステートマシンから各ステートに渡す情報です。</typeparam>
    public abstract class StateBase<TStateNameEnum, TStateMachine, TStateMessage> where TStateNameEnum : System.Enum
        where TStateMachine : StateMachineBase<TStateNameEnum, TStateMachine, TStateMessage>
    {
        public abstract TStateNameEnum StateName { get; }

        public abstract void OnEnter(TStateMachine sm, TStateMessage message);

        public abstract void Update(TStateMachine sm, TStateMessage message);

        public abstract void OnExit(TStateMachine sm, TStateMessage message);
    }
}
