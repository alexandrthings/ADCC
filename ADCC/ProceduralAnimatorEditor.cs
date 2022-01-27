#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using ASFramework.Characters;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;


/* This is the monster that scrapes your character's animator for actions to run.
 * 
 * 
 *
 */

[CustomEditor(typeof(Character), true)]
public class ProceduralAnimatorEditor : Editor
{
    private int childCounter;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        Character animator = (Character)target;

        if (GUILayout.Button("Find actions"))
        {
            FindActions(animator);
        }

        if (GUILayout.Button("Clear actions"))
        {
            ClearActions(animator);
        }
    }

    public void FindActions(Character charac)
    {
        childCounter = 0;

        AnimatorController AnimCont = (AnimatorController)charac.animator.animator.runtimeAnimatorController;
        // fsfr the animator starts returning incorrect layer index if i do something to it
       // int[] layerIndex = new int[2]
            // {charac.animator.animator.GetLayerIndex("FullBody"), charac.animator.animator.GetLayerIndex("UpperBody")};
       //     {2, 1}; // fuck you GetLayerIndex

        //charac.actions.Clear();
        //charac.chainActions.Clear();

        for (int i = 0; i < AnimCont.layers.Length; i++)
        {
            Debug.Log(i);
            FindFunction(charac, AnimCont, i);
        }
        
    }

    // FIRST TRANSITION NEEDS TO BE EMPTY
    public void FindFunction(Character _charac, AnimatorController cont, int layer)
    {
        if (cont.layers[layer].stateMachine.anyStateTransitions.Length == 0)
            return;

        AnimatorStateTransition cancelTransition = cont.layers[layer].stateMachine.anyStateTransitions[0];

        AnimatorCondition cancel1 = new AnimatorCondition();

        cancel1.mode = AnimatorConditionMode.NotEqual;
        cancel1.parameter = "layerSelect";
        cancel1.threshold = layer;

        cancelTransition.conditions = new AnimatorCondition[] {cancel1};

        List<ProceduralAction> tempActionStorage = new List<ProceduralAction>(); // to hold stuff until we replace the real thing
        List<ProceduralAction> tempChainActionStorage = new List<ProceduralAction>();

        // take the first steps of each chain (ignore 1st one because it's the empty one)
        for (int t = 1; t < cont.layers[layer].stateMachine.anyStateTransitions.Length; t++)
        {
            AnimatorStateTransition transition = cont.layers[layer].stateMachine.anyStateTransitions[t];
            AnimatorState destState = transition.destinationState;

            AnimatorCondition newCond = new AnimatorCondition();
            AnimatorCondition newCond2 = new AnimatorCondition();
            AnimatorCondition newCond3 = new AnimatorCondition();

            List<ProceduralAction> actions = new List<ProceduralAction>();

            float length = 0;

            if (transition.destinationState.motion is AnimationClip) // make sure it's not a blendtree
                length = ((AnimationClip) transition.destinationState.motion).length / transition.destinationState.speed;

            // condition for selector to be equal
            newCond.mode = AnimatorConditionMode.Equals;
            newCond.parameter = "actionSelect";
            newCond.threshold = t-1;

            // condition to start
            newCond2.mode = AnimatorConditionMode.If;
            newCond2.parameter = "startAction";
            newCond2.threshold = 1;

            // condition for right layer
            newCond3.mode = AnimatorConditionMode.Equals;
            newCond3.parameter = "layerSelect";
            newCond3.threshold = layer;

            transition.conditions = new AnimatorCondition[] {newCond, newCond2, newCond3};
            transition.canTransitionToSelf = true;

            //Debug.Log(transition.destinationState.name);

            int existingIndex = _charac.GetAnimationIndex(destState.name);
            if (existingIndex == -1) // if action does not exist, create new
            {
                actions.Add(new ProceduralAction(transition.destinationState.name, t - 1, layer, length, length * 0.8f,
                    cont.layers[layer].blendingMode == AnimatorLayerBlendingMode.Additive
                        ? ActionType.KeepState
                        : ActionType.StateChange));
            }
            else // copy properties of existing
            {
                actions.Add(new ProceduralAction(transition.destinationState.name, t - 1, layer, length, length * 0.8f,
                    cont.layers[layer].blendingMode == AnimatorLayerBlendingMode.Additive
                        ? ActionType.KeepState
                        : ActionType.StateChange));

                actions[actions.Count - 1].state = _charac.actions[existingIndex].state;
                actions[actions.Count - 1].AlwaysOverride = _charac.actions[existingIndex].AlwaysOverride;
                actions[actions.Count - 1].Infinite = _charac.actions[existingIndex].Infinite;
                actions[actions.Count - 1].type = _charac.actions[existingIndex].type;
            }

            int w = 0; // index for length of action chain

            newCond2.parameter = "continueChain";

            // add chainables
            while (destState.transitions.Length > 0)
            {
                length = 0; // incase its a blendtree
                transition = destState.transitions[0];
                destState = transition.destinationState;

                transition.conditions = new AnimatorCondition[] {newCond2};
                
                if (destState.motion is AnimationClip) // incase its a blendtree
                    length = ((AnimationClip) destState.motion).length;

                int existingChainIndex = _charac.GetChainIndex(destState.name);
                if (existingChainIndex == -1)
                {
                    actions.Add(
                        new ProceduralAction(destState.name,
                            w, layer,
                            length,
                            length * 0.8f,
                            cont.layers[layer].blendingMode == AnimatorLayerBlendingMode.Additive
                                ? ActionType.KeepState
                                : ActionType.StateChange));
                }
                else // copy properties of existing
                {
                    actions.Add(
                        new ProceduralAction(destState.name,
                            w, layer,
                            length,
                            length * 0.8f,
                            cont.layers[layer].blendingMode == AnimatorLayerBlendingMode.Additive
                                ? ActionType.KeepState
                                : ActionType.StateChange));

                    actions[actions.Count - 1].state = _charac.chainActions[existingChainIndex].state;
                    actions[actions.Count - 1].AlwaysOverride = _charac.chainActions[existingChainIndex].AlwaysOverride;
                    actions[actions.Count - 1].Infinite = _charac.chainActions[existingChainIndex].Infinite;
                    actions[actions.Count - 1].type = _charac.chainActions[existingChainIndex].type;
                }

                actions[w].childAction = childCounter;

                childCounter++;
                w++;
            }

            if (actions.Count > 0) // jic
            {
                actions[actions.Count - 1].childAction = -1; // set last child as terminator

                // set child to be the next action in array
                for (int c = 1; c < actions.Count; c++)
                {
                    tempChainActionStorage.Add(actions[c]);
                }

                tempActionStorage.Add(actions[0]); // add parent action
            }
        }
        
        // Clear the actions and replace them with the new ones
        ClearActions(_charac);

        _charac.actions = tempActionStorage;
        _charac.chainActions = tempChainActionStorage;

        // auto-configure some of them
        for (int i = 0; i < _charac.actions.Count; i++)
        {
            if (_charac.actions[i].name.ToLower().Contains("attack"))
            {
                _charac.actions[i].state = CharacterState.RootMotion;
                continue;
            }

            if (_charac.actions[i].name.ToLower().Contains("dodge"))
            {
                _charac.actions[i].state = CharacterState.Dodge;
                continue;
            }

            if (_charac.actions[i].name.ToLower().Contains("clamber"))
            {
                _charac.actions[i].state = CharacterState.Clamber;
                continue;
            }

            _charac.actions[i].state = CharacterState.StaticAttack;
        }

        AssetDatabase.SaveAssets();
    }

    public void ClearActions(Character charac)
    {
        charac.actions.Clear();
        charac.chainActions.Clear();
    }

    public AnimatorState GetNextActions(AnimatorState _state)
    {
        AnimatorState state = _state.transitions[0].destinationState;

        return state;
    }

    public int GetStateIndex(AnimatorState state, AnimatorController cont, int layer)
    {
        int ind = 0;

        for (int i = 0; i < cont.layers[layer].stateMachine.states.Length; i++)
        {
            if (cont.layers[layer].stateMachine.states[i].state == state)
                return i;
        }

        return ind;
    }
}
#endif