using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stencil.Primary.Emaling
{
    public enum EmailFormat
    {
        /// <summary>
        /// %recipient_email%, %recipient_name%, %sender_email%, %sender_name%, %subject%, %body%
        /// </summary>
        Generic,
        /// <summary>
        /// Email with password reset information
        /// %recipient_email%, %recipient_name%, %token%
        /// </summary>
        PasswordResetInitiated,
        /// <summary>
        /// Email notifying that password was reset
        /// %recipient_email%, %recipient_name%, %sender_email%, %sender_name%
        /// </summary>
        PasswordResetCompleted,
    }
}
