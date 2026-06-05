using Leosac.KeyManager.Library.UI.Domain;

namespace Leosac.KeyManager.Library.UI
{
    public static class KeyEntryDeletionHandler
    {
        public static void Handle(KeyEntriesControlViewModel? vm, object parameter)
        {
            if (vm?.DeleteKeyEntryCommand == null)
                return;

            if (parameter is SelectableKeyEntryId id)
            {
                vm.DeleteKeyEntryCommand.ExecuteAsync(id);
            }
            else if (parameter is IList<SelectableKeyEntryId> list)
            {
                var snapshot = list.Where(x => x.Selected).ToList();
                foreach (var idItem in snapshot)
                    vm.DeleteKeyEntryCommand.ExecuteAsync(idItem);
            }
        }
    }
}
