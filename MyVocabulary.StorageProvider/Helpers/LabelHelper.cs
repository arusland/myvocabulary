using System.Collections.Generic;
using System.Linq;

namespace MyVocabulary.StorageProvider.Helpers
{
    public static class LabelHelper
    {
        #region Methods
        
        #region Public

        public static IList<WordLabel> JoinLabels(IList<WordLabel> labels, IList<WordLabel> labelsNew, IEnumerable<WordLabel> allLabels)
        {
            var result = new List<WordLabel>();
            result.AddRange(labels);

            foreach (var labelNew in labelsNew)
            {
                if (WordLabel.LabelToRemove != labelNew && !allLabels.Contains(labelNew))
                {
                    continue;
                }

                if (labels.Any(p => p.Id == labelNew.Id))
                {
                    continue;
                }

                result.Add(labelNew);
            }

            return result;
        }        
        
        #endregion
        
        #endregion
    }
}
