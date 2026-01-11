using BepInEx;
using BepInEx.Logging;
using SlugBase.Features;
using System;
using UnityEngine;
using static SlugBase.Features.FeatureTypes;

namespace ScavSlug
{
    [BepInPlugin(MOD_ID, "Slugcat Template", "0.1.0")]
    class Plugin : BaseUnityPlugin
    {
        private const string MOD_ID = "scaggytowny";
        public static ManualLogSource SLogger;

        // Add hooks
        public void OnEnable()
        {
            SLogger = Logger;
            On.RainWorld.OnModsInit += Extras.WrapInit(LoadResources);

            // Put your custom hooks here!
            Hooks.ApplyPlayerHooks();
            Hooks.ApplySpearOnBackHooks();
        }
        
        // Load any resources, such as sprites or sounds
        private void LoadResources(RainWorld rainWorld)
        {
        }
    }
}