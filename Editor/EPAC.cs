#if UNITY_EDITOR
using UnityEditor;
using VRC.SDK3.Avatars.ScriptableObjects;
using System.Collections.Generic;
using System;
using System.Linq;

namespace EPAC
{
    /// <summary>
    /// Epression Parameter As Code Builder
    /// Class used to generate expression parameters with code.
    /// </summary>
    public class EPACBuilder
    {
        public VRCExpressionParameters TargetParameters;
        public string ParameterPrefix;
        public List<VRCExpressionParameters.Parameter> Parameters = new List<VRCExpressionParameters.Parameter>();

        /// <summary>
        /// Create a new builder for expression parameters. Existing parameters will be removed from the given parameter list.
        /// </summary>
        /// <param name="targetParameters">The parameter list asset all the parameters will be added to.</param>
        /// <exception cref="ArgumentException">Thrown when targetParameters is null.</exception>
        public EPACBuilder(VRCExpressionParameters targetParameters)
        {
            if (targetParameters == null)
                throw new ArgumentException("targetParameters cannot be null!");

            TargetParameters = targetParameters;
        }

        /// <summary>
        /// Sets the prefix all the parameters will be prefixed with durig build.
        /// </summary>
        public EPACBuilder WithPrefix(string prefix)
        {
            if (prefix == null) prefix = "";
            ParameterPrefix = prefix;
            return this;
        }

        /// <summary>
        /// Add a new parameter
        /// </summary>
        /// <param name="name">Name of the parameter</param>
        /// <param name="type">Type of the parameter</param>
        /// <param name="defaultValue">Default value for the parameter. If the type is boolean, true is mapped to 1.0 and false is mapped to 0.0</param>
        /// <param name="saved">If true, the parameter will be saved between avatar loads.</param>
        /// <param name="synced">If true, the parameter will be synchronized between other players.</param>
        /// <returns></returns>
        public EPACBuilder Add(string name, VRCExpressionParameters.ValueType type, float defaultValue, bool saved, bool synced = true)
        {
            Parameters.Add(
                new VRCExpressionParameters.Parameter()
                {
                    name = ParameterPrefix + name,
                    valueType = type,
                    defaultValue = defaultValue,
                    saved = saved,
                    networkSynced = synced,
                }
            );
            return this;
        }

        /// <summary>
        /// Add a new parameter of type bool
        /// </summary>
        /// <param name="name">Name of the parameter</param>
        /// <param name="defaultValue">Default value</param>
        /// <param name="saved">If true, the parameter will be saved between avatar loads.</param>
        /// <param name="synced">If true, the parameter will be synchronized between other players.</param>
        public EPACBuilder Bool(string name, bool defaultValue, bool saved, bool synced = true)
            => Add(name, VRCExpressionParameters.ValueType.Bool, defaultValue ? 1f : 0f, saved, synced);
        /// <summary>
        /// Add a new parameter of type float
        /// </summary>
        /// <param name="name">Name of the parameter</param>
        /// <param name="defaultValue">Default value</param>
        /// <param name="saved">If true, the parameter will be saved between avatar loads.</param>
        /// <param name="synced">If true, the parameter will be synchronized between other players.</param>
        public EPACBuilder Float(string name, float defaultValue, bool saved, bool synced = true)
            => Add(name, VRCExpressionParameters.ValueType.Float, defaultValue, saved, synced);
        /// <summary>
        /// Add a new parameter of type int
        /// </summary>
        /// <param name="name">Name of the parameter</param>
        /// <param name="defaultValue">Default value</param>
        /// <param name="saved">If true, the parameter will be saved between avatar loads.</param>
        /// <param name="synced">If true, the parameter will be synchronized between other players.</param>
        public EPACBuilder Int(string name, int defaultValue, bool saved, bool synced = true)
            => Add(name, VRCExpressionParameters.ValueType.Int, defaultValue, saved, synced);

        /// <summary>
        /// Build the parameter list. Removes existing parameters from the target parameter list asset and populates it with new ones.
        /// If multiple parameters have the same name, it's type and value will be set to the last one.
        /// If one or more parameter is synced or saved, that property will be set to true.
        /// </summary>
        public void Build()
        {
            TargetParameters.parameters = Parameters
                .Select(p => p.name)
                .Distinct()
                .Select(name =>
                {
                    var last = Parameters.Last(p => p.name == name);
                    last.saved = Parameters.Any(p => p.name == name && p.saved);
                    last.networkSynced = Parameters.Any(p => p.name == name && p.networkSynced);
                    return last;
                }).ToArray();
            EditorUtility.SetDirty(TargetParameters);
            AssetDatabase.SaveAssets();
        }
    }

}
#endif