﻿using ENTITIES;
using ENTITIES.CustomModels.Datatable;
using ENTITIES.CustomModels.ScienceManagement.Researcher;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
namespace BLL.ScienceManagement.ResearcherListRepo
{
    public class ResearchersListRepo
    {
        readonly ScienceAndInternationalAffairsEntities db = new ScienceAndInternationalAffairsEntities();
        public BaseServerSideData<ResearcherList> GetList(BaseDatatable baseDatatable, string coso, string name, int language_id)
        {
            var data = (from a in db.People
                        join b in db.Profiles on a.people_id equals b.people_id
                        join f in db.Offices.DefaultIfEmpty() on a.office_id equals f.office_id
                        where b.profile_page_active == true
                        select new ResearcherList
                        {
                            peopleId = a.people_id,
                            name = a.name,
                            email = a.email,
                            title = (
                            from c in db.AcademicDegreeLanguages.DefaultIfEmpty()
                            where b.current_academic_degree_id == c.academic_degree_id
                            select c.name
                            ).FirstOrDefault(),
                            website = b.website,
                            positions = ((from n in db.PeoplePositions
                                          join h in db.PositionLanguages on n.position_id equals h.position_id
                                          where h.language_id == language_id && n.people_id == a.people_id
                                          select h.name
                            ).ToList()),
                            avatar = (from ff in db.Files
                                      where b.avatar_id == ff.file_id
                                      select ff.link
                                      ).FirstOrDefault(),
                            office_id = f.office_id,
                            office_name = f.office_name,
                            googleScholar = b.google_scholar
                        });
            List<ResearcherList> result = null;
            if (coso.Trim() != "")
            {
                int cosoint = int.Parse(coso);
                data = data.Where(x => x.office_id == cosoint);
            }
            if (name.Trim() != "")
            {
                data = data.Where(x => x.name.Contains(name));
            }

            result = data.OrderBy(baseDatatable.SortColumnName + " " + baseDatatable.SortDirection)
                .Skip(baseDatatable.Start).Take(baseDatatable.Length).ToList();

            int recordsTotal = data.Count();
            return new BaseServerSideData<ResearcherList>(result, recordsTotal);
        }
    }
}
