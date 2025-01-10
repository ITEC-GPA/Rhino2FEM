using System.Collections.Generic;
using Rhino2Midas.Core.Base;
using Rhino2Midas.Core.Elements;

namespace Rhino2Midas.Core.Attributes
{
    public class ElementGroupModel : ModelObjectId
    {
        public List<NodeElementModel> NodeElementList { get; set; }

        public List<FrameElementModel> FrameElementList { get; set; }

        public List<AreaElementModel> AreaElementList { get; set; }

        public ElementGroupModel(string name)
            : base(name)
        {
        }

        public ElementGroupModel()
            : base()
        {
        }

        public ElementGroupModel(ElementGroupModel elementGroupModel)
            : base(elementGroupModel.Name)
        {
        }
    }
}
