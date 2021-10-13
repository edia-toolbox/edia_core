using System;
using System.Text;

namespace Tobii.XR.Internal
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;
    using UnityEditorInternal;
    using UnityEngine;

    [CustomPropertyDrawer(typeof(TobiiXR_Settings))]
    public class TobiiXR_SettingsDrawer : PropertyDrawer
    {
        private static bool _scriptsReloaded;

        private int _lineEndings = 26; // Arbitrary number, could be correct. Will count the lines in OnGUI, setting non-zero here to avoid rendering artifacts after compilation.
        private List<TobiiXR_Settings.ProviderElement> _allStandaloneProviders;
        private List<TobiiXR_Settings.ProviderElement> _allAndroidProviders;
        private ReorderableList _standaloneProviderList;
        private ReorderableList _androidProviderList;
        private float _providerListHeight;
        private bool _initialized;
        private readonly Color _redColor = new Color32(255,40,40, 255);

        private void Init(SerializedProperty property)
        {
            if (_initialized == true) return;

            _allStandaloneProviders = EditorUtils.GetAvailableProviders(BuildTargetGroup.Standalone);
            _allAndroidProviders = EditorUtils.GetAvailableProviders(BuildTargetGroup.Android);
            _standaloneProviderList = CreateReorderableList("Standalone Eye Tracking Providers", property, property.FindPropertyRelative("StandaloneEyeTrackingProviders"), _allStandaloneProviders);
            _androidProviderList = CreateReorderableList("Android Eye Tracking Providers", property, property.FindPropertyRelative("AndroidEyeTrackingProviders"), _allAndroidProviders);

            _initialized = true;
        }
        
        [UnityEditor.Callbacks.DidReloadScripts]
        private static void OnScriptsReloaded()
        {
            _scriptsReloaded = true;
        }
        
        private static ReorderableList CreateReorderableList(string title, SerializedProperty property, SerializedProperty listProperty, List<TobiiXR_Settings.ProviderElement> providers)
        {
            var reorderableList = new ReorderableList(listProperty.serializedObject, listProperty);

            reorderableList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var providerTypeName = reorderableList.serializedProperty.GetArrayElementAtIndex(index).FindPropertyRelative("TypeName").stringValue;
                var displayName = AssemblyUtils.GetCachedDisplayNameFor(providerTypeName);
                EditorGUI.LabelField(rect, new GUIContent(displayName));
            };

            reorderableList.drawHeaderCallback = (Rect r) => EditorGUI.LabelField(r, title);

            reorderableList.onAddDropdownCallback = (Rect buttonRect, ReorderableList list) =>
            {
                var menu = new GenericMenu();

                foreach (var provider in providers)
                {
                    if (list.serializedProperty.ArrayContains(element => element.FindPropertyRelative("TypeName").stringValue == provider.TypeName)) continue;

                    menu.AddItem(new GUIContent(provider.DisplayName), false, (object target) =>
                    {
                        var providerElement = (TobiiXR_Settings.ProviderElement)target;

                        var element = list.AddElement();
                        element.FindPropertyRelative("TypeName").stringValue = providerElement.TypeName;
                        property.serializedObject.ApplyModifiedProperties();

                    }, provider);
                }

                menu.ShowAsContext();
            };

            return reorderableList;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return (2 + EditorGUIUtility.singleLineHeight) * _lineEndings + 6 + _providerListHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (_scriptsReloaded)
            {
                _scriptsReloaded = false;
                _initialized = false;
                Init(property);
            }

            if (EditorApplication.isCompiling)
            {
                _initialized = false;
            }
            else if (!_initialized)
            {
                Init(property);
            }

            _lineEndings = 0;

            var layerMask = property.FindPropertyRelative("LayerMask");
            var eyeTrackingFilter = property.FindPropertyRelative("EyeTrackingFilter");
            var advancedEnabled = property.FindPropertyRelative("AdvancedEnabled");
            var licenseProp = property.FindPropertyRelative("LicenseAsset");

            position.height = EditorGUIUtility.singleLineHeight;

            EditorGUI.LabelField(position, "Information", EditorStyles.boldLabel);
            CarriageReturn(ref position);

            EditorGUI.LabelField(position, "Change settings used to initialize Tobii XR. For more info click the button below.");
            CarriageReturn(ref position);

            var buttonPosition = position;
            var buttonContent = new GUIContent("Open website");
            buttonPosition.width = GUI.skin.button.CalcSize(buttonContent).x;

            if (GUI.Button(buttonPosition, buttonContent))
            {
                Application.OpenURL("https://vr.tobii.com/sdk/develop/unity/documentation/tobii-settings/");
            }

            CarriageReturn(ref position, 2);

            EditorGUI.BeginDisabledGroup(Application.isPlaying);

            EditorGUI.PropertyField(position, layerMask, new GUIContent("G2OM Layer Mask", "Choose in which layers G2OM looks for potential candidates."));
            CarriageReturn(ref position);
            
            // ***************************
            // Ocumen
            // ***************************
            EditorGUI.LabelField(position, "", GUI.skin.horizontalSlider);
            CarriageReturn(ref position);
            
            EditorGUI.LabelField(position, new GUIContent("Tobii Ocumen", "This section adds settings for Ocumen usages of the XR SDK."), EditorStyles.boldLabel);
            CarriageReturn(ref position);

            EditorGUI.LabelField(position, "Ocumen features require a license and will only work with the Tobii Provider.");
            CarriageReturn(ref position);
            EditorGUI.LabelField(position, "For more info click the button below.");
            CarriageReturn(ref position);

            buttonPosition = position;
            buttonContent = new GUIContent("Open website");
            buttonPosition.width = GUI.skin.button.CalcSize(buttonContent).x;
            
            if (GUI.Button(buttonPosition, buttonContent))
            {
                Application.OpenURL("https://vr.tobii.com/sdk/solutions/tobii-ocumen/");
            }

            CarriageReturn(ref position);
            CarriageReturn(ref position);
            EditorGUI.PropertyField(position, licenseProp, new GUIContent("License Asset", "Choose license asset to use."));
            CarriageReturn(ref position);
            
            EditorGUI.indentLevel++;

            if (licenseProp.objectReferenceValue != null)
            {
                var licenseAsset = licenseProp.objectReferenceValue as TextAsset;
                var lic = new LicenseParser(Encoding.Unicode.GetString(licenseAsset.bytes));
                if (lic.LicenseIsParsed)
                {
                    var style = new GUIStyle();

                    EditorGUI.LabelField(position, "Licensee:", lic.Licensee);
                    CarriageReturn(ref position);

                    if (lic.ValidTo != null)
                    {
                        var time = (DateTime) lic.ValidTo;
                        EditorGUI.LabelField(position, "Expiry Date:", time.ToString("yyyy-MM-dd"), GetExpiryDateColor(lic.ValidTo, style));
                    }
                    else
                    {
                        EditorGUI.LabelField(position, "Expiry Date:", "No Expiry Date", GetExpiryDateColor(lic.ValidTo, style));
                    }

                    // CarriageReturn(ref position);
                    // var signalsFiltersAvailable = lic.FeatureGroup == FeatureGroup.Professional;
                    // EditorGUI.LabelField(position, "Signals & Filters:", GetLicenseAvailablityString(signalsFiltersAvailable), GetLicenseStyle(signalsFiltersAvailable, style));
                    // CarriageReturn(ref position);
                    //
                    // var configurationAvailable = lic.FeatureGroup >= FeatureGroup.Config;
                    // EditorGUI.LabelField(position, "Configuration:", GetLicenseAvailablityString(configurationAvailable), GetLicenseStyle(configurationAvailable, style));
                    // CarriageReturn(ref position);
                    //
                    // var eyeImagesAvailable = lic.EyeImages;
                    // EditorGUI.LabelField(position, "Eye Images:", GetLicenseAvailablityString(eyeImagesAvailable), GetLicenseStyle(eyeImagesAvailable, style));
                    // CarriageReturn(ref position);

                    advancedEnabled.boolValue = lic.FeatureGroup == FeatureGroup.Professional;

                }
                else
                {
                    var style = new GUIStyle {normal = {textColor = _redColor}};
                    EditorGUI.LabelField(position, "Invalid License", style);
                    advancedEnabled.boolValue = false;
                }
            }
            else
            {
                advancedEnabled.boolValue = false;
            }
            CarriageReturn(ref position);
            EditorGUI.indentLevel--;


            // ***************************
            // Providers
            // ***************************
            EditorGUI.LabelField(position, "", GUI.skin.horizontalSlider);
            CarriageReturn(ref position);
            
            EditorGUI.LabelField(position, new GUIContent("Eye Tracking Providers", "If no provider is initialized successfully TobiiXR will use Nose Direction Provider even if it is removed from this list."), EditorStyles.boldLabel);
            CarriageReturn(ref position);

            if (licenseProp.objectReferenceValue != null) // A license was provided
            {
                EditorGUI.LabelField(position, "Tobii Provider is the only provider that supports licenses.");
                CarriageReturn(ref position);
                EditorGUI.LabelField(position, "The core API can still be used when advanced is enabled.");
                CarriageReturn(ref position);
                EditorGUI.LabelField(position, "Clear the license field if you want to use another provider.");
                CarriageReturn(ref position);
                CarriageReturn(ref position);
            }
            else
            {
                var providerList = EditorUserBuildSettings.selectedBuildTargetGroup == BuildTargetGroup.Android ? _androidProviderList : _standaloneProviderList;
                providerList.DoList(position);
                CarriageReturn(ref position);
                _providerListHeight = providerList.GetHeight();
                position.y += _providerListHeight;
            }

            // ***************************
            // Filter
            // ***************************
            if (eyeTrackingFilter.objectReferenceValue != null && eyeTrackingFilter.objectReferenceValue is EyeTrackingFilterBase == false)
            {
                Debug.LogError("Filter must implement interface EyeTrackingFilterBase.");
                eyeTrackingFilter.objectReferenceValue = null;
            }

            EditorGUI.PropertyField(position, eyeTrackingFilter, new GUIContent("Eye Tracking Filter", "If you want the eye tracking data to be filtered. One example is the Gaze Modifier Filter."));
            CarriageReturn(ref position);

            EditorGUI.EndDisabledGroup();
        }

        private GUIStyle GetExpiryDateColor(DateTime? licValidTo, GUIStyle style)
        {
            if (!licValidTo.HasValue)
            {
                style.normal.textColor = Color.green;
                return style;
            }
            
            var daysLeft = (licValidTo.Value - DateTime.Now).Days;
            if (daysLeft < 0)
            {
                style.normal.textColor = _redColor; 
            }
            else if (daysLeft < 190)
            {
                style.normal.textColor = Color.yellow;
            }
            else
            {
                style.normal.textColor = Color.green;
            }

            return style;
        }

        private GUIStyle GetLicenseStyle(bool licenseAvailable, GUIStyle style)
        {
            style.normal.textColor = licenseAvailable ? Color.green : _redColor;
            return style;
        }
        
        private string GetLicenseAvailablityString(bool licenseAvailable)
        {
            return licenseAvailable ? "Included" : "Not Included";
        }
        
        private void CarriageReturn(ref Rect position, int count = 1)
        {
            for (; count > 0; count--)
            {
                position.y += position.height;
                _lineEndings++;
            }
        }
    }
}
