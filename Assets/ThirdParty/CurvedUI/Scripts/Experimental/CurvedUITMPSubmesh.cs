using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if CURVEDUI_TMP || TMP_PRESENT
using TMPro;
#endif

namespace CurvedUI
{
    [ExecuteInEditMode]
    public class CurvedUITMPSubmesh : MonoBehaviour
    {
#if CURVEDUI_TMP || TMP_PRESENT

        //saved references
        private VertexHelper _vh;
        private Mesh _straightMesh;
        private Mesh _curvedMesh;
        private CurvedUIVertexEffect _crvdVe;
        private TMP_SubMeshUI _tmPsub;
        private TextMeshProUGUI _tmPtext;

        public void UpdateSubmesh(bool tesselate, bool curve)
        {
            //find required components
            if (_tmPsub == null) _tmPsub = gameObject.GetComponent<TMP_SubMeshUI>();

            if (_tmPsub == null) return;

            if (_tmPtext == null) _tmPtext = GetComponentInParent<TextMeshProUGUI>();

            if (_crvdVe == null) _crvdVe = gameObject.AddComponentIfMissing<CurvedUIVertexEffect>();
            
            if (_tmPsub.materialForRendering == null && _tmPsub.fallbackMaterial == null) return;


            //perform tesselation so we have more vertices to work with during curving
            if (tesselate || _straightMesh == null || _vh == null || !Application.isPlaying)
            {
                _vh = new VertexHelper(_tmPsub.mesh);

                //save straight mesh - it will be curved then every time the object moves on the canvas.
                _straightMesh = new Mesh();
                _vh.FillMesh(_straightMesh);

                curve = true;
            }

            //curve the mesh
            if (curve)
            {
                _vh = new VertexHelper(_straightMesh);
                _crvdVe.ModifyMesh(_vh);
                _curvedMesh = new Mesh();
                _vh.FillMesh(_curvedMesh);
                _crvdVe.CurvingRequired = true;
                _crvdVe.TesselationRequired = true;
            }

            //upload mesh to TMP object's renderer
            _tmPsub.canvasRenderer.SetMesh(_curvedMesh);
            
            //cleanup for not needed submeshes.
            if (_tmPtext != null && _tmPtext.textInfo.materialCount < 2)
            {
                //Each submesh uses 1 additional material.
                //If materialCount is 1, this means Submesh is not needed. Bounce it to toggle cleanup.
                _tmPsub.enabled = false;
                _tmPsub.enabled = true;
            }
        }

#endif
    }
}


