using Rhino2Midas.Core.Base;

namespace Rhino2Midas.Core.Attributes
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
