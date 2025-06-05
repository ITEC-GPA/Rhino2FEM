using System.Collections.Generic;
using Rhino2Fem.Core.Base;
using Rhino2Fem.Core.Elements;

namespace Rhino2Fem.Core.Attributes
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
