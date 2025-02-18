﻿using BLL.ModelDAL;
using ENTITIES;
using ENTITIES.CustomModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace BLL.ScienceManagement.ConferenceSponsor
{
    public class ConferenceSponsorEditRepo
    {
        ScienceAndInternationalAffairsEntities db;
        public AlertModal<int> EditRequestConference(int account_id, string input, HttpPostedFileBase invite, HttpPostedFileBase paper, int request_id)
        {
            db = new ScienceAndInternationalAffairsEntities();

            RequestConference request = db.RequestConferences.Where(x => x.request_id == request_id && x.BaseRequest.account_id == account_id).FirstOrDefault();
            if (request == null)
                return new AlertModal<int>(false, "Đề nghị không tồn tại");
            if (request.status_id >= 2 || !request.editable)
                return new AlertModal<int>(false, "Đề nghị không thể chỉnh sửa");

            List<string> FileIDs = new List<string>();
            using (DbContextTransaction trans = db.Database.BeginTransaction())
            {
                try
                {
                    DataTable dt = new DataTable();
                    JObject @object = JObject.Parse(input);

                    Conference conference = @object["Conference"].ToObject<Conference>();
                    Conference temp = db.Conferences.Find(conference.conference_id);
                    if (temp != null)
                    {
                        request.conference_id = temp.conference_id;
                        if (!temp.is_verified)
                        {
                            temp.website = conference.website;
                            temp.keynote_speaker = conference.keynote_speaker;
                            temp.qs_university = conference.qs_university;
                            temp.country_id = conference.country_id;
                            temp.time_start = conference.time_start;
                            temp.time_end = conference.time_end;
                            temp.formality_id = conference.formality_id;
                            temp.co_organized_unit = conference.co_organized_unit;
                        }
                    }
                    else
                    {
                        conference.is_verified = false;
                        db.Conferences.Add(conference);
                        db.SaveChanges();
                        request.conference_id = conference.conference_id;
                    }

                    if (invite != null)
                    {
                        GoogleDriveService.UpdateFile(invite.FileName, invite.InputStream, invite.ContentType, request.File.file_drive_id);
                        request.File.name = invite.FileName;
                    }
                    if (paper != null)
                    {
                        GoogleDriveService.UpdateFile(paper.FileName, paper.InputStream, paper.ContentType, request.Paper.File.file_drive_id);
                        request.Paper.File.name = paper.FileName;
                    }

                    request.editable = false;
                    request.attendance_start = DateTime.Parse(@object["attendance_start"].ToString());
                    request.attendance_end = DateTime.Parse(@object["attendance_end"].ToString());
                    request.specialization_id = int.Parse(@object["specialization_id"].ToString());

                    db.Costs.RemoveRange(request.Costs);
                    List<Cost> costs = @object["Cost"].ToObject<List<Cost>>();
                    foreach (var item in costs)
                    {
                        int total = int.Parse(dt.Compute(item.detail.Replace(",", ""), "").ToString());
                        item.editable = false;
                        item.sponsoring_organization = "FPTU";
                        item.total = total;
                        item.request_id = request.request_id;
                    }
                    db.Costs.AddRange(costs);

                    db.ConferenceParticipants.RemoveRange(request.ConferenceParticipants);

                    ConferenceParticipant participant = @object["ConferenceParticipant"].ToObject<ConferenceParticipant>();
                    participant.request_id = request_id;
                    db.ConferenceParticipants.Add(participant);

                    foreach (var item in request.EligibilityConditions)
                    {
                        item.is_accepted = false;
                    }
                    db.SaveChanges();

                    trans.Commit();
                    return new AlertModal<int>(request.request_id, false);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    trans.Rollback();
                    foreach (var item in FileIDs)
                    {
                        GoogleDriveService.DeleteFile(item);
                    }
                    return new AlertModal<int>(false, "Có lỗi xảy ra");
                }
            }
        }
    }
}
