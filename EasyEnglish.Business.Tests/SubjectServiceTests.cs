using EasyEnglish.Business.Tests.Fixtures;
using EasyEnglish.Core.Entities;

namespace EasyEnglish.Business.Tests;

public class SubjectServiceTests : SqliteTestBase
{
    [Fact]
    public async Task GetCourseCountAsync_CountsOnlyCoursesForThatSubject()
    {
        int subjectId;
        await using (var ctx = CreateContext())
        {
            var subject = new SubjectEntity { Title = "English" };
            ctx.Subjects.Add(subject);
            await ctx.SaveChangesAsync();
            subjectId = subject.Id;

            ctx.Courses.AddRange(
                new CourseEntity { Title = "A", SubjectId = subject.Id },
                new CourseEntity { Title = "B", SubjectId = subject.Id },
                new CourseEntity { Title = "C", SubjectId = null });
            await ctx.SaveChangesAsync();
        }

        var count = await SubjectService.GetCourseCountAsync(subjectId);

        Assert.Equal(2, count);
    }
}
