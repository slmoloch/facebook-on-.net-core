using System;
using System.ComponentModel.DataAnnotations;

namespace facebook_demo.Services
{
    public class PostDto
    {
        [Display(Name = "Id")]
        public string Id { get; set; }

        [Display(Name = "Message")]
        public string Message { get; set; }

        [Display(Name = "Is Published")]
        public bool Published { get; set; }

        [Display(Name = "Publish On")]
        public string PublishOn { get; set; }
    }

    public class PageMetricDto
    {
        public string Id { get; set; }

        public string Value { get; set; }
    }
}