using System.ComponentModel.DataAnnotations;

namespace Leosac.KeyManager.Library.DivInput
{
    public class PaddingDivInputFragment : DivInputFragment
    {
        public PaddingDivInputFragment()
        {
            _padLength = 1;
        }

        public override string Name => "Padding";

        private int _padLength;
        public int PadLength
        {
            get => _padLength;
            set => SetProperty(ref _padLength, value);
        }

        private byte _padByte;
        public byte PadByte
        {
            get => _padByte;
            set => SetProperty(ref _padByte, value);
        }

        public override object Clone()
        {
            return new PaddingDivInputFragment
            {
                PadByte = PadByte,
                PadLength = PadLength
            };
        }

        public override string GetFragment(DivInputContext context)
        {
            int length = (context.CurrentDivInput?.Length).GetValueOrDefault(0);
            if (PadByte <= length)
            {
                return string.Empty;
            }

            var b = new byte[PadByte - length];
            for (int i = 0; i < b.Length; ++i)
            {
                b[i] = PadByte;
            }
            return Convert.ToHexString(b);
        }
    }
}
