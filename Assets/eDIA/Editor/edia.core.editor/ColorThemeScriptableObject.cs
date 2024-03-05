using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "ColorTheme", menuName = "eDIA/ColorTheme")]
public class ColorThemeScriptableObject : ScriptableObject
{
	[Header("[ Controller ----------------------- ]")]
	public Color Panels;
	public Color Progressbars;

	[Header("Buttons")]
	public ColorBlock ControllerButton = ColorBlock.defaultColorBlock;

	[Header("[ Executer ----------------------- ]")]

	[Header("Buttons")]
	public ColorBlock ExecuterButton = ColorBlock.defaultColorBlock;

	[Space(20)]
	public Color Slider;
	public Color PanelsBackground;

}
