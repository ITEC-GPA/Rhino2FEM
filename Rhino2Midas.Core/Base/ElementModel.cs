using Rhino2Midas.Core.Attributes;
using System.Collections.Generic;

namespace Rhino2Midas.Core.Base
{
    public abstract class ElementModel : ModelObjectId
    {
        public List<ElementGroupModel> Groups { get; set; }

        public ElementModel(int id)
            :base()
        {
            Id = id;
        }

        public ElementModel()
            :base()
        {
            Id = UNASSIGNED;
        }

        public ElementModel(ElementModel elementModel)
            : base(elementModel.Id, elementModel.Name)
        {
        }

        public virtual void Merge(ElementModel other)
        {            
            Groups.AddRange(other.Groups);
        }
    }
}
