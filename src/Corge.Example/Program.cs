using Corge;





var builder = new CorgeBuilder();
builder
    .SetupActorDialogue("Stephen", "darkorange3")
        .StartFromSentence("Hello")
            .ContinueWithSentence("I thought you are dead")
            .ContinueWithSentence("But...")
            .ContinueWithSentence("Who you are?")
            .ContinueWithDecision()
                .AddOption("What happened?", hideAfterUsed: false)
                    .SetAnswer("There was a storm and looks like you are the only survivor")
                    .Build()
                .AddOption("I am... I don't remember who I am...", hideAfterUsed: true)
                    .SetAnswer("That's weird")
                    .Build()
                .Build()
            .Build();





var runner = new CorgeRunner();
runner.Run();