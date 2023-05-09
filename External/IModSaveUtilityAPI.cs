using OWML.Common;
using OWML.ModHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Events;

namespace OuterWildsRPG.External
{
    public interface IModSaveUtilityAPI
    {
        UnityEvent<string> OnLoad { get; }
        UnityEvent<string> OnSave { get; }
        UnityEvent<string> OnReset { get; }

        bool IsReady();
        bool ResetAllValues(IModBehaviour mod);
        bool HasValue<T>(IModBehaviour mod, string key);
        bool TryReadValue<T>(IModBehaviour mod, string key, out T value);
        T ReadValue<T>(IModBehaviour mod, string key, T defaultValue = default);
        bool WriteValue<T>(IModBehaviour mod, string key, T value);
    }

    public class ModSaveUtility
    {
        IModBehaviour mod;
        IModSaveUtilityAPI api;

        public ModSaveUtility(IModBehaviour mod)
        {
            this.mod = mod;
            api = mod.ModHelper.Interaction.TryGetModApi<IModSaveUtilityAPI>("Hawkbar.ModSaveUtility");
        }

        public UnityEvent<string> OnLoad => api.OnLoad;
        public UnityEvent<string> OnSave => api.OnSave;
        public UnityEvent<string> OnReset => api.OnReset;

        public bool IsReady() => api.IsReady();
        public bool ResetAllValues() => api.ResetAllValues(mod);
        public bool HasValue<T>(string key) => api.HasValue<T>(mod, key);
        public bool TryReadValue<T>(string key, out T value) => api.TryReadValue(mod, key, out value);
        public T ReadValue<T>(string key, T defaultValue = default) => api.ReadValue(mod, key, defaultValue);
        public bool WriteValue<T>(string key, T value) => api.WriteValue(mod, key, value);
    }
}
