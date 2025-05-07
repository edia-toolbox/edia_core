using UnityEditor;
using UnityEngine;

namespace Edia.Editor {

    public class EdiaPrefabsMenu : MonoBehaviour {
        [MenuItem("GameObject/UI/EDIA/VR Canvas", false, 10)]
        private static void CreateVRCanvas(MenuCommand menuCommand) {
            InstantiatePrefab("Edia-VRCanvas", menuCommand);
        }
        
        [MenuItem("GameObject/UI/EDIA/Button Icon", false, 10)]
        private static void CreateButtonIcon(MenuCommand menuCommand) {
            InstantiatePrefab("Edia-ButtonIcon", menuCommand);
        }

        [MenuItem("GameObject/UI/EDIA/Button Text", false, 11)]
        private static void CreateButtonText(MenuCommand menuCommand) {
            InstantiatePrefab("Edia-ButtonText", menuCommand);
        }

        [MenuItem("GameObject/UI/EDIA/Dropdown", false, 10)]
        private static void CreateDropdown(MenuCommand menuCommand) {
            InstantiatePrefab("Edia-Dropdown", menuCommand);
        }

        [MenuItem("GameObject/UI/EDIA/Panel", false, 10)]
        private static void CreatePanel(MenuCommand menuCommand) {
            InstantiatePrefab("Edia-Panel", menuCommand);
        }

        [MenuItem("GameObject/UI/EDIA/Panel Outlined", false, 10)]
        private static void CreatePanelOutline(MenuCommand menuCommand) {
            InstantiatePrefab("Edia-PanelOutlined", menuCommand);
        }

        [MenuItem("GameObject/UI/EDIA/Slider", false, 10)]
        private static void CreateSlider(MenuCommand menuCommand) {
            InstantiatePrefab("Edia-Slider", menuCommand);
        }

        [MenuItem("GameObject/UI/EDIA/Toggle", false, 10)]
        private static void CreateToggle(MenuCommand menuCommand) {
            InstantiatePrefab("Edia-Toggle", menuCommand);
        }

        [MenuItem("GameObject/UI/EDIA/Toggle Text", false, 10)]
        private static void CreateToggleText(MenuCommand menuCommand) {
            InstantiatePrefab("Edia-ToggleText", menuCommand);
        }

        [MenuItem("GameObject/UI/EDIA/Timer bar", false, 10)]
        private static void CreateTimerBar(MenuCommand menuCommand) {
            InstantiatePrefab("Edia-HorizontalTimer", menuCommand);
        }
        
        private static void InstantiatePrefab(string prefabName, MenuCommand command) {
            var prefab = Resources.Load<GameObject>($"UI/{prefabName}");
            if (prefab == null) {
                Debug.LogError($"Prefab '{prefabName}' not found in Resources/");
                return;
            }

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            GameObjectUtility.SetParentAndAlign(instance, command.context as GameObject);
            Undo.RegisterCreatedObjectUndo(instance, $"Create {prefabName}");
            Selection.activeObject = instance;
        }
    }
}