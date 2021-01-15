using Google.Apis.Auth.AspNetCore3;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Classroom.v1;
using Google.Apis.Classroom.v1.Data;
using Google.Apis.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GoogleClassRoomPOC.Controllers
{
    public class TeacherController : Controller
    {
        
        public ClassroomService GetGoogleClassRoomService([FromServices] IGoogleAuthProvider auth)
        {
            GoogleCredential cred = auth.GetCredentialAsync().GetAwaiter().GetResult();
            ClassroomService service = new ClassroomService(new BaseClientService.Initializer
            {
                HttpClientInitializer = cred,
                ApplicationName = "WebCraft",
            });
            return service;
        }

        // GET: TeacherController
        [GoogleScopedAuthorize(ClassroomService.ScopeConstants.ClassroomCourses,
            ClassroomService.ScopeConstants.ClassroomCourseworkMe, 
            ClassroomService.ScopeConstants.ClassroomCourseworkStudents
            , ClassroomService.ScopeConstants.ClassroomProfilePhotos,
            ClassroomService.ScopeConstants.ClassroomRostersReadonly,
            ClassroomService.ScopeConstants.ClassroomProfileEmails)]
        public ActionResult Index([FromServices] IGoogleAuthProvider auth)
        {
            var myCources = new List<Course>();
            string pageToken = null;
            do
            {
                var request = this.GetGoogleClassRoomService(auth).Courses.List();
                request.PageSize = 100;
                request.PageToken = pageToken;
                request.TeacherId = "me";
                var response = request.Execute();
                myCources.AddRange(response.Courses);
                pageToken = response.NextPageToken;

            } while (pageToken != null);

            List<Models.Course> CourseList = new List<Models.Course>();

            if (myCources.Count > 0)
            {
                foreach (Course item in myCources)
                {
                    if (item.CourseState == "ACTIVE")
                    {
                        CourseList.Add(
                            new Models.Course()
                            {
                                Id = item.Id,
                                Name = item.Name,
                                Room = item.Room,
                                Section = item.Section,
                                AlternateLink = item.AlternateLink,
                                CourseGroupEmail = item.CourseGroupEmail,
                                TeacherGroupEmail = item.TeacherGroupEmail
                            });
                    }
                }
            }
            return View(CourseList);
        }

        public ActionResult CreateCourse()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateCourse(IFormCollection collection,
            [FromServices] IGoogleAuthProvider auth)
        {
            string name = collection["Name"];
            string room = collection["Room"];
            string section = collection["Section"];

            Link link = new Link()
            {
                Url = "https://webpack.js.org/loaders/css-loader/#sourcemap"
            };
            List<CourseMaterial> materials = new List<CourseMaterial>()
            {
               new CourseMaterial()
               {
                   Link = link
               }
            };
            List<CourseMaterialSet> materialSets = new List<CourseMaterialSet>()
            {
                new CourseMaterialSet()
                {
                    Materials = materials
                }
            };

            Course course = new Course()
            {
                Name = name,
                Section = section,
                Room = room,
                CourseState = "ACTIVE",
                CourseMaterialSets = materialSets,
                OwnerId = "me"
            };

            var request = this.GetGoogleClassRoomService(auth).Courses.Create(course);
            request.Execute();

            return RedirectToAction(nameof(Index));
        }

        public ActionResult AddCourseWork(string courseId)
        {
            Models.CourseWork courseWork = new Models.CourseWork()
            {
                CourseId = courseId
            };

            return View(courseWork);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddCourseWork(IFormCollection collection,
            [FromServices] IGoogleAuthProvider auth)
        {
            string title = collection["Title"];
            string description = collection["Description"];
            string courseId = collection["CourseId"];
            List<Material> materials = new List<Material>()
            {
                new Material()
                {
                    
                    Link = new Link()
                    {
                        Url = "https://www.researchgate.net/publication/303182670_Minimum_Viable_Product_or_Multiple_Facet_Product_The_Role_of_MVP_in_Software_Startups",   
                    }
                }
            };

            CourseWork courseWork = new CourseWork()
            {
                CourseId = courseId,
                Title = title,
                Description = description,
                WorkType = "ASSIGNMENT",
                MaxPoints = 100,
                State = "PUBLISHED",
                AssociatedWithDeveloper = true,
                Materials = materials,
            };
            var request = this.GetGoogleClassRoomService(auth).Courses.CourseWork.Create(courseWork, courseId).Execute();
            return Redirect(request.AlternateLink);
        }

        public ActionResult GetCourseWorks(string courseId,
            string className,
            [FromServices] IGoogleAuthProvider auth)
        {
            var classRoomService = this.GetGoogleClassRoomService(auth);

            var courseWorks = new List<CourseWork>();
            string pageToken = null;
            do
            {
                var request = classRoomService.Courses.CourseWork.List(courseId);
                request.PageSize = 30;
                request.PageToken = pageToken;
                var response = request.Execute();
                if(response.CourseWork != null)
                {
                    courseWorks.AddRange(response.CourseWork);
                }

            } while (pageToken != null);

            List<Models.CourseWork> CourseWorkList = new List<Models.CourseWork>();
            foreach (CourseWork item in courseWorks)
            {
                CourseWorkList.Add(
                    new Models.CourseWork()
                    {
                        CourseWorkId = item.Id,
                        CourseId = courseId,
                        Type = "Assigmment",
                        Title = item.Title,
                        Description = item.Description,
                    });
            }
            ViewData["ClassName"] = className;
            ViewData["CourseId"] = courseId;
            return View(CourseWorkList);
        }

        public ActionResult GetSubmissions(string courseId,
            string courseWorkId,
            [FromServices] IGoogleAuthProvider auth)
        {
            var googleService = this.GetGoogleClassRoomService(auth);

            var students = googleService
                .Courses
                .Students
                .List(courseId)
                .Execute();

            var submissions = googleService
                .Courses
                .CourseWork
                .StudentSubmissions
                .List(courseId, courseWorkId)
                .Execute();

            var course = googleService.Courses.Get(courseId).Execute();
            var courseWork = googleService.Courses.CourseWork.Get(courseId, courseWorkId).Execute();

            List<Models.Student> studentListModel = new List<Models.Student>();
            Models.Student modelStudent = new Models.Student();

            if(submissions.StudentSubmissions != null && submissions.StudentSubmissions.Count > 0)
            {
                foreach (var item in submissions.StudentSubmissions)
                {
                    var studentSubmission = googleService
                       .Courses
                       .CourseWork
                       .StudentSubmissions
                       .Get(courseId, courseWorkId, item.Id)
                       .Execute();

                    string studentId = studentSubmission.UserId;
                    var student = students.Students.Where(s => s.UserId == studentId).FirstOrDefault();

                    modelStudent = new Models.Student()
                    {
                        Id = item.Id,
                        Name = student.Profile.Name.FullName,
                        Grade = string.IsNullOrEmpty(item.AssignedGrade.ToString()) ? "00" : item.AssignedGrade.ToString(),
                        Assignments = new List<Models.Assignment>()
                    {
                        new Models.Assignment() { Url = item.AssignmentSubmission.Attachments[0].Link.Url }
                    },
                        Class = course.Name,
                        Section = course.Section,
                    };
                    studentListModel.Add(modelStudent);
                }
            }

            ViewData["ClassName"] = course.Name;
            ViewData["Section"] = course.Section;
            ViewData["Assignment"] = courseWork.Title;

            return View(studentListModel);
        }
        // GET: TeacherController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: TeacherController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: TeacherController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: TeacherController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: TeacherController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: TeacherController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: TeacherController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
