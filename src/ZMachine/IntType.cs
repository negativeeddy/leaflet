using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZMachine
{
    class IntType
    {
        private int _value;
        public IntType(int value)
        {
            _value = value;
        }

        public static explicit operator IntType(int value) 
        {
            return new IntType(value);  // explicit conversion
        }

        public static explicit operator int(IntType value) 
        {
            return value._value;  // explicit conversion
        }
    }

    class ZAddress : IntType
    {
        public ZAddress(int value) : base(value) { }
    }
}
