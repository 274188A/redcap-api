using System.ComponentModel.DataAnnotations;

namespace Redcap.Models
{
    /// <summary>
    /// API Action => Export, Import, Delete 
    /// </summary>
    public enum RedcapAction
    {
        /// <summary>
        /// Export Action
        /// </summary>
        /// 
        [Display(Name = "export")] Export,

        /// <summary>
        /// Import Action
        /// </summary>
        /// 
        [Display(Name = "import")] Import,

        /// <summary>
        /// Delete Action
        /// </summary>
        /// 
        [Display(Name = "delete")] Delete,
    }
}