//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ENTITIES
{
    using System;
    using System.Collections.Generic;
    
    public partial class ActivityExpenseType
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ActivityExpenseType()
        {
            this.ActivityExpenseDetails = new HashSet<ActivityExpenseDetail>();
        }
    
        public int expense_type_id { get; set; }
        public string expense_type_name { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ActivityExpenseDetail> ActivityExpenseDetails { get; set; }
    }
}
