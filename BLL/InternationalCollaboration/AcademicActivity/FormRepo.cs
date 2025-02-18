﻿using ENTITIES;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;

namespace BLL.InternationalCollaboration.AcademicActivity
{
    public class FormRepo
    {
        public static string GuestURL;
        public static string ManagerURL;
        private readonly ScienceAndInternationalAffairsEntities db = new ScienceAndInternationalAffairsEntities();
        public DetailOfAcademicActivityRepo.baseForm GetFormbyPhase(int phase_id)
        {
            try
            {
                db.Configuration.ProxyCreationEnabled = false;
                Form f = db.Forms.Where(x => x.phase_id == phase_id).FirstOrDefault();
                string sql = @"SELECT q.question_id, q.title, at.answer_type_id,cast(q.is_compulsory as int) as is_compulsory,cast(q.is_changeable as int) as is_changeable 
                                    FROM SMIA_AcademicActivity.Form f
                                    INNER JOIN SMIA_AcademicActivity.Question q ON q.form_id = f.form_id
                                    INNER JOIN SMIA_AcademicActivity.AnswerType at ON at.answer_type_id = q.answer_type_id
                                    where f.phase_id = @phase_id order by q.is_changeable";
                List<DetailOfAcademicActivityRepo.Ques> ques = db.Database.SqlQuery<DetailOfAcademicActivityRepo.Ques>(sql, new SqlParameter("phase_id", phase_id)).ToList();
                string ques_id = "";
                List<int> type = new List<int> { 3, 5 };
                foreach (DetailOfAcademicActivityRepo.Ques q in ques)
                {
                    if (type.Contains(q.answer_type_id))
                    {
                        ques_id += q.question_id + ",";
                    }
                }
                List<DetailOfAcademicActivityRepo.QuesOption> quesOption = new List<DetailOfAcademicActivityRepo.QuesOption>();
                if (!String.IsNullOrEmpty(ques_id))
                {
                    ques_id = ques_id.Remove(ques_id.Length - 1);
                    sql = @"select qo.* from SMIA_AcademicActivity.QuestionOption qo where qo.question_id in (" + ques_id + ")";
                    quesOption = db.Database.SqlQuery<DetailOfAcademicActivityRepo.QuesOption>(sql).ToList();
                }
                DetailOfAcademicActivityRepo.baseForm data = new DetailOfAcademicActivityRepo.baseForm
                {
                    form = f,
                    ques = ques,
                    quesOption = quesOption
                };
                return data;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return new DetailOfAcademicActivityRepo.baseForm();
            }
        }
        public bool UpdateForm(DetailOfAcademicActivityRepo.baseForm data, List<DetailOfAcademicActivityRepo.CustomQuestion> data_unchange)
        {
            using (DbContextTransaction transaction = db.Database.BeginTransaction())
            {
                try
                {
                    List<Question> questions = db.Questions.Where(x => x.form_id == data.form.form_id).ToList();
                    List<int> quess_id = questions.Select(x => x.question_id).ToList();
                    Form f = db.Forms.Find(data.form.form_id);
                    if (f == null)
                    {
                        f = db.Forms.Add(new Form
                        {
                            title = data.form.title ?? String.Empty,
                            title_description = data.form.title_description ?? String.Empty,
                            phase_id = data.form.phase_id
                        });
                        db.SaveChanges();
                    }
                    else
                    {
                        f.title = data.form.title ?? String.Empty;
                        f.title_description = data.form.title_description ?? String.Empty;
                        db.Entry(f).State = EntityState.Modified;
                    }
                    UpdateQuestion(data, f, quess_id);
                    foreach (DetailOfAcademicActivityRepo.CustomQuestion cq in data_unchange)
                    {
                        db.Questions.Add(new Question
                        {
                            form_id = f.form_id,
                            title = cq.title ?? String.Empty,
                            answer_type_id = 1,
                            is_compulsory = cq.is_compulsory == 1,
                            is_changeable = false
                        });
                    }
                    db.SaveChanges();
                    transaction.Commit();
                    return true;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    transaction.Rollback();
                    return false;
                }
            }
        }
        public void UpdateQuestion(DetailOfAcademicActivityRepo.baseForm data, Form f, List<int> quess_id)
        {
            foreach (DetailOfAcademicActivityRepo.Ques q in data.ques)
            {
                if (quess_id.Contains(q.question_id))
                {
                    UpdateQuesOption(q, data);
                }
                else
                {
                    AddQuesOption(q, data, f);
                }
                quess_id.Remove(q.question_id);
            }
            RemoveQues(quess_id);
        }
        public void UpdateQuesOption(DetailOfAcademicActivityRepo.Ques q, DetailOfAcademicActivityRepo.baseForm data)
        {
            Question qt = db.Questions.Find(q.question_id);
            qt.title = q.title ?? String.Empty;
            qt.answer_type_id = q.answer_type_id;
            qt.is_compulsory = q.is_compulsory == 1;
            qt.is_changeable = true;
            db.Entry(qt).State = EntityState.Modified;
            if (q.answer_type_id == 3 || q.answer_type_id == 5)
            {
                QuestionOption qo = db.QuestionOptions.Where(x => x.question_id == q.question_id).FirstOrDefault();
                if (data.quesOption != null)
                {
                    DetailOfAcademicActivityRepo.QuesOption qon = data.quesOption.Find(x => x.question_id == q.question_id);
                    if (qo != null)
                    {
                        if (qon != null)
                        {
                            qo.option_title = qon.option_title ?? String.Empty;
                            db.Entry(qo).State = EntityState.Modified;
                        }
                        else
                        {
                            db.QuestionOptions.Remove(qo);
                        }
                    }
                    else
                    {
                        if (qon != null)
                        {
                            db.QuestionOptions.Add(new QuestionOption
                            {
                                question_id = q.question_id,
                                option_title = qon.option_title
                            });
                        }
                    }
                    db.SaveChanges();
                }
            }
        }
        public void AddQuesOption(DetailOfAcademicActivityRepo.Ques q, DetailOfAcademicActivityRepo.baseForm data, Form f)
        {
            Question qn = db.Questions.Add(new Question
            {
                form_id = f.form_id,
                answer_type_id = q.answer_type_id,
                title = q.title ?? String.Empty,
                is_compulsory = q.is_compulsory == 1,
                is_changeable = true
            });
            db.SaveChanges();
            if (q.answer_type_id == 3 || q.answer_type_id == 5)
            {
                if (data.quesOption != null)
                {
                    DetailOfAcademicActivityRepo.QuesOption qon = data.quesOption.Find(x => x.question_id == q.question_id);
                    if (qon != null)
                    {
                        db.QuestionOptions.Add(new QuestionOption
                        {
                            question_id = qn.question_id,
                            option_title = qon.option_title
                        });
                    }
                }
            }
        }
        public void RemoveQues(List<int> quess_id)
        {
            foreach (int i in quess_id)
            {
                Question q = db.Questions.Find(i);
                if (q.answer_type_id == 3 || q.answer_type_id == 5)
                {
                    QuestionOption qo = db.QuestionOptions.Where(x => x.question_id == i).FirstOrDefault();
                    if (qo != null)
                    {
                        db.QuestionOptions.Remove(qo);
                    }
                }
                db.Questions.Remove(q);
            }
        }
        public bool DeleteForm(int phase_id)
        {
            using (DbContextTransaction transaction = db.Database.BeginTransaction())
            {
                try
                {
                    Form f = db.Forms.Where(x => x.phase_id == phase_id).FirstOrDefault();
                    db.Forms.Remove(f);
                    db.SaveChanges();
                    transaction.Commit();
                    return true;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    transaction.Rollback();
                    return false;
                }
            }
        }
        public viewResponse GetResponse(int phase_id)
        {
            try
            {
                string sql = @"select q.* from SMIA_AcademicActivity.Question q
                                inner join SMIA_AcademicActivity.Form f on q.form_id = f.form_id
                                where f.phase_id = @phase_id order by q.is_changeable";
                List<Question> ques = db.Database.SqlQuery<Question>(sql, new SqlParameter("phase_id", phase_id)).ToList();
                if (ques.Count == 0)
                {
                    viewResponse rest = new viewResponse
                    {
                        ques = new List<Question>(),
                        res = new List<Answer>()
                    };
                    return rest;
                }
                sql = @"select r.answer from SMIA_AcademicActivity.Form f
                            inner join SMIA_AcademicActivity.Response r on f.form_id = r.form_id
                            where f.phase_id = @phase_id";
                List<Answer> res = db.Database.SqlQuery<Answer>(sql, new SqlParameter("phase_id", phase_id)).ToList();
                viewResponse data = new viewResponse
                {
                    ques = ques,
                    res = res
                };
                return data;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return new viewResponse();
            }
        }
        public bool SendEmailForm(EmailForm data)
        {
            try
            {
                AcademicActivityPhase aap = db.AcademicActivityPhases.Find(data.phase_id);
                string sql = @"select distinct p.email from SMIA_AcademicActivity.AcademicActivityPhase aap
                                inner join SMIA_AcademicActivity.ParticipantRole pr on pr.phase_id = aap.phase_id
                                left join SMIA_AcademicActivity.Participant p on p.participant_role_id = pr.participant_role_id
                                where aap.activity_id = @activity_id and p.email != ''";
                List<string> to = db.Database.SqlQuery<string>(sql, new SqlParameter("activity_id", aap.activity_id)).ToList();
                string uri = "/AcademicActivity/loadForm?pid=" + data.phase_id;
                data.content += "<br/>Link mẫu đăng ký/khảo sát: <a href='" + GuestURL + uri + "' alt='_blank'>Tại đây</a>";
                ENTITIES.CustomModels.SmtpMailService.Send(to, data.title, data.content, true);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return false;
            }
        }
        public class viewResponse
        {
            public List<Question> ques { get; set; }
            public List<Answer> res { get; set; }
        }
        public class Answer
        {
            public string answer { get; set; }
        }
        public class EmailForm
        {
            public int phase_id { get; set; }
            public string title { get; set; }
            public string content { get; set; }
        }
    }
}
