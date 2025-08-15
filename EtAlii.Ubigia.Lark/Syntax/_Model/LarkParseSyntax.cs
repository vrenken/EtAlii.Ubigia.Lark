namespace EtAlii.Ubigia.Lark;

/// <summary>
/// Represents the syntax parsed from Lark grammar definitions.
/// This class encapsulates the results of parsing, providing access
/// to parsed items, detected errors, and additional metadata.
/// </summary>
public partial class LarkParseSyntax
{
    public Rule? Start { get; set; }

    /// Indicates whether the parsed syntax contains a defined starting rule.
    /// This property evaluates to `true` if a rule with a name ending in "start" exists within the parsed items,
    /// signifying the presence of an entry point in the syntax structure.
    /// If no such starting rule is present, the property returns `false`.
    /// This property is determined during the parsing process and cannot be modified afterwards.
    public required bool HasStart { get; init; }

    /// Provides access to all parsed rules extracted from the Lark grammar definitions.
    /// This property contains an array of parsed `Rule` objects, each representing an individual
    /// rule defined in the grammar.
    /// Rules encompass details such as their names, parameters, priority levels, and expansions.
    /// The collection of rules is determined during the parsing process and cannot be modified.
    /// This property serves as an essential component in analyzing or utilizing the grammar structure.
    public required Rule[] Rules { get; init; }

    /// Contains all tokens parsed from the Lark grammar definitions.
    /// Tokens are individual lexical units identified during the parsing process,
    /// representing named patterns or elements in the grammar syntax.
    /// Each token is described by attributes such as its name, parameters,
    /// priority, and possible expansions, encapsulated within the `Token` class.
    /// This property provides access to all tokens extracted during the grammar analysis.
    public required Token[] Tokens { get; init; }

    /// Provides access to the import statements defined within the parsed syntax.
    /// This property holds an array of `ImportStatement` objects, representing the
    /// `import` rules encountered during the parsing process, typically used to
    /// reference external grammar files or resources.
    /// The `Imports` property is initialized when parsing completes and cannot be modified.
    public required ImportStatement[] Imports { get; init; }

    /// Represents the ignore statements detected within the parsed syntax.
    /// This property contains an array of `IgnoreStatement` objects, each defining specific patterns or tokens
    /// that are to be ignored during the parsing process as dictated by the Lark grammar.
    /// The content of this property is determined during parse-time based on the `!` (ignore) directives
    /// encountered in the input grammar.
    /// This property is immutable and reflects the structure of the parsed input.
    public required IgnoreStatement[] Ignores { get; init; }

    /// Represents a collection of override statements parsed from the syntax.
    /// This property contains rules redefined or modified within the grammar,
    /// encapsulated as `OverrideStatement` instances. It provides access to
    /// alterations explicitly applied to existing rules in the parsed grammar.
    /// The property is populated during the parsing process and cannot be modified afterward.
    public required OverrideStatement[] Overrides { get; init; }

    /// Provides access to all `DeclareStatement` instances parsed from the syntax structure.
    /// This property contains declarations that specify named elements within the grammar definitions.
    /// Each `DeclareStatement` represents a semantic rule intended for interpretation
    /// or validation during execution of the syntax processing pipeline.
    /// The property is automatically populated during parsing
    /// and reflects all declarations detected in the input grammar.
    /// It cannot be modified after the parsing has been completed.
    public required DeclareStatement[] Declares { get; init; }

    /// Indicates whether the parsing operation was successful and yielded a valid result.
    /// This property returns a boolean value that reflects the integrity of the syntax representation.
    /// A `true` value signifies that no errors were encountered during parsing, and the result is valid.
    /// A `false` value indicates that errors were found, and the result is considered invalid.
    /// This property is initialized during the parsing process and cannot be modified afterwards.
    public required bool IsValid { get; init; }

    /// Provides access to the collection of parsed items resulting from the Lark grammar processing.
    /// This property contains an array of `Item` objects that represent the individual components of the
    /// parsed syntax, such as rules, tokens, or other structural elements.
    /// The value of this property is determined during the parsing process and cannot be modified afterwards.
    public required Item[] Items { get; init; }

    /// Represents a collection of errors that were encountered during the parsing process.
    /// This property contains an array of string messages, each describing a specific error
    /// detected in the syntax or during processing of the input data.
    /// The errors include those generated by the parser as well as any additional
    /// syntax-related issues from imported dependencies.
    /// If no errors are found during parsing, this property will be an empty array.
    /// This property is initialized during the parsing process and cannot be modified afterwards.
    public required string[] Errors { get; init; }

    /// Represents the textual representation of the parsed syntax.
    /// This property contains a string that summarizes the parsed elements
    /// generated by processing Lark grammar definitions.
    /// It is constructed by concatenating the string output of all parsed items
    /// and is finalized during the parsing process.
    /// The text is trimmed of any trailing whitespace or line breaks.
    public required string Text { get; init; }
}