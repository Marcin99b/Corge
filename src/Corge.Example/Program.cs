using Corge;

//var storage = new CorgeBuilder()
//    .SetPlayerName("You")
//    .SetupActorDialogue("Stephen", "darkorange3")
//        .StartFromSentence("Hello")
//            .ContinueWithSentence("I thought you are dead")
//            .ContinueWithSentence("But...")
//            .ContinueWithSentence("Who you are?")
//            .ContinueWithDecision()
//                .AddOption("What happened?", hideAfterUsed: false)
//                    .SetAnswer("There was a storm and looks like you are the only survivor")
//                    .Build()
//                .AddOption("I am... I don't remember who I am...", hideAfterUsed: true)
//                    .SetAnswer("That's weird")
//                    .ContinueWithSentence("You lost all you memories?")
//                    .Build()
//                .AddExitOption("Exit", hideAfterUsed: false)
//                .Build()
//            .Build()
//        .Build();

var gameStorage = new GameStorage();
gameStorage
    .Actor("Stephen", color: "darkorange3", defaultSentence: "Hello")
    .MultiSentence(
        "I thought you are dead",
        "But...",
        "Who you are?"
    )
    .Decision()
        .AddOption("What happened?", hideAfterUser: false);

CorgeRunner.FromStorage(gameStorage).Run();