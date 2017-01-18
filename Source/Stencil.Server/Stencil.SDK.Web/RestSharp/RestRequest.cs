using System.Collections.Generic;

namespace RestSharp
{
    public class RestRequest
    {
        public RestRequest(Method method)
        {
            this.Method = method;
            this.Parameters = new Dictionary<string, object>();
            this.Segments = new Dictionary<string, string>();
            this.ContentType = "application/json";
        }

        public Method Method;
        public string Resource;
        public object JsonBody;
        public Dictionary<string, object> Parameters;
        public Dictionary<string, string> Segments;
        public string ContentType;

        public void AddJsonBody(object body)
        {
            this.JsonBody = body;
        }
        public void AddParameter(string name, object value)
        {
            this.Parameters[name] = value;
        }
        public void AddFile(string name, object value, string fileName)
        {
            this.ContentType = "multipart/form-data";
            this.Parameters[name] = value;
        }

        public void AddUrlSegment(string name, string value)
        {
            this.Segments[name] = value;
        }
        public string GetResource()
        {
            string result = this.Resource;
            foreach (var item in this.Segments)
            {
                result = result.Replace("{" + item.Key + "}", item.Value);
            }
            return result;
        }
        public object GetData()
        {
            if (this.JsonBody != null)
            {
                return this.JsonBody;
            }
            return this.Parameters;
        }
    }
}
