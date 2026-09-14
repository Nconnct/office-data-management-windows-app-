using System.Collections.Generic;
using UniversalOfficeDataManager.Helpers;
using UniversalOfficeDataManager.Models;

namespace UniversalOfficeDataManager.ViewModels
{
    /// <summary>
    /// One row of the read-only records grid. Display strings are
    /// precomputed once at load time (rather than converted live in the
    /// binding) since Stage 1's grid is view-only; edits happen through
    /// RecordEditWindow and then trigger a full reload.
    ///
    /// The indexer below lets a DataGrid column bind to "[fieldId]"
    /// directly against a row instance — a standard WPF technique for
    /// grids whose column set is only known at runtime.
    /// </summary>
    public class RecordRowViewModel
    {
        public RecordItem Record { get; }
        private readonly Dictionary<int, string> _displayValues = new();

        public RecordRowViewModel(RecordItem record, IEnumerable<FieldDefinition> fields)
        {
            Record = record;
            foreach (var field in fields)
            {
                Record.Values.TryGetValue(field.Id, out var raw);
                _displayValues[field.Id] = FieldValueFormatter.Format(field, raw);
            }
        }

        public string this[int fieldId] =>
            _displayValues.TryGetValue(fieldId, out var value) ? value : string.Empty;
    }
}
