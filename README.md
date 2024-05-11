# Corge

Game engine for text based games

## Example

```csharp
var gameStorage = new GameStorage()
    .Actor("Stephen", color: "darkorange3", defaultSentence: "Hello")
    .MultiSentence(
        "I thought you are dead",
        "But...",
        "Who you are?"
    )
    .Decision([

        x => x.AddOption("What happened?", hideAfterUser: true)
        .MultiSentence(
            "There was a storm",
            "Looks like you are the only survivor"
        ),

        x => x.AddOption("I don't remember who I am", hideAfterUser: false)
        .Sentence("That's weird")
        .Decision([
            d => d.AddOption("Where we are?", hideAfterUser: false)
                .Sentence("On the beach of big island")
                .ExitDialogue(),

            d => d.AddOption("How did you found me?", hideAfterUser: false)
                .Sentence("I was walking through the beach")
            ]),
        
        x => x.AddOption("I have to look around", hideAfterUser: false).ExitDialogue()

        ])
    .Actor("Adam", color: "darkorange3", defaultSentence: "I have no time")
    .Decision([
        x => x.AddOption("Bye", hideAfterUser: false).ExitDialogue()
        ]);

CorgeRunner.FromStorage(gameStorage).Run();
```
