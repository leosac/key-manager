using System.Windows.Input;

namespace Leosac.KeyManager.Library.UI.Helpers
{
    public sealed class WaitCursor : IDisposable
    {
        private readonly Cursor? _previousCursor;

        public WaitCursor()
        {
            _previousCursor = Mouse.OverrideCursor;
            Mouse.OverrideCursor = Cursors.Wait;
        }

        public void Dispose()
        {
            Mouse.OverrideCursor = _previousCursor;
        }
    }
}