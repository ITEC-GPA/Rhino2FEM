using Rhino2Fem.Core.Base;

namespace Rhino2Fem.Core.Attributes
{
    public class LoadGroupModel : ModelObjectId
    {
        public string Suffix { get; set; }

        public LoadGroupModel(string name, string suffix = "")
            : base(name)
        {
            Suffix = suffix;
        }

        public LoadGroupModel()
            : base()
        {
        }

        public LoadGroupModel(ElementGroupModel elementGroupModel)
            : base(elementGroupModel.Name)
        {
        }
    }
}
