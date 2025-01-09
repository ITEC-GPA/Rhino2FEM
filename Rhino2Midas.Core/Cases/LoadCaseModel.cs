using Rhino2Midas.Core.Base;

namespace Rhino2Midas.Core.Cases
{
    public class LoadCaseModel : LoadCaseBase
    {
        public enum LoadCaseTypes
        {

        }

        public LoadCaseTypes Type { get; set; }

        public LoadCaseModel(string name, LoadCaseTypes type, string description)
            : base(name, description)
        {
            Type = type;
            Description = description;
        }

        public LoadCaseModel()
            : base()
        {
        }

        public LoadCaseModel(LoadCaseModel loadCaseModel)
            : base(loadCaseModel.Name, loadCaseModel.Description)
        {
            Type = loadCaseModel.Type;
        }
    }
}
