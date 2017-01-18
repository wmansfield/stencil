using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Stencil.SDK.Models
{
    public partial class SDKModel
    {
        public Dictionary<string, string> Tag { get; set; }

        public void TagClear()
        {
            if (this.Tag != null)
            {
                this.Tag.Clear();
            }
        }
        public bool TagExists(string key)
        {
            return this.Tag != null && this.Tag.ContainsKey(key);
        }
        public bool TagRemove(string key)
        {
            return this.Tag.Remove(key);
        }
        public string TagGet(string key, string defaultValue)
        {
            if (this.Tag != null && this.Tag.ContainsKey(key))
            {
                return this.Tag[key];
            }
            return defaultValue;
        }
        public int TagGetAsInt(string key, int defaultValue)
        {
            if (this.Tag != null && this.Tag.ContainsKey(key) && !string.IsNullOrEmpty(this.Tag[key]))
            {
                int result = 0;
                if (int.TryParse(this.Tag[key], out result))
                {
                    return result;
                }
            }
            return defaultValue;
        }
        public bool TagGetAsBool(string key, bool defaultValue)
        {
            if (this.Tag != null && this.Tag.ContainsKey(key) && !string.IsNullOrEmpty(this.Tag[key]))
            {
                bool result = false;
                if (bool.TryParse(this.Tag[key], out result))
                {
                    return result;
                }
            }
            return defaultValue;
        }
        public void TagSet(string key, bool value)
        {
            if (this.Tag == null)
            {
                this.Tag = new Dictionary<string, string>();
            }
            this.Tag[key] = value.ToString();
        }
        public void TagSet(string key, string value)
        {
            if (this.Tag == null)
            {
                this.Tag = new Dictionary<string, string>();
            }
            this.Tag[key] = value;
        }
    }
}

