﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENTITIES.CustomModels.InternationalCollaboration.AcademicCollaborationEntities
{
    public class ProcedureInfoManager : ProcedureInfo
    {
        public string full_name { get; set; }
        public int article_id { get; set; }
        public int language_id { get; set; }
    }
}
