using Microsoft.EntityFrameworkCore;
using OnlineLearningPlatform.Entityes;
using OnlineLearningPlatform;

namespace OnlineLearningPlatform
{
    internal class Program
    {
        static void Main(string[] args)
        {
            using var context = new OnlineLearningPlatformDbContext();
            Console.WriteLine("=== ONLINE LEARNING PLATFORM OPERATIONS ===\n");


            Console.WriteLine("--- CREATE ---");
            int newInstructorId = AddInstructor(context, "Khaled Mansour", "khaled.m@onlinelearning.com", "Cloud Engineer");
            int newCourseId = AddCourse(context, "Advanced SQL Tuning", "Query optimization", 59.99m, CourseLevel.Advanced, newInstructorId, categoryId: 2);
            AddCourse(context, "Invalid Instructor Course", null, 15m, CourseLevel.Beginner, instructorId: 999, categoryId: 2);
            List<int> newStudentIds = AddThreeStudents(context);

            if (newStudentIds.Count > 0)
            {
                EnrollStudent(context, newStudentIds[0], courseId: 1); 
                EnrollStudent(context, newStudentIds[0], courseId: 1); 
                EnrollStudent(context, newStudentIds[0], courseId: 2); 
            }
           

            Console.WriteLine("\n--- READ ---");
            FindCourse(context, id: 1, title: "EF Core Mastery");
            FindCourse(context, id: 999, title: "Missing Course");
            FilterCoursesByPrice(context, minPrice: 20m, maxPrice: 80m);
            PrintSubCategoriesAndTopLevel(context, parentCategoryId: 1);
            PrintCourseRoster(context, courseId: 1);

            
            Console.WriteLine("\n--- UPDATE ---");
            ChangeCoursePrice(context, courseId: 1, newPrice: 64.99m);
            UpdateEnrollmentProgress(context, studentId: 1, courseId: 1, newProgress: 120); // Out of range
            UpdateEnrollmentProgress(context, studentId: 1, courseId: 1, newProgress: 100, finalGrade: 95.0m); // Completed
            PublishCourse(context, courseId: 2);

            
            Console.WriteLine("\n--- DELETE ---");
            if (newStudentIds.Count > 0)
            {
                UnenrollStudent(context, studentId: newStudentIds[0], courseId: 1);
            }
            DeleteCategoryWithExceptionHandling(context, categoryId: 1);
            DeleteCategorySafely(context, categoryId: 1);
            DeleteCourseWithEnrollments(context, courseId: 1);
        }
        static int AddInstructor(OnlineLearningPlatformDbContext db, string fullName, string email, string? bio)
        {
            if (db.Instructors.Any(i => i.Email == email))
            {
                Console.WriteLine($"Instructor with email '{email}' already exists.");
                return db.Instructors.First(i => i.Email == email).Id;
            }

            var instructor = new Instructor
            {
                FullName = fullName,
                Email = email,
                Bio = bio,
                JoinedDate = DateTime.UtcNow
            };

            db.Instructors.Add(instructor);
            db.SaveChanges();
            Console.WriteLine($"Added Instructor '{instructor.FullName}' with Id = {instructor.Id}");
            return instructor.Id;
        }

        static int AddCourse(OnlineLearningPlatformDbContext db, string title, string? description, decimal price, CourseLevel level, int instructorId, int categoryId)
        {
            if (!db.Instructors.Any(i => i.Id == instructorId))
            {
                Console.WriteLine($"Cannot add course '{title}': Instructor Id {instructorId} does not exist.");
                return -1;
            }

            if (!db.Categories.Any(c => c.Id == categoryId))
            {
                Console.WriteLine($"Cannot add course '{title}': Category Id {categoryId} does not exist.");
                return -1;
            }

            var course = new Course
            {
                Title = title,
                Description = description,
                Price = price,
                Level = level,
                IsPublished = false,
                PublishedDate = null,
                InstructorId = instructorId,
                CategoryId = categoryId
            };

            db.Courses.Add(course);
            db.SaveChanges();
            Console.WriteLine($"Added Course '{course.Title}' with Id = {course.Id}");
            return course.Id;
        }

        static List<int> AddThreeStudents(OnlineLearningPlatformDbContext db)
        {
            var students = new List<Student>
            {
                new() { FullName = "Youssef Ali", Email = $"youssef_{Guid.NewGuid():N}@student.com", DateOfBirth = new DateOnly(2002, 3, 14) },
                new() { FullName = "Nourhan Sami", Email = $"nourhan_{Guid.NewGuid():N}@student.com", DateOfBirth = new DateOnly(2004, 11, 2) },
                new() { FullName = "Karim Adel", Email = $"karim_{Guid.NewGuid():N}@student.com", DateOfBirth = new DateOnly(2003, 7, 19) }
            };

            db.Students.AddRange(students);
            db.SaveChanges();
            Console.WriteLine($"Added 3 Students with Ids: {string.Join(", ", students.Select(s => s.Id))}");
            return students.Select(s => s.Id).ToList();
        }

        static void EnrollStudent(OnlineLearningPlatformDbContext db, int studentId, int courseId)
        {
            if (db.Students.Find(studentId) is null)
            {
                Console.WriteLine($"Enrollment refused: Student Id {studentId} does not exist.");
                return;
            }

            var course = db.Courses.Find(courseId);
            if (course is null)
            {
                Console.WriteLine($"Enrollment refused: Course Id {courseId} does not exist.");
                return;
            }

            if (!course.IsPublished)
            {
                Console.WriteLine($"Enrollment refused: Course '{course.Title}' is not published.");
                return;
            }

            if (db.Enrollments.Any(e => e.StudentId == studentId && e.CourseId == courseId))
            {
                Console.WriteLine($"Enrollment refused: Student Id {studentId} is already enrolled in Course Id {courseId}.");
                return;
            }

            var enrollment = new Enrollment
            {
                StudentId = studentId,
                CourseId = courseId,
                EnrollmentDate = DateTime.UtcNow,
                ProgressPercentage = 0,
                Status = EnrollmentStatus.Active,
                FinalGrade = null
            };

            db.Enrollments.Add(enrollment);
            db.SaveChanges();
            Console.WriteLine($"Enrolled Student Id {studentId} into Course '{course.Title}'.");
        }




        static void FindCourse(OnlineLearningPlatformDbContext db, int id, string title)
        {
            var courseById = db.Courses.Find(id);
            Console.WriteLine(courseById is null ? $"Find by Id ({id}): not found" : $"Find by Id ({id}): {courseById.Title} (${courseById.Price})");

            var courseByTitle = db.Courses.FirstOrDefault(c => c.Title == title);
            Console.WriteLine(courseByTitle is null ? $"FirstOrDefault by Title ('{title}'): not found" : $"FirstOrDefault by Title ('{title}'): Id = {courseByTitle.Id}, Level = {courseByTitle.Level}");
        }

        static void FilterCoursesByPrice(OnlineLearningPlatformDbContext db, decimal minPrice, decimal maxPrice)
        {
            var courses = db.Courses
                .Where(c => c.Price >= minPrice && c.Price <= maxPrice)
                .OrderByDescending(c => c.Price)
                .Select(c => new { c.Title, c.Level, c.Price })
                .ToList();

            Console.WriteLine($"Courses priced between ${minPrice} and ${maxPrice} (descending):");
            if (courses.Count == 0)
            {
                Console.WriteLine("  No courses found.");
                return;
            }

            foreach (var c in courses)
            {
                Console.WriteLine($"  - {c.Title} | {c.Level} | ${c.Price}");
            }
        }

        static void PrintSubCategoriesAndTopLevel(OnlineLearningPlatformDbContext db, int parentCategoryId)
        {
            var parent = db.Categories.Find(parentCategoryId);
            if (parent is not null)
            {
                var subs = db.Categories.Where(c => c.ParentCategoryId == parentCategoryId).ToList();
                Console.WriteLine($"Direct sub-categories of '{parent.Name}':");
                if (subs.Count == 0)
                    Console.WriteLine("  None");
                else
                    subs.ForEach(s => Console.WriteLine($"  - {s.Name}"));
            }

            var topLevels = db.Categories.Where(c => c.ParentCategoryId == null).ToList();
            Console.WriteLine("Top-level categories:");
            topLevels.ForEach(t => Console.WriteLine($"  - {t.Name}"));
        }

        static void PrintCourseRoster(OnlineLearningPlatformDbContext db, int courseId)
        {
            var course = db.Courses.Find(courseId);
            if (course is null)
            {
                Console.WriteLine($"Course Id {courseId} not found.");
                return;
            }

            List<int> studentIds = db.Enrollments.Where(e => e.CourseId == courseId).Select(e => e.StudentId).ToList();
            List<string> studentNames = db.Students.Where(s => studentIds.Contains(s.Id)).Select(s => s.FullName).ToList();

            Console.WriteLine($"Roster for '{course.Title}' ({studentIds.Count} students enrolled):");
            foreach (var name in studentNames)
            {
                Console.WriteLine($"  - {name}");
            }
        }




        static void ChangeCoursePrice(OnlineLearningPlatformDbContext db, int courseId, decimal newPrice)
        {
            var course = db.Courses.Find(courseId);
            if (course is null)
            {
                Console.WriteLine($"Course Id {courseId} not found.");
                return;
            }

            decimal oldPrice = course.Price;
            course.Price = newPrice;
            db.SaveChanges();
            Console.WriteLine($"Updated price for '{course.Title}': ${oldPrice} -> ${course.Price}");
        }

        static void UpdateEnrollmentProgress(OnlineLearningPlatformDbContext db, int studentId, int courseId, int newProgress, decimal? finalGrade = null)
        {
            if (newProgress < 0 || newProgress > 100)
            {
                Console.WriteLine($"Invalid progress ({newProgress}%): Progress must be between 0 and 100.");
                return;
            }

            var enrollment = db.Enrollments.Find(studentId, courseId);
            if (enrollment is null)
            {
                Console.WriteLine($"Enrollment for Student {studentId} in Course {courseId} not found.");
                return;
            }

            enrollment.ProgressPercentage = newProgress;
            if (newProgress == 100)
            {
                enrollment.Status = EnrollmentStatus.Completed;
                enrollment.FinalGrade = finalGrade;
            }

            db.SaveChanges();
            Console.WriteLine($"Updated progress for Student {studentId}: {enrollment.ProgressPercentage}% (Status: {enrollment.Status})");
        }

        static void PublishCourse(OnlineLearningPlatformDbContext db, int courseId)
        {
            var course = db.Courses.Find(courseId);
            if (course is null)
            {
                Console.WriteLine($"Course Id {courseId} not found.");
                return;
            }

            course.IsPublished = true;
            course.PublishedDate = DateTime.UtcNow;
            db.SaveChanges();
            Console.WriteLine($"Published course '{course.Title}'.");
        }



        static void UnenrollStudent(OnlineLearningPlatformDbContext db, int studentId, int courseId)
        {
            var enrollment = db.Enrollments.Find(studentId, courseId);
            if (enrollment is null)
            {
                Console.WriteLine("Enrollment not found.");
                return;
            }

            db.Enrollments.Remove(enrollment);
            db.SaveChanges();
            Console.WriteLine($"Unenrolled Student {studentId} from Course {courseId}.");
        }

        static void DeleteCategoryWithExceptionHandling(OnlineLearningPlatformDbContext db, int categoryId)
        {
            var category = db.Categories.Find(categoryId);
            if (category is null) return;

            try
            {
                db.Categories.Remove(category);
                db.SaveChanges();
            }
            catch (DbUpdateException ex)
            {
                db.Entry(category).State = EntityState.Unchanged;
                Console.WriteLine($"DbUpdateException caught when deleting '{category.Name}': {ex.InnerException?.Message ?? ex.Message}");
            }
        }

        static void DeleteCategorySafely(OnlineLearningPlatformDbContext db, int categoryId)
        {
            var category = db.Categories.Find(categoryId);
            if (category is null) return;

            bool hasSubs = db.Categories.Any(c => c.ParentCategoryId == categoryId);
            if (hasSubs)
            {
                Console.WriteLine($"Safe delete prevented: Category '{category.Name}' still has sub-categories.");
                return;
            }

            db.Categories.Remove(category);
            db.SaveChanges();
            Console.WriteLine($"Safely deleted category '{category.Name}'.");
        }

        static void DeleteCourseWithEnrollments(OnlineLearningPlatformDbContext db, int courseId)
        {
            var course = db.Courses.Find(courseId);
            if (course is null) return;

            int countBefore = db.Enrollments.Count(e => e.CourseId == courseId);
            Console.WriteLine($"Enrollments for Course Id {courseId} BEFORE delete: {countBefore}");

            db.Courses.Remove(course);
            db.SaveChanges();


            int countAfter = db.Enrollments.Count(e => e.CourseId == courseId);
            Console.WriteLine($"Enrollments for Course Id {courseId} AFTER delete: {countAfter}");
        }
    }
}
