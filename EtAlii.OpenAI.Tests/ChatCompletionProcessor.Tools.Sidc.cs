using OpenAI.Chat;

namespace EtAlii.OpenAI.Tests;

public partial class ChatCompletionProcessor
{
    public static SidcRefinementResult GetSidcRefinementOptions(string prefix, string hint = null!)
    {
        switch (prefix.Length)
        {
            case 0:
                return new SidcRefinementResult
                {
                    Options = [ new SidcRefinementOption { Code = 'S', Description = "Warfighting" }], 
                    KeepRefining = true
                };
            case 1:
                return new SidcRefinementResult
                {
                    // affiliation: "F" | "H" | "N" | "U"  // Friendly, Hostile, Neutral, Unknown
                    Options =
                    [
                        new SidcRefinementOption { Code = 'F', Description = "Friendly affiliation" },
                        new SidcRefinementOption { Code = 'H', Description = "Hostile affiliation" },
                        new SidcRefinementOption { Code = 'N', Description = "Neutral affiliation" },
                        new SidcRefinementOption { Code = 'U', Description = "Unknown affiliation" },
                    ],
                    KeepRefining = true,
                };
            
            case 2:
                return new SidcRefinementResult
                {
                    // battle_dimension: "G" | "A" | "S" | "U" | "F" | "P"  // Ground, Air, Sea Surface, Subsurface, Special Operations Forces, Space
                    Options =
                    [
                        new SidcRefinementOption { Code = 'G', Description = "Ground" },
                        new SidcRefinementOption { Code = 'A', Description = "Air" },
                        new SidcRefinementOption { Code = 'S', Description = "Sea Surface" },
                        new SidcRefinementOption { Code = 'U', Description = "Subsurface" },
                        new SidcRefinementOption { Code = 'F', Description = "Special Operations Forces" },
                        new SidcRefinementOption { Code = 'P', Description = "Space" },
                    ],
                    KeepRefining = true,
                };
            
            case 3:
                return new SidcRefinementResult
                {
                    // status: "P" | "U" | "A" | "F" | "N" | "S" | "H"  // Pending, Unknown, Assumed Friend, Friend, Neutral, Suspect, Hostile
                    Options =
                    [
                        new SidcRefinementOption { Code = 'P', Description = "Pending" },
                        new SidcRefinementOption { Code = 'U', Description = "Unknown" },
                        new SidcRefinementOption { Code = 'A', Description = "Assumed Friend" },
                        new SidcRefinementOption { Code = 'F', Description = "Friend" },
                        new SidcRefinementOption { Code = 'N', Description = "Neutral" },
                        new SidcRefinementOption { Code = 'S', Description = "Suspect" },
                        new SidcRefinementOption { Code = 'H', Description = "Hostile" },
                    ],
                    KeepRefining = true,
                };

            case 4:
            {
                var a = prefix.ToLower()[1];

                var start = a switch
                {
                    'f' => 0,
                    'h' => 5,
                    'n' => 10,
                    'u' => 15,
                    _ => throw new ArgumentOutOfRangeException()
                };
                var end = a switch
                {
                    'f' => 4,
                    'h' => 9,
                    'n' => 14,
                    'u' => 19,
                    _ => throw new ArgumentOutOfRangeException()
                };
            
                return new SidcRefinementResult
                {
                    Options = Enumerable
                        .Range(start, end - start + 1)
                        .Select(i => new SidcRefinementOption { Code = (char)('A' + i), Description = $"Subtype {i}" })
                        .ToArray(),
                    KeepRefining = false,
                };
            }
        }

        return new SidcRefinementResult()
        {
            Options = [],
            KeepRefining = false,
        };
    }

    public static readonly ChatTool GetSidcRefinementOptionsTool = ChatTool.CreateFunctionTool(
        functionName: nameof(GetSidcRefinementOptions),
        functionDescription: $"Gets the options with which to complement the provided SIDC code with. If you do not know the code then call this method with a blank string. Also Keep calling this function until the result has the '{nameof(SidcRefinementResult.KeepRefining)}' value is to true.",
        functionParameters: BinaryData.FromBytes(
            """
            {
                "type": "object",
                "properties": {
                    "sidc": {
                        "type": "string",
                        "description": "The SIDC as it has already been determined, append one of the options to this code."
                    },
                    "hint": {
                        "type": "string",
                        "description": "The general textual description of what the final SIDC should represent."
                    }
                },
                "required": [ "location" ]
            }
            """u8.ToArray())
    );
}