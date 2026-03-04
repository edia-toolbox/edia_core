using System.Collections.Generic;
using System.Linq;
using Edia.Data;

namespace Edia {

    /// <summary>
    /// A set of trials, often used to group consecutive Trial objects that share something in common.
    /// </summary>
    public class Block {

        public List<Trial> trials = new List<Trial>();

        public Trial firstTrial { get { return trials.Count > 0 ? trials[0] : null; } }
        public Trial lastTrial { get { return trials.Count > 0 ? trials[trials.Count - 1] : null; } }

        /// <summary>
        /// Returns the block number (1-based) based on its position in the experiment's block list.
        /// </summary>
        public int number { get { return Experiment.Instance.blocks.IndexOf(this) + 1; } }

        /// <summary>
        /// Block settings. Cascades up to experiment settings.
        /// </summary>
        public Settings settings { get; protected set; }

        public Block(uint numberOfTrials) {
            settings = Settings.empty;
            Experiment.Instance.blocks.Add(this);
            settings.SetParent(Experiment.Instance.settings);
            for (int i = 0; i < numberOfTrials; i++) {
                var t = new Trial(this);
                trials.Add(t);
            }
        }

        /// <summary>
        /// Create a trial within this block.
        /// </summary>
        public Trial CreateTrial() {
            var t = new Trial(this);
            trials.Add(t);
            return t;
        }

        /// <summary>
        /// Get a trial in this block by relative trial number (non-zero indexed).
        /// </summary>
        public Trial GetRelativeTrial(int relativeTrialNumber) {
            return trials[relativeTrialNumber - 1];
        }
    }
}
