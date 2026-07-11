using EasyEnglish.Core.Entities;
using EasyEnglish.Data.Repositories;
using EasyEnglish.Data.Tests.Fixtures;

namespace EasyEnglish.Data.Tests;

public class SubjectRepositoryTests : SqliteTestBase
{
    private SubjectRepository CreateRepository() => new(Factory, UserContext);

    [Fact]
    public async Task CountCoursesAsync_CountsOnlyCoursesForThatSubject()
    {
        int subjectId;
        await using (var ctx = CreateContext())
        {
            var subject = new SubjectEntity { Title = "English" };
            var otherSubject = new SubjectEntity { Title = "German" };
            ctx.Subjects.AddRange(subject, otherSubject);
            await ctx.SaveChangesAsync();
            subjectId = subject.Id;

            ctx.Courses.AddRange(
                new CourseEntity { Title = "A", SubjectId = subject.Id },
                new CourseEntity { Title = "B", SubjectId = subject.Id },
                new CourseEntity { Title = "C", SubjectId = otherSubject.Id },
                new CourseEntity { Title = "D", SubjectId = null });
            await ctx.SaveChangesAsync();
        }

        var count = await CreateRepository().CountCoursesAsync(subjectId);

        Assert.Equal(2, count);
    }

    [Fact]
    public async Task CountCoursesAsync_NoCourses_ReturnsZero()
    {
        int subjectId;
        await using (var ctx = CreateContext())
        {
            var subject = new SubjectEntity { Title = "English" };
            ctx.Subjects.Add(subject);
            await ctx.SaveChangesAsync();
            subjectId = subject.Id;
        }

        var count = await CreateRepository().CountCoursesAsync(subjectId);

        Assert.Equal(0, count);
    }
}
