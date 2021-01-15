using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GoogleClassRoomPOC.Models
{
    public class CourseWork
    {
        public string CourseId { get; set; }
        public string CourseWorkId { get; set; }
        public string Type { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Answer { get; set; }
    }
}
