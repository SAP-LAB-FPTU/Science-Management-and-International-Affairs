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
    
    public partial class ActivityOffice
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ActivityOffice()
        {
            this.ActivityExpenseCategories = new HashSet<ActivityExpenseCategory>();
        }
    
        public int activity_office_id { get; set; }
        public Nullable<int> activity_id { get; set; }
        public Nullable<int> office_id { get; set; }
    
        public virtual Office Office { get; set; }
        public virtual AcademicActivity AcademicActivity { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ActivityExpenseCategory> ActivityExpenseCategories { get; set; }
    }
}
