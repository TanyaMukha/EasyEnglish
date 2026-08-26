using EasyPeasy.Core.Enums;
using EasyPeasy.Core.Models;

namespace EasyPeasy.Core.Content;

/// <summary>
/// Ready-made courses for an app with nothing in it.
///
/// The real course archives are not in this repository — they are large, and they are not mine to
/// publish. So a fresh clone builds, runs, and shows an empty library, and there is no way to see
/// what a unit looks like without authoring one first. These fill that gap.
///
/// They are small on purpose, but between them they use every <see cref="StudyCardKind"/> and
/// every <see cref="TestCardKind"/>, plus words with examples and irregular forms — so opening one
/// shows what the app actually does rather than that it starts. The phrasal-verb module doubles as
/// a live demonstration of the entry notation in <c>EasyPeasy.Docs/Guides/entry-notation.md</c>.
///
/// <c>Id</c> and the foreign keys are left at <c>0</c>: EF assigns them on the cascaded insert,
/// the same way an imported archive does.
/// </summary>
public static class DemoCourses
{
    /// <summary>Every demo course, in the order they are offered.</summary>
    public static IReadOnlyList<CourseModel> All() =>
    [
        Interview(),
        IrregularVerbs(),
        PhrasalVerbs(),
    ];

    // =========================================================================
    // ENGLISH FOR AN INTERVIEW
    // =========================================================================

    private static CourseModel Interview() => new()
    {
        Title = "Англійська для співбесіди",
        Description =
            "Слова й звороти, які потрібні, щоб розповісти про свій досвід і свої проєкти. "
            + "Рівень B1.",
        LanguageCode = "en-us",
        Units =
        [
            new UnitModel
            {
                Title = "Про себе",
                Description = "Як назвати свою роль, досвід і сильні сторони.",
                Words =
                [
                    Word("background", "ˈbækɡraʊnd", "досвід, підготовка",
                        ("My background is in backend development.",
                         "Мій досвід — це бекенд-розробка.")),
                    Word("to be responsible for sth", null, "відповідати за щось",
                        ("I was responsible for the payment service.",
                         "Я відповідала за платіжний сервіс.")),
                    Word("strength", "streŋθ", "сильна сторона",
                        ("My main strength is attention to detail.",
                         "Моя головна сильна сторона — увага до деталей.")),
                    Word("to look for sth", null, "шукати щось",
                        ("I am looking for a role with more ownership.",
                         "Я шукаю роль із більшою відповідальністю.")),
                    Word("notice period", "ˈnəʊtɪs ˈpɪəriəd", "термін відпрацювання"),
                    Word("{a} few", null, "кілька",
                        ("I have a few questions about the team.",
                         "У мене є кілька питань про команду.")),
                ],
                StudyCards =
                [
                    StudyCardBuilder.Term(
                        "background",
                        "Досвід і підготовка людини загалом, не одне місце роботи. "
                        + "«My background is in QA» — «Я загалом із тестування»."),
                    StudyCardBuilder.Text(
                        "Розповідь про себе за хвилину",
                        "Класична відповідь на «Tell me about yourself» складається з трьох частин: "
                        + "хто ви зараз, що зробили раніше, і чому шукаєте нову роль. "
                        + "Минулий досвід — у Past Simple, теперішню роботу — у Present Simple або "
                        + "Present Perfect Continuous.",
                        dialogue:
                        "— Tell me about yourself.\n"
                        + "— I am a backend developer with six years of experience. "
                        + "For the last three years I have been working on payment systems, "
                        + "and I am looking for a role where I can own a service end to end."),
                    StudyCardBuilder.BlurredText(
                        "I **have been working** here for three years, "
                        + "and before that I **worked** at a smaller company."),
                ],
                TestCards =
                [
                    TestCardBuilder.SingleChoice(
                        "Оберіть правильну форму: «I ___ as a developer since 2019.»",
                        ["work", "worked", "have been working", "am working"],
                        "have been working",
                        hint: "Дія почалася в минулому й триває досі."),
                    TestCardBuilder.ShortAnswer(
                        "Як сказати «сильна сторона»?",
                        ["strength", "a strength"]),
                    TestCardBuilder.Cloze(
                        "I was {0} for the release process, and my main {1} is planning.",
                        [["responsible"], ["strength"]],
                        title: "Заповніть пропуски"),
                    TestCardBuilder.Matching(
                        "З'єднайте запитання з відповіддю",
                        [
                            "What is your notice period?",
                            "What are you looking for?",
                            "What is your background?"
                        ],
                        [
                            "Two weeks.",
                            "A role with more ownership.",
                            "Backend development."
                        ]),
                ],
            },
            new UnitModel
            {
                Title = "Про проєкт",
                Description = "Як описати, що ви зробили і який це дало результат.",
                Words =
                [
                    Word("to deliver sth", null, "здати, доставити (результат)",
                        ("We delivered the feature two weeks early.",
                         "Ми здали функціонал на два тижні раніше.")),
                    Word("trade-off", "ˈtreɪd ɒf", "компроміс, поступка",
                        ("It was a trade-off between speed and accuracy.",
                         "Це був компроміс між швидкістю й точністю.")),
                    Word("to roll sth out", null, "розгорнути, викотити",
                        ("We rolled the change out to ten percent of users first.",
                         "Спершу ми викотили зміну на десять відсотків користувачів.")),
                    Word("downtime", "ˈdaʊntaɪm", "простій, час недоступності"),
                    Word("root cause", "ruːt kɔːz", "першопричина"),
                ],
                StudyCards =
                [
                    StudyCardBuilder.Term(
                        "trade-off",
                        "Свідомий вибір, у якому щось виграєш, а щось втрачаєш. "
                        + "На співбесіді це слово цінують більше за «best practice»."),
                    StudyCardBuilder.Text(
                        "Структура STAR",
                        "Situation, Task, Action, Result. Спершу контекст, потім задача, "
                        + "потім що саме зробили ви, і обов'язково результат у цифрах. "
                        + "Без останньої частини розповідь звучить як опис обов'язків.",
                        codeBlock:
                        "S: The nightly job had grown past its window.\n"
                        + "T: I was asked to bring it back under two hours.\n"
                        + "A: I profiled it and replaced three queries with one.\n"
                        + "R: It now finishes in forty minutes."),
                ],
                TestCards =
                [
                    TestCardBuilder.MultipleChoice(
                        "Які з цих слів описують результат, а не процес?",
                        ["downtime", "to deliver", "root cause", "trade-off"],
                        ["downtime", "root cause"]),
                    TestCardBuilder.Cloze(
                        "We {0} the change out gradually, so there was no {1}.",
                        [["rolled"], ["downtime"]],
                        options: [["rolled", "delivered", "looked"], []],
                        title: "Оберіть або впишіть"),
                    TestCardBuilder.ShortAnswer(
                        "Як англійською «першопричина»?",
                        ["root cause", "the root cause"]),
                ],
            },
        ],
    };

    // =========================================================================
    // IRREGULAR VERBS
    // =========================================================================

    private static CourseModel IrregularVerbs() => new()
    {
        Title = "Неправильні дієслова",
        Description = "Три форми найуживаніших неправильних дієслів, з перекладом.",
        LanguageCode = "en-us",
        Units =
        [
            new UnitModel
            {
                Title = "Перша двадцятка",
                Description = "Ті, що трапляються найчастіше.",
                IrregularForms =
                [
                    Irregular("be", "wʌz / wɜː", "was / were", "been", "бути"),
                    Irregular("begin", "bɪˈɡæn", "began", "begun", "починати"),
                    Irregular("break", "brəʊk", "broke", "broken", "ламати"),
                    Irregular("bring", "brɔːt", "brought", "brought", "приносити"),
                    Irregular("build", "bɪlt", "built", "built", "будувати"),
                    Irregular("choose", "tʃəʊz", "chose", "chosen", "вибирати"),
                    Irregular("come", "keɪm", "came", "come", "приходити"),
                    Irregular("do", "dɪd", "did", "done", "робити"),
                    Irregular("find", "faʊnd", "found", "found", "знаходити"),
                    Irregular("get", "ɡɒt", "got", "got / gotten", "отримувати"),
                    Irregular("give", "ɡeɪv", "gave", "given", "давати"),
                    Irregular("go", "went", "went", "gone", "йти"),
                    Irregular("know", "njuː", "knew", "known", "знати"),
                    Irregular("make", "meɪd", "made", "made", "робити, виготовляти"),
                    Irregular("read", "red", "read", "read", "читати"),
                    Irregular("run", "ræn", "ran", "run", "бігти, керувати"),
                    Irregular("see", "sɔː", "saw", "seen", "бачити"),
                    Irregular("take", "tʊk", "took", "taken", "брати"),
                    Irregular("write", "rəʊt", "wrote", "written", "писати"),
                ],
                StudyCards =
                [
                    StudyCardBuilder.Text(
                        "Навіщо третя форма",
                        "Друга форма — це Past Simple: «I wrote the report yesterday». "
                        + "Третя — дієприкметник, потрібний для Present Perfect і пасивного стану: "
                        + "«I have written it», «the report was written by me». "
                        + "Плутанина між ними — найпомітніша помилка в мовленні."),
                ],
                TestCards =
                [
                    TestCardBuilder.Cloze(
                        "I have {0} the documentation, and yesterday I {1} the tests.",
                        [["written"], ["wrote"]],
                        title: "Друга чи третя форма?",
                        hint: "Present Perfect бере третю форму, Past Simple — другу."),
                    TestCardBuilder.Matching(
                        "З'єднайте дієслово з його третьою формою",
                        ["choose", "break", "take", "begin"],
                        ["chosen", "broken", "taken", "begun"]),
                ],
            },
        ],
    };

    // =========================================================================
    // PHRASAL VERBS
    // =========================================================================

    private static CourseModel PhrasalVerbs() => new()
    {
        Title = "Фразові дієслова",
        Description =
            "Дієслова з прийменниками, які змінюють значення. Заразом показує, "
            + "як записуються варіанти відповіді.",
        LanguageCode = "en-us",
        Units =
        [
            new UnitModel
            {
                Title = "У роботі",
                Description = "Ті, що чути на щоденних дзвінках.",
                Content =
                    "<p>Записи в цьому модулі навмисно показують нотацію запису:</p>"
                    + "<ul>"
                    + "<li><code>[]</code> — цю частину можна не писати;</li>"
                    + "<li><code>/</code> — рівноцінні варіанти;</li>"
                    + "<li><code>sb</code> / <code>sth</code> — будь-хто і будь-що;</li>"
                    + "<li><code>{}</code> — артикль є частиною виразу й потрібен обов'язково.</li>"
                    + "</ul>",
                Words =
                [
                    Word("to figure sth out", null, "розібратися в чомусь",
                        ("It took me a day to figure the bug out.",
                         "Мені знадобився день, щоб розібратися з помилкою.")),
                    Word("to come up with sth", null, "придумати щось",
                        ("She came up with a simpler approach.",
                         "Вона придумала простіший підхід.")),
                    Word("to look [in]to sth", null, "розглянути, дослідити щось",
                        ("I will look into it tomorrow.",
                         "Я подивлюся це завтра.")),
                    Word("to catch up [with sb]", null, "наздогнати; поспілкуватися",
                        ("Let us catch up after the demo.",
                         "Поспілкуймося після демо.")),
                    Word("to put sth off / to postpone sth", null, "відкласти щось"),
                    Word("to hand sth over [to sb]", null, "передати щось комусь"),
                    Word("{the} other day", null, "нещодавно, днями"),
                ],
                StudyCards =
                [
                    StudyCardBuilder.Term(
                        "to figure sth out",
                        "Розібратися самотужки, дійти до розуміння. "
                        + "Не те саме, що «to find out» — дізнатися від когось."),
                    StudyCardBuilder.BlurredText(
                        "Can you **look into** it? I could not **figure** it **out** myself, "
                        + "so I would rather **hand it over** than **put it off** again.",
                        BlurRevealMode.Independent),
                ],
                TestCards =
                [
                    TestCardBuilder.ShortAnswer(
                        "«розібратися в чомусь» — фразове дієслово",
                        ["figure sth out", "to figure sth out", "figure out"]),
                    TestCardBuilder.SingleChoice(
                        "«I will ___ it tomorrow» — розгляну це завтра",
                        ["look into", "look after", "look for", "look up"],
                        "look into"),
                    TestCardBuilder.Cloze(
                        "We had to {0} the release {1} because of the outage.",
                        [["put"], ["off"]],
                        title: "Відкласти реліз"),
                ],
            },
        ],
    };

    // =========================================================================
    // HELPERS
    // =========================================================================

    private static WordModel Word(
        string word,
        string? transcription,
        string translation,
        params (string Sentence, string Translation)[] examples) => new()
        {
            Word = word,
            Transcription = transcription,
            Translation = translation,
            Examples = examples
                .Select(one => new ExampleModel
                {
                    Sentence = one.Sentence,
                    Translation = one.Translation,
                })
                .ToList(),
        };

    private static IrregularFormModel Irregular(
        string first,
        string? secondTranscription,
        string second,
        string third,
        string translation) => new()
        {
            FirstForm = first,
            PartOfSpeech = nameof(PartOfSpeech.Verb),
            FirstFormTranslation = translation,
            SecondForm = second,
            SecondFormTranscription = secondTranscription,
            ThirdForm = third,
        };
}
