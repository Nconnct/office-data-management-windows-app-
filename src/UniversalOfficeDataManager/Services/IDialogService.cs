using System.Collections.Generic;
using UniversalOfficeDataManager.Models;
using UniversalOfficeDataManager.Repositories;

namespace UniversalOfficeDataManager.Services
{
    /// <summary>
    /// Abstracts modal-window/message-box calls away from the ViewModels
    /// so they don't take a hard dependency on WPF Window types — keeps
    /// the ViewModels straightforward to unit test once Stage 8's test
    /// suite is added.
    /// </summary>
    public interface IDialogService
    {
        bool? ShowWorkspaceEditor(Workspace workspace);
        bool? ShowSheetEditor(Sheet sheet);
        bool? ShowFieldManager(Sheet sheet, IFieldRepository fieldRepository);
        bool? ShowRecordEditor(List<FieldDefinition> fields, RecordItem record);
        bool ConfirmDelete(string message);
        void ShowError(string message);
    }
}
