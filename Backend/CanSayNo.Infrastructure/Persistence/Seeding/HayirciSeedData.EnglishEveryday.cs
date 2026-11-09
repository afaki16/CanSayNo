using CanSayNo.Domain.Entities.Hayirci;
using CanSayNo.Domain.Enums;
using static CanSayNo.Domain.Enums.CharacterArchetype;
using static CanSayNo.Domain.Enums.PressureTactic;

namespace CanSayNo.Infrastructure.Persistence.Seeding
{
    /// <summary>English scenarios — Sales &amp; Services, Home &amp; Neighbours, School &amp; Campus, Digital &amp; Social Media (premium). Language is stamped "en" by BuildEnglishScenarios.</summary>
    public static partial class HayirciSeedData
    {
        private static List<Scenario> BuildEnglishEverydayScenarios() => new()
        {
            // ───────────────────────── Sales & Services ─────────────────────────
            Make("en_satici_001_gym_cancellation", "Cancelling the gym membership",
                "The cancellation desk keeps steering you towards a two-year \"loyalty\" contract instead",
                ContextId.Satici, 2, 4, 1, new[] { "subscription", "cancellation", "upsell" },
                "Saturday morning, the front desk of your gym",
                "You've barely used your gym membership in four months and came in to cancel it. " +
                "The membership advisor greets you warmly, ignores the word \"cancel\" and starts presenting a two-year plan " +
                "with a lower monthly fee and a free personal training session.",
                "Cancel the membership today without signing up for anything new",
                "Repeat your request in one sentence; you don't owe a reason for leaving.",
                "Hey, good to see you! Before we do any paperwork, can I show you something? Honestly, most people who come in to cancel end up saving money with our two-year plan. Takes two minutes.",
                Who("Tyler", 27, "Membership advisor at the gym", "You've never really talked before; he signed you up a year ago", TatliDilliSatici),
                T(SosyalKanit), T(KarsiTeklif, SahteZamanBaskisi), T(SahteAnlayis, KarsiTeklif)),

            Make("en_satici_002_phone_insurance", "The phone insurance add-on",
                "At the till, the salesperson insists your new phone needs a monthly protection plan",
                ContextId.Satici, 1, 3, 2, new[] { "add_on", "insurance", "checkout" },
                "Thursday evening, a busy phone shop",
                "You've chosen a new phone and are ready to pay. The salesperson adds a monthly protection plan to the bill " +
                "and asks you to just confirm. You already have a case, a screen protector and home contents cover that includes gadgets.",
                "Buy only the phone and decline the protection plan politely",
                "A simple \"No thanks, just the phone\" is a full answer; you don't need to argue about the plan.",
                "Great choice, honestly the best camera we've got. I've popped the protection plan on there too, it's only a small monthly charge. Shall I just confirm everything?",
                Who("Priya", 31, "Phone shop salesperson", "You've never met before", TatliDilliSatici),
                T(OvguVeGuven), T(AcilDurumYaratma), T(Kiyaslama, SosyalKanit)),

            Make("en_satici_003_doorstep_energy", "Doorstep energy deal",
                "A friendly rep at your door wants you to switch energy supplier on the spot",
                ContextId.Satici, 3, 5, 3, new[] { "door_to_door", "contract", "energy" },
                "Weekday evening, your front door",
                "A rep with a lanyard and a tablet knocks just as you're cooking dinner. He says your street is switching to " +
                "a new energy supplier and he only needs your signature on the tablet. You haven't compared any prices and don't sign contracts at the door.",
                "Refuse to sign anything at the door and end the conversation",
                "You can close the conversation yourself: \"I don't sign anything at the door. Goodnight.\"",
                "Evening! Sorry to catch you at dinner. Your neighbours at number 12 and 14 have already switched today, and you're paying way more than you need to. I just need a quick signature here.",
                Who("Marcus", 35, "Door-to-door energy sales rep", "You've never met before", BabacanManipulator),
                T(SosyalKanit), T(SahteZamanBaskisi, Kiyaslama), T(SahteAnlayis, PasifTehdit)),

            Make("en_satici_004_salon_products", "The hairdresser's product haul",
                "After your haircut, the stylist lines up expensive products you \"really need\"",
                ContextId.Satici, 2, 4, 4, new[] { "salon", "products", "checkout" },
                "Saturday afternoon, at the salon till",
                "Your stylist has just finished a cut you like. At the till she places a shampoo, a serum and a heat spray next to the bill, " +
                "saying your hair will be ruined without them. Together they cost more than the haircut itself.",
                "Pay for the haircut only and say no to the products without awkwardness",
                "Thank her for the cut and say no to the products separately; the two don't have to be linked.",
                "Okay, so I've put aside the three things you really need, otherwise this cut won't last two weeks honestly. Your hair is so dry at the ends. Shall I add them to the bill?",
                Who("Chloe", 29, "Your hairdresser", "She's cut your hair a few times and is always chatty and friendly", TatliDilliSatici),
                T(OvguVeGuven), T(DuyguSomurusu, Kiyaslama), T(SuclulukYukleme)),

            Make("en_satici_005_mechanic_extras", "The mechanic's extra repairs",
                "You came in for a routine service; the mechanic now wants to replace half the car",
                ContextId.Satici, 4, 5, 5, new[] { "car", "repairs", "expertise" },
                "Monday lunchtime, phone call from the garage",
                "You left your car for its yearly service. The mechanic calls with a list of \"urgent\" extras: brake discs, " +
                "a new battery and a fluid flush, doubling the price. The car drives fine and your last inspection found nothing wrong.",
                "Approve only the agreed service and ask for the other items in writing to decide later",
                "\"Just the service today, please send me the rest in writing\" keeps the decision in your hands.",
                "Hi, it's Dave from the garage. Car's on the lift and, to be honest, I wouldn't let my own family drive it like this. Discs, battery, the lot. Shall I just crack on while it's up here?",
                Who("Dave", 52, "Car mechanic", "You've used this garage once before, years ago", Otoriter),
                T(Otorite), T(AcilDurumYaratma, DuyguSomurusu), T(PasifTehdit, KarsiTeklif)),

            // ───────────────────────── Home & Neighbours ─────────────────────────
            Make("en_komsu_001_parcels_and_pets", "The neighbour's weekly favours",
                "Your neighbour wants you to take in parcels and feed her cat every week",
                ContextId.Komsu, 1, 3, 1, new[] { "neighbour", "favours", "routine" },
                "Sunday morning, on the landing outside your flats",
                "Your neighbour Margaret has started travelling for work most weeks. She asks you to take in her parcels and " +
                "feed her cat whenever she's away, as a regular arrangement. You helped once and were happy to, but you can't commit every week.",
                "Turn down the regular arrangement; offer a one-off if you want to",
                "Be warm but clear: you can help occasionally, not as a weekly routine.",
                "Morning, love! You were such a star with Biscuit last time. I've got this new job with loads of travel, so I was thinking, could you just be my parcel and cat person? You're the only one I trust.",
                Who("Margaret", 61, "Your next-door neighbour", "You've lived next to each other for three years; she often brings you homemade cake", MasumRicaci),
                T(OvguVeGuven), T(DuyguSomurusu, SuclulukYukleme), T(KarsiTeklif)),

            Make("en_komsu_002_partner_moves_in", "The flatmate's partner moving in",
                "Your flatmate wants her partner to live with you \"just for a bit\"",
                ContextId.Komsu, 3, 5, 2, new[] { "flatmate", "shared_space", "rent" },
                "Tuesday evening, the shared kitchen",
                "You and your flatmate Jess rent a two-bedroom flat together. She asks if her boyfriend Sam can move in for a while " +
                "because his lease is ending. He already stays four nights a week, the bills have gone up and the bathroom is always busy.",
                "Say no to Sam moving in and set a clear rule about guests",
                "Keep it about the flat, not about Sam: \"We rented this place for two people.\"",
                "So… don't hate me. Sam's lease ends next week and he hasn't found anything yet. Could he stay with us for a bit? He's basically here anyway, you'd hardly notice.",
                Who("Jess", 26, "Your flatmate", "You've shared the flat for a year and get on well", MasumRicaci),
                T(SahteAnlayis), T(AcilDurumYaratma, DuyguSomurusu), T(SuclulukYukleme, KarsiTeklif)),

            Make("en_komsu_003_borrowed_car", "\"Can I borrow the car again?\"",
                "Your neighbour borrows your car for the third time this month",
                ContextId.Komsu, 2, 4, 3, new[] { "neighbour", "borrowing", "car" },
                "Friday evening, your doorstep",
                "Your neighbour Luca has borrowed your car twice this month and returned it once with an empty tank. " +
                "Now he's at the door asking for the keys for the whole weekend. You'd planned to use it yourself on Saturday.",
                "Refuse to lend the car this weekend without apologising too much",
                "One short \"sorry\" is enough; you don't need to justify needing your own car.",
                "Hey mate! Huge favour, any chance I could grab the car for the weekend? My cousin's moving and I've got no one else. I'll bring it back Sunday night, promise.",
                Who("Luca", 33, "Neighbour across the hall", "You've been friendly for two years; he's borrowed tools and the car before", BabacanManipulator),
                T(OvguVeGuven), T(AcilDurumYaratma, SosyalKanit), T(SuclulukYukleme, Kiyaslama)),

            Make("en_komsu_004_landlord_viewings", "Viewings at short notice",
                "Your landlord wants to show the flat to buyers with no proper notice",
                ContextId.Komsu, 4, 5, 4, new[] { "landlord", "privacy", "notice" },
                "Wednesday, a call during your lunch break",
                "Your landlord is selling the flat you rent. He calls to say an agent will bring buyers round tomorrow morning, " +
                "and asks you to leave a key so they can come in whenever suits them over the next weeks. You work from home and want proper notice each time.",
                "Refuse open access and insist on agreed times with notice",
                "Stay calm and specific: say which notice you need and when viewings can happen.",
                "Hi, just a heads-up, the agent's bringing a couple round at 9 tomorrow. Easiest thing is if you leave a key under the mat for the next few weeks, then nobody has to bother you. That alright?",
                Who("Richard", 57, "Your landlord", "You've rented from him for two years; he's polite but used to getting his way", Otoriter),
                T(SahteAnlayis), T(Otorite, AcilDurumYaratma), T(PasifTehdit)),

            Make("en_komsu_005_petition", "The neighbourhood petition",
                "A neighbour pressures you to sign a petition against a family on your street",
                ContextId.Komsu, 3, 4, 5, new[] { "neighbour", "petition", "conflict" },
                "Saturday afternoon, your front garden",
                "Your neighbour Sandra is collecting signatures to complain to the council about the new family at number 9 " +
                "and their noisy extension work. The work is legal, finishes at 5 pm and doesn't bother you. You don't want to take sides.",
                "Decline to sign and stay out of the dispute without falling out with Sandra",
                "You can respect her frustration and still say: \"I'm not going to sign this.\"",
                "Oh good, you're in! I'm getting everyone to sign this about number 9, the drilling is unbearable. Nearly the whole street's signed already. It'll only take a second.",
                Who("Sandra", 49, "Neighbour two doors down", "She runs the street's group chat and organises most local things", PasifAgresif),
                T(SosyalKanit), T(DuyguSomurusu, Kiyaslama), T(PasifTehdit, KisKisSessizlik)),

            // ───────────────────────── School & Campus ─────────────────────────
            Make("en_okul_001_sign_me_in", "\"Sign me in, will you?\"",
                "A friend asks you to sign the attendance sheet for them again",
                ContextId.Okul, 1, 3, 1, new[] { "attendance", "honesty", "friends" },
                "Monday 8:50, just before a lecture",
                "Attendance counts towards your grade in this module. Your friend Ben texts asking you to sign his name on the sheet " +
                "because he overslept. You've done it once before and felt uneasy; the lecturer has warned that it's treated as misconduct.",
                "Refuse to sign for him without lecturing him",
                "Keep it short and friendly; you can still offer to share your notes.",
                "Mate, I've overslept again. Sign me in? Just scribble my name, nobody checks. You're a legend.",
                Who("Ben", 20, "Coursemate and friend", "You've been friends since first year and share most modules", MasumRicaci),
                T(OvguVeGuven), T(SosyalKanit, AcilDurumYaratma), T(SuclulukYukleme)),

            Make("en_okul_002_copy_assignment", "Copying your assignment",
                "A classmate wants to \"just look at\" your finished essay before the deadline",
                ContextId.Okul, 2, 4, 2, new[] { "plagiarism", "assignment", "classmates" },
                "Thursday night, the evening before the deadline",
                "You spent two weeks on your essay. Your classmate Aisha, who hasn't started, messages asking you to send it so she can " +
                "\"get an idea of the structure\". Plagiarism checks would flag both of you if she copies anything.",
                "Refuse to send your essay; offer a different kind of help if you want",
                "You can say no to the file and yes to something smaller, like explaining your approach in five minutes.",
                "Heyy, I'm so behind, I haven't even started. Could you send me yours? I won't copy, I swear, I just need to see how you structured it. You're literally the smartest person in our seminar.",
                Who("Aisha", 21, "Classmate from your seminar group", "You study together sometimes; she's always friendly", DuygusalKurban),
                T(OvguVeGuven), T(DuyguSomurusu, AcilDurumYaratma), T(SuclulukYukleme, KarsiTeklif)),

            Make("en_okul_003_group_project", "Doing their part of the group project",
                "A group member asks you to take over their section \"because you're better at it\"",
                ContextId.Okul, 3, 5, 3, new[] { "group_project", "workload", "fairness" },
                "Sunday afternoon, the project group chat",
                "Your group of four has a presentation due on Friday. Ryan, who agreed to do the data analysis, messages you " +
                "privately asking you to do it instead. You already have your own section and two other deadlines this week.",
                "Refuse to take over Ryan's section and keep the split fair",
                "Point back to what the group agreed; you're not the only one who can fix this.",
                "Hey, can I be honest? The analysis bit is way over my head and you're so good with numbers. Could you just do it? I'll make the slides look great in return. Otherwise we're all going to fail.",
                Who("Ryan", 22, "Group project member", "You met this semester when the group was assigned", PasifAgresif),
                T(OvguVeGuven), T(KarsiTeklif, AcilDurumYaratma), T(SuclulukYukleme, Kiyaslama)),

            Make("en_okul_004_committee_role", "Running for the society committee",
                "The society president keeps pushing you to stand for treasurer",
                ContextId.Okul, 2, 4, 4, new[] { "society", "volunteering", "time" },
                "Wednesday evening, after the society meeting",
                "You enjoy the hiking society but only want to join the trips. Elections are next week and the president, Hannah, " +
                "wants you to stand for treasurer because nobody else has. It would mean weekly meetings and managing the budget during your final year.",
                "Decline to stand for the committee and stay just a member",
                "You can value the society and still not want a role in it; say both.",
                "You'd be the perfect treasurer, honestly, you're the most organised person here. Nobody else has put their name down and the society might not even run next year without one. Just think about it?",
                Who("Hannah", 22, "President of the hiking society", "You've been on several trips together and get along well", BabacanManipulator),
                T(OvguVeGuven), T(DuyguSomurusu, SuclulukYukleme), T(SosyalKanit, SahteZamanBaskisi)),

            Make("en_okul_005_unpaid_research", "The lecturer's unpaid hours",
                "Your lecturer asks you to help with a research project for free, several hours a week",
                ContextId.Okul, 5, 6, 5, new[] { "lecturer", "authority", "unpaid_work" },
                "Friday, the lecturer's office hours",
                "Dr. Okafor, who will also supervise your dissertation, asks you to code survey data for his research project " +
                "for about ten hours a week, unpaid, \"for the experience\". You have a part-time job and your own dissertation to write.",
                "Decline the unpaid hours while keeping a good relationship with your supervisor",
                "Be respectful and concrete: explain your current commitments without over-apologising.",
                "Ah, good, come in. I've got an exciting opportunity for you. I need someone sharp to help code data for my project, about ten hours a week. Unpaid, of course, but it's invaluable experience. I thought of you first.",
                Who("Dr. Daniel Okafor", 48, "Your lecturer and future dissertation supervisor", "He taught you last year and rates your work highly", Otoriter),
                T(OvguVeGuven, Otorite), T(Kiyaslama, SosyalKanit), T(PasifTehdit, SuclulukYukleme)),

            // ───────────────────────── Digital & Social Media ─────────────────────────
            Make("en_dijital_001_daily_votes", "Vote for me every day!",
                "A friend wants you to vote, like and share their contest entry every single day",
                ContextId.Dijital, 1, 3, 1, new[] { "social_media", "friends", "online_voting" },
                "Tuesday morning, a private message",
                "Your friend Sofia entered an online photo contest. You voted once and shared it. Now she wants you to vote daily for " +
                "three weeks, share it to all your accounts and get your family to vote too. You don't want your feed to become her campaign.",
                "Say no to daily voting and sharing without hurting the friendship",
                "Acknowledge what you already did and say clearly what you won't do from now on.",
                "Omg thank you for voting!! Could you do it every day though? It resets at midnight. And maybe share it on your story and ask your mum? I'm only 40 votes behind 🙏",
                Who("Sofia", 24, "A friend from your old job", "You worked together for a year and still meet up sometimes", DuygusalKurban),
                T(OvguVeGuven), T(DuyguSomurusu, SosyalKanit), T(SuclulukYukleme)),

            Make("en_dijital_002_streaming_password", "\"What's your streaming password?\"",
                "An acquaintance wants your login so they can use your streaming account",
                ContextId.Dijital, 2, 4, 2, new[] { "password", "privacy", "accounts" },
                "Saturday night, a message from an acquaintance",
                "Leo, a friend of a friend you met at a party, messages asking for your streaming service login so he can watch a new series. " +
                "The account is linked to your payment card and your viewing history. You don't share passwords.",
                "Refuse to share your login and keep it friendly",
                "\"I don't share my passwords\" is a rule, not a judgement of him; you can say it exactly like that.",
                "Hey! It's Leo from Nina's party. Random one, but could I borrow your streaming login for a bit? Everyone shares them, it's no big deal. I'll only use it on my laptop.",
                Who("Leo", 28, "Friend of a friend", "You met once at a party a month ago", TatliDilliSatici),
                T(SosyalKanit), T(SahteAnlayis, KarsiTeklif), T(PasifTehdit, Kiyaslama)),

            Make("en_dijital_003_after_hours_chat", "The after-hours group chat",
                "A colleague expects quick replies in the team chat late in the evening",
                ContextId.Dijital, 3, 4, 3, new[] { "work_chat", "boundaries", "after_hours" },
                "Thursday, 10:15 pm, at home",
                "A colleague, Megan, keeps posting questions in the team chat late at night and tagging you. Tonight she messages you " +
                "directly because you didn't reply in the group. Nothing is urgent and your working hours ended five hours ago.",
                "Avoid working tonight and set a clear expectation about replying after hours",
                "You can reply once, briefly, with when you'll look at it; you don't need to keep chatting.",
                "Hey, saw you were online 😅 did you see my question in the group? Everyone else has replied already. It's quick, just need your figures for tomorrow.",
                Who("Megan", 32, "Colleague on the same team", "You've worked together for eight months; she's hard-working and always online", PasifAgresif),
                T(SosyalKanit), T(AcilDurumYaratma, Kiyaslama), T(KisKisSessizlik, SuclulukYukleme)),

            Make("en_dijital_004_fundraiser", "The fundraiser that never ends",
                "An acquaintance keeps asking you to donate and promote their fundraiser",
                ContextId.Dijital, 3, 5, 4, new[] { "fundraising", "money", "social_media" },
                "Sunday afternoon, a direct message",
                "Rachel, someone you know from a yoga class, is raising money for a charity run. You already donated a small amount " +
                "last week. Now she's asking for a bigger donation and for you to post about it on all your accounts. You've set a budget for giving this month.",
                "Decline a second donation and the promotion without feeling guilty",
                "Your earlier donation counts; you can thank her and still close the topic.",
                "Thank you SO much for last week! We're so close to the target now. Could you maybe top it up a bit and share the link? Even a couple more would make a real difference for the kids.",
                Who("Rachel", 38, "Acquaintance from your yoga class", "You chat after class sometimes but aren't close", DuygusalKurban),
                T(OvguVeGuven), T(DuyguSomurusu, SosyalKanit), T(SuclulukYukleme, AsiriOzurAlan)),

            Make("en_dijital_005_business_opportunity", "The \"business opportunity\" group",
                "An old schoolmate adds you to a group chat and pushes you to join their scheme",
                ContextId.Dijital, 4, 5, 5, new[] { "pyramid_scheme", "group_chat", "money" },
                "Wednesday evening, you've just been added to a group chat",
                "Jordan, someone you went to school with but haven't spoken to in years, added you to a group called \"Freedom Squad\". " +
                "He's selling a \"starter pack\" that lets you earn by recruiting others. You want to leave the group and not buy anything.",
                "Leave the group and refuse the starter pack firmly",
                "You don't need to debate the business model; \"Not for me, I'm leaving the group\" is enough.",
                "Heyyy long time! Added you to the squad because you were always one of the ambitious ones. We've got people quitting their jobs already. Starter pack is open till Friday, want me to save you a spot?",
                Who("Jordan", 30, "Old schoolmate", "You were in the same class at school but haven't spoken in years", BabacanManipulator),
                T(OvguVeGuven), T(SosyalKanit, SahteZamanBaskisi), T(Kiyaslama, PasifTehdit)),
        };
    }
}
