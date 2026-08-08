using UnityEngine;
using UnityEngine.UI;

namespace Nobi.UiRoundedCorners {
	[ExecuteInEditMode]                             //Required to check the OnEnable function
	[DisallowMultipleComponent]                     //You can only have one of these in every object.
	[RequireComponent(typeof(RectTransform))]
	public class ImageWithRoundedCorners : MonoBehaviour {
		private static readonly int Props = Shader.PropertyToID("_WidthHeightRadius");
		private static readonly int prop_OuterUV = Shader.PropertyToID("_OuterUV");

		public float radius = 40f;
		private Material material;
		private Vector4 outerUV = new Vector4(0, 0, 1, 1);

		[HideInInspector, SerializeField] private MaskableGraphic image;

		private void OnValidate() {
			Validate();
			Refresh();
		}

		private void OnDestroy() {
			if (image != null) {
				image.material = null;      //This makes so that when the component is removed, the UI material returns to null
			}

			DestroyHelper.Destroy(material);
			image = null;
			material = null;
		}

		private void OnEnable() {
			//You can only add either ImageWithRoundedCorners or ImageWithIndependentRoundedCorners
			//It will replace the other component when added into the object.

			/*
			 * Commented out this section as the `ImageWithIndependentRoundedCorners` part of the package is removed in Edia.
			 */
			// var other = GetComponent<ImageWithIndependentRoundedCorners>();
			// if (other != null) {
			// 	radius = other.r.x;                 //When it does, transfer the radius value to this script
			// 	DestroyHelper.Destroy(other);
			// }

			Validate();
			Refresh();
		}

		private void OnRectTransformDimensionsChange() {
			if (enabled && material != null) {
				Refresh();
			}
		}

		public void Validate() {
			if (material == null) {
				/*
				 * EDIA patch: guard against Shader.Find returning null.
				 * Shader.Find cannot resolve project/package shaders while an asset is being imported, and
				 * [ExecuteInEditMode] makes OnValidate run in exactly that context for every prefab shipped
				 * inside the EDIA package. Constructing a Material from the null shader threw
				 * "ArgumentNullException: ... Parameter name: shader" once per component on a fresh install.
				 * Bailing out here is safe: OnEnable/OnValidate run again outside import context, where the
				 * shader does resolve and the material is created as usual.
				 */
				var shader = Shader.Find("UI/RoundedCorners/RoundedCorners");
				if (shader == null) {
					return;
				}

				material = new Material(shader);
				// material = Resources.Load<Material>("RoundedCorners");
			}

			if (image == null) {
				TryGetComponent(out image);
			}

			if (image != null) {
				image.material = material;
			}

			if (image is Image uiImage && uiImage.sprite != null) {
				outerUV = UnityEngine.Sprites.DataUtility.GetOuterUV(uiImage.sprite);
			}
		}

		public void Refresh() {
			// EDIA patch: Validate() can leave the material null when the shader is not resolvable yet
			// (see the note there); without this guard that turns the old exception into a NullReferenceException.
			if (material == null) {
				return;
			}

			var rect = ((RectTransform)transform).rect;

			//Multiply radius value by 2 to make the radius value appear consistent with ImageWithIndependentRoundedCorners script.
			//Right now, the ImageWithIndependentRoundedCorners appears to have double the radius than this.
			material.SetVector(Props, new Vector4(rect.width, rect.height, radius * 2, 0));
			material.SetVector(prop_OuterUV, outerUV);
		}
	}
}