using System.Collections.Generic;
using System.Runtime.Serialization;
using Rhino2Midas.Core.Base;

namespace Rhino2Midas.Core.Collections
{
    public class UniqueNameCollection<T> : Dictionary<string, T> where T : ModelObjectId
    {
        public UniqueNameCollection()
        {
        }

        public UniqueNameCollection(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }

        public void Add(T item)
        {
            if(ContainsKey(item.Name))
                this[item.Name] = item;
            else
                Add(item.Name, item);
        }

        public bool AddRange(IEnumerable<T> items)
        {
            if (items != null)
            {
                foreach (var item in items)
                {
                    Add(item);
                }
                return true;
            }
            return false;
        }
    }
}
