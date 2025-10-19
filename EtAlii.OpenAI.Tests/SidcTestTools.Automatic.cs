using System.ComponentModel;

namespace EtAlii.OpenAI.Tests;

public static partial class SidcTestTools
{
    [Description($"Gets the options with which to complement the provided SIDC code with. If you do not know the code then call this method with a blank string. Also Keep calling this function until the result has the '{nameof(SidcRefinementResult.KeepRefining)}' value is to true.")]
    public static SidcRefinementResult GetSidcRefinementOptionsAutomatic(
        [Description("The SIDC as it has already been determined, append one of the options to this code.")]
        string prefix, 
        [Description("The general textual description of what the final SIDC should represent.")]
        string hint) => GetSidcRefinementOptionsManual(prefix, hint);
}