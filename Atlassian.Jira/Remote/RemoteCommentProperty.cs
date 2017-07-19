using System.Collections.Generic;

namespace Atlassian.Jira.Remote
{
    /// <summary>
    /// This class will hold properties that live on a comment. Although it initially appears as if the object would be a KeyValuePair, 
    /// the web service send back lower case values, which would not map correctly to a KeyValuePair
    /// </summary>
    public class RemoteCommentProperty
    {
        private string keyField;
        private object valueField;

        public string key
        {
            get { return this.keyField; }
            set { this.keyField = value; }
        }

        //I would like to be able to create a type for this, however, it is unclear how many different types can come across here. For now therefore
        //this will need to be an object and the consumer will need to handle reading/parsing it for their environment
        public object value
        {
            get { return this.valueField; }
            set { this.valueField = value; }
        }

        public KeyValuePair<string, object> ToKeyValuePair()
        {
            return new KeyValuePair<string, object>(this.key, this.value);
        }
    }
}
