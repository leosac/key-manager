using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leosac.KeyManager.Library.UI
{
    public static class SnackbarHelper
    {
        public static void EnqueueError(ISnackbarMessageQueue queue, string message)
        {
            EnqueueError(queue, null, message);
        }

        public static void EnqueueError(ISnackbarMessageQueue queue, Exception? ex, string? message = null)
        {
            if (ex != null)
            {
                if (message == null) message = ex.Message;
                else message += ": " + ex.Message;
            }

            if (string.IsNullOrEmpty(message)) message = "An error occured.";

            EnqueueMessage(queue, message);
        }

        public static void EnqueueMessage(ISnackbarMessageQueue queue, string message)
        {
            queue?.Enqueue(message, new PackIcon { Kind = PackIconKind.CloseBold }, (object? p) => { }, null, false, true, TimeSpan.FromSeconds(5));
        }
    }
}
