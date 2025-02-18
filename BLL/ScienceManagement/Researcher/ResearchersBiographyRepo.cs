﻿using ENTITIES;
using ENTITIES.CustomModels.ScienceManagement.Researcher;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Web;

namespace BLL.ScienceManagement.Researcher
{
    public class ResearchersBiographyRepo
    {
        private readonly ScienceAndInternationalAffairsEntities db;
        public ResearchersBiographyRepo(ScienceAndInternationalAffairsEntities db)
        {
            this.db = db;
        }
        public ResearchersBiographyRepo()
        {
            db = new ScienceAndInternationalAffairsEntities();
        }
        public List<AcadBiography> GetAcadHistory(int id, int language_id)
        {
            var profile = (
                from b in db.ProfileAcademicDegrees
                join c in db.AcademicDegrees on b.academic_degree_id equals c.academic_degree_id
                join d in db.AcademicDegreeLanguages on c.academic_degree_id equals d.academic_degree_id
                where d.language_id == language_id && b.people_id == id
                select new AcadBiography
                {
                    people_id = b.people_id,
                    acad_id = b.academic_degree_id,
                    degree = d.name,
                    time = b.start_year.ToString() + "-" + b.end_year.ToString(),
                    place = b.study_place
                }).AsEnumerable<AcadBiography>().Select((x, index) => new AcadBiography
                {
                    rownum = index + 1,
                    people_id = x.people_id,
                    acad_id = x.acad_id,
                    degree = x.degree,
                    time = x.time,
                    place = x.place
                }).ToList();
            return profile;
        }
        public List<BaseRecord<WorkingProcess>> GetWorkHistory(int id)
        {
            var list = (from a in db.WorkingProcesses
                        where a.Profile.people_id == id
                        select new BaseRecord<WorkingProcess>
                        {
                            records = a
                        }).AsEnumerable<BaseRecord<WorkingProcess>>()
                        .Select((x, index) => new BaseRecord<WorkingProcess>
                        {
                            index = index + 1,
                            records = x.records
                        }).OrderBy(x => x.records.start_year).ToList();
            return list;
        }

        public List<ResearcherPublications> GetPublications(int id)
        {
            var data = (from a in db.Papers
                        join b in db.AuthorPapers on a.paper_id equals b.paper_id
                        join c in db.Profiles on b.Author.mssv_msnv equals c.mssv_msnv
                        where c.people_id == id
                        select new ResearcherPublications
                        {
                            paper_id = a.paper_id,
                            journal_or_cfr_name = b.Paper.journal_name,
                            paper_name = a.name,
                            publish_date = a.publish_date,
                            co_author =
                            (from m in db.AuthorPapers
                             where m.paper_id == a.paper_id
                             select m.Author.name).ToList(),
                            link = a.link_doi,
                            rank = a.PaperWithCriterias.FirstOrDefault().PaperCriteria.name
                        }).OrderByDescending(x => x.publish_date).AsEnumerable<ResearcherPublications>().Select((x, index) => new ResearcherPublications
                        {
                            rownum = index + 1,
                            paper_id = x.paper_id,
                            journal_or_cfr_name = x.journal_or_cfr_name,
                            paper_name = x.paper_name,
                            year = x.publish_date == null ? "" : x.publish_date.Value.Year.ToString(),
                            co_author = x.co_author,
                            link = x.link,
                            rank = x.rank
                        }).ToList();
            return data;
        }
        public List<ResearcherPublications> GetConferencePublic(int id)
        {
            var data = (from a in db.Papers
                        join b in db.RequestConferences on a.paper_id equals b.paper_id
                        join c in db.Conferences on b.conference_id equals c.conference_id
                        join d in db.ConferenceParticipants on b.request_id equals d.request_id
                        join e in db.Profiles on d.mssv_msnv equals e.mssv_msnv
                        where e.people_id == id
                        select new ResearcherPublications
                        {
                            paper_id = a.paper_id,
                            journal_or_cfr_name = c.conference_name,
                            paper_name = a.name,
                            publish_date = a.publish_date,
                        }).OrderByDescending(x => x.publish_date).AsEnumerable<ResearcherPublications>().Select((x, index) => new ResearcherPublications
                        {
                            rownum = index + 1,
                            paper_id = x.paper_id,
                            journal_or_cfr_name = x.journal_or_cfr_name,
                            paper_name = x.paper_name,
                            publish_date = x.publish_date,
                        }).ToList();
            return data;
        }
        public List<BaseRecord<Award>> GetAwards(int id)
        {
            var list = (from a in db.Awards
                        where a.people_id == id
                        orderby a.award_time descending
                        select new BaseRecord<Award>
                        {
                            records = a
                        }).ToList()
                        .Select((x, index) => new BaseRecord<Award>
                        {
                            index = index + 1,
                            records = x.records
                        }).ToList();
            return list;
        }
        public List<SelectField> GetAcadDegrees()
        {
            var data = (from c in db.AcademicDegrees
                        join d in db.AcademicDegreeLanguages on c.academic_degree_id equals d.academic_degree_id
                        where d.language_id == 1
                        select new SelectField
                        {
                            id = c.academic_degree_id,
                            name = d.name,
                            selected = 0
                        }).ToList();
            return data;
        }
        public int AddNewAcadEvent(string data)
        {
            using (DbContextTransaction trans = db.Database.BeginTransaction())
            {
                try
                {
                    var info = JObject.Parse(data);
                    int people_id = (int)info["data"]["people_id"];
                    int degree = (int)info["data"]["degree"];
                    string location = (string)info["data"]["location"];
                    int start = (int)info["data"]["start"];
                    int end = (int)info["data"]["end"];
                    Profile profile = db.Profiles.Find(people_id);
                    db.ProfileAcademicDegrees.Add(new ProfileAcademicDegree
                    {
                        people_id = people_id,
                        academic_degree_id = degree,
                        start_year = start,
                        end_year = end,
                        study_place = location,
                        Profile = db.Profiles.Find(people_id),
                        AcademicDegree = db.AcademicDegrees.Find(degree)
                    });
                    db.SaveChanges();
                    trans.Commit();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    trans.Rollback();
                    return 0;
                }
            }
            return 1;
        }
        public List<SelectField> GetTitles(int language_id)
        {
            var data = (from a in db.Titles
                        join b in db.TitleLanguages on a.title_id equals b.title_id
                        where b.language_id == language_id
                        select new SelectField
                        {
                            id = a.title_id,
                            name = b.name,
                            selected = 0
                        }).ToList();
            return data;
        }

        public List<SelectField> GetOffices(int language_id)
        {
            var data = (from a in db.Offices
                        select new SelectField
                        {
                            id = a.office_id,
                            name = a.office_abbreviation,
                            selected = 0
                        }).ToList();
            return data;
        }
        public int AddNewWorkEvent(string data)
        {
            using (DbContextTransaction trans = db.Database.BeginTransaction())
            {
                try
                {
                    var info = JObject.Parse(data);
                    int people_id = (int)info["data"]["people_id"];
                    string title = (string)info["data"]["title"];
                    string location = (string)info["data"]["location"];
                    int start = (int)info["data"]["start"];
                    int end = (int)info["data"]["end"];
                    db.WorkingProcesses.Add(new WorkingProcess
                    {
                        pepple_id = people_id,
                        work_unit = location,
                        start_year = start,
                        end_year = end,
                        title = title,
                        Profile = db.Profiles.Find(people_id)
                    });
                    db.SaveChanges();
                    trans.Commit();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    trans.Rollback();
                    return 0;
                }
                return 1;
            }
        }
        public int EditAcadEvent(string data)
        {
            using (DbContextTransaction trans = db.Database.BeginTransaction())
            {
                try
                {
                    var info = JObject.Parse(data);
                    int people_id = (int)info["data"]["people_id"];
                    int acad_id = (int)info["data"]["acad_id"];
                    int degree = (int)info["data"]["degree"];
                    string location = (string)info["data"]["location"];
                    int start = (int)info["data"]["start"];
                    int end = (int)info["data"]["end"];
                    ProfileAcademicDegree acad = db.ProfileAcademicDegrees.Find(people_id, acad_id);
                    db.ProfileAcademicDegrees.Remove(acad);
                    db.SaveChanges();
                    db.ProfileAcademicDegrees.Add(new ProfileAcademicDegree
                    {
                        people_id = people_id,
                        academic_degree_id = degree,
                        start_year = start,
                        end_year = end,
                        study_place = location,
                        Profile = db.Profiles.Find(people_id),
                        AcademicDegree = db.AcademicDegrees.Find(degree)
                    });
                    db.SaveChanges();
                    trans.Commit();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    trans.Rollback();
                    return 0;
                }
            }
            return 1;
        }
        public int EditWorkEvent(string data)
        {
            using (DbContextTransaction trans = db.Database.BeginTransaction())
            {
                try
                {
                    var info = JObject.Parse(data);
                    int id = (int)info["data"]["id"];
                    string place = (string)info["data"]["place"];
                    string work_title = (string)info["data"]["work_title"];
                    int start = (int)info["data"]["start"];
                    int end = (int)info["data"]["end"];
                    WorkingProcess w = db.WorkingProcesses.Find(id);
                    w.title = work_title;
                    w.start_year = start;
                    w.end_year = end;
                    w.work_unit = place;
                    db.SaveChanges();
                    trans.Commit();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    trans.Rollback();
                    return 0;
                }
            }
            return 1;
        }
        public int DeleteAcadEvent(string data)
        {
            using (DbContextTransaction trans = db.Database.BeginTransaction())
            {
                try
                {
                    var info = JObject.Parse(data);
                    int acad_id = (int)info["data"]["acad_id"];
                    int people_id = (int)info["data"]["people_id"];
                    ProfileAcademicDegree pa = db.ProfileAcademicDegrees.Find(people_id, acad_id);
                    db.ProfileAcademicDegrees.Remove(pa);
                    db.SaveChanges();
                    trans.Commit();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    trans.Rollback();
                    return 0;
                }
            }
            return 1;
        }
        public int DeleteWorkEvent(string data)
        {
            using (DbContextTransaction trans = db.Database.BeginTransaction())
            {
                try
                {
                    var info = JObject.Parse(data);
                    int id = (int)info["data"]["id"];
                    WorkingProcess pa = db.WorkingProcesses.Find(id);
                    db.WorkingProcesses.Remove(pa);
                    db.SaveChanges();
                    trans.Commit();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    trans.Rollback();
                    return 0;
                }
            }
            return 1;
        }

        public int AddAward(Award a)
        {
            using (DbContextTransaction trans = db.Database.BeginTransaction())
            {
                try
                {
                    db.Awards.Add(a);
                    db.SaveChanges();
                    trans.Commit();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    trans.Rollback();
                    return 0;
                }
            }
            return 1;
        }
        public int EditAward(HttpRequestBase Request)
        {
            using (DbContextTransaction trans = db.Database.BeginTransaction())
            {
                try
                {
                    Award b = db.Awards.Find(Int32.Parse(Request["id"]));
                    b.competion_name = Request["competion_name"];
                    b.rank = Request["rank"];
                    b.award_time = DateTime.ParseExact(Request["award_time"], "dd/MM/yyyy", CultureInfo.InvariantCulture);
                    db.SaveChanges();
                    trans.Commit();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    trans.Rollback();
                    return 0;
                }
            }
            return 1;
        }
        public int DeleteAward(HttpRequestBase Request)
        {
            using (DbContextTransaction trans = db.Database.BeginTransaction())
            {
                try
                {
                    db.Awards.Remove(db.Awards.Find(Int32.Parse(Request["id"])));
                    db.SaveChanges();
                    trans.Commit();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    trans.Rollback();
                    return 0;
                }
            }
            return 1;
        }
    }
}
