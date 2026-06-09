using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Cloud.Firestore;
using Exam.Domain.Models;
using Exam.Domain.Entities;
using Exam.Application.Data;

namespace Exam.Infrastructure.Data
{
    public class ExamRepository : IExamRepository
    {
        private readonly FirestoreDb _firestoreDb;

        public ExamRepository(FirestoreDb firestoreDb)
        {
            _firestoreDb = firestoreDb;
        }

        private async Task<List<Question>> LoadQuestionsAsync(DocumentReference docRef, Dictionary<string, object> data)
        {
            var questionsList = new List<Question>();
            bool hasQuestions = false;

            // 1. Load from subcollection "questions"
            try
            {
                var subQuestionsSnap = await docRef.Collection("questions").GetSnapshotAsync();
                if (subQuestionsSnap.Documents.Count > 0)
                {
                    var sortedDocs = subQuestionsSnap.Documents
                        .Select(d => new { Doc = d, Order = d.ContainsField("QuestionOrder") ? Convert.ToInt32(d.GetValue<object>("QuestionOrder")) : 0 })
                        .OrderBy(x => x.Order)
                        .Select(x => x.Doc);

                    foreach (var doc in sortedDocs)
                    {
                        var qData = doc.ToDictionary();
                        string qText = qData.TryGetValue("Content", out var content) ? content?.ToString() : (qData.TryGetValue("QuestionText", out var qt) ? qt?.ToString() : "");
                        var opts = qData.TryGetValue("Options", out var optsVal) && optsVal is System.Collections.IEnumerable enumOpts
                            ? enumOpts.Cast<object>().Select(o => o.ToString() ?? "").ToList()
                            : new List<string>();
                        int correctIdx = qData.TryGetValue("CorrectAnswerIndex", out var corr) ? Convert.ToInt32(corr) : (qData.TryGetValue("CorrectOptionIndex", out var coi) ? Convert.ToInt32(coi) : 0);
                        double points = qData.TryGetValue("Points", out var pts) ? Convert.ToDouble(pts) : 1.0;

                        questionsList.Add(new Question(doc.Id, qText, opts, correctIdx, points));
                    }
                    hasQuestions = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading subcollection questions: {ex.Message}");
            }

            // 2. Load from array "Questions" in exam document
            if (!hasQuestions && data.TryGetValue("Questions", out var questionsObj) && questionsObj != null)
            {
                if (questionsObj is System.Collections.IEnumerable enumerable)
                {
                    int index = 0;
                    foreach (var qObj in enumerable)
                    {
                        if (qObj is Dictionary<string, object> qData)
                        {
                            string qId = qData.TryGetValue("QuestionId", out var qid) ? qid?.ToString() ?? index.ToString() : index.ToString();
                            string qText = qData.TryGetValue("Content", out var content) ? content?.ToString() : (qData.TryGetValue("QuestionText", out var qt) ? qt?.ToString() : "");
                            var opts = qData.TryGetValue("Options", out var optsVal) && optsVal is System.Collections.IEnumerable enumOpts
                                ? enumOpts.Cast<object>().Select(o => o.ToString() ?? "").ToList()
                                : new List<string>();
                            int correctIdx = qData.TryGetValue("CorrectAnswerIndex", out var corr) ? Convert.ToInt32(corr) : (qData.TryGetValue("CorrectOptionIndex", out var coi) ? Convert.ToInt32(coi) : 0);
                            double points = qData.TryGetValue("Points", out var pts) ? Convert.ToDouble(pts) : 1.0;

                            questionsList.Add(new Question(qId, qText, opts, correctIdx, points));
                        }
                        index++;
                    }
                    hasQuestions = true;
                }
            }

            // 3. Load from "QuestionIds" mapped to "questions" collection
            if (!hasQuestions && data.TryGetValue("QuestionIds", out var qIdsObj) && qIdsObj is System.Collections.IEnumerable qIdsEnum)
            {
                foreach (var qIdObj in qIdsEnum)
                {
                    var qId = qIdObj?.ToString();
                    if (string.IsNullOrEmpty(qId)) continue;
                    try
                    {
                        var qDoc = await _firestoreDb.Collection("questions").Document(qId).GetSnapshotAsync();
                        if (qDoc.Exists)
                        {
                            var qData = qDoc.ToDictionary();
                            string qText = qData.TryGetValue("Content", out var content) ? content?.ToString() : (qData.TryGetValue("QuestionText", out var qt) ? qt?.ToString() : "");
                            var opts = qData.TryGetValue("Options", out var optsVal) && optsVal is System.Collections.IEnumerable enumOpts
                                ? enumOpts.Cast<object>().Select(o => o.ToString() ?? "").ToList()
                                : new List<string>();
                            int correctIdx = qData.TryGetValue("CorrectAnswerIndex", out var corr) ? Convert.ToInt32(corr) : (qData.TryGetValue("CorrectOptionIndex", out var coi) ? Convert.ToInt32(coi) : 0);
                            double points = qData.TryGetValue("Points", out var pts) ? Convert.ToDouble(pts) : 1.0;

                            questionsList.Add(new Question(qDoc.Id, qText, opts, correctIdx, points));
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error loading question {qId}: {ex.Message}");
                    }
                }
            }

            return questionsList;
        }

        private Exam.Domain.Models.Exam MapToExam(string id, Dictionary<string, object> data, List<Question> questions)
        {
            var exam = new Exam.Domain.Models.Exam
            {
                Id = id,
                ClassId = data.TryGetValue("ClassId", out var cid) ? cid?.ToString() ?? "" : "",
                ClassName = data.TryGetValue("ClassName", out var cname) ? cname?.ToString() ?? "" : "",
                Title = data.TryGetValue("Title", out var title) ? title?.ToString() ?? "" : "",
                DurationMinutes = data.TryGetValue("DurationMinutes", out var dur) ? Convert.ToInt32(dur) : 0,
                TimeLimitMinutes = data.TryGetValue("TimeLimitMinutes", out var tlm) ? Convert.ToInt32(tlm) : 0,
                Description = data.TryGetValue("Description", out var desc) ? desc?.ToString() ?? "" : "",
                PassingScore = data.TryGetValue("PassingScore", out var ps) ? Convert.ToDouble(ps) : 0.0,
                IsPublished = data.TryGetValue("IsPublished", out var ip) && Convert.ToBoolean(ip),
                IsActive = data.TryGetValue("IsActive", out var ia) && Convert.ToBoolean(ia),
                AllowReview = data.TryGetValue("AllowReview", out var ar) && Convert.ToBoolean(ar),
                RandomizeQuestions = data.TryGetValue("RandomizeQuestions", out var rq) && Convert.ToBoolean(rq),
                ShowScore = data.TryGetValue("ShowScore", out var ss) && Convert.ToBoolean(ss),
                AllowMultipleAttempts = data.TryGetValue("AllowMultipleAttempts", out var ama) && Convert.ToBoolean(ama),
                MaxAttempts = data.TryGetValue("MaxAttempts", out var ma) ? Convert.ToInt32(ma) : 0,
                TotalQuestions = data.TryGetValue("TotalQuestions", out var tq) ? Convert.ToInt32(tq) : 0,
                CreatedAt = data.TryGetValue("CreatedAt", out var ca) && ca is Timestamp caTs ? caTs.ToDateTime() : DateTime.UtcNow,
                UpdatedAt = data.TryGetValue("UpdatedAt", out var ua) && ua is Timestamp uaTs ? uaTs.ToDateTime() : DateTime.UtcNow,
                InstructorId = data.TryGetValue("InstructorId", out var instId) ? instId?.ToString() ?? "" : "",
                Questions = questions,
                QuestionIds = data.TryGetValue("QuestionIds", out var qids) && qids is System.Collections.IEnumerable enumerable
                    ? enumerable.Cast<object>().Select(o => o.ToString() ?? "").ToList()
                    : new List<string>()
            };

            if (exam.DurationMinutes == 0 && exam.TimeLimitMinutes > 0)
            {
                exam.DurationMinutes = exam.TimeLimitMinutes;
            }
            else if (exam.TimeLimitMinutes == 0 && exam.DurationMinutes > 0)
            {
                exam.TimeLimitMinutes = exam.DurationMinutes;
            }

            return exam;
        }

        private Dictionary<string, object> MapToDictionary(Exam.Domain.Models.Exam exam)
        {
            var questionsList = new List<Dictionary<string, object>>();
            foreach (var q in exam.Questions)
            {
                questionsList.Add(new Dictionary<string, object>
                {
                    { "QuestionId", q.QuestionId },
                    { "QuestionText", q.QuestionText },
                    { "Options", q.Options },
                    { "CorrectOptionIndex", q.CorrectOptionIndex },
                    { "Points", q.Points }
                });
            }

            return new Dictionary<string, object>
            {
                { "ClassId", exam.ClassId },
                { "ClassName", exam.ClassName },
                { "Title", exam.Title },
                { "DurationMinutes", exam.DurationMinutes },
                { "TimeLimitMinutes", exam.TimeLimitMinutes },
                { "Description", exam.Description },
                { "PassingScore", exam.PassingScore },
                { "IsPublished", exam.IsPublished },
                { "IsActive", exam.IsActive },
                { "AllowReview", exam.AllowReview },
                { "RandomizeQuestions", exam.RandomizeQuestions },
                { "ShowScore", exam.ShowScore },
                { "AllowMultipleAttempts", exam.AllowMultipleAttempts },
                { "MaxAttempts", exam.MaxAttempts },
                { "TotalQuestions", exam.TotalQuestions },
                { "CreatedAt", DateTime.SpecifyKind(exam.CreatedAt, DateTimeKind.Utc) },
                { "UpdatedAt", DateTime.SpecifyKind(exam.UpdatedAt, DateTimeKind.Utc) },
                { "InstructorId", exam.InstructorId },
                { "Questions", questionsList },
                { "QuestionIds", exam.QuestionIds }
            };
        }

        private ExamSubmission MapToSubmission(string id, Dictionary<string, object> data)
        {
            var sub = new ExamSubmission
            {
                Id = id,
                StudentId = data.TryGetValue("StudentId", out var sid) ? sid?.ToString() ?? "" : "",
                StudentName = data.TryGetValue("StudentName", out var sname) ? sname?.ToString() ?? "" : "",
                ExamId = data.TryGetValue("ExamId", out var examId) ? examId?.ToString() ?? "" : "",
                CourseId = data.TryGetValue("CourseId", out var cid) ? cid?.ToString() ?? "" : "",
                Score = data.TryGetValue("Score", out var score) ? Convert.ToDouble(score) : 0.0,
                TotalQuestions = data.TryGetValue("TotalQuestions", out var tq) ? Convert.ToInt32(tq) : 0,
                Percentage = data.TryGetValue("Percentage", out var pct) ? Convert.ToDouble(pct) : 0.0,
                SubmittedAt = data.TryGetValue("SubmittedAt", out var sa) && sa is Timestamp saTs ? saTs.ToDateTime() : DateTime.UtcNow,
                TimeSpentSeconds = data.TryGetValue("TimeSpentSeconds", out var tss) ? Convert.ToInt32(tss) : 0,
            };

            var answersList = new List<AnswerResponse>();
            if (data.TryGetValue("Answers", out var ansObj))
            {
                if (ansObj is System.Collections.IEnumerable enumerable)
                {
                    foreach (var item in enumerable)
                    {
                        if (item is Dictionary<string, object> dict)
                        {
                            answersList.Add(new AnswerResponse
                            {
                                QuestionId = dict.TryGetValue("QuestionId", out var qid) ? qid?.ToString() ?? "" : "",
                                QuestionOrder = dict.TryGetValue("QuestionOrder", out var qo) ? Convert.ToInt32(qo) : 0,
                                StudentAnswer = dict.TryGetValue("StudentAnswer", out var saVal) ? saVal?.ToString() ?? "" : "",
                                IsCorrect = dict.TryGetValue("IsCorrect", out var ic) && Convert.ToBoolean(ic),
                                PointsEarned = dict.TryGetValue("PointsEarned", out var pe) ? Convert.ToDouble(pe) : 0.0
                            });
                        }
                        else if (item is IDictionary<string, object> idict)
                        {
                            answersList.Add(new AnswerResponse
                            {
                                QuestionId = idict.TryGetValue("QuestionId", out var qid) ? qid?.ToString() ?? "" : "",
                                QuestionOrder = idict.TryGetValue("QuestionOrder", out var qo) ? Convert.ToInt32(qo) : 0,
                                StudentAnswer = idict.TryGetValue("StudentAnswer", out var saVal) ? saVal?.ToString() ?? "" : "",
                                IsCorrect = idict.TryGetValue("IsCorrect", out var ic) && Convert.ToBoolean(ic),
                                PointsEarned = idict.TryGetValue("PointsEarned", out var pe) ? Convert.ToDouble(pe) : 0.0
                            });
                        }
                    }
                }
            }
            sub.Answers = answersList;
            return sub;
        }

        private Dictionary<string, object> MapToDictionary(ExamSubmission sub)
        {
            var answersList = new List<Dictionary<string, object>>();
            foreach (var ans in sub.Answers)
            {
                answersList.Add(new Dictionary<string, object>
                {
                    { "QuestionId", ans.QuestionId },
                    { "QuestionOrder", ans.QuestionOrder },
                    { "StudentAnswer", ans.StudentAnswer },
                    { "IsCorrect", ans.IsCorrect },
                    { "PointsEarned", ans.PointsEarned }
                });
            }

            return new Dictionary<string, object>
            {
                { "StudentId", sub.StudentId },
                { "StudentName", sub.StudentName },
                { "ExamId", sub.ExamId },
                { "CourseId", sub.CourseId },
                { "Score", sub.Score },
                { "TotalQuestions", sub.TotalQuestions },
                { "Percentage", sub.Percentage },
                { "Answers", answersList },
                { "SubmittedAt", DateTime.SpecifyKind(sub.SubmittedAt, DateTimeKind.Utc) },
                { "TimeSpentSeconds", sub.TimeSpentSeconds }
            };
        }

        private ExamDraft MapToDraft(string examId, string studentId, Dictionary<string, object> data)
        {
            var draft = new ExamDraft
            {
                Id = $"{examId}_{studentId}",
                ExamId = examId,
                StudentId = studentId,
                StudentName = data.TryGetValue("StudentName", out var sname) ? sname?.ToString() ?? "" : "",
                StartedAt = data.TryGetValue("StartedAt", out var sa) && sa is Timestamp saTs ? saTs.ToDateTime() : DateTime.UtcNow,
                LastQuestionIndex = data.TryGetValue("LastQuestionIndex", out var lqi) ? Convert.ToInt32(lqi) : 0,
                SavedAt = data.TryGetValue("SavedAt", out var saved) && saved is Timestamp savedTs ? savedTs.ToDateTime() : DateTime.UtcNow,
            };

            var answers = new Dictionary<string, string>();
            if (data.TryGetValue("Answers", out var ansObj) && ansObj is IDictionary<string, object> dict)
            {
                foreach (var kvp in dict)
                {
                    answers[kvp.Key] = kvp.Value?.ToString() ?? "";
                }
            }
            draft.Answers = answers;

            var marked = new List<string>();
            if (data.TryGetValue("MarkedForReview", out var markedObj) && markedObj is System.Collections.IEnumerable enumerable)
            {
                foreach (var item in enumerable)
                {
                    if (item != null) marked.Add(item.ToString()!);
                }
            }
            draft.MarkedForReview = marked;

            return draft;
        }

        private Dictionary<string, object> MapToDictionary(ExamDraft draft)
        {
            return new Dictionary<string, object>
            {
                { "ExamId", draft.ExamId },
                { "StudentId", draft.StudentId },
                { "StudentName", draft.StudentName },
                { "StartedAt", DateTime.SpecifyKind(draft.StartedAt, DateTimeKind.Utc) },
                { "Answers", draft.Answers },
                { "MarkedForReview", draft.MarkedForReview },
                { "LastQuestionIndex", draft.LastQuestionIndex },
                { "SavedAt", DateTime.SpecifyKind(draft.SavedAt, DateTimeKind.Utc) }
            };
        }

        // --- IExamRepository Implementation ---

        public async Task<Exam.Domain.Models.Exam?> GetByIdAsync(string id)
        {
            var docRef = _firestoreDb.Collection("exams").Document(id);
            var snapshot = await docRef.GetSnapshotAsync();
            if (!snapshot.Exists) return null;

            var data = snapshot.ToDictionary();
            var questions = await LoadQuestionsAsync(docRef, data);
            return MapToExam(snapshot.Id, data, questions);
        }

        public async Task<List<Exam.Domain.Models.Exam>> GetExamsByCourseIdAsync(string courseId)
        {
            var snapshot = await _firestoreDb.Collection("exams")
                .WhereEqualTo("ClassId", courseId)
                .GetSnapshotAsync();

            var list = new List<Exam.Domain.Models.Exam>();
            foreach (var doc in snapshot.Documents)
            {
                var data = doc.ToDictionary();
                var questions = await LoadQuestionsAsync(doc.Reference, data);
                list.Add(MapToExam(doc.Id, data, questions));
            }
            return list;
        }

        public async Task<List<Exam.Domain.Models.Exam>> GetExamsByInstructorIdAsync(string instructorId)
        {
            var snapshot = await _firestoreDb.Collection("exams")
                .WhereEqualTo("InstructorId", instructorId)
                .GetSnapshotAsync();

            var list = new List<Exam.Domain.Models.Exam>();
            foreach (var doc in snapshot.Documents)
            {
                var data = doc.ToDictionary();
                var questions = await LoadQuestionsAsync(doc.Reference, data);
                list.Add(MapToExam(doc.Id, data, questions));
            }
            return list;
        }

        public async Task<string> CreateAsync(Exam.Domain.Models.Exam exam)
        {
            var dict = MapToDictionary(exam);
            if (!string.IsNullOrEmpty(exam.Id))
            {
                var docRef = _firestoreDb.Collection("exams").Document(exam.Id);
                await docRef.SetAsync(dict);
                return exam.Id;
            }
            else
            {
                var docRef = await _firestoreDb.Collection("exams").AddAsync(dict);
                exam.Id = docRef.Id;
                return docRef.Id;
            }
        }

        public async Task UpdateAsync(string id, Dictionary<string, object> updates)
        {
            var docRef = _firestoreDb.Collection("exams").Document(id);
            updates["UpdatedAt"] = DateTime.UtcNow;
            await docRef.UpdateAsync(updates);
        }

        public async Task DeleteAsync(string id)
        {
            var docRef = _firestoreDb.Collection("exams").Document(id);
            await docRef.DeleteAsync();
        }

        public async Task<ExamSubmission?> GetSubmissionByIdAsync(string submissionId)
        {
            var docRef = _firestoreDb.Collection("exam_submissions").Document(submissionId);
            var snapshot = await docRef.GetSnapshotAsync();
            if (!snapshot.Exists) return null;

            return MapToSubmission(snapshot.Id, snapshot.ToDictionary());
        }

        public async Task<List<ExamSubmission>> GetSubmissionsByExamIdAsync(string examId)
        {
            var snapshot = await _firestoreDb.Collection("exam_submissions")
                .WhereEqualTo("ExamId", examId)
                .GetSnapshotAsync();

            return snapshot.Documents.Select(doc => MapToSubmission(doc.Id, doc.ToDictionary())).ToList();
        }

        public async Task<List<ExamSubmission>> GetSubmissionsByStudentIdAsync(string studentId)
        {
            var snapshot = await _firestoreDb.Collection("exam_submissions")
                .WhereEqualTo("StudentId", studentId)
                .GetSnapshotAsync();

            return snapshot.Documents.Select(doc => MapToSubmission(doc.Id, doc.ToDictionary())).ToList();
        }

        public async Task CreateSubmissionAsync(ExamSubmission submission)
        {
            var dict = MapToDictionary(submission);
            var docRef = await _firestoreDb.Collection("exam_submissions").AddAsync(dict);
            submission.Id = docRef.Id;
        }

        public async Task<ExamDraft?> GetDraftAsync(string examId, string studentId)
        {
            var docRef = _firestoreDb.Collection("exams").Document(examId).Collection("drafts").Document(studentId);
            var snapshot = await docRef.GetSnapshotAsync();
            if (!snapshot.Exists) return null;

            return MapToDraft(examId, studentId, snapshot.ToDictionary());
        }

        public async Task SaveDraftAsync(ExamDraft draft)
        {
            var dict = MapToDictionary(draft);
            var docRef = _firestoreDb.Collection("exams").Document(draft.ExamId).Collection("drafts").Document(draft.StudentId);
            await docRef.SetAsync(dict);
        }

        public async Task DeleteDraftAsync(string examId, string studentId)
        {
            var docRef = _firestoreDb.Collection("exams").Document(examId).Collection("drafts").Document(studentId);
            await docRef.DeleteAsync();
        }

        public async Task<bool> DeleteQuestionAsync(string examId, string questionId)
        {
            var examRef = _firestoreDb.Collection("exams").Document(examId);
            var docSnap = await examRef.GetSnapshotAsync();
            if (!docSnap.Exists) return false;

            var data = docSnap.ToDictionary();
            bool deleted = false;

            if (data.TryGetValue("Questions", out var questionsObj) && questionsObj is System.Collections.IList questionsList)
            {
                var newQuestions = new List<object>();
                foreach (var qObj in questionsList)
                {
                    if (qObj is Dictionary<string, object> qData)
                    {
                        string qId = qData.TryGetValue("QuestionId", out var qidVal) ? qidVal?.ToString() ?? "" : "";
                        if (qId == questionId)
                        {
                            deleted = true;
                            continue;
                        }
                    }
                    newQuestions.Add(qObj);
                }

                if (deleted)
                {
                    var updates = new Dictionary<string, object>
                    {
                        { "Questions", newQuestions },
                        { "TotalQuestions", newQuestions.Count },
                        { "UpdatedAt", DateTime.UtcNow }
                    };

                    if (data.TryGetValue("QuestionIds", out var qIdsObj) && qIdsObj is System.Collections.IList qIdsList)
                    {
                        var newQIds = new List<string>();
                        foreach (var qId in qIdsList)
                        {
                            if (qId?.ToString() != questionId)
                            {
                                newQIds.Add(qId?.ToString() ?? "");
                            }
                        }
                        updates["QuestionIds"] = newQIds;
                    }

                    await examRef.UpdateAsync(updates);
                }
            }

            try
            {
                var subDocRef = examRef.Collection("questions").Document(questionId);
                var subDocSnap = await subDocRef.GetSnapshotAsync();
                if (subDocSnap.Exists)
                {
                    await subDocRef.DeleteAsync();
                    deleted = true;

                    var subQuestionsSnap = await examRef.Collection("questions").GetSnapshotAsync();
                    await examRef.UpdateAsync(new Dictionary<string, object>
                    {
                        { "TotalQuestions", subQuestionsSnap.Documents.Count },
                        { "UpdatedAt", DateTime.UtcNow }
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting subcollection question in DeleteQuestionAsync: {ex.Message}");
            }

            return deleted;
        }
    }
}
