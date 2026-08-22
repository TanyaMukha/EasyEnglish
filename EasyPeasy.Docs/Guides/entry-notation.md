# Entry Notation

How a word or phrase is written when it is added to a module, and what the app accepts as a
correct answer for it.

This is authoring syntax, not a rendering trick: the same text is what the learner types against in
the manual-input exercises, so the notation exists to describe **one entry that has several correct
typings** without listing them all. `AnswerMatcher` parses it; `EntryTextRenderer` decides how much
of it the learner sees.

## The one-way rule

Everything below allows the learner to type **less** than the entry contains. Nothing allows typing
**more**.

> The learner may leave out what the entry marked as skippable; the learner may not add a word the
> entry never had.

`look [at] sb` accepts *look at sb* and *look sb*. It does not accept *look after sb* — that word
was never in the entry.

## What may be left out by default

| Written | Also accepted | Why |
|---|---|---|
| `a book`, `an hour` | `book`, `hour` | `a` / `an` are optional wherever they stand — see below |
| `to look at sb` | `look at sb` | a leading `to` reads as the infinitive marker |
| `the same` | `same` | a leading `the` follows the same logic |
| `go to school` | — | `to` here is not leading, so it stays required |
| `in the end` | — | same for `the` — not the first word, not optional |

`a` / `an` are the only ones optional in any position, and that is deliberate: an indefinite
article is rarely a fixed part of the phrase. Put the phrase in a sentence and a possessive
usually takes the article's place — *make up your mind* for `make up a mind`, *do your best* for
`do a best`. An entry that demanded the article would be demanding a word the language itself
drops, so the app never requires it.

`the` and `to` are different: they are droppable only as the first word, where they act as a
grammatical marker for the entry as a whole. Further along they carry meaning — *go to school*,
*in the end* — and stay required.

## `{}` — the particle is part of the entry

Sometimes `a` / `an` / `the` / `to` at the start is not a grammatical marker but an inseparable
part of the expression itself. Wrap it in braces and it becomes required:

```
{a} few             → "a few" is several; "few" is hardly any — the article carries the meaning
{to} and fro        → this "to" is part of the idiom, it can never be an infinitive marker
{the} other day     → a fixed idiom; "other day" on its own is not the phrase
{the} Netherlands   → the article belongs to the name
```

Without the braces the app would accept *few* for `a few`, and *and fro* for `to and fro`: it
reads a leading article or `to` as a droppable marker, which is exactly the wrong call here.

Braces are the escape hatch, so use them only for that case: for an ordinary verb (`to look at
sb`) leave the `to` bare, so that both typings pass.

The braces are **not shown to the learner** — the entry appears as plain *a few*. They only tell
the matcher not to treat the first word as droppable.

## `[]` — this part is optional

The whole group inside the brackets may be typed or skipped, and several groups combine freely
(`take [good] care [of sb]` has four acceptable typings — the matcher backtracks instead of
expanding all 2ⁿ combinations).

The bracket characters are not displayed either — the text inside them is simply dimmed. That
keeps the entry readable as a phrase while still saying "this part is optional".

## `/` — equivalent wordings

The spacing decides the scope:

| Written | Means |
|---|---|
| `configuration / config` | two whole alternatives — either one is a correct answer |
| `to configure / to set up` | likewise, whole phrases |
| `look at sb/sth` | a choice for that position only: *look at sb* or *look at sth* |

## Placeholders

`sb` and `sth` may be typed in any of their usual spellings, so the learner is never made to guess
which one the author picked:

- `sb` = `smb` = `somebody` = `someone` (and the `'s` forms)
- `sth` = `smth` = `something` (and the `'s` forms)
- `oneself` = `yourself`

Placeholders are dimmed in the shown entry the same way bracketed parts are.

## Where this lives

| Piece | File |
|---|---|
| Parsing and matching | [`EasyPeasy.App/Services/AnswerMatcher.cs`](../../EasyPeasy.App/Services/AnswerMatcher.cs) |
| What the learner sees | [`EasyPeasy.App/Services/EntryTextRenderer.cs`](../../EasyPeasy.App/Services/EntryTextRenderer.cs), rendered by `Components/Shared/EntryText.razor` |
| Tests | `EasyPeasy.App.Tests/AnswerMatcherTests.cs`, `EntryTextRendererTests.cs` |

What the learner sees comes from `EntryTextRenderer`: it removes every marker character and dims
the optional parts and placeholders instead. `AnswerMatcher` also exposes two plain-text helpers —
`ToDisplayForm` (drops all markers) and `StripLiteralMarkers` (drops only the braces) — for callers
that need the bare string rather than HTML; nothing in the app uses them yet outside the tests.
