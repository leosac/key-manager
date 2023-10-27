namespace Leosac.KeyManager.Library.DivInput
{
    public class StaticDivInputFragment : DivInputFragment
    {
        public StaticDivInputFragment()
        {
            _input = string.Empty;
        }

        public override string Name => "Static";

        private string _input;
        public string Input
        {
            get => _input;
            set => SetProperty(ref _input, value);
        }

        public override object Clone()
        {
            return new StaticDivInputFragment
            {
                Input = Input
            };
        }

        public override string GetFragment(DivInputContext context)
        {
            return Input;
        }
    }
}
