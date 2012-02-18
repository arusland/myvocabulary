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
                if (!allLabels.Contains(labelNew))
                {
                    break;
                }

                if (labels.Any(p => p.Id == labelNew.Id))
                {
                    break;
                }

                result.Add(labelNew);
            }

            return result;
        }        
        
        #endregion
        
        #endregion
    }
}
