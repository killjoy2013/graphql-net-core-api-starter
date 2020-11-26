using System;

namespace Helpers
{
    public class OrderAttribute : Attribute
    {
        public int I { get; }

        public OrderAttribute(int i)
        {
            I = i;
        }
    }

}
