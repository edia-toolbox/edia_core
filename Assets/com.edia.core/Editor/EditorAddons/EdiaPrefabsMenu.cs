using UnityEditor;
using UnityEngine;

namespace Edia.Editor {

    public class EdiaPrefabsMenu : MonoBehaviour {

#region UI Prefabs

        [MenuItem("GameObject/EDIA/UI/VR Canvas", false, 0)]
        private static void CreatePrefabVRCanvas(MenuCommand menuCommand) {
            InstantiatePrefab("UI", "Edia-VRCanvas", menuCommand);
        }

        [MenuItem("GameObject/EDIA/UI/Panel", false, 1)]
        private static void CreatePrefabPanel(MenuCommand menuCommand) {
            InstantiatePrefab("UI", "Edia-Panel", menuCommand);
        }

        [MenuItem("GameObject/EDIA/UI/Panel Outlined", false, 2)]
        private static void CreatePrefabPanelOutline(MenuCommand menuCommand) {
            InstantiatePrefab("UI", "Edia-PanelOutlined", menuCommand);
        }

        [MenuItem("GameObject/EDIA/UI/Button Text", false, 3)]
        private static void CreatePrefabButtonText(MenuCommand menuCommand) {
            InstantiatePrefab("UI", "Edia-ButtonText", menuCommand);
        }

        [MenuItem("GameObject/EDIA/UI/Button Icon", false, 4)]
        private static void CreatePrefabButtonIcon(MenuCommand menuCommand) {
            InstantiatePrefab("UI", "Edia-ButtonIcon", menuCommand);
        }

        [MenuItem("GameObject/EDIA/UI/Dropdown", false, 5)]
        private static void CreatePrefabDropdown(MenuCommand menuCommand) {
            InstantiatePrefab("UI", "Edia-Dropdown", menuCommand);
        }

        [MenuItem("GameObject/EDIA/UI/Slider", false, 6)]
        private static void CreatePrefabSlider(MenuCommand menuCommand) {
            InstantiatePrefab("UI", "Edia-Slider", menuCommand);
        }

        [MenuItem("GameObject/EDIA/UI/Toggle", false, 7)]
        private static void CreatePrefabToggle(MenuCommand menuCommand) {
            InstantiatePrefab("UI", "Edia-Toggle", menuCommand);
        }

        [MenuItem("GameObject/EDIA/UI/Toggle Text", false, 8)]
        private static void CreatePrefabToggleText(MenuCommand menuCommand) {
            InstantiatePrefab("UI", "Edia-ToggleText", menuCommand);
        }

        [MenuItem("GameObject/EDIA/UI/Timer bar", false, 9)]
        private static void CreatePrefabTimerBar(MenuCommand menuCommand) {
            InstantiatePrefab("UI", "Edia-TimerBar", menuCommand);
        }

        [MenuItem("GameObject/EDIA/UI/Progress bar", false, 10)]
        private static void CreatePrefabProgressBar(MenuCommand menuCommand) {
            InstantiatePrefab("UI", "Edia-ProgressBar", menuCommand);
        }
#endregion
#region Core prefabs
        
        [MenuItem("GameObject/EDIA/Core/Edia-XRRig", false, 1)]
        private static void CreatePrefabEdiaXRRig(MenuCommand menuCommand) {
            InstantiatePrefab("Core", "Edia-XRRig", menuCommand);
        }

        [MenuItem("GameObject/EDIA/Core/Edia-Executor", false, 2)]
        private static void CreatePrefabEdiaExecutor(MenuCommand menuCommand) {
            InstantiatePrefab("Core", "Edia-Executor", menuCommand);
        }

        [MenuItem("GameObject/EDIA/Core/Edia-Controller", false, 3)]
        private static void CreatePrefabEdiaController(MenuCommand menuCommand) {
            InstantiatePrefab("Core", "Edia-Controller", menuCommand);
        }

        [MenuItem("GameObject/EDIA/Core/Edia-SceneLoader", false, 4)]
        private static void CreatePrefabEdiaSceneLoader(MenuCommand menuCommand) {
            InstantiatePrefab("Core", "Edia-SceneLoader", menuCommand);
        }

#endregion

        private static void InstantiatePrefab(string subFolder, string prefabName, MenuCommand command) {
            var prefab = Resources.Load<GameObject>($"Prefabs/{subFolder}/{prefabName}");
            if (prefab == null) {
                Debug.LogError($"Prefab '{prefabName}' not found in Resources/{subFolder}");
                return;
            }

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            GameObjectUtility.SetParentAndAlign(instance, command.context as GameObject);
            Undo.RegisterCreatedObjectUndo(instance, $"Create {prefabName}");
            Selection.activeObject = instance;
        }

    }
}