﻿using ENTITIES;
using ENTITIES.CustomModels;
using ENTITIES.CustomModels.Datatable;
using ENTITIES.CustomModels.ScienceManagement.Researcher;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Dynamic.Core;
namespace BLL.ScienceManagement.Researcher
{
    public class ResearcherCandidateRepo
    {
        readonly ScienceAndInternationalAffairsEntities db = new ScienceAndInternationalAffairsEntities();
        public BaseServerSideData<ResearcherCandidate> GetList(BaseDatatable baseDatatable, string name, string chucdanh, string coso, int language_id)
        {
            var data = (from b in db.People
                        join c in db.Profiles on b.people_id equals c.people_id
                        join d in db.Titles on c.title_id equals d.title_id
                        join e in db.TitleLanguages on d.title_id equals e.title_id
                        join f in db.Offices.DefaultIfEmpty() on b.office_id equals f.office_id
                        where e.language_id == language_id && (d.title_id == 1 || d.title_id == 2) && c.profile_page_active == false
                        select new ResearcherCandidate
                        {
                            people_id = b.people_id,
                            name = b.name,
                            title_id = e.title_id,
                            title = e.name,
                            office_id = f.office_id,
                            office_name = f.office_name,
                            paperNumber = (from m in db.AuthorPapers where m.people_id == b.people_id select m).Count()
                        });
            List<ResearcherCandidate> result = null;
            if (name.Trim() != "")
            {
                data = data.Where(x => x.name.Contains(name));
            }
            if (chucdanh.Trim() != "")
            {
                int chucdanhint = Int32.Parse(chucdanh);
                data = data.Where(x => x.title_id == chucdanhint);
            }
            if (coso.Trim() != "")
            {
                int cosoint = Int32.Parse(coso);
                data = data.Where(x => x.office_id == cosoint);
            }
            result = data.OrderBy(baseDatatable.SortColumnName + " " + baseDatatable.SortDirection)
                    .Skip(baseDatatable.Start).Take(baseDatatable.Length).ToList();
            int recordsTotal = data.Count();
            return new BaseServerSideData<ResearcherCandidate>(result, recordsTotal);
        }

        public bool UpdateProfilePage(int id, bool status)
        {
            using (DbContextTransaction trans = db.Database.BeginTransaction())
            {
                try
                {
                    if (status)
                    {
                        db.Profiles.Find(id).profile_page_active = true;
                    }
                    else
                    {
                        db.Profiles.Find(id).profile_page_active = false;
                    }
                    db.SaveChanges();
                    trans.Commit();
                    return true;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    trans.Rollback();
                    return false;
                }
            }
        }
    }
}
