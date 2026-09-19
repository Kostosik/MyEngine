using MyEngine.Dialogue;

namespace Tests;

public class DialogueTests
{
    private static DialogueTree BuildSimpleTree()
    {
        var tree = new DialogueTree
        {
            Id = "test",
            Speaker = "NPC",
            StartNode = "start"
        };

        tree.Nodes["start"] = new DialogueNode
        {
            Id = "start",
            Text = "Hello!",
            NextNode = "second"
        };

        tree.Nodes["second"] = new DialogueNode
        {
            Id = "second",
            Text = "How are you?",
            Choices = new List<DialogueChoice>
            {
                new() { Text = "Good", NextNode = "good_reply" },
                new() { Text = "Bad", NextNode = "bad_reply", SetFlag = "player_sad" }
            }
        };

        tree.Nodes["good_reply"] = new DialogueNode
        {
            Id = "good_reply",
            Text = "Great!",
            NextNode = ""
        };

        tree.Nodes["bad_reply"] = new DialogueNode
        {
            Id = "bad_reply",
            Text = "Sorry to hear.",
            NextNode = ""
        };

        return tree;
    }

    [Fact]
    public void Start_SetsCurrentNode()
    {
        var runner = new DialogueRunner();
        var tree = BuildSimpleTree();
        var ctx = new DialogueContext();

        runner.Start(tree, ctx);

        Assert.True(runner.IsRunning);
        Assert.Equal("start", runner.CurrentNode!.Id);
    }

    [Fact]
    public void Advance_GoesToNextNode()
    {
        var runner = new DialogueRunner();
        var tree = BuildSimpleTree();
        var ctx = new DialogueContext();

        runner.Start(tree, ctx);
        runner.Advance();

        Assert.Equal("second", runner.CurrentNode!.Id);
    }

    [Fact]
    public void Advance_OnNodeWithChoices_DoesNothing()
    {
        var runner = new DialogueRunner();
        var tree = BuildSimpleTree();
        var ctx = new DialogueContext();

        runner.Start(tree, ctx);
        runner.Advance();  // теперь на "second"
        runner.Advance();  // должен игнорироваться

        Assert.Equal("second", runner.CurrentNode!.Id);
    }

    [Fact]
    public void Choose_GoesToSelectedBranch()
    {
        var runner = new DialogueRunner();
        var tree = BuildSimpleTree();
        var ctx = new DialogueContext();

        runner.Start(tree, ctx);
        runner.Advance();       // "second"
        runner.Choose(0);       // "Good"

        Assert.Equal("good_reply", runner.CurrentNode!.Id);
    }

    [Fact]
    public void Choose_AppliesSetFlag()
    {
        var runner = new DialogueRunner();
        var tree = BuildSimpleTree();
        var ctx = new DialogueContext();

        runner.Start(tree, ctx);
        runner.Advance();
        runner.Choose(1);       // "Bad" → SetFlag "player_sad"

        Assert.True(ctx.GetFlag("player_sad"));
    }

    [Fact]
    public void EmptyNextNode_EndsDialogue()
    {
        var runner = new DialogueRunner();
        var tree = BuildSimpleTree();
        var ctx = new DialogueContext();

        runner.Start(tree, ctx);
        runner.Advance();       // "second"
        runner.Choose(0);       // "good_reply"
        runner.Advance();       // NextNode пусто → конец

        Assert.False(runner.IsRunning);
    }

    [Fact]
    public void ConditionalChoice_HiddenByDefault()
    {
        var tree = new DialogueTree
        {
            Id = "test",
            StartNode = "start"
        };

        tree.Nodes["start"] = new DialogueNode
        {
            Id = "start",
            Text = "Hi",
            Choices = new List<DialogueChoice>
            {
                new() { Text = "Normal" },
                new() { Text = "Secret", ShowIf = "knows_secret" }
            }
        };

        var runner = new DialogueRunner();
        var ctx = new DialogueContext();

        runner.Start(tree, ctx);

        Assert.Single(runner.AvailableChoices);
        Assert.Equal("Normal", runner.AvailableChoices[0].Text);
    }

    [Fact]
    public void ConditionalChoice_VisibleWhenFlagSet()
    {
        var tree = new DialogueTree
        {
            Id = "test",
            StartNode = "start"
        };

        tree.Nodes["start"] = new DialogueNode
        {
            Id = "start",
            Text = "Hi",
            Choices = new List<DialogueChoice>
            {
                new() { Text = "Normal" },
                new() { Text = "Secret", ShowIf = "knows_secret" }
            }
        };

        var runner = new DialogueRunner();
        var ctx = new DialogueContext();
        ctx.SetFlag("knows_secret", true);

        runner.Start(tree, ctx);

        Assert.Equal(2, runner.AvailableChoices.Count);
    }
}