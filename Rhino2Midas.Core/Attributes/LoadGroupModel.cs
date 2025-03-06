using Rhino2Midas.Core.Base;

namespace Rhino2Midas.Core.Attributes
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
