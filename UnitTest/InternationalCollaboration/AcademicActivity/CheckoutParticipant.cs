﻿using BLL.InternationalCollaboration.AcademicActivity;
using ENTITIES;
using ENTITIES.CustomModels;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTest.InternationalCollaboration.AcademicActivity
{
    [TestFixture]
    public class CheckoutParticipant
    {
        ScienceAndInternationalAffairsEntities db = new ScienceAndInternationalAffairsEntities();
        [TestCase]
        public void CheckoutParticipantUT_1()
        {
            new CheckinParticipant().CheckinParticipantUT_1();
            Participant p = db.Participants.Last();
            CheckInRepo repo = new CheckInRepo();
            bool res = repo.Checkout(p.participant_id);
            if (res)
                Assert.Pass();
        }
        [TestCase]
        public void CheckoutParticipantUT_2()
        {
            CheckInRepo repo = new CheckInRepo();
            bool res = repo.Checkout(0);
            if (res)
                Assert.Pass();
        }
    }
}