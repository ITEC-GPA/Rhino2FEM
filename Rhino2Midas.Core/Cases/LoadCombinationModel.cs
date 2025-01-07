using System.Collections.Generic;

namespace Rhino2Midas.Core.Cases
{
    public class LoadCombinationModel
    {
        public enum LoadCombinationTypes
        {
            Linear,
            Envelope,
        }

        public string Name { get; set; }

        public List<LoadFactorModel> LoadFactorList { get; set; }

        public LoadCombinationTypes Type { get; set; }

        public string Description { get; set; }

        public LoadCombinationModel(string name, List<LoadFactorModel> loadFactorList, LoadCombinationTypes type, string description)
        {
            Name = name;
            LoadFactorList = loadFactorList;
            Type = type;
            Description = description;
        }

        public LoadCombinationModel()
        {
        }
    }
}
