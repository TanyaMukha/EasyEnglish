using EasyEnglish.ContentTools;
using EasyEnglish.Core.Enums;
using EasyEnglish.Core.Models;
using System.Text.Json.Nodes;

// Usage: dotnet run -- <module-key>
// Each course module has its own method below — just add a new one when the next module comes up.
// With no arguments, the most recently added module runs by default.

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

var moduleKey = args.Length > 0 ? args[0] : "english-for-it-b2-unit1-reset-first10-ids";

switch (moduleKey)
{
    case "english-for-it-b2-unit1":
        EnglishForItB2Unit1.Run();
        break;

    case "english-for-it-b2-unit1-traps":
        EnglishForItB2Unit1TranslationTraps.Run();
        break;

    case "english-for-it-b2-prepositions-of-time":
        EnglishForItB2PrepositionsOfTime.Run();
        break;

    case "english-for-it-b2-prepositions-of-place":
        EnglishForItB2PrepositionsOfPlace.Run();
        break;

    case "english-for-it-b2-unit1-reset-first10-ids":
        EnglishForItB2Unit1ResetFirst10Ids.Run();
        break;

    default:
        Console.WriteLine($"Невідомий ключ модуля: {moduleKey}");
        Environment.Exit(1);
        break;
}

/// <summary>Prints a short summary of a unit's contents — how many words/cards of each kind.</summary>
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
/// "Present Simple #1" (<c>english_for_it_b2_1.zip</c>) — a mix of card kinds: <c>StudyCard</c>
/// (Term, Text, BlurredText) and <c>TestCard</c> (SingleChoice, MultipleChoice, Cloze, Matching).
/// ShortAnswer cards (sentence translation) for this same module were already added separately
/// earlier.
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

        // ── StudyCard: Term -- a grammar rule, not a single vocabulary word ──
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

        // ── StudyCard: Text -- a short passage weaving several expressions together ──
        unit.StudyCards.Add(StudyCardBuilder.Text(
            "Ранок у команді розробки",
            "Every morning our team gathers for a standup. The Scrum Master insists that everyone " +
            "updates Jira by the end of the day. If an architectural decision seems dodgy, we get " +
            "together for a tech brainstorm. Our tech lead usually signs off on the plan before we " +
            "start coding."));

        // ── StudyCard: BlurredText -- Independent (one span) and Grouped (a split expression) ──
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

        // ── TestCard: Cloze -- one blank with a dropdown, the other free-text input ──
        unit.TestCards.Add(TestCardBuilder.Cloze(
            "The Scrum Master {0} that everyone updates Jira {1}.",
            [["insists"], ["by the end of the day", "by end of day"]],
            [["insists", "expects", "ensures"], []]));

        unit.TestCards.Add(TestCardBuilder.Cloze(
            "Please {0} this feature {1} smaller user stories.",
            [["break down"], ["into"]]));

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
/// A second pass over the same module — "gotcha" cards on translation nuance. The module is
/// dense with pairs of near-synonyms that a dictionary translates almost identically but that are
/// actually used differently (register/formality, emotional vs. technical meaning, who typically
/// uses which). Every card deliberately puts a "looks right at first glance but wrong for this
/// context" synonym among the answer options as a distractor.
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

        // 1. dodgy vs questionable vs doubtful -- all three translate as "сумнівний", but the
        //    context here clearly calls for the colloquial/slang word, not a neutral professional one.
        unit.TestCards.Add(TestCardBuilder.SingleChoice(
            "'Цей тимчасовий костиль виглядає для мене трохи ___.' (розмовне, сленгове слово)",
            ["doubtful", "questionable", "dodgy", "difficult"],
            "dodgy"));

        // 2. difficult vs complex -- both mean "складний", but difficult is about something
        //    emotionally/situationally hard, while complex is about technical architecture/code complexity.
        unit.TestCards.Add(TestCardBuilder.SingleChoice(
            "'Ця складна архітектура вимагає глибокого аналізу.' Яке слово тут доречне?",
            ["difficult", "complex", "dodgy", "doubtful"],
            "complex"));

        // 3. make sure vs ensure -- synonyms, but ensure is more formal, for official text.
        unit.TestCards.Add(TestCardBuilder.SingleChoice(
            "Офіційний звіт для менеджменту: 'We need to ___ smooth deployment.'",
            ["make sure", "ensure", "expect", "insist"],
            "ensure"));

        // 4. meet standards vs comply with standards vs be up to standard -- all about "meeting
        //    standards," but a legal/regulatory context specifically calls for comply with, not the
        //    more common-in-IT meet standards.
        unit.TestCards.Add(TestCardBuilder.SingleChoice(
            "'Медичне ПЗ повинно ___ суворим стандартам безпеки.' (йдеться про обов'язкові регламенти)",
            ["meet standards", "comply with standards", "be up to standard", "take on standards"],
            "comply with standards"));

        // 5. to have a standup vs to hold a standup -- both mean "running a standup," but hold
        //    emphasizes organizing/moderating the process, have just participation.
        unit.TestCards.Add(TestCardBuilder.SingleChoice(
            "'Скрам-майстер організовує щоденний стендап.' (акцент саме на організації)",
            ["has a standup", "holds a standup", "gets a standup", "takes a standup"],
            "holds a standup"));

        // 6. to gather vs to get together for -- both mean "gathering together," but get together
        //    for has a warmer, informal shade (a Friday brainstorm among a close-knit team).
        unit.TestCards.Add(TestCardBuilder.SingleChoice(
            "'Ми зазвичай ___ на технічний брейншторм щоп'ятниці.' (неформально, по-дружньому)",
            ["gather", "get together for", "hold", "meet"],
            "get together for"));

        // 7. Grammar trap: "insist that + subject + verb" -- the module establishes exactly how
        //    this is used ('insists that everyone updates'); the distractors are typical mistakes
        //    mixing "insist on + gerund/to-infinitive" with "insist that + clause."
        unit.TestCards.Add(TestCardBuilder.SingleChoice(
            "Scrum-майстер наполягає, щоб кожен оновлював Jira. Оберіть граматично правильний варіант:",
            [
                "insists on everyone updates Jira",
                "insists that everyone updates Jira",
                "insists on everyone to update Jira",
                "insists that everyone to updating Jira"
            ],
            "insists that everyone updates Jira"));

        // 8. overloaded vs overcommitted -- the same dropdown-blank trick across both clauses:
        //    the options are identical, so the trap isn't the vocabulary itself but which subject
        //    (a server vs. a sprint) each word actually attaches to.
        unit.TestCards.Add(TestCardBuilder.Cloze(
            "Our servers get {0} during marketing campaigns, while a sprint becomes {1} " +
            "when the team takes on too many tasks during planning.",
            [["overloaded"], ["overcommitted"]],
            [["overloaded", "overcommitted"], ["overloaded", "overcommitted"]]));

        // 9. make sure vs ensure -- same trick: identical options on both blanks, and the right
        //    choice depends on the sentence's register (colloquial vs. official contract).
        unit.TestCards.Add(TestCardBuilder.Cloze(
            "Run the tests to {0} everything works. In the official contract, the vendor must {1} 99.9% uptime.",
            [["make sure"], ["ensure"]],
            [["make sure", "ensure"], ["make sure", "ensure"]]));

        // 10. Matching across all three "сумнівний"-translating words at once -- the hardest
        //     variant of the trap, since every option on the right sounds like a plausible match
        //     and the learner has to pin down the exact context.
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

/// <summary>
/// The course's second module — "Prepositions of place" (at/on/in/by by spatial meaning). Unlike
/// earlier runs, this doesn't just append cards to an existing unit file: the whole
/// <c>unit_2.json</c> is new, so it also has to be registered in <c>course.json</c> — otherwise
/// the app won't see this file on import, since the manifest is the sole source of truth for
/// which module files exist in the archive.
/// </summary>
internal static class EnglishForItB2PrepositionsOfPlace
{
    private const string SourceZip = @"C:\Users\User\Downloads\english_for_it_b2_2026-07-08_updated.zip";
    private const string TargetZip = @"C:\Users\User\Downloads\english_for_it_b2_2026-07-08_updated2.zip";
    private const string UnitFile = "units/unit_2.json";
    private const string UnitTitle = "Prepositions of place";

    // (sentence template with {0}, the correct preposition, full-sentence translation -- becomes the Hint)
    private static readonly (string Template, string Answer, string Hint)[] Cards =
    [
        // ── AT -- precise points and locations ──
        ("She lives {0} 35 Hill St.", "at", "Вона живе на Хілл-стріт, 35."),
        ("Sherlock Holmes lived {0} 221B Baker Street.", "at", "Шерлок Голмс жив на Бейкер-стріт, 221Б."),
        ("Let's meet {0} the intersection of Pine & Maple.", "at", "Давай зустрінемось на перехресті Пайн і Мейпл."),
        ("The shop is located {0} the corner of First and Main.", "at", "Магазин розташований на розі Фьорст і Мейн."),
        ("I met my friends {0} a party last night.", "at", "Учора ввечері я зустрів(-ла) друзів на вечірці."),
        ("We listened to live music {0} a concert.", "at", "Ми слухали живу музику на концерті."),
        ("They are watching a film {0} the movies.", "at", "Вони дивляться фільм у кінотеатрі."),
        ("My plane lands {0} the airport in an hour.", "at", "Мій літак приземлиться в аеропорту через годину."),
        ("I am waiting for you {0} the bus stop.", "at", "Я чекаю на тебе на автобусній зупинці."),
        ("The cargo arrived {0} the train station.", "at", "Вантаж прибув на залізничну станцію."),
        ("My dad is currently {0} work.", "at", "Мій тато зараз на роботі."),
        ("Children learn many things {0} school.", "at", "Діти вчаться багато чого в школі."),
        ("She is typing a report {0} the office.", "at", "Вона друкує звіт в офісі."),
        ("I need to withdraw some cash {0} the bank.", "at", "Мені потрібно зняти готівку в банку."),
        ("It's late, so I am going to stay {0} home.", "at", "Вже пізно, тож я залишусь вдома."),
        ("Someone is knocking {0} the door.", "at", "Хтось стукає у двері."),
        ("Show your ticket {0} the entrance.", "at", "Покажіть квиток на вході."),
        ("You will find the footnotes {0} the bottom of the page.", "at", "Виноски ви знайдете внизу сторінки."),
        ("There is a beautiful view {0} the top of the mountain.", "at", "На вершині гори чудовий краєвид."),
        ("His house is {0} the end of the street.", "at", "Його будинок в кінці вулиці."),
        ("The family gathered {0} the table for dinner.", "at", "Родина зібралася за столом на вечерю."),
        ("I spent the whole day working {0} my desk.", "at", "Я провів(-ла) цілий день, працюючи за своїм столом."),
        ("She stood {0} the window, looking at the rain.", "at", "Вона стояла біля вікна, дивлячись на дощ."),
        ("We discussed the new project {0} lunch.", "at", "Ми обговорили новий проєкт за обідом."),
        ("He is currently speaking {0} a meeting.", "at", "Зараз він виступає на нараді."),
        ("Leaders met {0} a conference to discuss climate change.", "at", "Лідери зустрілися на конференції, щоб обговорити зміну клімату."),

        // ── ON -- surfaces, streets, and transport ──
        ("Turn left {0} Hill St.", "on", "Поверніть ліворуч на Хілл-стріт."),
        ("There is a lot of traffic {0} San Fernando Rd today.", "on", "Сьогодні на Сан-Фернандо-роуд великі затори."),
        ("They live in a small house {0} Fairfax Ave.", "on", "Вони живуть у невеликому будинку на Ферфакс-авеню."),
        ("The hotel is located {0} Riverside Dr.", "on", "Готель розташований на Ріверсайд-драйв."),
        ("Walk down the stars {0} Hollywood Blvd.", "on", "Пройдись вздовж зірок на Голлівудському бульварі."),
        ("We drove our car {0} Highway 101.", "on", "Ми їхали машиною по трасі 101."),
        ("Our office is {0} the 15th floor.", "on", "Наш офіс на 15-му поверсі."),
        ("The reception desk is {0} the ground floor.", "on", "Стійка реєстрації на першому поверсі."),
        ("You can find the gym {0} the second level.", "on", "Спортзал ви знайдете на другому рівні."),
        ("Put the keys {0} the table, please.", "on", "Поклади ключі на стіл, будь ласка."),
        ("He hung a beautiful picture {0} the wall.", "on", "Він повісив красиву картину на стіну."),
        ("There is a fly {0} the ceiling!", "on", "На стелі муха!"),
        ("I read an interesting book while {0} the bus.", "on", "Я читав(-ла) цікаву книгу, поки їхав(-ла) в автобусі."),
        ("You can buy snacks {0} the train.", "on", "У потязі можна купити перекуски."),
        ("It is mandatory to fasten seatbelts {0} the plane.", "on", "У літаку обов'язково пристібати ремені безпеки."),
        ("Central Park is located {0} Manhattan Island.", "on", "Центральний парк розташований на острові Мангеттен."),
        ("We spent our summer vacation {0} Maui.", "on", "Ми провели літню відпустку на Мауї."),
        ("The pharmacy is {0} the left.", "on", "Аптека зліва."),
        ("You need to turn {0} the right after the traffic light.", "on", "Тобі потрібно повернути праворуч після світлофора."),
        ("The grass is greener {0} the other side.", "on", "З іншого боку трава завжди зеленіша."),
        ("I found this information {0} the website.", "on", "Я знайшов(-ла) цю інформацію на сайті."),
        ("What is written {0} the screen?", "on", "Що написано на екрані?"),
        ("You can watch this video {0} the internet.", "on", "Це відео можна подивитись в інтернеті."),
        ("Put a hat {0} your head, it's cold outside.", "on", "Одягни шапку на голову, надворі холодно."),
        ("She rested her head {0} my shoulder.", "on", "Вона поклала голову мені на плече."),
        ("He had a look of surprise {0} his face.", "on", "На його обличчі був вираз здивування."),

        // ── IN -- enclosed spaces and geography ──
        ("We live {0} an apartment in the city center.", "in", "Ми живемо в квартирі в центрі міста."),
        ("She rents a small flat {0} London.", "in", "Вона орендує невелику квартиру в Лондоні."),
        ("There are five rooms {0} a house.", "in", "У будинку п'ять кімнат."),
        ("Turn off the light when you are not {0} a room.", "in", "Вимикай світло, коли тебе немає в кімнаті."),
        ("Hollywood is located {0} Los Angeles.", "in", "Голлівуд розташований у Лос-Анджелесі."),
        ("I want to visit Times Square {0} New York.", "in", "Я хочу відвідати Таймс-сквер у Нью-Йорку."),
        ("Millions of people live {0} Tokyo.", "in", "Мільйони людей живуть у Токіо."),
        ("Silicon Valley is {0} California.", "in", "Кремнієва долина знаходиться в Каліфорнії."),
        ("Austin is the capital city {0} Texas.", "in", "Остін — столиця штату Техас."),
        ("Toronto is a major city {0} Ontario.", "in", "Торонто — велике місто в провінції Онтаріо."),
        ("Washington D.C. is {0} the U.S.", "in", "Вашингтон, округ Колумбія, знаходиться у США."),
        ("It gets very cold during winter {0} Canada.", "in", "Взимку в Канаді буває дуже холодно."),
        ("Kyoto is an ancient city {0} Japan.", "in", "Кіото — стародавнє місто в Японії."),
        ("Canada and the U.S. are located {0} North America.", "in", "Канада та США розташовані в Північній Америці."),
        ("Ukraine is the largest country located entirely {0} Europe.", "in", "Україна — найбільша країна, розташована повністю в Європі."),
        ("China and India are located {0} Asia.", "in", "Китай та Індія розташовані в Азії."),
        ("Many diverse creatures live {0} the ocean.", "in", "Багато різноманітних істот живуть в океані."),
        ("We enjoyed swimming {0} the lake.", "in", "Нам сподобалось плавати в озері."),
        ("The water {0} the river was crystal clear.", "in", "Вода в річці була кришталево чистою."),
        ("The Walk of Fame is a famous landmark {0} Hollywood.", "in", "Алея Слави — відома визначна пам'ятка в Голлівуді."),
        ("We had a great dinner at a restaurant {0} Soho.", "in", "Ми чудово повечеряли в ресторані в Сохо."),
        ("Many families prefer to live {0} the suburbs.", "in", "Багато сімей воліють жити в передмісті."),
        ("My grandmother grows roses {0} the garden.", "in", "Моя бабуся вирощує троянди в саду."),
        ("Children are playing football {0} the yard.", "in", "Діти грають у футбол у дворі."),
        ("Let's take a walk {0} the park.", "in", "Давай прогуляємось у парку."),
        ("Don't leave your keys {0} the car.", "in", "Не залишай ключі в машині."),
        ("We caught a ride {0} a taxi to get to the station.", "in", "Ми зловили таксі, щоб дістатись до станції."),
        ("The rescue team arrived {0} a helicopter.", "in", "Рятувальна команда прибула на гелікоптері."),
        ("What is hidden {0} the box?", "in", "Що заховано в коробці?"),
        ("I keep my passport {0} my bag.", "in", "Я тримаю паспорт у своїй сумці."),
        ("The documents are stored {0} the drawer.", "in", "Документи зберігаються в шухляді."),

        // ── Special cases -- the same physical object, different preposition meaning ──
        ("He is still {0} school; he wants to become a doctor.", "in", "Він досі навчається (у школі); хоче стати лікарем. (in — означає бути учнем/студентом)"),
        ("My mom is physically {0} school right now for a parent meeting.", "at", "Моя мама зараз фізично в школі — на батьківських зборах. (at — фізично перебуває у будівлі)"),
        ("He is lucky to be {0} work during the economic crisis.", "in", "Йому пощастило мати роботу під час економічної кризи. (in — має роботу, працевлаштований)"),
        ("I cannot talk right now, I am {0} work.", "at", "Я не можу зараз говорити, я на роботі. (at — фізично на робочому місці)"),
        ("Kids play football {0} the street and it can be dangerous.", "in", "Діти грають у футбол на проїжджій частині вулиці, і це може бути небезпечно. (in — буквально на дорозі)"),
        ("He lives {0} the street, meaning he is homeless.", "on", "Він живе на вулиці, тобто він бездомний. (on — вздовж вулиці / як безхатько)"),
        ("The fish is swimming {0} the water.", "in", "Риба плаває у воді. (in — занурена у воду)"),
        ("The boat is floating {0} the water.", "on", "Човен пливе по воді. (on — на поверхні води)"),
        ("He is still {0} bed, sleeping soundly.", "in", "Він досі в ліжку, міцно спить. (in — під ковдрою, спить)"),
        ("I left my phone {0} the bed, on top of the blanket.", "on", "Я залишив(-ла) телефон на ліжку, зверху на ковдрі. (on — на поверхні ліжка)"),
        ("He is recovering from surgery {0} the hospital.", "in", "Він одужує після операції в лікарні. (in — як пацієнт)"),
        ("I am visiting my friend {0} the hospital.", "at", "Я відвідую свого друга в лікарні. (at — як відвідувач)"),

        // ── BY -- proximity and being located nearby ──
        ("Sit {0} the window to get more light.", "by", "Сядь біля вікна, щоб було більше світла."),
        ("Please leave your umbrella {0} the door.", "by", "Будь ласка, залиш парасольку біля дверей."),
        ("It is cozy to sit {0} the fireplace in winter.", "by", "Взимку затишно сидіти біля каміна."),
        ("We built a beautiful campfire {0} the river.", "by", "Ми розклали чудове багаття біля річки."),
        ("They bought a house {0} the ocean.", "by", "Вони купили будинок біля океану."),
        ("We stayed in a cabin {0} the mountains.", "by", "Ми зупинялися в хатинці біля гір."),
        ("Don't worry, I will always stay {0} your side.", "by", "Не хвилюйся, я завжди буду поруч з тобою."),
        ("The hitchhiker stood {0} the road, trying to catch a car.", "by", "Автостопник стояв біля дороги, намагаючись зловити машину."),
        ("Meet me {0} the entrance of the cinema.", "by", "Зустрінь мене біля входу в кінотеатр."),
        ("Leave a note {0} the telephone.", "by", "Залиш записку біля телефону."),
        ("I spent hours sitting {0} the computer today.", "by", "Сьогодні я годинами сидів(-ла) біля комп'ютера."),
        ("Let's meet {0} the coffee machine for a quick chat.", "by", "Давай зустрінемось біля кавомашини, щоб швидко поговорити."),
        ("The park is located right {0} the school.", "by", "Парк розташований прямо біля школи."),
        ("There is a bus stop {0} the hospital.", "by", "Біля лікарні є автобусна зупинка."),
        ("You can park your car {0} the library.", "by", "Ти можеш припаркувати машину біля бібліотеки."),
        ("Place the chair {0} the table.", "by", "Постав стілець біля столу."),
        ("I relaxed {0} the sofa after a long day.", "by", "Я відпочивав(-ла) біля дивана після довгого дня."),
        ("The cat is sleeping {0} the bookshelf.", "by", "Кіт спить біля книжкової шафи."),
    ];

    public static void Run()
    {
        CourseZipEditor.CopyArchive(SourceZip, TargetZip);

        var unit = new UnitModel
        {
            RecordGuid = Guid.NewGuid(),
            Title      = UnitTitle,
            TestCards  = new List<TestCardModel>(),
        };

        foreach (var (template, answer, hint) in Cards)
        {
            unit.TestCards.Add(TestCardBuilder.Cloze(template, [[answer]], hint: hint));
        }

        CourseZipEditor.SaveUnit(TargetZip, UnitFile, unit);

        var manifest = CourseZipEditor.LoadManifestNode(TargetZip);
        var unitsArray = manifest["units"]!.AsArray();
        unitsArray.Add(new JsonObject
        {
            ["unitGuid"]   = unit.RecordGuid.ToString(),
            ["fileName"]   = UnitFile,
            ["title"]      = UnitTitle,
            ["wordCount"]  = 0,
            ["audioFiles"] = new JsonArray(),
            ["imageFiles"] = new JsonArray(),
        });
        CourseZipEditor.SaveManifestNode(TargetZip, manifest);

        Console.WriteLine($"[OK] Written: {TargetZip}");
        Console.WriteLine($"  new unit \"{UnitTitle}\": testCards={unit.TestCards.Count}");
        Console.WriteLine($"  course.json units: {unitsArray.Count}");
    }
}
/// <summary>
/// The course's first module — "Prepositions of time" (at/on/in/by by time meaning). The opening
/// cards (all with answer "at") were already created by hand through the app and are already in
/// this ZIP archive. This run appends the remaining 61 cards of the same exercise (at/on/in/by),
/// each with a full-sentence translation in the <c>Hint</c> field.
/// </summary>
internal static class EnglishForItB2PrepositionsOfTime
{
    private const string SourceZip = @"C:\Users\User\Downloads\english_for_it_b2_2026-07-08.zip";
    private const string TargetZip = @"C:\Users\User\Downloads\english_for_it_b2_2026-07-08_updated.zip";
    private const string UnitFile = "units/unit_1.json";

    // (sentence template with {0}, the correct preposition, full-sentence translation -- becomes the Hint)
    private static readonly (string Template, string Answer, string Hint)[] Cards =
    [
        ("{0} the end, everyone was crying.", "at", "Наприкінці всі плакали."),
        ("{0} first, I didn't like the book.", "at", "Спочатку мені не сподобалась ця книга."),
        ("{0} last, I understand!", "at", "Нарешті я розумію!"),
        ("The road can be dangerous {0} times.", "at", "Ця дорога часом буває небезпечною."),
        ("Keep your passport with you {0} all times.", "at", "Завжди тримайте паспорт при собі."),
        ("Come here {0} once!", "at", "Іди сюди негайно!"),
        ("I knew {0} a glance that something was wrong.", "at", "Я з першого погляду зрозумів(-ла), що щось не так."),
        ("She moved to Paris {0} the age of 20.", "at", "Вона переїхала до Парижа у віці 20 років."),
        ("The baby weighed 7 pounds {0} birth.", "at", "При народженні дитина важила 7 фунтів."),
        ("{0} death, the body stops functioning.", "at", "На момент смерті організм припиняє функціонувати."),
        ("The birds start singing {0} sunrise.", "at", "Птахи починають співати на світанку."),
        ("The temperature drops quickly {0} sunset.", "at", "Температура швидко падає на заході сонця."),
        ("The train leaves {0} noon.", "at", "Потяг відправляється опівдні."),
        ("We'll discuss it {0} lunch.", "at", "Ми обговоримо це під час обіду."),
        ("Let's all jump {0} the same time!", "at", "Давайте всі стрибнемо одночасно!"),
        ("Please arrive {0} time.", "on", "Будь ласка, прийдіть вчасно."),
        ("The project is {0} schedule.", "on", "Проєкт іде за графіком."),
        ("I'm {0} a break right now.", "on", "Я зараз на перерві."),
        ("They're {0} vacation in Spain.", "on", "Вони у відпустці в Іспанії."),
        ("We'll be {0} holiday next week.", "on", "Наступного тижня ми будемо у відпустці."),
        ("He's {0} leave from the army.", "on", "Він у звільненні з армії."),
        ("She's in Tokyo {0} business.", "on", "Вона в Токіо у відрядженні."),
        ("They met {0} a trip to India.", "on", "Вони познайомилися під час поїздки до Індії."),
        ("The doctor is {0} duty tonight.", "on", "Лікар сьогодні на чергуванні."),
        ("I like to hike {0} the weekend.", "on", "Мені подобається ходити в походи на вихідних."),
        ("The gym opens early {0} weekdays.", "on", "У будні дні спортзал відкривається рано."),
        ("We play board games {0} a rainy day.", "on", "У дощовий день ми граємо в настільні ігри."),
        ("The park is crowded {0} a sunny day.", "on", "У сонячний день у парку багато людей."),
        ("The meeting is {0} Monday morning.", "on", "Зустріч у понеділок вранці."),
        ("Let's meet {0} Friday evening.", "on", "Давай зустрінемось у п'ятницю ввечері."),
        ("The accident happened {0} a winter afternoon.", "on", "Аварія сталася одного зимового дня після обіду."),
        ("There are always fireworks {0} New Year's Eve.", "on", "На Новорічну ніч завжди бувають феєрверки."),
        ("We'll leave {0} the day before Christmas.", "on", "Ми поїдемо за день до Різдва."),
        ("{0} occasion, I enjoy a glass of wine.", "on", "Іноді мені подобається келих вина."),
        ("Sorry, I was {0} the phone.", "on", "Вибач, я розмовляв(-ла) по телефону."),
        ("This happened {0} the past.", "in", "Це сталося в минулому."),
        ("I'll be ready {0} a moment.", "in", "Я буду готовий(-а) за мить."),
        ("The bus arrives {0} a minute.", "in", "Автобус приїде за хвилину."),
        ("I'll be with you {0} a second.", "in", "Я буду з тобою за секунду."),
        ("The plane leaves {0} an hour.", "in", "Літак вилітає за годину."),
        ("We arrived {0} time for the concert.", "in", "Ми прибули вчасно на концерт."),
        ("Please book {0} advance.", "in", "Будь ласка, бронюйте заздалегідь."),
        ("{0} those days, people wrote letters.", "in", "У ті часи люди писали листи."),
        ("Life was simpler {0} the old days.", "in", "У старі часи життя було простішим."),
        ("{0} my childhood, I lived near the sea.", "in", "У дитинстві я жив(-ла) біля моря."),
        ("The economy usually grows {0} peacetime.", "in", "У мирний час економіка зазвичай зростає."),
        ("Food was rationed {0} wartime.", "in", "У воєнний час їжу видавали за нормами."),
        ("The country is {0} crisis.", "in", "Країна перебуває в кризі."),
        ("{0} theory, the plan should work.", "in", "Теоретично цей план має спрацювати."),
        ("{0} practice, it's more complicated.", "in", "На практиці все складніше."),
        ("These drugs are dangerous {0} combination.", "in", "Ці препарати небезпечні в поєднанні."),
        ("{0} conclusion, I believe we should accept the offer.", "in", "На завершення, я вважаю, що нам слід прийняти цю пропозицію."),
        ("We spent $500 {0} total.", "in", "Загалом ми витратили 500 доларів."),
        ("{0} general, I prefer summer to winter.", "in", "Загалом, я віддаю перевагу літу, а не зимі."),
        ("{0} the time we arrived, the movie had started.", "by", "На той час, коли ми прибули, фільм уже почався."),
        ("I'll be finished {0} then.", "by", "На той час я вже закінчу."),
        ("They should have arrived {0} now.", "by", "Вони вже мали б прибути на цей час."),
        ("{0} this time tomorrow, we'll be in Paris.", "by", "Завтра о цій порі ми будемо в Парижі."),
        ("{0} the end of the day, please submit your report.", "by", "Будь ласка, здайте звіт до кінця дня."),
        ("This is {0} far the best solution.", "by", "Це, безсумнівно, найкраще рішення."),
        ("{0} all means, come to dinner tonight.", "by", "Обов'язково приходь сьогодні на вечерю."),
    ];

    public static void Run()
    {
        CourseZipEditor.CopyArchive(SourceZip, TargetZip);
        var unit = CourseZipEditor.LoadUnit(TargetZip, UnitFile);

        Console.WriteLine($"Loaded unit: {unit.Title}");
        Console.WriteLine($"  testCards: {unit.TestCards?.Count ?? 0}");

        unit.TestCards ??= new List<TestCardModel>();
        var before = unit.TestCards.Count;

        foreach (var (template, answer, hint) in Cards)
        {
            unit.TestCards.Add(TestCardBuilder.Cloze(template, [[answer]], hint: hint));
        }

        CourseZipEditor.SaveUnit(TargetZip, UnitFile, unit);

        Console.WriteLine();
        Console.WriteLine($"[OK] Written: {TargetZip}");
        Console.WriteLine($"  testCards: {unit.TestCards.Count} (+{unit.TestCards.Count - before})");
    }
}

/// <summary>
/// A one-off correction: the first 10 "Prepositions of time" cards kept the <c>Id</c>/<c>UnitId</c>
/// (91-100 / 116) from their original real export out of the app. But the user hasn't imported any
/// of the archives built here back in yet — so those IDs aren't actually tied to anything real and
/// are just misleading. Resets them to <c>0</c> (matching every other card this tool builds), so
/// that on a future import all 71+113 cards are treated the same way — as new.
/// </summary>
internal static class EnglishForItB2Unit1ResetFirst10Ids
{
    private const string ZipPath = @"C:\Users\User\Downloads\english_for_it_b2_2026-07-08_updated2.zip";
    private const string UnitFile = "units/unit_1.json";
    private const int FirstCardsCount = 10;

    public static void Run()
    {
        var unit = CourseZipEditor.LoadUnit(ZipPath, UnitFile);
        unit.TestCards ??= new List<TestCardModel>();

        var reset = 0;
        foreach (var card in unit.TestCards.Take(FirstCardsCount))
        {
            card.Id = 0;
            card.UnitId = 0;
            card.UpdatedAt = null;
            reset++;
        }

        CourseZipEditor.SaveUnit(ZipPath, UnitFile, unit);

        Console.WriteLine($"[OK] Reset {reset} card(s) to clean/new state in {ZipPath}");
    }
}
