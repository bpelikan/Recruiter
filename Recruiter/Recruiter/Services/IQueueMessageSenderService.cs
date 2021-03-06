﻿using Recruiter.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Recruiter.Services
{
    public interface IQueueMessageSenderService
    {
        Task SendAppointmentReminderAsync(InterviewAppointment appointment, int time);
    }
}
