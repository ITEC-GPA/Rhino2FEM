using Rhino2Fem.Core.Base;

namespace Rhino2Fem.Core.Attributes
{
    public class BoundaryGroupModel : ModelObjectId
    {
        public BoundaryGroupModel(string name)
            : base(name)
        { 
        }

        public BoundaryGroupModel()
            : base()
        {
        }

        public BoundaryGroupModel(ElementGroupModel elementGroupModel)
            : base(elementGroupModel.Name)
        {
        }
    }
}
