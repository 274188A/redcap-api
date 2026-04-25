using Redcap.Models;

namespace Redcap.Interfaces
{
    public partial interface IRedcap
    {
        /// <summary>
        /// Export List of Export Field Names (i.e. variables used during exports and imports)<br/><br/>
        ///
        /// This method returns a list of the export/import-specific version of field names for all fields (or for one field, if desired) in a project.
        /// This is mostly used for checkbox fields because during data exports and data imports, checkbox fields have a different variable name used than the exact one defined for them in the Online Designer and Data Dictionary, in which *each checkbox option* gets represented as its own export field name in the following format: field_name + triple underscore + converted coded value for the choice.
        /// For non-checkbox fields, the export field name will be exactly the same as the original field name.
        /// Note: The following field types will be automatically removed from the list returned by this method since they cannot be utilized during the data import process: 'calc', 'file', and 'descriptive'.
        /// <br/><br/>
        /// The list that is returned will contain the three following attributes for each field/choice: 'original_field_name', 'choice_value', and 'export_field_name'.
        /// The choice_value attribute represents the raw coded value for a checkbox choice.For non-checkbox fields, the choice_value attribute will always be blank/empty.
        /// The export_field_name attribute represents the export/import-specific version of that field name.
        /// </summary>
        /// <remarks>
        /// To use this method, you must have API Export privileges in the project.
        /// </remarks>
        /// <param name="format">csv, json [default], xml</param>
        /// <param name="field">A field's variable name. By default, all fields are returned, but if field is provided, then it will only the export field name(s) for that field. If the field name provided is invalid, it will return an error.</param>
        /// <param name="returnFormat">csv, json [default], xml - specifies the format of error messages. If you do not pass in this flag, it will select the default format for you passed based on the 'format' flag you passed in or if no format flag was passed in, it will default to 'json'.
        /// The list that is returned will contain the original field name (variable) of the field and also the export field name(s) of that field.</param>
        /// <param name="cancellationToken"></param>
        /// <param name="timeOutSeconds">Number of seconds before the http request times out.</param>
        /// <returns>Returns a list of the export/import-specific version of field names for all fields (or for one field, if desired) in a project in the format specified and ordered by their field order .
        /// The list that is returned will contain the three following attributes for each field/choice: 'original_field_name', 'choice_value', and 'export_field_name'. The choice_value attribute represents the raw coded value for a checkbox choice. For non-checkbox fields, the choice_value attribute will always be blank/empty. The export_field_name attribute represents the export/import-specific version of that field name.
        /// </returns>
        Task<string> ExportFieldNamesAsync(RedcapFormat format = RedcapFormat.json, string? field = default, RedcapReturnFormat returnFormat = RedcapReturnFormat.json, CancellationToken cancellationToken = default, long timeOutSeconds = 100);
    }
}
