#if UNITY_EDITOR
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using UnityEditor;
using UnityEditor.Animations;
using AnimatorAsCode.V1.VRCDestructiveWorkflow;
using AnimatorAsCode.V1;
using System.Collections.Generic;

public abstract class ExpressionGenerator : MonoBehaviour
{
    public VRCAvatarDescriptor avatar;
    public AnimatorController assetContainer;

    public AacFlBase aac;
    public abstract string SystemName { get; }
    public abstract void Generate();

    public AacFlBase GetAAC()
    {
        return AacV1.Create(new AacConfiguration
        {
            SystemName = SystemName,
            AnimatorRoot = avatar.transform,
            DefaultValueRoot = avatar.transform,
            AssetContainer = assetContainer,
            AssetKey = SystemName,
            DefaultsProvider = new AacDefaultsProvider(writeDefaults: true)
        }.WithAvatarDescriptor(avatar));
    }

    public AacFlLayer CreateLayer(string name)
    {
        return aac.CreateSupportingArbitraryControllerLayer(assetContainer, name);
    }
}

[CustomEditor(typeof(ExpressionGenerator), true)]
public class ExpressionGeneratorInspector : Editor
{
    Dictionary<string, bool> foldoutState = new Dictionary<string, bool>();

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Create"))
        {
            var gen = (ExpressionGenerator)target;
            gen.aac = gen.GetAAC();
            gen.Generate();
        }
    }
}

#endif