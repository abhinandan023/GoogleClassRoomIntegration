using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GoogleClassRoomPOC.Models
{
    public class Student
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Class { get; set; }
        public string Section { get; set; }
        public string Grade { get; set; }
        public IList<Assignment> Assignments { get; set; }
    }

    public class Assignment
    {
        public string Id { get; set; }
        public string Url { get; set; }
      
    }
}
