using EasyPeasy.Core.Content;
using EasyPeasy.Core.Interfaces.Services;
using EasyPeasy.Core.Models;

namespace EasyPeasy.App.Services;

/// <summary>
/// Puts a ready-made course into an empty library.
///
/// The saving is deliberately the same two calls the ZIP importer makes — create the course, then
/// create each unit against its id, letting EF cascade the words, forms and cards. A second way of
/// writing a course would be a second thing to keep correct.
/// </summary>
public class DemoContentService(
    ICourseService courseService,
    IUnitService unitService)
{
    /// <summary>The courses on offer, unsaved.</summary>
    public static IReadOnlyList<CourseModel> Available => DemoCourses.All();

    /// <summary>
    /// Saves one demo course and everything under it.
    /// </summary>
    /// <returns>The created course, with its assigned id.</returns>
    public async Task<CourseModel> AddAsync(CourseModel demo)
    {
        // A fresh GUID each time: loading the same demo twice gives two independent courses
        // rather than something a later import would treat as an update of the first
        var course = await courseService.CreateAsync(new CourseModel
        {
            RecordGuid = Guid.NewGuid(),
            Title = demo.Title,
            Description = demo.Description,
            LanguageCode = demo.LanguageCode,
        });

        foreach (var unit in demo.Units ?? [])
        {
            unit.RecordGuid = Guid.NewGuid();
            unit.CourseId = course.Id;

            await unitService.CreateAsync(unit);
        }

        return course;
    }
}
