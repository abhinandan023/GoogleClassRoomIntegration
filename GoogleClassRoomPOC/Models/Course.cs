using Google.Apis.Classroom.v1.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GoogleClassRoomPOC.Models
{
    public class Course
    {

        public string Id { get; set; }
        public string Name { get; set; }
        public string Room { get; set; }
        public string Section { get; set; }
        public string AlternateLink { get; set; }
        public string CourseGroupEmail { get; set; }
        public string TeacherGroupEmail { get; set; }
    }
}
