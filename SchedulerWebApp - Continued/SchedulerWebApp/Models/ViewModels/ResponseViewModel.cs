using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace SchedulerWebApp.Models.ViewModels
{
    public class ResponseViewModel
    {
        [HiddenInput(DisplayValue = false)]
        public int ParticipantId { get; set; }

        [HiddenInput(DisplayValue = false)]
        public int EventId { get; set; }

        [HiddenInput(DisplayValue = false)]
        public string ParticipantEmail { get; set; }

        [HiddenInput(DisplayValue = false)]
        public bool Responce { get; set; }

        public bool Availability { get; set; }

        [DataType(DataType.MultilineText)]
        public string Comments { get; set; }

        public string EventsTitle { get; set; }
        public string EventsLocation { get; set; }
        public DateTime EventDate { get; set; }
    }
}