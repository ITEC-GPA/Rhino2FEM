using Rhino2Midas.Core.Base;

namespace Rhino2Midas.Core.Attributes
{
    public class ElementGroupModel : ModelObjectId
    {
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
