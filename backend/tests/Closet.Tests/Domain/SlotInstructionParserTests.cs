using ClosetAPI.Domain.Items;
using ClosetAPI.Domain.Outfits;

namespace Closet.Tests.Domain;

public class SlotInstructionParserTests
{
    [Theory]
    [InlineData("different shoes", Slot.Shoes)]
    [InlineData("swap the sneakers", Slot.Shoes)]
    [InlineData("a nicer shirt", Slot.Top)]
    [InlineData("try a sweater", Slot.Top)]
    [InlineData("other trousers", Slot.Bottom)]
    [InlineData("not jeans", Slot.Bottom)]
    [InlineData("a warmer jacket", Slot.Outer)]
    [InlineData("add a layer", Slot.Outer)]
    [InlineData("change the watch", Slot.Acc)]
    [InlineData("a different belt", Slot.Acc)]
    public void Maps_instructions_to_slots(string instruction, Slot expected)
    {
        Assert.Equal(expected, SlotInstructionParser.ParseSlot(instruction));
    }

    [Theory]
    [InlineData("make it fancier")]
    [InlineData("")]
    public void Returns_null_when_no_slot_is_mentioned(string instruction)
    {
        Assert.Null(SlotInstructionParser.ParseSlot(instruction));
    }

    [Fact]
    public void Is_case_insensitive()
    {
        Assert.Equal(Slot.Shoes, SlotInstructionParser.ParseSlot("DIFFERENT SHOES"));
    }
}
