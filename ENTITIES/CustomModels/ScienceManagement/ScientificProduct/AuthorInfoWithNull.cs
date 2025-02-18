﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENTITIES.CustomModels.ScienceManagement.ScientificProduct
{
    public class AuthorInfoWithNull : Profile
    {
        public string title_name { get; set; }
        public string contract_name { get; set; }
        public Nullable<int> money_reward { get; set; }
        public string office_abbreviation { get; set; }
        public string link { get; set; }
        public Nullable<int> contract_id { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public Nullable<int> office_id_string { get; set; }
        public string money_string { get; set; }
        public string title_string { get; set; }
        public string identification_file_link { get; set; }
        public Nullable<int> title_id_string { get; set; }
    }
}
