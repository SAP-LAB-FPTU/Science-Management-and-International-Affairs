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
    
    public partial class NotificationBase
    {
        public int notification_id { get; set; }
        public int account_id { get; set; }
        public int notification_type_id { get; set; }
        public bool is_read { get; set; }
        public string URL { get; set; }
        public System.DateTime created_date { get; set; }
    
        public virtual Account Account { get; set; }
        public virtual NotificationType NotificationType { get; set; }
    }
}
