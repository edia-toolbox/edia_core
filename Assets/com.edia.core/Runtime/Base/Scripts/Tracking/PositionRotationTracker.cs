using UnityEngine;
using System.Collections.Generic;
using Edia.Data;

namespace Edia.Tracking {

    /// <summary>
    /// Tracks position and rotation of the attached GameObject each frame.
    /// </summary>
    public class PositionRotationTracker : Tracker {

        public override string MeasurementDescriptor => "movement";
        public override IEnumerable<string> CustomHeader => new string[] { "pos_x", "pos_y", "pos_z", "rot_x", "rot_y", "rot_z" };

        protected override DataRow GetCurrentValues() {
            Vector3 p = gameObject.transform.position;
            Vector3 r = gameObject.transform.eulerAngles;

            var values = new DataRow() {
                ("pos_x", p.x),
                ("pos_y", p.y),
                ("pos_z", p.z),
                ("rot_x", r.x),
                ("rot_y", r.y),
                ("rot_z", r.z)
            };

            return values;
        }
    }
}
