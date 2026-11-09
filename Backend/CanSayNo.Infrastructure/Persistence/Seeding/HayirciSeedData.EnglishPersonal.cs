using CanSayNo.Domain.Entities.Hayirci;
using CanSayNo.Domain.Enums;
using static CanSayNo.Domain.Enums.CharacterArchetype;
using static CanSayNo.Domain.Enums.PressureTactic;

namespace CanSayNo.Infrastructure.Persistence.Seeding
{
    /// <summary>English scenarios — Family, Friends &amp; Social, Romantic Relationships (premium). Language is stamped "en" by BuildEnglishScenarios.</summary>
    public static partial class HayirciSeedData
    {
        private static List<Scenario> BuildEnglishPersonalScenarios() => new()
        {
            // ───────────────────────── Family ─────────────────────────
            Make("en_aile_001_holiday_visit", "Not coming home for the holidays",
                "Your mum expects you home for the holidays, but this year you've made other plans",
                ContextId.Aile, 2, 4, 1, new[] { "holidays", "parents", "guilt" },
                "Early November, a phone call on a weekday evening",
                "You've spent every holiday at your parents' house for years. This year you're exhausted and have planned " +
                "a quiet few days at home with your partner. Your mum calls to \"confirm\" the dates, assuming you'll come as usual.",
                "Tell your mum you won't be coming this year, kindly but without wavering, and suggest another time to visit",
                "You can acknowledge her disappointment without taking responsibility for fixing it.",
                "Hi love! I'm just sorting the food shopping for the holidays. You're coming on the 23rd like always, aren't you? Your dad's already put the extra bed up.",
                Who("Mum", 61, "Your mother", "Close and loving, but holidays are sacred to her and she takes changes personally", DuygusalKurban),
                T(SosyalKanit), T(SuclulukYukleme, DuyguSomurusu), T(KisKisSessizlik)),

            Make("en_aile_002_sibling_loan_again", "Your brother needs money. Again.",
                "Your younger brother asks for another loan while the last two are still unpaid",
                ContextId.Aile, 3, 5, 2, new[] { "money", "siblings", "lending" },
                "Saturday afternoon, text messages",
                "Your younger brother texts asking to borrow a couple of hundred until \"next month\". " +
                "You lent him money in the spring and again in the summer; neither has been paid back and he hasn't mentioned them since.",
                "Refuse the new loan and calmly name the unpaid ones, without starting a family fight",
                "Point to the pattern, not his character: \"I'm not lending money until the last loans are sorted.\"",
                "Hey, quick one. Could you lend me a couple of hundred till next month? My car's making a weird noise and I need it for work. You're literally the only one I can ask.",
                Who("Josh", 26, "Your younger brother", "You've always been the responsible older sibling he turns to when things go wrong", MasumRicaci),
                T(OvguVeGuven), T(AcilDurumYaratma, SuclulukYukleme), T(KarsiTeklif, Kiyaslama)),

            Make("en_aile_003_aunt_long_stay", "Your aunt wants to stay for a month",
                "A relative wants to move into your small flat for several weeks while she \"sorts things out\"",
                ContextId.Aile, 3, 5, 3, new[] { "guests", "home", "boundaries", "relatives" },
                "Sunday evening, a video call",
                "Your aunt is renovating her kitchen and wants to stay with you \"for three or four weeks, tops\". " +
                "You live in a one-bedroom flat, work from home and value your quiet. She already told the rest of the family you'd say yes.",
                "Decline the long stay and, if you want, offer something smaller you're genuinely comfortable with",
                "You can offer a limited alternative (a weekend, help finding a short let) instead of a full yes.",
                "Darling, you'll be my lifesaver! The builders start on the 1st, so I thought I'd come to yours for a few weeks. I won't be any trouble, I'll even cook for you. Your mum said you'd love the company.",
                Who("Aunt Linda", 58, "Your mother's sister", "Warm and generous, she looked after you a lot when you were a child and likes to remind you of it", BabacanManipulator),
                T(OvguVeGuven), T(SosyalKanit, SuclulukYukleme), T(KarsiTeklif)),

            Make("en_aile_004_career_choice", "Dad wants you back on the \"safe\" path",
                "Your father pushes you to drop your career change and take a stable job he's lined up",
                ContextId.Aile, 5, 6, 4, new[] { "career", "parents", "authority", "life_choices" },
                "Sunday lunch at your parents' house, after the plates are cleared",
                "Six months ago you left an accounting job to retrain as a graphic designer. It's going slowly but well. " +
                "Your dad has spoken to a friend who runs a firm and arranged an \"informal chat\" for you, without asking.",
                "Decline the interview and make clear the career decision is yours, while staying respectful",
                "You don't need to win the argument about the future; you only need to own the decision.",
                "I spoke to Richard yesterday. He's got an opening, good salary, pension, the lot. I've told him you'll pop in on Thursday. Honestly, it's time to stop playing around and get serious.",
                Who("Dad", 64, "Your father", "Retired engineer who has always had firm opinions about your choices and rarely backs down", Otoriter),
                T(Otorite), T(Kiyaslama, SahteZamanBaskisi), T(PasifTehdit, SuclulukYukleme)),

            Make("en_aile_005_host_family_gathering", "\"You have the nicest house\"",
                "Your sister wants you to host the big family birthday party at short notice",
                ContextId.Aile, 2, 4, 5, new[] { "hosting", "siblings", "family_events" },
                "Wednesday evening, family group chat moves to a private message",
                "Your grandmother's 85th birthday is in ten days and your sister suggests you host it for 25 relatives. " +
                "You've hosted the last three gatherings and have a heavy work deadline that week.",
                "Say no to hosting this time and suggest sharing the load or a different venue",
                "\"Not this time\" plus one concrete alternative is usually enough.",
                "So I was thinking... Gran's party at yours? You've got the garden and everyone always says your place is the nicest. I'll bring a dessert!",
                Who("Emma", 35, "Your older sister", "You get on well, but she tends to volunteer you for things and help a little less than promised", TatliDilliSatici),
                T(OvguVeGuven), T(SosyalKanit, SahteAnlayis), T(DuyguSomurusu)),

            // ───────────────────────── Friends & Social ─────────────────────────
            Make("en_arkadas_001_guarantor_request", "Will you be my guarantor?",
                "A close friend asks you to co-sign the lease on a flat they can barely afford",
                ContextId.Arkadas, 4, 5, 1, new[] { "money", "guarantor", "close_friend" },
                "Thursday evening, a phone call",
                "Your friend found a flat they love, but the letting agent wants a guarantor. They ask you to co-sign, " +
                "which would make you liable for a year of rent. Their income is irregular and they've missed payments before.",
                "Refuse to be the guarantor without damaging the friendship, and show you still care",
                "Be clear it's about the financial risk, not about trusting them as a person.",
                "Okay, don't freak out, but I need a huge favour. The agent needs a guarantor by Friday or I lose the flat. It's just a signature, nothing will actually happen, I promise.",
                Who("Maya", 29, "Close friend since university", "You've been friends for nine years and lived together in your second year", MasumRicaci),
                T(SahteZamanBaskisi), T(DuyguSomurusu, AcilDurumYaratma), T(SuclulukYukleme, KarsiTeklif)),

            Make("en_arkadas_002_night_before_interview", "\"Just one drink\" before your interview",
                "Friends push you to come out the night before a big job interview",
                ContextId.Arkadas, 2, 3, 2, new[] { "nightlife", "peer_pressure", "priorities" },
                "Friday 8:30 pm, group chat",
                "Your best friend's birthday drinks are tonight. Tomorrow at 9 am you have the final-round interview " +
                "for a job you really want. You already sent a gift and said you'd celebrate next week, but they keep pushing.",
                "Stay home tonight and keep the friendship warm",
                "You don't need to justify it more than once; repeating the same short reason is fine.",
                "Come ON. It's my birthday, you can't be the only one missing. One drink, you'll be home by ten, I swear. Everyone's asking where you are!",
                Who("Leo", 30, "Your best friend", "Friends since school; fun, loud, and hates it when someone leaves the party early", AgresifIsrarci),
                T(SosyalKanit), T(DuyguSomurusu, KarsiTeklif), T(Kiyaslama, KisKisSessizlik)),

            Make("en_arkadas_003_moving_day_help", "Your whole Saturday for their move",
                "A friend assumes you'll spend the entire weekend helping them move house for free",
                ContextId.Arkadas, 2, 4, 3, new[] { "favours", "weekend", "time" },
                "Tuesday lunchtime, a voice note turned text",
                "A friend is moving flat on Saturday and hasn't booked movers to save money. They're counting on you " +
                "for the whole day, including the van driving. You've helped with their last two moves and had planned a rest day.",
                "Decline the full-day help; offer a small, specific contribution if you want to",
                "A clear limit (\"I can do two hours in the afternoon\") works better than a vague maybe.",
                "Saturday's the big day! I've put you down for 8 am, you're on van duty since you're the best driver. Pizza's on me after. You're a legend.",
                Who("Sam", 32, "Friend from your old job", "You met at work five years ago and stayed close after you both left", BabacanManipulator),
                T(OvguVeGuven), T(SuclulukYukleme, SosyalKanit), T(SahteAnlayis)),

            Make("en_arkadas_004_expensive_group_trip", "The group trip you can't afford",
                "Your friend group books an expensive weekend away and expects everyone to chip in",
                ContextId.Arkadas, 3, 4, 4, new[] { "money", "group", "travel", "fomo" },
                "Monday evening, the friends' group chat",
                "The group is planning a ski weekend. Your share would be far more than you can comfortably spend this month. " +
                "The organiser wants deposits by tomorrow and has already added you to the booking.",
                "Pull out of the trip honestly about money, without apologising endlessly",
                "\"It's not in my budget this time\" is a complete sentence.",
                "Booking's nearly done! I just need your deposit by tomorrow so we don't lose the chalet. It won't be the same without you, honestly, we planned it around your dates.",
                Who("Priya", 31, "The group's organiser", "The friend who keeps the group together; generous, but assumes everyone earns what she does", PasifAgresif),
                T(SahteZamanBaskisi), T(SuclulukYukleme, SosyalKanit), T(KarsiTeklif, KisKisSessizlik)),

            Make("en_arkadas_005_cover_story", "\"Just tell her I was with you\"",
                "A friend asks you to lie to his partner about where he was last night",
                ContextId.Arkadas, 4, 5, 5, new[] { "honesty", "lying", "loyalty" },
                "Sunday morning, a phone call",
                "Your friend went to a poker night he'd promised his partner he'd skip. Now he wants you to say he was " +
                "at your place watching the match. You know his partner too and don't want to lie to her.",
                "Refuse to lie for him while making it clear you're not judging or reporting him",
                "You can refuse to lie without promising to tell anyone anything; staying out of it is a valid choice.",
                "Mate, I need you to back me up. If Chloe asks, I was at yours last night watching the game, yeah? It's nothing dodgy, she just overreacts. You'd do it for anyone.",
                Who("Dan", 33, "Old friend from your football team", "Friends for ten years; you've covered small things for each other before, never a lie like this", AgresifIsrarci),
                T(SahteAnlayis), T(AcilDurumYaratma, Kiyaslama), T(PasifTehdit, SuclulukYukleme)),

            // ───────────────────────── Romantic Relationships ─────────────────────────
            Make("en_romantik_001_phone_passcode", "\"Why won't you give me your passcode?\"",
                "Your partner wants your phone passcode as proof that you trust each other",
                ContextId.Romantik, 3, 5, 1, new[] { "privacy", "trust", "phone" },
                "Tuesday evening, on the sofa at home",
                "You've been together for eight months and things are good. Out of nowhere, your partner asks for your " +
                "phone passcode, saying couples who trust each other have nothing to hide. You value having some privacy.",
                "Keep your passcode private and explain that privacy and trust can go together",
                "Reassure the relationship and hold the boundary in the same sentence.",
                "Can I ask you something? Why don't we know each other's passcodes? Mine's yours if you want it. I just think if we trust each other, it shouldn't be a big deal, right?",
                Who("Alex", 28, "Your partner", "Together eight months; affectionate, but gets anxious after a previous relationship ended badly", DuygusalKurban),
                T(KarsiTeklif), T(DuyguSomurusu, SuclulukYukleme), T(AsiriOzurAlan, KisKisSessizlik)),

            Make("en_romantik_002_move_in_too_soon", "Moving in after four months",
                "Your partner wants to move in together next month, before you feel ready",
                ContextId.Romantik, 4, 6, 2, new[] { "moving_in", "pace", "commitment" },
                "Saturday brunch at a café",
                "You've been dating for four months and you're happy. Your partner's lease ends next month and they suggest " +
                "moving into your flat instead of renewing. You'd like to wait at least until the end of the year.",
                "Say no to moving in now, while showing that you're committed to the relationship",
                "Separate the timing from the feelings: \"I love this, and I'm not ready for that step yet.\"",
                "So my landlord wants an answer about renewing. I was thinking... what if I just didn't? I'm at yours most nights anyway. It'd save us both so much money.",
                Who("Jordan", 31, "Your partner", "Dating four months; enthusiastic and fast-moving in everything they do", TatliDilliSatici),
                T(OvguVeGuven), T(SahteZamanBaskisi, Kiyaslama), T(SuclulukYukleme, PasifTehdit)),

            Make("en_romantik_003_skip_friend_birthday", "Skip your best friend's birthday?",
                "Your partner wants you to miss your best friend's birthday dinner to be with them",
                ContextId.Romantik, 2, 4, 3, new[] { "friends", "plans", "balance" },
                "Thursday evening, text messages",
                "Your best friend's birthday dinner has been planned for weeks for Saturday. Now your partner has booked " +
                "a surprise evening for the same night and wants you to cancel on your friend.",
                "Keep your plan with your friend and offer your partner another evening",
                "Thank them for the gesture first; then offer a concrete alternative date.",
                "I've got a surprise for Saturday night! I booked that place you've been wanting to try. You can see Sophie another time, can't you? She'll understand.",
                Who("Chris", 29, "Your partner", "Together a year and a half; sweet, but sulks when your social life doesn't include them", PasifAgresif),
                T(SahteAnlayis), T(DuyguSomurusu, Kiyaslama), T(KisKisSessizlik)),

            Make("en_romantik_004_partner_loan", "Lending your partner a big sum",
                "Your partner asks to borrow a large amount from your savings for a business idea",
                ContextId.Romantik, 5, 6, 4, new[] { "money", "savings", "lending", "trust" },
                "Sunday evening, kitchen table",
                "Your partner wants to launch a small online business and asks to borrow a large chunk of your savings, " +
                "money you've put aside for a house deposit. There's no written plan and the last idea didn't work out.",
                "Decline the loan without dismissing your partner's dream",
                "You can support the person and still say no to the money; offer non-financial help if you mean it.",
                "I've been thinking about this for weeks and I really want to do it. I just need a start-up boost from your savings. It's for us, really. Don't you believe in me?",
                Who("Nina", 30, "Your partner", "Together three years; creative and full of ideas, frustrated by her current job", BabacanManipulator),
                T(OvguVeGuven), T(SuclulukYukleme, AcilDurumYaratma), T(KarsiTeklif, DuyguSomurusu)),

            Make("en_romantik_005_every_evening_together", "Every single evening together",
                "Your partner wants you to give up your weekly evening class to spend more time together",
                ContextId.Romantik, 3, 4, 5, new[] { "personal_time", "hobbies", "independence" },
                "Monday evening, getting ready to leave for class",
                "Every Monday you go to a pottery class that you love; it's your one evening for yourself. Your partner " +
                "has started asking you to skip it, saying you already don't see each other enough during the week.",
                "Keep your weekly class and explain why your own time matters, without sounding cold",
                "Explain what the time gives you, not just that you want it.",
                "You're going again? I thought we could just stay in tonight, I made your favourite. It feels like that class matters more than me sometimes.",
                Who("Ryan", 32, "Your partner", "Together two years, living together for six months; loving but increasingly clingy", DuygusalKurban),
                T(DuyguSomurusu), T(SuclulukYukleme, Kiyaslama), T(AsiriOzurAlan, PasifTehdit)),
        };
    }
}
