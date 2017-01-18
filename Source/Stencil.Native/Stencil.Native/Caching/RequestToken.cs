using System;
using System.Collections.Generic;
using System.Text;

namespace Stencil.Native.Caching
{
    public struct RequestToken
    {
        public static RequestToken Empty = new RequestToken();
        public RequestToken(string parameter)
        {
            this.Token = Guid.NewGuid();
            this.Parameter = parameter;
        }
        public RequestToken(Guid token, string parameter)
        {
            this.Token = token;
            this.Parameter = parameter;
        }
        public Guid Token { get; set; }
        public string Parameter { get; set; }

        public override bool Equals(Object obj)
        {
            return obj is RequestToken && this == (RequestToken)obj;
        }
        public override int GetHashCode()
        {
            if (this.Parameter == null)
            {
                return Token.GetHashCode();
            }
            return Token.GetHashCode() ^ Parameter.GetHashCode();
        }
        public static bool operator ==(RequestToken left, RequestToken right)
        {
            return left.Token == right.Token && left.Parameter == right.Parameter;
        }
        public static bool operator !=(RequestToken left, RequestToken right)
        {
            return !(left == right);
        }
    }
}
