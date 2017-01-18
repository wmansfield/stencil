using System;
using Java.Lang;

namespace Stencil.Native.Droid
{
    public interface INamePageAdapter
    {
        int Count
        {
            get;
        }
        ICharSequence GetNameFormatted(int index);
    }
}

