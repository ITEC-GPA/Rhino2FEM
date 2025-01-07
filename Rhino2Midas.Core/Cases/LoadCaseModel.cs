namespace Rhino2Midas.Core.Cases
{
    public class LoadCaseModel
    {
        public enum LoadCaseTypes
        {

        }

        public string Name { get; set; }

        public LoadCaseTypes Type { get; set; }

        public string Description { get; set; }

        public LoadCaseModel(string name, LoadCaseTypes type, string description)
        {
            Name = name;
            Type = type;
            Description = description;
        }

        public LoadCaseModel()
        {
        }
    }
}
