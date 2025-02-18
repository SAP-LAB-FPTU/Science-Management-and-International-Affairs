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
    
    public partial class Invention
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Invention()
        {
            this.AuthorInventions = new HashSet<AuthorInvention>();
            this.InventionCountries = new HashSet<InventionCountry>();
            this.RequestInventions = new HashSet<RequestInvention>();
        }
    
        public int invention_id { get; set; }
        public string name { get; set; }
        public string no { get; set; }
        public Nullable<int> type_id { get; set; }
        public Nullable<System.DateTime> date { get; set; }
        public int file_id { get; set; }
        public Nullable<bool> is_verified { get; set; }
    
        public virtual File File { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<AuthorInvention> AuthorInventions { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<InventionCountry> InventionCountries { get; set; }
        public virtual InventionType InventionType { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RequestInvention> RequestInventions { get; set; }
    }
}
