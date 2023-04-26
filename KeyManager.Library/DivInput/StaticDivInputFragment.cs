namespace Leosac.KeyManager.Library.DivInput
{
    public class StaticDivInputFragment : DivInputFragment
    {
        public StaticDivInputFragment()
        {
            _input = string.Empty;
        }

        private string _input;

        public string Input
        {
            get => _input;
            set => SetProperty(ref _input, value);
        }

        public override string GetFragment(DivInputContext context)
        {
            return Input;
        }
    }
}
