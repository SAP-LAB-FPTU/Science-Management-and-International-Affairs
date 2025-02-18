﻿using ENTITIES;
using ENTITIES.CustomModels;
using ENTITIES.CustomModels.Datatable;
using ENTITIES.CustomModels.InternationalCollaboration;
using ENTITIES.CustomModels.InternationalCollaboration.AcademicCollaborationEntities;
using ENTITIES.CustomModels.InternationalCollaboration.AcademicCollaborationEntities.SaveAcademicCollaborationEntities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace BLL.InternationalCollaboration.AcademicCollaborationRepository
{
    public class AcademicCollaborationRepo
    {
        private readonly ScienceAndInternationalAffairsEntities db = new ScienceAndInternationalAffairsEntities();

        public BaseServerSideData<AcademicCollaboration_Ext> AcademicCollaborations(int direction, int collab_type_id, ObjectSearching_AcademicCollaboration obj_searching, BaseDatatable baseDatatable)
        {
            try
            {
                var sql = @"select
                        collab.collab_id, pp.people_id, pp.[name] 'people_name', pp.email, offi.office_name, pn.partner_name, c.country_name,
                        collab.plan_study_start_date, collab.plan_study_end_date,
                        acs.collab_status_id, acs.collab_status_id, acs.collab_status_name,
                        collab.is_supported, collab.note
                        from IA_AcademicCollaboration.AcademicCollaboration collab
                        join IA_Collaboration.PartnerScope mpc on collab.partner_scope_id = mpc.partner_scope_id
                        join IA_Collaboration.[Partner] pn on pn.partner_id = mpc.partner_id
                        left join General.Country c on c.country_id = pn.country_id
                        join General.People pp on collab.people_id = pp.people_id
                        left join General.[Profile] pf on pf.people_id = pp.people_id
                        left join General.Office offi on pp.office_id = offi.office_id
                        join (select csh1.collab_id, csh2.collab_status_id, csh1.change_date
		                        from
		                        (select csh1.collab_id, MAX(csh1.change_date) 'change_date'
			                        from IA_AcademicCollaboration.CollaborationStatusHistory csh1
			                        group by csh1.collab_id) as csh1
		                        join
		                        (select csh2.collab_status_id, csh2.collab_id, csh2.change_date
			                        from IA_AcademicCollaboration.CollaborationStatusHistory csh2) as csh2
		                        on csh1.collab_id = csh2.collab_id and csh1.change_date = csh2.change_date) as csh
                        on csh.collab_id = collab.collab_id
                        join IA_AcademicCollaboration.AcademicCollaborationStatus acs on acs.collab_status_id = csh.collab_status_id
                        where collab.direction_id = @direction /*Dài hạn = 2, Ngắn hạn = 1*/ and collab.collab_type_id = @collab_type_id /*Chiều đi = 1, Chiều đến = 2*/
                        and ISNULL(c.country_name, '') like @country_name
                        and ISNULL(pn.partner_name, '') like @partner_name
                        and ISNULL(offi.office_name, '') like @office_name ";

                var count_sql = @"select count(*)                                                              
                                from IA_AcademicCollaboration.AcademicCollaboration collab
                                join IA_Collaboration.PartnerScope mpc on collab.partner_scope_id = mpc.partner_scope_id
                                join IA_Collaboration.[Partner] pn on pn.partner_id = mpc.partner_id
                                left join General.Country c on c.country_id = pn.country_id
                                join General.People pp on collab.people_id = pp.people_id
                                left join General.[Profile] pf on pf.people_id = pp.people_id
                                left join General.Office offi on pp.office_id = offi.office_id
                                join (select csh1.collab_id, csh2.collab_status_id, csh1.change_date
		                                from
		                                (select csh1.collab_id, MAX(csh1.change_date) 'change_date'
			                                from IA_AcademicCollaboration.CollaborationStatusHistory csh1
			                                group by csh1.collab_id) as csh1
		                                join
		                                (select csh2.collab_status_id, csh2.collab_id, csh2.change_date
			                                from IA_AcademicCollaboration.CollaborationStatusHistory csh2) as csh2
		                                on csh1.collab_id = csh2.collab_id and csh1.change_date = csh2.change_date) as csh
                                on csh.collab_id = collab.collab_id
                                join IA_AcademicCollaboration.AcademicCollaborationStatus acs on acs.collab_status_id = csh.collab_status_id
                                where collab.direction_id = @direction /*Dài hạn = 2, Ngắn hạn = 1*/ and collab.collab_type_id = @collab_type_id /*Chiều đi = 1, Chiều đến = 2*/
                                and ISNULL(c.country_name, '') like @country_name
                                and ISNULL(pn.partner_name, '') like @partner_name
                                and ISNULL(offi.office_name, '') like @office_name ";
                if (obj_searching.year != 0)
                {
                    sql += @"and @year between YEAR(collab.plan_study_start_date) and YEAR(collab.plan_study_end_date)";
                    count_sql += @"and @year between YEAR(collab.plan_study_start_date) and YEAR(collab.plan_study_end_date)";
                }
                sql += @"ORDER BY " + baseDatatable.SortColumnName + " " + baseDatatable.SortDirection +
                        " OFFSET " + baseDatatable.Start + " ROWS FETCH NEXT " + baseDatatable.Length + " ROWS ONLY";

                List<AcademicCollaboration_Ext> academicCollaborations;
                int recordsTotal;
                if (obj_searching.year != 0)
                {
                    academicCollaborations = db.Database.SqlQuery<AcademicCollaboration_Ext>(sql,
                                                        new SqlParameter("direction", direction),
                                                        new SqlParameter("collab_type_id", collab_type_id),
                                                        new SqlParameter("country_name", obj_searching.country_name == null ? "%%" : "%" + obj_searching.country_name + "%"),
                                                        new SqlParameter("partner_name", obj_searching.partner_name == null ? "%%" : "%" + obj_searching.partner_name + "%"),
                                                        new SqlParameter("office_name", obj_searching.office_name == null ? "%%" : "%" + obj_searching.office_name + "%"),
                                                        new SqlParameter("year", obj_searching.year),
                                                        new SqlParameter("sortColumnName", baseDatatable.SortColumnName),
                                                        new SqlParameter("sortDirection", baseDatatable.SortDirection),
                                                        new SqlParameter("start", baseDatatable.Start),
                                                        new SqlParameter("length", baseDatatable.Length)).ToList();
                    recordsTotal = db.Database.SqlQuery<int>(count_sql,
                                                                new SqlParameter("direction", direction),
                                                                new SqlParameter("collab_type_id", collab_type_id),
                                                                new SqlParameter("country_name", obj_searching.country_name == null ? "%%" : "%" + obj_searching.country_name + "%"),
                                                                new SqlParameter("partner_name", obj_searching.partner_name == null ? "%%" : "%" + obj_searching.partner_name + "%"),
                                                                new SqlParameter("office_name", obj_searching.office_name == null ? "%%" : "%" + obj_searching.office_name + "%"),
                                                                new SqlParameter("year", obj_searching.year)).FirstOrDefault();
                }
                else
                {
                    academicCollaborations = db.Database.SqlQuery<AcademicCollaboration_Ext>(sql,
                                                        new SqlParameter("direction", direction),
                                                        new SqlParameter("collab_type_id", collab_type_id),
                                                        new SqlParameter("country_name", obj_searching.country_name == null ? "%%" : "%" + obj_searching.country_name + "%"),
                                                        new SqlParameter("partner_name", obj_searching.partner_name == null ? "%%" : "%" + obj_searching.partner_name + "%"),
                                                        new SqlParameter("office_name", obj_searching.office_name == null ? "%%" : "%" + obj_searching.office_name + "%"),
                                                        new SqlParameter("sortColumnName", baseDatatable.SortColumnName),
                                                        new SqlParameter("sortDirection", baseDatatable.SortDirection),
                                                        new SqlParameter("start", baseDatatable.Start),
                                                        new SqlParameter("length", baseDatatable.Length)).ToList();
                    recordsTotal = db.Database.SqlQuery<int>(count_sql,
                                                                new SqlParameter("direction", direction),
                                                                new SqlParameter("collab_type_id", collab_type_id),
                                                                new SqlParameter("country_name", obj_searching.country_name == null ? "%%" : "%" + obj_searching.country_name + "%"),
                                                                new SqlParameter("partner_name", obj_searching.partner_name == null ? "%%" : "%" + obj_searching.partner_name + "%"),
                                                                new SqlParameter("office_name", obj_searching.office_name == null ? "%%" : "%" + obj_searching.office_name + "%")).FirstOrDefault();
                }
                return new BaseServerSideData<AcademicCollaboration_Ext>(academicCollaborations, recordsTotal);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public AlertModal<List<Year>> YearSearching()
        {
            try
            {
                var sql = @"-----1.2. Danh sách Năm
                    select YEAR(MIN(plan_study_start_date)) as 'year_from', YEAR(MAX(plan_study_end_date)) as 'year_to'
                    from IA_AcademicCollaboration.AcademicCollaboration";
                YearSearching yearSearching = db.Database.SqlQuery<YearSearching>(sql).FirstOrDefault();
                if (yearSearching.year_from == null) yearSearching.year_from = DateTime.Now.Year;
                if (yearSearching.year_to == null) yearSearching.year_to = DateTime.Now.Year;

                List<Year> yearsSearching = new List<Year>();
                Year year;
                for (var y = yearSearching.year_from; y <= yearSearching.year_to; y++)
                {
                    year = new Year
                    {
                        year = y
                    };
                    yearsSearching.Add(year);
                }
                return new AlertModal<List<Year>>(yearsSearching, true);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public AlertModal<List<AcademicCollaborationPartner_Ext>> Partners(string partner_name)
        {
            try
            {
                var sql = @"-----1.3. Đơn vị đào tạo - chiều đi/chiều đến -> partner/office
                    select distinct par.*, cou.country_name from IA_Collaboration.[Partner] par
                    inner join General.Country cou on cou.country_id = par.country_id
					left join IA_Collaboration.MOUPartner mp on mp.partner_id = par.partner_id
					left join IA_Collaboration.MOU m on m.mou_id = mp.mou_id and (m.mou_id IS NULL OR m.is_deleted = 0)
                    where par.is_deleted = 0 and partner_name like @partner_name";
                List<AcademicCollaborationPartner_Ext> partners = db.Database.SqlQuery<AcademicCollaborationPartner_Ext>(sql,
                    new SqlParameter("partner_name", partner_name == null ? "%%" : "%" + partner_name + "%")).ToList();
                return new AlertModal<List<AcademicCollaborationPartner_Ext>>(partners, true);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public AlertModal<List<AcademicCollaborationPartner_Ext>> PartnersSearching(string partner_name)
        {
            try
            {
                var sql = @"-----1.3. Đơn vị đào tạo - chiều đi/chiều đến -> partner/office
                    select distinct par.*, cou.country_name from IA_Collaboration.[Partner] par
                    inner join General.Country cou on cou.country_id = par.country_id
					where partner_name like @partner_name";
                List<AcademicCollaborationPartner_Ext> partners = db.Database.SqlQuery<AcademicCollaborationPartner_Ext>(sql,
                    new SqlParameter("partner_name", partner_name == null ? "%%" : "%" + partner_name + "%")).ToList();
                return new AlertModal<List<AcademicCollaborationPartner_Ext>>(partners, true);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public AlertModal<AcademicCollaborationPartner_Ext> Partner(int partner_id, string partner_name)
        {
            try
            {
                var sql = @"------1.3.1. Check đơn vị đào tạo
                    select par.*, cou.country_name from IA_Collaboration.[Partner] par
                    inner join General.Country cou on cou.country_id = par.country_id
                    where partner_name = @partner_name and is_deleted = 0
                    or par.partner_id = @partner_id";
                AcademicCollaborationPartner_Ext partner = db.Database.SqlQuery<AcademicCollaborationPartner_Ext>(sql,
                    new SqlParameter("partner_name", partner_name),
                    new SqlParameter("partner_id", partner_id)).FirstOrDefault();
                return new AlertModal<AcademicCollaborationPartner_Ext>(partner, true);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public AlertModal<List<Office>> Offices(string office_name)
        {
            try
            {
                var sql = @"-----1.4. Đơn vị công tác - chiều đi/chiều đến -> office/partner
                    select * from General.Office
                    where office_name like @office_name";
                List<Office> offices = db.Database.SqlQuery<Office>(sql,
                    new SqlParameter("office_name", office_name == null ? "%%" : "%" + office_name + "%")).ToList();
                return new AlertModal<List<Office>>(offices, true);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public AlertModal<List<AcademicCollaborationPerson_Ext>> People(string person_name)
        {
            try
            {
                var sql = @"-----1.7. Danh sách cán bộ
                    select peo.people_id, name, email, phone_number, pro.mssv_msnv
                    from General.People peo
                    left join General.Profile pro on peo.people_id = pro.people_id
                    where name like @person_name";
                List<AcademicCollaborationPerson_Ext> people = db.Database.SqlQuery<AcademicCollaborationPerson_Ext>(sql,
                    new SqlParameter("person_name", person_name == null ? "%%" : "%" + person_name + "%")).ToList();
                return new AlertModal<List<AcademicCollaborationPerson_Ext>>(people, true);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public AlertModal<AcademicCollaborationPerson_Ext> Person(int people_id, string people_name)
        {
            try
            {
                var sql = @"-----1.7.1. Check person
                    select peo.people_id, peo.name, peo.email, peo.phone_number, peo.office_id, offi.office_name, pro.mssv_msnv
                    from General.People peo
                    left join General.Profile pro on peo.people_id = pro.people_id
                    left join General.Office offi on offi.office_id = peo.office_id
                    where peo.name = @people_name or peo.people_id = @people_id";
                AcademicCollaborationPerson_Ext person = db.Database.SqlQuery<AcademicCollaborationPerson_Ext>(sql,
                    new SqlParameter("people_name", people_name),
                    new SqlParameter("people_id", people_id)).FirstOrDefault();
                if (person != null)
                {
                    return new AlertModal<AcademicCollaborationPerson_Ext>(person, true);
                }
                else
                {
                    return new AlertModal<AcademicCollaborationPerson_Ext>(false, "Lấy dữ liệu về cán bộ đã có lỗi xảy ra.");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return new AlertModal<AcademicCollaborationPerson_Ext>(false, "Lấy dữ liệu về cán bộ đã có lỗi xảy ra.");
            }
        }

        public AlertModal<List<CollaborationScope>> CollaborationScopes(string collab_abbreviation_name)
        {
            try
            {
                var sql = @"-----1.8. Phạm vi hợp tác
                    select * from IA_Collaboration.CollaborationScope
                    where scope_abbreviation = @collab_abbreviation_name";
                List<CollaborationScope> collaborationScopes = db.Database.SqlQuery<CollaborationScope>(sql,
                    new SqlParameter("collab_abbreviation_name", collab_abbreviation_name)).ToList();
                return new AlertModal<List<CollaborationScope>>(collaborationScopes, true);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public AlertModal<List<AcademicCollaborationStatu>> AcademicCollaborationStatus(int status_type_specific)
        {
            try
            {
                var sql = @"----3.Chuyển đổi trạng thái - Danh sách trạng thái
                    select collab_status_id, collab_status_name, status_type
                    from IA_AcademicCollaboration.AcademicCollaborationStatus
                    where status_type = 3 /*both long & short-term*/ or status_type = @status_type_specific /*1:short;2:long;3:both*/";
                List<AcademicCollaborationStatu> academicCollaborationStatus = db.Database.SqlQuery<AcademicCollaborationStatu>(sql,
                    new SqlParameter("status_type_specific", status_type_specific)).ToList();
                return new AlertModal<List<AcademicCollaborationStatu>>(academicCollaborationStatus, true);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public Google.Apis.Drive.v3.Data.File UploadEvidenceFile(HttpPostedFileBase InputFile, string FolderName, int TypeFolder, bool isFolder)
        {
            try
            {
                Google.Apis.Drive.v3.Data.File f = new Google.Apis.Drive.v3.Data.File();
                if (InputFile != null)
                {
                    f = GoogleDriveService.UploadIAFile(InputFile, FolderName, TypeFolder, isFolder);
                }
                return f;
            }
            catch (Exception e)
            {
                if (InputFile.FileName != "")
                {
                    GoogleDriveService.DeleteFile(InputFile.FileName);
                }
                throw e;
            }
        }

        public File SaveFileToFile(Google.Apis.Drive.v3.Data.File f, HttpPostedFileBase evidence)
        {
            File evidence_file = new File();
            try
            {
                if (evidence != null)
                {
                    //add infor to File
                    if (evidence.FileName != null) evidence_file.name = evidence.FileName;
                    if (f.WebViewLink != null) evidence_file.link = f.WebViewLink;
                    if (f.Id != null) evidence_file.file_drive_id = f.Id;
                    db.Files.Add(evidence_file);
                    db.SaveChanges();
                }
            }
            catch (Exception e)
            {
                throw e;
            }
            return evidence_file;
        }

        public void SaveFileAndCollabHistory(HttpPostedFileBase evidence, string file_action, string folder_name,
            int collab_id, int status_id, string note, int account_id)
        {
            File evidence_file = new File();
            try
            {
                Google.Apis.Drive.v3.Data.File f;
                switch (file_action)
                {
                    case "add":
                        //add file to Google Drive
                        f = UploadEvidenceFile(evidence, folder_name, 4, false);
                        //add infor to File
                        evidence_file = SaveFileToFile(f, evidence);
                        //add to Collab Status History
                        SaveCollabStatusHistory(evidence, collab_id, status_id, note, evidence_file, account_id);
                        break;
                    case "edit":
                        //add file to Google Drive
                        f = UploadEvidenceFile(evidence, folder_name, 4, false);
                        //add infor to File
                        evidence_file = SaveFileToFile(f, evidence);
                        //add to Collab Status History
                        SaveCollabStatusHistory(evidence, collab_id, status_id, note, evidence_file, account_id);
                        break;
                    case "remove":
                        //add to Collab Status History
                        SaveCollabStatusHistory(evidence, collab_id, status_id, note, evidence_file, account_id);
                        break;
                    case "none":
                        var lastest_status_history = db.CollaborationStatusHistories.Where(x => x.collab_id == collab_id).OrderByDescending(t => t.change_date).FirstOrDefault();
                        evidence_file = db.Files.Find(lastest_status_history.file_id);
                        //add to Collab Status History
                        SaveCollabStatusHistory(evidence, collab_id, status_id, note, evidence_file, account_id);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public AlertModal<AcademicCollaboration_Ext> SaveAcademicCollaboration(int direction_id, int collab_type_id,
            SaveAcadCollab_Person obj_person,
            SaveAcadCollab_Partner obj_partner,
            SaveAcadCollab_AcademicCollaboration obj_academic_collab,
            HttpPostedFileBase evidence, string folder_name,
            int account_id)
        {
            using (DbContextTransaction trans = db.Database.BeginTransaction())
            {
                try
                {
                    AutoActiveInactive autoActiveInactive = new AutoActiveInactive();
                    db.Configuration.LazyLoadingEnabled = false;
                    //check duplicate academic collaboration: person, partner, collab_scope base on time
                    if (CheckDuplicateAcademicCollaboration(obj_person, obj_partner, obj_academic_collab))
                    {
                        Person person;
                        var person_id = obj_person.person_id;
                        Partner partner;
                        var partner_id = obj_partner.partner_id;
                        var partner_scope_id = 0;
                        PartnerScope partner_scope;

                        //check available person
                        if (!obj_person.available_person)
                        {
                            //add person
                            person = SavePerson(obj_person);
                            person_id = person.people_id;
                            //add profile
                            //saveProfile(person, obj_person);
                        }

                        //check available partner
                        if (!obj_partner.available_partner)
                        {
                            var country = db.Countries.Find(obj_partner.partner_country_id);
                            if (country == null)
                            {
                                trans.Rollback();
                                return new AlertModal<AcademicCollaboration_Ext>(null, false, "Lỗi", "Không tìm thấy quốc gia tương ứng.");
                            }
                            //add Article 
                            var article = SaveArticle(account_id);
                            //add ArticleVersion
                            SaveArticleVersion(obj_partner, article);
                            //add Partner
                            partner = SavePartner(obj_partner, article);
                            //get corresponsing partner_id value
                            partner_id = partner.partner_id;
                            //add partner_id & scope_id to PartnerScope
                            partner_scope = SavePartnerScope(partner_id, obj_partner.collab_scope_id);
                            //get corresponding partner_scope_id
                            partner_scope_id = partner_scope.partner_scope_id;
                            //add Academic Collab
                            var academic_collaboration = SaveAcademicCollaboration(direction_id, collab_type_id, person_id, partner_scope_id, obj_academic_collab);
                            //add file and collab staus history
                            SaveFileAndCollabHistory(evidence, "add", folder_name, academic_collaboration.collab_id, obj_academic_collab.status_id, null, account_id);
                        }
                        else
                        {
                            //check exist partner_scope
                            partner_scope = db.PartnerScopes.Where<PartnerScope>(x => x.partner_id == partner_id && x.scope_id == obj_partner.collab_scope_id).FirstOrDefault();
                            if (partner_scope != null)
                            {
                                //incease 1 to referecen count
                                IncreaseReferenceCountOfPartnerScope(partner_scope);
                                db.SaveChanges();
                            }
                            else
                            {
                                //add partner_id & scope_id to PartnerScope
                                partner_scope = SavePartnerScope(partner_id, obj_partner.collab_scope_id);
                            }
                            //get corresponding partner_scope_id
                            partner_scope_id = partner_scope.partner_scope_id;
                            //add Academic Collab
                            var academic_collaboration = SaveAcademicCollaboration(direction_id, collab_type_id, person_id, partner_scope_id, obj_academic_collab);
                            //add file and collab staus history
                            SaveFileAndCollabHistory(evidence, "add", folder_name, academic_collaboration.collab_id, obj_academic_collab.status_id, null, account_id);
                        }
                        trans.Commit();
                        //change status corressponding MOU/MOA
                        using (DbContextTransaction dbContext = db.Database.BeginTransaction())
                        {
                            try
                            {
                                List<int> list_partner_scope_id = new List<int>
                                {
                                    partner_scope_id
                                };
                                autoActiveInactive.changeStatusMOUMOA(list_partner_scope_id, db);
                                dbContext.Commit();
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.ToString());
                                dbContext.Rollback();
                                return new AlertModal<AcademicCollaboration_Ext>(null, false, "Có lỗi xảy ra khi tự động active/inactive MOU/MOA.");
                            }
                        }
                        return new AlertModal<AcademicCollaboration_Ext>(null, true, "Thêm cán bộ giảng viên thành công.");
                    }
                    else
                    {
                        AlertModal<AcademicCollaboration_Ext> alertModal = new AlertModal<AcademicCollaboration_Ext>(null, false, "Cảnh báo", "Với thời gian kế hoạch, CBGV đang đi học tại đối tác.");
                        return alertModal;
                    }
                }
                catch (Exception e)
                {
                    trans.Rollback();
                    Console.WriteLine(e.ToString());
                    return new AlertModal<AcademicCollaboration_Ext>(false);
                }
            }
        }

        public bool CheckDuplicateAcademicCollaboration(SaveAcadCollab_Person obj_person, SaveAcadCollab_Partner obj_partner, SaveAcadCollab_AcademicCollaboration obj_academic_collab)
        {
            if (obj_person.available_person && obj_partner.available_partner)
            {
                AcademicCollaboration academicCollaboration = db.AcademicCollaborations.Where(x => x.people_id == obj_person.person_id
                                                            &&
                                                            x.collab_id != obj_academic_collab.collab_id
                                                            &&
                                                            ((x.plan_study_start_date <= obj_academic_collab.plan_start_date && x.plan_study_end_date >= obj_academic_collab.plan_start_date)
                                                            ||
                                                            (x.plan_study_start_date <= obj_academic_collab.plan_end_date && x.plan_study_end_date >= obj_academic_collab.plan_end_date)
                                                            ||
                                                            (x.plan_study_start_date <= obj_academic_collab.plan_start_date && x.plan_study_end_date >= obj_academic_collab.plan_end_date)
                                                            ||
                                                            (x.plan_study_start_date >= obj_academic_collab.plan_start_date && x.plan_study_end_date <= obj_academic_collab.plan_end_date))).FirstOrDefault();
                if (academicCollaboration != null)
                {
                    return false;
                }
            }
            return true;
        }

        public Person SavePerson(SaveAcadCollab_Person obj_person)
        {
            Person person = new Person();
            try
            {
                //add new person
                //add information to People
                person.name = obj_person.person_name;
                person.email = obj_person.person_email;
                if (obj_person.person_profile_office_id != 0) person.office_id = obj_person.person_profile_office_id;
                db.People.Add(person);
                db.SaveChanges();
            }
            catch (Exception e)
            {
                throw e;
            }
            return person;
        }

        public Person UpdatePerson(SaveAcadCollab_Person obj_person)
        {
            Person person;
            try
            {
                person = db.People.Find(obj_person.person_id);
                //update person
                if (obj_person.person_profile_office_id != 0)
                {
                    person.office_id = obj_person.person_profile_office_id;
                }
                else
                {
                    person.office_id = null;
                }
                db.SaveChanges();
            }
            catch (Exception e)
            {
                throw e;
            }
            return person;
        }

        //public void saveProfile(Person person, SaveAcadCollab_Person obj_person)
        //{
        //    try
        //    {
        //        var profile = new Profile();
        //        //check office_id with Office
        //        var office = db.Offices.Find(obj_person.person_profile_office_id);
        //        if (office != null)
        //        {
        //            profile.office_id = office.office_id;
        //            profile.people_id = person.people_id;
        //            profile.mssv_msnv = ""; //can make issues for Sicence Management
        //            profile.profile_page_active = false;
        //            db.Profiles.Add(profile);
        //            db.SaveChanges();
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        throw e;
        //    }
        //}

        public Article SaveArticle(int account_id)
        {
            Article article;
            try
            {
                //add Article
                article = new Article()
                {
                    need_approved = false,
                    article_status_id = 2, //Chấp thuận
                    account_id = account_id
                };
                db.Articles.Add(article);
                db.SaveChanges();
            }
            catch (Exception e)
            {
                throw e;
            }
            return article;
        }

        public void SaveArticleVersion(SaveAcadCollab_Partner obj_partner, Article article)
        {
            try
            {
                //add ArticleVersion
                var articleVersion = new ArticleVersion()
                {
                    publish_time = DateTime.Now,
                    version_title = obj_partner.partner_name,
                    article_id = article.article_id,
                    language_id = 1 //Vietnamese
                };
                db.ArticleVersions.Add(articleVersion);
                db.SaveChanges();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public Partner SavePartner(SaveAcadCollab_Partner obj_partner, Article article)
        {
            Partner partner = new Partner();
            try
            {
                partner.country_id = obj_partner.partner_country_id;
                partner.partner_name = obj_partner.partner_name;
                partner.article_id = article.article_id;
                db.Partners.Add(partner);
                db.SaveChanges();
            }
            catch (Exception e)
            {
                throw e;
            }
            return partner;
        }

        public PartnerScope SavePartnerScope(int partner_id, int scope_id)
        {
            PartnerScope partner_scope;
            try
            {
                partner_scope = new PartnerScope()
                {
                    partner_id = partner_id,
                    scope_id = scope_id,
                    reference_count = 1 //init first count for new PartnerScope
                };
                db.PartnerScopes.Add(partner_scope);
                db.SaveChanges();
            }
            catch (Exception e)
            {
                throw e;
            }
            return partner_scope;
        }

        public void IncreaseReferenceCountOfPartnerScope(PartnerScope partner_scope)
        {
            try
            {
                partner_scope.reference_count += 1;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void DecreaseReferenceCountOfPartnerScope(PartnerScope partner_scope)
        {
            try
            {
                partner_scope.reference_count -= 1;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public AcademicCollaboration SaveAcademicCollaboration(int direction_id, int collab_type_id, int person_id, int partner_scope_id,
            SaveAcadCollab_AcademicCollaboration obj_academic_collab)
        {
            AcademicCollaboration academic_collaboration = new AcademicCollaboration();
            try
            {
                //add infor to AcademicCollaboration
                academic_collaboration.direction_id = direction_id;
                academic_collaboration.collab_type_id = collab_type_id;
                academic_collaboration.people_id = person_id;
                academic_collaboration.partner_scope_id = partner_scope_id;
                academic_collaboration.plan_study_start_date = obj_academic_collab.plan_start_date;
                academic_collaboration.plan_study_end_date = obj_academic_collab.plan_end_date;
                if (obj_academic_collab.actual_start_date != null) academic_collaboration.actual_study_start_date = obj_academic_collab.actual_start_date;
                if (obj_academic_collab.actual_end_date != null) academic_collaboration.actual_study_end_date = obj_academic_collab.actual_end_date;
                academic_collaboration.is_supported = obj_academic_collab.support;
                if (obj_academic_collab.note != null) academic_collaboration.note = obj_academic_collab.note;
                db.AcademicCollaborations.Add(academic_collaboration);
                db.SaveChanges();
            }
            catch (Exception e)
            {
                throw e;
            }
            return academic_collaboration;
        }

        public void SaveCollabStatusHistory(HttpPostedFileBase evidence, int collab_id, int collab_status_id, string note, File evidence_file, int account_id)
        {
            CollaborationStatusHistory collab_status_hist = new CollaborationStatusHistory();
            try
            {
                //add infor to CollaborationStatusHistory
                collab_status_hist.collab_id = collab_id;
                collab_status_hist.collab_status_id = collab_status_id;
                collab_status_hist.change_date = DateTime.Now;
                if (note != null) collab_status_hist.note = note;
                if (evidence_file != null && evidence_file.file_id == 0)
                {
                    evidence_file = null;
                }
                if (evidence != null || evidence_file != null) collab_status_hist.file_id = evidence_file.file_id;
                collab_status_hist.account_id = account_id;
                db.CollaborationStatusHistories.Add(collab_status_hist);
                db.SaveChanges();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        //EDIT
        public AlertModal<AcademicCollaboration_Ext> GetAcademicCollaboration(int direction, int collab_type_id, int acad_collab_id)
        {
            try
            {
                db.Configuration.LazyLoadingEnabled = false;
                var sql = @"select
                            collab.collab_id, collab.partner_scope_id, collab.collab_type_id, pp.people_id, pp.people_id, pp.[name] 'people_name', pp.email, offi.office_id, offi.office_name,
                            pn.partner_id, pn.partner_name, c.country_id, c.country_name, cs.scope_id, cs.scope_name,
                            collab.plan_study_start_date, collab.plan_study_end_date, csh.file_id, csh.file_name, csh.file_link, csh.file_drive_id, collab.actual_study_start_date, collab.actual_study_end_date,
                            acs.collab_status_id, acs.collab_status_name,
                            collab.is_supported, collab.note
                            from IA_AcademicCollaboration.AcademicCollaboration collab
                            join IA_Collaboration.PartnerScope mpc on collab.partner_scope_id = mpc.partner_scope_id
                            join IA_Collaboration.[Partner] pn on pn.partner_id = mpc.partner_id
							join IA_Collaboration.CollaborationScope cs on cs.scope_id = mpc.scope_id
                            left join General.Country c on c.country_id = pn.country_id
                            join General.People pp on collab.people_id = pp.people_id 
                            left join General.[Profile] pf on pf.people_id = pp.people_id
                            left join General.Office offi on pp.office_id = offi.office_id
                            join (select csh1.collab_id, csh2.collab_status_id, csh1.change_date, csh2.file_id, csh2.file_name, csh2.file_link, csh2.file_drive_id
		                            from 
		                            (select csh1.collab_id, MAX(csh1.change_date) 'change_date' 
			                            from IA_AcademicCollaboration.CollaborationStatusHistory csh1
			                            group by csh1.collab_id) as csh1
		                            join 
		                            (select csh2.collab_status_id, csh2.collab_id, csh2.change_date, fi.file_id, fi.name 'file_name', fi.link 'file_link', fi.file_drive_id
			                            from IA_AcademicCollaboration.CollaborationStatusHistory csh2
										left join General.[File] fi on fi.file_id = csh2.file_id) as csh2 
		                            on csh1.collab_id = csh2.collab_id and csh1.change_date = csh2.change_date) as csh 
                            on csh.collab_id = collab.collab_id
                            join IA_AcademicCollaboration.AcademicCollaborationStatus acs on acs.collab_status_id = csh.collab_status_id
                            where collab.direction_id = @direction /*Dài hạn = 2, Ngắn hạn = 1*/ and collab.collab_type_id = @collab_type_id /*Chiều đi = 1, Chiều đến = 2*/
                            and collab.collab_id = @collab_id";
                AcademicCollaboration_Ext academicCollaboration = db.Database.SqlQuery<AcademicCollaboration_Ext>(sql,
                    new SqlParameter("direction", direction),
                    new SqlParameter("collab_type_id", collab_type_id),
                    new SqlParameter("collab_id", acad_collab_id)).FirstOrDefault();
                if (academicCollaboration != null)
                {
                    return new AlertModal<AcademicCollaboration_Ext>(academicCollaboration, true, null, null);
                }
                else
                {
                    return new AlertModal<AcademicCollaboration_Ext>(false, "Có lỗi xảy ra khi lấy dữ liệu hợp tác học thuật tương ứng");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return new AlertModal<AcademicCollaboration_Ext>(false, "Lấy dữ liệu về hợp tác học thuật đã có lỗi xảy ra.");
            }
        }

        //public Google.Apis.Drive.v3.Data.File updateEvidenceFile(File old_evidence, HttpPostedFileBase new_evidence, string folder_name, int type_folder, bool is_folder)
        //{
        //    Google.Apis.Drive.v3.Data.File f = new Google.Apis.Drive.v3.Data.File();
        //    //update file
        //    if (new_evidence != null)
        //    {
        //        //add new file
        //        f = uploadEvidenceFile(new_evidence, folder_name, type_folder, is_folder);
        //    }
        //    else if (new_evidence == null && old_evidence.file_id != 0)
        //    {
        //        GoogleDriveService.DeleteFile(old_evidence.file_drive_id);
        //    }
        //    return f;
        //}

        public AlertModal<AcademicCollaboration_Ext> UpdateAcademicCollaboration(int direction_id, int collab_type_id,
            SaveAcadCollab_Person obj_person,
            SaveAcadCollab_Partner obj_partner,
            SaveAcadCollab_AcademicCollaboration obj_academic_collab,
            HttpPostedFileBase new_evidence, string file_action, string folder_name,
            int account_id)
        {
            AutoActiveInactive autoActiveInactive = new AutoActiveInactive();
            AcademicCollaboration academicCollaboration = new AcademicCollaboration();
            Person person;
            var person_id = obj_person.person_id;
            Partner partner;
            var partner_id = obj_partner.partner_id;
            var partner_scope_id = 0;
            PartnerScope partner_scope = new PartnerScope();
            PartnerScope old_partner_scope = new PartnerScope();
            using (DbContextTransaction trans = db.Database.BeginTransaction())
            {
                try
                {
                    //check duplicate academic collaboration: person, partner, collab_scope base on time
                    if (CheckDuplicateAcademicCollaboration(obj_person, obj_partner, obj_academic_collab))
                    {
                        //check available person
                        if (!obj_person.available_person)
                        {
                            person = SavePerson(obj_person);
                        }
                        else
                        {
                            person = UpdatePerson(obj_person);
                        }
                        person_id = person.people_id;
                        //check available partner
                        if (!obj_partner.available_partner)
                        {
                            //check country_id with Country
                            var country = db.Countries.Find(obj_partner.partner_country_id);
                            if (country == null)
                            {
                                return new AlertModal<AcademicCollaboration_Ext>(false, "Không thêm được quốc gia tương ứng.");
                            }
                            else
                            {
                                var article = SaveArticle(account_id);
                                //add ArticleVersion
                                SaveArticleVersion(obj_partner, article);
                                //add Partner
                                partner = SavePartner(obj_partner, article);
                                //get corresponsing partner_id value
                                partner_id = partner.partner_id;
                                //add partner_id & scope_id to PartnerScope
                                partner_scope = SavePartnerScope(partner_id, obj_partner.collab_scope_id);
                                //get corresponding partner_scope_id
                                partner_scope_id = partner_scope.partner_scope_id;
                            }
                        }
                        else
                        {
                            //check exist partner_scope
                            partner_scope = db.PartnerScopes.Where<PartnerScope>(x => x.partner_id == partner_id && x.scope_id == obj_partner.collab_scope_id).FirstOrDefault();
                            if (partner_scope != null)
                            {
                                AcademicCollaboration ac = db.AcademicCollaborations.Find(obj_academic_collab.collab_id);
                                if (ac.partner_scope_id != partner_scope.partner_scope_id)
                                {
                                    //decrease ref_coou of old partner_scope
                                    old_partner_scope = db.PartnerScopes.Find(ac.partner_scope_id);
                                    DecreaseReferenceCountOfPartnerScope(old_partner_scope);
                                    //update infor to AcademicCollaboration
                                    academicCollaboration = UpdateAcademicCollaboration(direction_id, collab_type_id, person_id, partner_scope.partner_scope_id, obj_academic_collab);
                                    //add file and collab staus history
                                    SaveFileAndCollabHistory(new_evidence, file_action, folder_name, academicCollaboration.collab_id, obj_academic_collab.status_id, null, account_id);
                                    //incease 1 to new referecen_count PartnerScope
                                    IncreaseReferenceCountOfPartnerScope(partner_scope);
                                    if (old_partner_scope.reference_count <= 0)
                                    {
                                        db.PartnerScopes.Remove(old_partner_scope);
                                    }
                                    db.SaveChanges();
                                }
                                else
                                {
                                    //update infor to AcademicCollaboration
                                    academicCollaboration = UpdateAcademicCollaboration(direction_id, collab_type_id, person_id, partner_scope.partner_scope_id, obj_academic_collab);
                                    //add file and collab staus history
                                    SaveFileAndCollabHistory(new_evidence, file_action, folder_name, academicCollaboration.collab_id, obj_academic_collab.status_id, null, account_id);
                                }
                            }
                            else
                            {
                                //decrease ref_coou of old partner_scope
                                AcademicCollaboration ac = db.AcademicCollaborations.Where(x => x.collab_id == obj_academic_collab.collab_id).FirstOrDefault();
                                old_partner_scope = db.PartnerScopes.Where(x => x.partner_scope_id == ac.partner_scope_id).FirstOrDefault();
                                DecreaseReferenceCountOfPartnerScope(old_partner_scope);

                                //add partner_id & scope_id to PartnerScope
                                partner_scope = SavePartnerScope(partner_id, obj_partner.collab_scope_id);
                                //update infor to AcademicCollaboration
                                academicCollaboration = UpdateAcademicCollaboration(direction_id, collab_type_id, person_id, partner_scope.partner_scope_id, obj_academic_collab);
                                //add file and collab staus history
                                SaveFileAndCollabHistory(new_evidence, file_action, folder_name, academicCollaboration.collab_id, obj_academic_collab.status_id, null, account_id);
                                //delete 0 ref_cou partner_scope
                                if (old_partner_scope.reference_count <= 0)
                                {
                                    db.PartnerScopes.Remove(old_partner_scope);
                                }
                                db.SaveChanges();
                            }
                        }
                        trans.Commit();

                        using (DbContextTransaction dbContext = db.Database.BeginTransaction())
                        {
                            try
                            {
                                //change status corressponding MOU/MOA
                                if (old_partner_scope.partner_scope_id != 0)
                                {
                                    List<int> list_old_partner_scope_id = new List<int>
                                    {
                                        old_partner_scope.partner_scope_id
                                    };
                                    autoActiveInactive.changeStatusMOUMOA(list_old_partner_scope_id, db);
                                }
                                //change status corressponding MOU/MOA
                                List<int> list_new_partner_scope_id = new List<int>
                                {
                                    partner_scope.partner_scope_id
                                };
                                autoActiveInactive.changeStatusMOUMOA(list_new_partner_scope_id, db);
                                dbContext.Commit();
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.ToString());
                                dbContext.Rollback();
                                return new AlertModal<AcademicCollaboration_Ext>(null, false, "Có lỗi xảy ra khi tự động active/inactive MOU/MOA.");
                            }
                        }
                        return new AlertModal<AcademicCollaboration_Ext>(null, true, "Cập nhật cán bộ giảng viên thành công.");
                    }
                    else
                    {
                        AlertModal<AcademicCollaboration_Ext> alertModal = new AlertModal<AcademicCollaboration_Ext>(null, false, "Cảnh báo", "Với thời gian kế hoạch, CBGV đang đi học tại đối tác.");
                        return alertModal;
                    }
                }
                catch (Exception e)
                {
                    trans.Rollback();
                    throw e;
                }
            }
        }

        public AcademicCollaboration UpdateAcademicCollaboration(int direction_id, int collab_type_id, int person_id, int partner_scope_id, SaveAcadCollab_AcademicCollaboration obj_academic_collab)
        {
            try
            {
                //update infor to AcademicCollaboration
                AcademicCollaboration academicCollaboration = db.AcademicCollaborations.Find(obj_academic_collab.collab_id);
                academicCollaboration.direction_id = direction_id;
                academicCollaboration.collab_type_id = collab_type_id;
                academicCollaboration.people_id = person_id;
                academicCollaboration.partner_scope_id = partner_scope_id;
                academicCollaboration.plan_study_start_date = obj_academic_collab.plan_start_date;
                academicCollaboration.plan_study_end_date = obj_academic_collab.plan_end_date;
                academicCollaboration.actual_study_start_date = obj_academic_collab.actual_start_date;
                academicCollaboration.actual_study_end_date = obj_academic_collab.actual_end_date;
                academicCollaboration.is_supported = obj_academic_collab.support;
                academicCollaboration.note = obj_academic_collab.note;
                db.SaveChanges();
                return academicCollaboration;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        //DELETE
        public AlertModal<string> DeleteAcademicCollaboration(int acad_collab_id)
        {
            using (DbContextTransaction trans = db.Database.BeginTransaction())
            {
                try
                {
                    AutoActiveInactive autoActiveInactive = new AutoActiveInactive();
                    AcademicCollaboration academicCollaboration = db.AcademicCollaborations.Find(acad_collab_id);
                    int partner_scope_id = academicCollaboration.partner_scope_id;

                    //delete AcademicCollab
                    db.AcademicCollaborations.Remove(academicCollaboration);
                    db.SaveChanges();
                    //decrease reference_count in PartnerScope
                    PartnerScope partnerScope = db.PartnerScopes.Find(partner_scope_id);
                    if (partnerScope != null)
                    {
                        DecreaseReferenceCountOfPartnerScope(partnerScope);
                        db.SaveChanges();
                    }
                    //delete partner_scope records with reference_count = 0
                    if (partnerScope.reference_count <= 0)
                    {
                        db.PartnerScopes.Remove(partnerScope);
                        db.SaveChanges();
                    }
                    trans.Commit();

                    //change status corressponding MOU/MOA
                    using (DbContextTransaction dbContext = db.Database.BeginTransaction())
                    {
                        try
                        {
                            List<int> list_partner_scope_id = new List<int>
                            {
                                partner_scope_id
                            };
                            autoActiveInactive.changeStatusMOUMOA(list_partner_scope_id, db);
                            dbContext.Commit();
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.ToString());
                            dbContext.Rollback();
                            return new AlertModal<string>(null, false, "Có lỗi xảy ra khi tự động active/inactive MOU/MOA.");
                        }
                    }

                    return new AlertModal<string>(null, true, "Thành công", "Xóa hợp tác học thuật thành công.");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    trans.Rollback();
                    return new AlertModal<string>(null, false, "Lỗi", "Có lỗi xảy ra.");
                }
            }
        }

        //VIEW STATUS HISTORY
        public BaseServerSideData<StatusHistory> GetStatusHistories(BaseDatatable baseDatatable, int collab_id)
        {
            try
            {
                var sql = @"select csh.change_date, acs.collab_status_id, a.full_name, 
                            ISNULL(f.name, '') 'file_name', ISNULL(f.link, '') 'file_link', csh.note
                            from IA_AcademicCollaboration.CollaborationStatusHistory csh
                            join IA_AcademicCollaboration.AcademicCollaborationStatus acs on acs.collab_status_id = csh.collab_status_id
                            join General.Account a on a.account_id = csh.account_id
                            left join General.[File] f on f.[file_id] = csh.[file_id]
                            where csh.collab_id = @collab_id
                            ORDER BY " + baseDatatable.SortColumnName + " " + baseDatatable.SortDirection +
                            " OFFSET " + baseDatatable.Start + " ROWS FETCH NEXT " + baseDatatable.Length + " ROWS ONLY";
                List<StatusHistory> statusHistory = db.Database.SqlQuery<StatusHistory>(sql, new SqlParameter("collab_id", collab_id)).ToList();
                int totalRecords = db.CollaborationStatusHistories.Where(x => x.collab_id == collab_id).Count();
                return new BaseServerSideData<StatusHistory>(statusHistory, totalRecords);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return null;
            }
        }

        //CHANGE STATUS HISTORY
        public AlertModal<string> ChangeStatus(int collab_id, HttpPostedFileBase evidence_file, string folder_name, string status_id, string note, int account_id)
        {
            using (DbContextTransaction dbContext = db.Database.BeginTransaction())
            {
                try
                {
                    if (!(String.IsNullOrEmpty(status_id)))
                    {
                        int num_status_id = int.Parse(status_id);
                        //add file and collab staus history
                        SaveFileAndCollabHistory(evidence_file, "add", folder_name, collab_id, num_status_id, note, account_id);
                        db.SaveChanges();
                        dbContext.Commit();
                        return new AlertModal<string>(null, true, "Thành công", "Chuyển trạng thái hợp tác học thuật thành công.");
                    }
                    else
                    {
                        return new AlertModal<string>(null, false, "Lỗi", "Thông tin về trạng thái chưa được chọn lựa.");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    dbContext.Rollback();
                    return null;
                }
            }
        }
    }
}
