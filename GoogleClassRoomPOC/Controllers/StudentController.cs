
using Google.Apis.Auth.AspNetCore3;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Classroom.v1;
using Google.Apis.Classroom.v1.Data;
using Google.Apis.Docs.v1;
using Google.Apis.Docs.v1.Data;
using Google.Apis.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GoogleClassRoomPOC.Controllers
{
    
    public class StudentController : Controller
    {
        // GET: StudentControlleR
        
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

        public DocsService GetGoogleDocsService([FromServices] IGoogleAuthProvider auth)
        {

            GoogleCredential cred = auth.GetCredentialAsync().GetAwaiter().GetResult();
            DocsService service = new DocsService(new BaseClientService.Initializer
            {
                HttpClientInitializer = cred,
                ApplicationName = "WebCraft",
            });
            return service;
        }

        [GoogleScopedAuthorize(ClassroomService.ScopeConstants.ClassroomCourses,
             ClassroomService.ScopeConstants.ClassroomCourseworkMe, 
            DocsService.ScopeConstants.Documents)]
        public ActionResult Index([FromServices] IGoogleAuthProvider auth)
        {
            var myCources = new List<Course>();
            string pageToken = null;
            do
            {
                var request = this.GetGoogleClassRoomService(auth).Courses.List();
                request.PageSize = 100;
                request.PageToken = pageToken;
                request.StudentId = "me";
                var response = request.Execute();
                myCources.AddRange(response.Courses);
                pageToken = response.NextPageToken;

            } while (pageToken != null);

            List<Models.Course> CourseList = new List<Models.Course>();
            if(myCources.Count > 0)
            {
                foreach (Course item in myCources)
                {
                    if(item.CourseState == "ACTIVE")
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

        // GET: StudentController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: StudentController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: StudentController/Create
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

        // GET: StudentController/Edit/5
        public ActionResult GetAssignment(string id, [FromServices] IGoogleAuthProvider auth)
        {

            var listCourseWorkResponse = new ListCourseWorkResponse();

            string pageToken = null;
            do
            {
                var request = this.GetGoogleClassRoomService(auth).Courses.CourseWork.List(id);
                request.PageSize = 100;
                request.PageToken = pageToken;
                var response = request.Execute();
                listCourseWorkResponse = response;
            } while (pageToken != null);

            List<Models.CourseWork> CourseWorkList = new List<Models.CourseWork>();
           
            foreach (CourseWork item in listCourseWorkResponse.CourseWork)
            {
                CourseWorkList.Add(
                    new Models.CourseWork() {
                    CourseWorkId = item.Id,
                    CourseId = id,
                    Type = "Assigmment",
                    Title = item.Title,
                    Description = item.Description,
                    });
            }
            return View(CourseWorkList);
        }

        public ActionResult EditAssignment(string courseWorkId,
            string CourseId, 
            [FromServices] IGoogleAuthProvider auth)
        {
            var course = this.GetGoogleClassRoomService(auth)
                .Courses.CourseWork.Get(CourseId, courseWorkId)
                .Execute();
            Models.CourseWork CourseWork = new Models.CourseWork()
            {
                CourseWorkId = courseWorkId,
                CourseId = CourseId,
                Title = course.Title,
                Description = course.Description,
            };

            return View(CourseWork);
        }

        public ActionResult UpdateAssignment(IFormCollection collection,
           [FromServices] IGoogleAuthProvider auth)
        {
            string courseId = collection["CourseId"];
            string courseWorkId = collection["CourseWorkId"];
            string answer = collection["Answer"];

            Document document = new Document()
            {
                Title = "My Assignment " + courseWorkId
            };

            var docService = this.GetGoogleDocsService(auth);

            var docReq = docService.Documents.Create(document);
            var docRes = docReq.Execute();

            string documentId = docRes.DocumentId;
            string docUrl = $"https://docs.google.com/document/d/{documentId}/edit";

            DocumentsResource.BatchUpdateRequest batchUpdate = 
                docService
                .Documents
                .BatchUpdate(GenerateGoogleDocText(answer), documentId);

            var batchUpdateResponse = batchUpdate.Execute();

            var submission = this.GetGoogleClassRoomService(auth)
                .Courses
                .CourseWork
                .StudentSubmissions
                .List(courseId, courseWorkId).Execute();

            string submissionId = submission.StudentSubmissions[0].Id;


            List<Attachment> attachments = new List<Attachment>()
            {
                new Attachment()
                {
                    Link = new Google.Apis.Classroom.v1.Data.Link()
                    {
                        Url = docUrl
                    }
                }
            };

            ModifyAttachmentsRequest body = new ModifyAttachmentsRequest()
            {
               AddAttachments = attachments
            };

            var req = this.GetGoogleClassRoomService(auth)
                .Courses
                .CourseWork
                .StudentSubmissions
                .ModifyAttachments(body, courseId, courseWorkId, submissionId).Execute();
            return Redirect(req.AlternateLink);
        }

        // POST: StudentController/Edit/5
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

        // GET: StudentController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: StudentController/Delete/5
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

        private BatchUpdateDocumentRequest GenerateGoogleDocText(string transcription)
        {
            var endOfSegment = new EndOfSegmentLocation();
            var inserttextRequest = new InsertTextRequest
            {
                Text = transcription,
                EndOfSegmentLocation = endOfSegment
            };

            var request = new Request
            {
                InsertText = inserttextRequest
            };

            var requestList = new List<Request>()
                {
                    request
                };

            var updateRequest = new BatchUpdateDocumentRequest
            {
                Requests = requestList
            };

            return updateRequest;
        }
    }
}
