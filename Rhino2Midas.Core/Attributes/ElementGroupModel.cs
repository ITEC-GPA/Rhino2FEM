using Rhino2Fem.Core.Base;
using Rhino2Fem.Core.Elements;
using System;
using System.Collections.Generic;

namespace Rhino2Fem.Core.Attributes
{
    public class ElementGroupModel : ModelObjectId, IEquatable<ElementGroupModel>
    {
        public const string _separator = "::";

        public List<NodeElementModel> NodeElementList { get; set; }

        public List<FrameElementModel> FrameElementList { get; set; }

        public List<AreaElementModel> AreaElementList { get; set; }

        public ElementGroupModel GroupParent { get; set; }

        public bool HasGroupParent => GroupParent != null;

        public string GroupFullName => GetFullName();

        public ElementGroupModel(string name, ElementGroupModel parent = null)
            : base(name)
        {
        }

        public ElementGroupModel()
            : base()
        {
        }

        public ElementGroupModel(ElementGroupModel elementGroupModel)
            : this(elementGroupModel.Name, elementGroupModel.GroupParent)
        {
        }

        private string GetFullName()
        {
            if (HasGroupParent)
                return GroupParent.GetFullName() + _separator + Name;
            else
                return Name;
        }

        public static ElementGroupModel ConvertStringToGroup(string groupName)
        {
            if (string.IsNullOrEmpty(groupName))
                return null;
            string[] names = groupName.Split(new[] { _separator }, StringSplitOptions.RemoveEmptyEntries);
            ElementGroupModel group = new ElementGroupModel(names[names.Length - 1]);
            for (int i = names.Length - 2; i >= 0; i--)
            {
                group = new ElementGroupModel(names[i]) { GroupParent = group };
            }
            return group;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ElementGroupModel);
        }

        public bool Equals(ElementGroupModel other)
        {
            return !(other is null) &&
                   base.Equals(other) &&
                   (!HasGroupParent || EqualityComparer<ElementGroupModel>.Default.Equals(GroupParent, other.GroupParent));
        }

        public override int GetHashCode()
        {
            int hashCode = -397375110;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            if (HasGroupParent)
                hashCode = hashCode * -1521134295 + EqualityComparer<ElementGroupModel>.Default.GetHashCode(GroupParent);
            return hashCode;
        }

        public static bool operator ==(ElementGroupModel left, ElementGroupModel right)
        {
            return EqualityComparer<ElementGroupModel>.Default.Equals(left, right);
        }

        public static bool operator !=(ElementGroupModel left, ElementGroupModel right)
        {
            return !(left == right);
        }
    }
}
