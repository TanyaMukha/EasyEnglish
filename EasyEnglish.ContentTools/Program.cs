using EasyEnglish.ContentTools;
using EasyEnglish.Core.Enums;
using EasyEnglish.Core.Models;

// Використання: dotnet run -- <module-key>
// Кожен модуль курсу має свій метод нижче — просто додай новий, коли з'явиться наступний.
// За замовчуванням (без аргументів) запускається останній доданий модуль.

if (args.Length > 0 && args[0] == "verify")
{
    if (args.Length < 3)
    {
        Console.WriteLine("Використання: dotnet run -- verify <zip-path> <units/unit_N.json>");
        Environment.Exit(1);
        return;
    }

    Verify(args[1], args[2]);
    return;
}

var moduleKey = args.Length > 0 ? args[0] : "english-for-it-b2-unit1";

switch (moduleKey)
{
    case "english-for-it-b2-unit1":
        EnglishForItB2Unit1.Run();
        break;

    case "english-for-it-b2-unit1-traps":
        EnglishForItB2Unit1TranslationTraps.Run();
        break;

    default:
        Console.WriteLine($"Невідомий ключ модуля: {moduleKey}");
        Environment.Exit(1);
        break;
}

/// <summary>Друкує короткий підсумок вмісту юніта — скільки слів/карток кожного виду.</summary>
static void Verify(string zipPath, string unitFile)
{
    var unit = CourseZipEditor.LoadUnit(zipPath, unitFile);

    Console.WriteLine($"Unit: {unit.Title}");
    Console.WriteLine($"  Words: {unit.Words?.Count ?? 0}");
    Console.WriteLine($"  IrregularForms: {unit.IrregularForms?.Count ?? 0}");

    Console.WriteLine($"  StudyCards: {unit.StudyCards?.Count ?? 0}");
    foreach (var group in (unit.StudyCards ?? []).GroupBy(c => c.Kind))
        Console.WriteLine($"    {group.Key}: {group.Count()}");

    Console.WriteLine($"  TestCards: {unit.TestCards?.Count ?? 0}");
    foreach (var group in (unit.TestCards ?? []).GroupBy(c => c.Kind))
        Console.WriteLine($"    {group.Key}: {group.Count()}");
}

/// <summary>
/// "Present Simple #1" (english_for_it_b2_1.zip) — гра з різними видами карток:
/// StudyCard (Term, Text, BlurredText) і TestCard (SingleChoice, MultipleChoice, Cloze, Matching).
/// ShortAnswer-картки (переклад речень) для цього ж модуля вже додані окремо раніше.
/// </summary>
internal static class EnglishForItB2Unit1
{
    private const string SourceZip = @"C:\Users\User\Downloads\english_for_it_b2_1_updated.zip";
    private const string TargetZip = @"C:\Users\User\Downloads\english_for_it_b2_1_updated2.zip";
    private const string UnitFile = "units/unit_1.json";

    public static void Run()
    {
        CourseZipEditor.CopyArchive(SourceZip, TargetZip);
        var unit = CourseZipEditor.LoadUnit(TargetZip, UnitFile);

        Console.WriteLine($"Loaded unit: {unit.Title}");
        Console.WriteLine($"  words: {unit.Words?.Count ?? 0}, studyCards: {unit.StudyCards?.Count ?? 0}, testCards: {unit.TestCards?.Count ?? 0}");

        unit.StudyCards ??= new List<StudyCardModel>();
        unit.TestCards ??= new List<TestCardModel>();

        // ── StudyCard: Term — граматичне правило, а не окреме слово зі списку ──
        unit.StudyCards.Add(StudyCardBuilder.Term(
            "Present Simple: рутинні дії",
            "Вживається для звичних дій, фактів і розкладів. I/you/we/they + дієслово; " +
            "he/she/it + дієслово+s. З модуля: 'Our tech lead usually signs off on changes' " +
            "(he — сигналізує -s), 'We regularly touch base' (we — без -s)."));

        unit.StudyCards.Add(StudyCardBuilder.Term(
            "he/she/it + дієслово + -s",
            "У Present Simple дієслово отримує закінчення -s (або -es після s/sh/ch/x/o) з " +
            "підметом третьої особи однини. З модуля: insists, expects, breaks down (break → breaks down), " +
            "gets together."));

        // ── StudyCard: Text — короткий текст, що поєднує кілька виразів разом ──
        unit.StudyCards.Add(StudyCardBuilder.Text(
            "Ранок у команді розробки",
            "Every morning our team gathers for a standup. The Scrum Master insists that everyone " +
            "updates Jira by the end of the day. If an architectural decision seems dodgy, we get " +
            "together for a tech brainstorm. Our tech lead usually signs off on the plan before we " +
            "start coding."));

        // ── StudyCard: BlurredText — Independent (один фрагмент) і Grouped (розділений вираз) ──
        unit.StudyCards.Add(StudyCardBuilder.BlurredText(
            "Our tech lead usually **signs off on** architectural changes before we start coding.",
            BlurRevealMode.Independent));

        unit.StudyCards.Add(StudyCardBuilder.BlurredText(
            "The dev team **breaks** complex epics **down** into smaller tasks during planning.",
            BlurRevealMode.Grouped));

        // ── TestCard: SingleChoice ──
        unit.TestCards.Add(TestCardBuilder.SingleChoice(
            "Виберіть правильне дієслово: 'Наша команда ___ стендап о 10 ранку.'",
            ["has a standup", "makes a standup", "opens a standup", "builds a standup"],
            "has a standup"));

        unit.TestCards.Add(TestCardBuilder.SingleChoice(
            "Яке слово означає тимчасовий, ненадійний код (сленг)?",
            ["complex", "dodgy", "doubtful", "questionable"],
            "dodgy"));

        // ── TestCard: MultipleChoice ──
        unit.TestCards.Add(TestCardBuilder.MultipleChoice(
            "Які вислови означають 'відповідати стандартам якості'? (виберіть усі правильні)",
            ["to meet standards", "to comply with standards", "to be up to standard", "to take on standards"],
            ["to meet standards", "to comply with standards", "to be up to standard"]));

        unit.TestCards.Add(TestCardBuilder.MultipleChoice(
            "Які слова описують сумнів чи непевність? (виберіть усі правильні)",
            ["questionable", "doubtful", "dodgy", "complex"],
            ["questionable", "doubtful", "dodgy"]));

        // ── TestCard: Cloze — один з випадаючим списком, другий повністю на ввід ──
        unit.TestCards.Add(TestCardBuilder.Cloze(
            "The Scrum Master {0} that everyone updates Jira {1}.",
            ["insists", "by the end of the day"],
            [["insists", "expects", "ensures"], []]));

        unit.TestCards.Add(TestCardBuilder.Cloze(
            "Please {0} this feature {1} smaller user stories.",
            ["break down", "into"]));

        // ── TestCard: Matching ──
        unit.TestCards.Add(TestCardBuilder.Matching(
            "Зіставте вислів з його значенням",
            ["to touch base", "to sign off on", "to break down", "to take on"],
            ["з'єднатися для звірки інформації", "офіційно затвердити", "розділити на частини", "взяти додаткові завдання"]));

        unit.TestCards.Add(TestCardBuilder.Matching(
            "Зіставте прикметник з характеристикою",
            ["dodgy", "doubtful", "questionable", "complex"],
            ["ненадійний, стрьомний (сленг)", "малоймовірний", "викликає сумніви/питання", "заплутаний технічно"]));

        CourseZipEditor.SaveUnit(TargetZip, UnitFile, unit);

        Console.WriteLine();
        Console.WriteLine($"[OK] Written: {TargetZip}");
        Console.WriteLine($"  studyCards: {unit.StudyCards.Count} (+5), testCards: {unit.TestCards.Count} (+8)");
    }
}

/// <summary>
/// Другий заїзд по тому самому модулю — картки "з підвохом" на тонкощі перекладу.
/// Модуль насичений парами близьких синонімів, які словник перекладає майже однаково,
/// але вживаються по-різному (регістр/офіційність, емоційне vs технічне значення,
/// суб'єкт вживання). Кожна картка навмисно ставить у варіанти відповіді "правильний
/// на перший погляд, але неправильний за контекстом" синонім як дистрактор.
/// </summary>
internal static class EnglishForItB2Unit1TranslationTraps
{
    private const string SourceZip = @"C:\Users\User\Downloads\english_for_it_b2_1_updated2.zip";
    private const string TargetZip = @"C:\Users\User\Downloads\english_for_it_b2_1_updated3.zip";
    private const string UnitFile = "units/unit_1.json";

    public static void Run()
    {
        CourseZipEditor.CopyArchive(SourceZip, TargetZip);
        var unit = CourseZipEditor.LoadUnit(TargetZip, UnitFile);

        Console.WriteLine($"Loaded unit: {unit.Title}");
        Console.WriteLine($"  words: {unit.Words?.Count ?? 0}, studyCards: {unit.StudyCards?.Count ?? 0}, testCards: {unit.TestCards?.Count ?? 0}");

        unit.TestCards ??= new List<TestCardModel>();
        var before = unit.TestCards.Count;

        // 1. dodgy vs questionable vs doubtful — усі три перекладаються як "сумнівний",
        //    але тут контекст явно вимагає розмовний сленг, а не нейтрально-професійне слово.
        unit.TestCards.Add(TestCardBuilder.SingleChoice(
            "'Цей тимчасовий костиль виглядає для мене трохи ___.' (розмовне, сленгове слово)",
            ["doubtful", "questionable", "dodgy", "difficult"],
            "dodgy"));

        // 2. difficult vs complex — обидва "складний", але difficult про емоційно/ситуативно
        //    важке, а complex — про технічну складність архітектури/коду.
        unit.TestCards.Add(TestCardBuilder.SingleChoice(
            "'Ця складна архітектура вимагає глибокого аналізу.' Яке слово тут доречне?",
            ["difficult", "complex", "dodgy", "doubtful"],
            "complex"));

        // 3. make sure vs ensure — синоніми, але ensure формальніший, для офіційного тексту.
        unit.TestCards.Add(TestCardBuilder.SingleChoice(
            "Офіційний звіт для менеджменту: 'We need to ___ smooth deployment.'",
            ["make sure", "ensure", "expect", "insist"],
            "ensure"));

        // 4. meet standards vs comply with standards vs be up to standard — усі про
        //    "відповідати стандартам", але саме юридично-регуляторний контекст
        //    вимагає comply with, а не найчастотніший в IT варіант meet standards.
        unit.TestCards.Add(TestCardBuilder.SingleChoice(
            "'Медичне ПЗ повинно ___ суворим стандартам безпеки.' (йдеться про обов'язкові регламенти)",
            ["meet standards", "comply with standards", "be up to standard", "take on standards"],
            "comply with standards"));

        // 5. to have a standup vs to hold a standup — обидва "проводити стендап",
        //    але hold підкреслює організацію/модерацію процесу, have — просто участь.
        unit.TestCards.Add(TestCardBuilder.SingleChoice(
            "'Скрам-майстер організовує щоденний стендап.' (акцент саме на організації)",
            ["has a standup", "holds a standup", "gets a standup", "takes a standup"],
            "holds a standup"));

        // 6. to gather vs to get together for — обидва "збиратися разом", але get together
        //    for — тепліший, неформальний відтінок (п'ятничний брейншторм для команди-друзів).
        unit.TestCards.Add(TestCardBuilder.SingleChoice(
            "'Ми зазвичай ___ на технічний брейншторм щоп'ятниці.' (неформально, по-дружньому)",
            ["gather", "get together for", "hold", "meet"],
            "get together for"));

        // 7. Граматична пастка: "insist that + підмет + дієслово" — з модуля точно відомо,
        //    як це вживається ('insists that everyone updates'), дистрактори — типові помилки
        //    змішування конструкцій "insist on + gerund/to-infinitive" з "insist that + clause".
        unit.TestCards.Add(TestCardBuilder.SingleChoice(
            "Scrum-майстер наполягає, щоб кожен оновлював Jira. Оберіть граматично правильний варіант:",
            [
                "insists on everyone updates Jira",
                "insists that everyone updates Jira",
                "insists on everyone to update Jira",
                "insists that everyone to updating Jira"
            ],
            "insists that everyone updates Jira"));

        // 8. overloaded vs overcommitted — той самий пропуск-дропдаун для обох речень:
        //    підказки однакові, тож пастка не в лексиці, а саме в тому, до якого суб'єкта
        //    (сервер чи спринт) яке слово прив'язане.
        unit.TestCards.Add(TestCardBuilder.Cloze(
            "Our servers get {0} during marketing campaigns, while a sprint becomes {1} " +
            "when the team takes on too many tasks during planning.",
            ["overloaded", "overcommitted"],
            [["overloaded", "overcommitted"], ["overloaded", "overcommitted"]]));

        // 9. make sure vs ensure — той самий підступ: однакові варіанти на обидва пропуски,
        //    правильний вибір залежить від регістру речення (розмовне vs офіційний контракт).
        unit.TestCards.Add(TestCardBuilder.Cloze(
            "Run the tests to {0} everything works. In the official contract, the vendor must {1} 99.9% uptime.",
            ["make sure", "ensure"],
            [["make sure", "ensure"], ["make sure", "ensure"]]));

        // 10. Matching на всі три "сумнівні" слова одночасно — найважчий варіант пастки,
        //     бо всі варіанти праворуч звучать як "сумнівний", і треба зіставити з точним контекстом.
        unit.TestCards.Add(TestCardBuilder.Matching(
            "Зіставте слово з контекстом, де воно вживається правильно (усі варіанти справа " +
            "перекладаються схоже, але підходить лише один)",
            ["dodgy", "doubtful", "questionable"],
            [
                "тимчасовий костиль виглядає ___ (сленг, розмовне)",
                "малоймовірно, що ми встигнемо (сумнів в успіху)",
                "його вибір патернів дуже ___ (професійна оцінка рішення)"
            ]));

        CourseZipEditor.SaveUnit(TargetZip, UnitFile, unit);

        Console.WriteLine();
        Console.WriteLine($"[OK] Written: {TargetZip}");
        Console.WriteLine($"  testCards: {unit.TestCards.Count} (+{unit.TestCards.Count - before})");
    }
}
