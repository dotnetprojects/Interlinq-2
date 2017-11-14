using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace InterLinq.Types.Anonymous
{
    /// <summary>
    /// Represents an instance of an <see cref="AnonymousMetaType"/>.
    /// </summary>
#if !SILVERLIGHT
    [Serializable]
#endif
    [DataContract(Namespace="http://schemas.interlinq.com/2011/03/")]
    public class AnonymousObject : DynamicObject
    {

        #region Properties

        /// <summary>
        /// The properties of the instance.
        /// </summary>
        [DataMember]
        public List<AnonymousProperty> Properties { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Instance a new instance of the class <see cref="AnonymousObject"/>.
        /// </summary>
        public AnonymousObject()
        {
            Properties = new List<AnonymousProperty>();
        }

        /// <summary>
        /// Instance a new instance of the class <see cref="AnonymousObject"/>
        /// and initialze it with a list of properties.
        /// </summary>
        /// <param name="properties"><see cref="AnonymousProperty">Anonymous properties</see> to add.</param>
        public AnonymousObject(IEnumerable<AnonymousProperty> properties)
        {
            Properties = new List<AnonymousProperty>();
            Properties.AddRange(properties);
        }

        #endregion

        #region Methods

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return Properties.Select(x => x.Name).ToList();
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            var prp = this.Properties.FirstOrDefault(x => x.Name == binder.Name);

            if (prp != null)
            {
                result = prp.Value;
                return true;
            }

            result = null;
            return false;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            var prp = this.Properties.FirstOrDefault(x => x.Name == binder.Name);

            if (prp != null)
            {
                prp.Value = value;
            }
            else
            {
                this.Properties.Add(new AnonymousProperty(binder.Name, value));
            }
            return true;
        }

        /// <summary>
        /// Returns a string representing this <see cref="AnonymousObject"/>.
        /// </summary>
        /// <returns>Returns a string representing this <see cref="AnonymousObject"/>.</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{ ");
            bool first = true;
            foreach (AnonymousProperty property in Properties)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    sb.Append(", ");
                }
                sb.Append(property.ToString());
            }
            sb.Append(" }");
            return sb.ToString();
        }

        #endregion

    }
}
