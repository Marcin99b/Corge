using Corge;

var gameStorage = new GameStorage()
    .Actor("Stephen", color: "darkorange3", defaultSentence: "Hello")
        .MultiSentence(
            "I thought you are dead",
            "But...",
            "Who you are?"
        )
        .Decision()
            .AddOption("What happened?", hideAfterUser: true)
                .MultiSentence(
                    "There was a storm", 
                    "Looks like you are the only survivor"
                )
                .Return().Return()
            .AddOption("I don't remember who I am", hideAfterUser: false)
                .Sentence("That's weird")
                .Decision()
                    .AddOption("Where we are?", hideAfterUser: false)
                        .Sentence("On the beach of big island")
                        .ExitDialogue()
                        .Return()
                    .AddOption("How did you found me?", hideAfterUser: false)
                        .Sentence("I was walking through the beach")
                        .Return()
                        .Return()
                .Return()
                .Return()
                .Return()
            .AddOption("I have to look around", hideAfterUser: false)
            .ExitDialogue()
            .Return()
            .Return()
            .Return()
    .Actor("Adam", color: "darkorange3", defaultSentence: "I have no time")
    .Decision()
        .AddOption("Bye", hideAfterUser: false)
        .ExitDialogue()
.Build();

CorgeRunner.FromStorage(gameStorage).Run();