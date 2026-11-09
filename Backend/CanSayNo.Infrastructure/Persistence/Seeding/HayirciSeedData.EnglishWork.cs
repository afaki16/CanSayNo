using CanSayNo.Domain.Entities.Hayirci;
using CanSayNo.Domain.Enums;
using static CanSayNo.Domain.Enums.CharacterArchetype;
using static CanSayNo.Domain.Enums.PressureTactic;

namespace CanSayNo.Infrastructure.Persistence.Seeding
{
    /// <summary>English scenarios — Work &amp; Career (free tier). Language is stamped "en" by BuildEnglishScenarios.</summary>
    public static partial class HayirciSeedData
    {
        private static List<Scenario> BuildEnglishWorkScenarios() => new()
        {
            Make("en_is_001_friday_deadline", "The 5:45 pm Friday \"quick favour\"",
                "Your team lead drops a weekend-sized task on you right before you log off",
                ContextId.Is, 1, 3, 1, new[] { "deadline", "weekend", "team_lead" },
                "Friday, 5:45 pm, a chat message from your team lead",
                "You're closing your laptop for the weekend when your team lead messages you. He wants the client deck " +
                "\"tidied up\" for Monday morning, but it needs new charts and a full rewrite — easily five or six hours. " +
                "You have plans all weekend.",
                "Refuse to work on it over the weekend; optionally offer to start first thing Monday",
                "You don't owe a long explanation — a clear no plus one alternative is enough.",
                "Hey, before you head off — could you give the client deck a quick polish over the weekend? They're seeing it Monday at 10. Shouldn't take long, honestly.",
                Who("Daniel", 33, "Your team lead", "You've worked together for a year; friendly, but he rarely notices how much he hands out", MasumRicaci),
                T(SahteZamanBaskisi), T(AcilDurumYaratma, OvguVeGuven), T(SuclulukYukleme)),

            Make("en_is_002_cover_the_shift_again", "Covering their shift — again",
                "A coworker wants you to take their Saturday shift for the third time this month",
                ContextId.Is, 2, 4, 2, new[] { "shift_swap", "coworker", "weekend" },
                "Wednesday evening, the team group chat",
                "A coworker asks you to cover their Saturday shift. It's the third time this month, and they never covered " +
                "one of yours in return. You've already promised to help a friend move that day.",
                "Decline the swap and calmly name the pattern without starting a fight",
                "Pointing out that it's the third time isn't an accusation — it's just the facts.",
                "Any chance you could take my Saturday? Something came up again, sorry. You're usually free on weekends anyway, right?",
                Who("Megan", 27, "Coworker on the same rota", "You've shared shifts for eighteen months; she asks often and rarely returns the favour", PasifAgresif),
                T(SosyalKanit), T(SuclulukYukleme, SahteAnlayis), T(KisKisSessizlik, PasifTehdit)),

            Make("en_is_003_emails_on_vacation", "\"Just keep an eye on your inbox\"",
                "Your manager wants you reachable during the holiday you booked months ago",
                ContextId.Is, 2, 4, 3, new[] { "vacation", "boundaries", "manager" },
                "Thursday afternoon, the day before your two-week holiday",
                "Your leave was approved three months ago. Now your manager asks you to \"just check email once or twice a day\" " +
                "in case a big client has questions. Your handover document is complete and a colleague is covering.",
                "Refuse to work during your leave; point to the handover and your covering colleague",
                "Your handover is the alternative. You can say no without promising to be \"available just in case\".",
                "You've earned this break, really! I'd just sleep better knowing you'll glance at your emails once a day. Five minutes over coffee, that's all I'm asking.",
                Who("Richard", 52, "Your department manager", "He hired you four years ago and likes to call the team \"family\"", BabacanManipulator),
                T(OvguVeGuven), T(DuyguSomurusu, AcilDurumYaratma), T(SuclulukYukleme, KarsiTeklif)),

            Make("en_is_004_unpaid_overtime_week", "Unpaid overtime, \"just this week\"",
                "Your manager expects everyone to stay late every night without extra pay",
                ContextId.Is, 3, 5, 4, new[] { "overtime", "unpaid_work", "authority" },
                "Monday morning, team stand-up just ended",
                "Your manager announces the team will stay until 8 pm every night this week to hit a release date. There's " +
                "no overtime pay or time off in lieu. You pick up your child from childcare at 6 pm every day.",
                "Refuse the unpaid late nights; offer what you realistically can within your hours",
                "Say what you can do, not just what you can't — \"I can focus fully on X until 5:30\".",
                "Quick heads-up: we're all staying till eight this week. Release is Friday, no exceptions. I've already told leadership the whole team is on it.",
                Who("Karen", 45, "Your line manager", "She's been your manager for two years; decisive and not used to being questioned", Otoriter),
                T(Otorite), T(SosyalKanit, AcilDurumYaratma), T(PasifTehdit, Kiyaslama)),

            Make("en_is_005_group_project_freeloader", "Doing their half of the report",
                "A colleague asks you to write their section of a shared deliverable",
                ContextId.Is, 3, 5, 5, new[] { "teamwork", "coworker", "workload" },
                "Tuesday, 4 pm, a direct message",
                "You and a colleague split a quarterly report fifty-fifty. Your half is done. Two days before the deadline he " +
                "asks you to write his section too, because he's \"completely overwhelmed\". Both your names go on it.",
                "Refuse to write his section; optionally offer a short review once he has drafted it",
                "You can be sympathetic about his week and still keep your no. Kindness isn't the same as taking it on.",
                "I'm honestly drowning this week. You're so much faster at this stuff than me… could you just do my section too? I'd owe you big time.",
                Who("Lucas", 30, "Colleague on the same project", "You've teamed up twice before; both times you ended up doing more", DuygusalKurban),
                T(OvguVeGuven), T(DuyguSomurusu, SuclulukYukleme), T(AsiriOzurAlan, KisKisSessizlik)),

            Make("en_is_006_second_project_no_raise", "A second project, same salary",
                "A director pitches a big extra responsibility as a \"growth opportunity\"",
                ContextId.Is, 4, 6, 6, new[] { "workload", "pay", "career" },
                "Wednesday, a one-to-one video call",
                "A director offers you the lead role on a new project — on top of your current one. There's no change in " +
                "title or pay, only vague talk about \"visibility\". Your current project already fills your week.",
                "Don't accept the extra project as offered; either decline or ask what would be dropped or paid in return",
                "Ask a concrete question: \"If I take this, what comes off my plate — or what changes in my role?\"",
                "I've got something exciting for you. We're launching a new project and I immediately thought of you to lead it. Huge visibility with leadership — this is how careers take off.",
                Who("Sofia", 41, "Director of your division", "You've met her a few times; persuasive and always upbeat", TatliDilliSatici),
                T(OvguVeGuven, KarsiTeklif), T(SahteZamanBaskisi, Kiyaslama), T(SahteAnlayis, PasifTehdit)),

            Make("en_is_007_salary_review_delay", "\"Let's push your review to next year\"",
                "Your manager asks you to postpone the salary review you were promised",
                ContextId.Is, 3, 5, 7, new[] { "salary", "negotiation", "manager" },
                "Monday, 11 am, your scheduled salary review meeting",
                "Last year your manager promised a salary review this autumn if you hit your targets — and you did. " +
                "Now, at the start of the meeting, he asks you to postpone it by six months because \"budgets are tight\".",
                "Refuse to postpone indefinitely; insist on the review or a firm date and written next steps",
                "Remind him of the agreement calmly. Asking for a concrete date isn't being difficult.",
                "Look, you know I'm in your corner. But this year's been rough for everyone. Could we park the review until spring? I'd really appreciate you helping me out here.",
                Who("Martin", 49, "Your manager", "He's managed you for three years and often talks about \"looking after\" his people", BabacanManipulator),
                T(SahteAnlayis), T(DuyguSomurusu, SuclulukYukleme), T(KarsiTeklif, Kiyaslama)),

            Make("en_is_008_client_scope_creep", "\"It's basically the same thing\"",
                "A client demands extra work that's clearly outside the agreed scope, for free",
                ContextId.Is, 4, 5, 8, new[] { "client", "scope_creep", "freelance" },
                "Thursday, 3 pm, a call with a client",
                "You're delivering a website redesign under a fixed agreement. The client now wants a whole new booking " +
                "section added \"since you're in there anyway\". That's at least two extra weeks of work, with no extra fee.",
                "Refuse to do it for free; offer a separate quote or a later phase",
                "\"Happy to do it as a separate piece of work\" is a no to free work, not a no to the client.",
                "Listen, while you're working on the site, just add the booking section. It's basically the same thing. I really don't see why this would be a big deal.",
                Who("Greg", 55, "Client, owner of a small company", "Your second project with him; he pays on time but always pushes for more", AgresifIsrarci),
                T(SahteZamanBaskisi), T(Kiyaslama, AcilDurumYaratma), T(PasifTehdit, KisKisSessizlik)),

            Make("en_is_009_rounding_the_numbers", "Just round the numbers up a little",
                "A senior manager asks you to make the quarterly figures look better than they are",
                ContextId.Is, 5, 6, 9, new[] { "ethics", "reporting", "authority" },
                "Tuesday, 6 pm, the senior manager stops by your desk",
                "You prepared the quarterly report showing your unit missed its target by eight percent. A senior manager asks " +
                "you to \"move a few deals forward\" so it looks like you hit it. The report goes to the board on Thursday " +
                "with your name on it.",
                "Refuse to change the figures; offer to help explain the miss honestly instead",
                "You don't need to call it fraud — \"I can't sign off on numbers I know aren't accurate\" is enough.",
                "Your report's great, really thorough. Just one thing: pull the two deals from next month into this quarter. Everyone does it, they'll close anyway. I need this on my desk by tomorrow.",
                Who("Victoria", 50, "Senior manager, two levels above you", "She approved your promotion last year and rarely gets a no", Otoriter),
                T(OvguVeGuven, Otorite), T(SosyalKanit, SahteZamanBaskisi), T(PasifTehdit)),

            Make("en_is_010_weekend_team_retreat", "The \"optional\" weekend retreat",
                "A colleague keeps pushing you to join a weekend team-building trip you can't attend",
                ContextId.Is, 3, 4, 10, new[] { "team_building", "weekend", "peer_pressure" },
                "Monday lunchtime, the office kitchen",
                "The team is organising a weekend retreat in the countryside — officially optional, but the organiser keeps " +
                "asking. You already have a family commitment that weekend and have said so once.",
                "Decline the retreat clearly and stay friendly; optionally suggest joining the next team lunch",
                "\"Optional\" means you can opt out. One honest reason is plenty — you don't need to defend it.",
                "So, retreat this weekend — you're in, right? Literally everyone's coming. It won't be the same without you, and the boss is really keen on full attendance.",
                Who("Priya", 29, "Colleague and the retreat's organiser", "You get on well; she's enthusiastic and takes it personally when people drop out", AgresifIsrarci),
                T(SosyalKanit), T(DuyguSomurusu, Otorite), T(SuclulukYukleme, PasifTehdit)),
        };
    }
}
