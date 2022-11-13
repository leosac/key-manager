using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leosac.KeyManager.Library.DivInput
{
    public class RandomDivInputFragment : DivInputFragment
    {
        public RandomDivInputFragment()
        {
            _byteLength = 1;
        }

        private int _byteLength;

        public int ByteLength
        {
            get => _byteLength;
            set => SetProperty(ref _byteLength, value);
        }

        public override string GetFragment(DivInputContext context)
        {
            var rnd = new Random();
            var b = new byte[ByteLength];
            rnd.NextBytes(b);
            return Convert.ToHexString(b);
        }
    }
}
