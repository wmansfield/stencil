using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Stencil.Primary.Health.Daemons
{
    /// <summary>
    /// Externally written
    /// </summary>
    public class HostedGraphiteTcpClient : IDisposable
    {
        public HostedGraphiteTcpClient(string apikey = null, string hostname = "cd645d67.carbon.hostedgraphite.com", int port = 2003)
        {
            KeyPrefix = apikey;
            this.TcpClient = new TcpClient(hostname, port);
        }
        public string KeyPrefix { get; private set; }

        public TcpClient TcpClient { get; private set; }

        public void SendMany(List<string> rawValues)
        {
            try
            {
                StringBuilder builder = new StringBuilder();
                foreach (var item in rawValues)
                {
                    builder.Append(string.Format("{0}.{1}\n", this.KeyPrefix, item));
                }

                byte[] message = Encoding.UTF8.GetBytes(builder.ToString());
                this.TcpClient.GetStream().Write(message, 0, message.Length);
            }
            catch
            {
                // Supress all exceptions for now.
            }
        }

        #region IDisposable
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;
            if (this.TcpClient != null)
            {
                this.TcpClient.Close();
            }
        }
        #endregion
    }
}
