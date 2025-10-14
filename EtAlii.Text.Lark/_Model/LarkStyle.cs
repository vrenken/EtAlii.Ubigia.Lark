#nullable disable
namespace EtAlii.Text.Lark;

/// <summary>
/// Styles for the <see cref="LarkGrammar"/>
/// </summary>
[Flags]
public enum LarkStyle
{
    /// <summary>
    /// ISO format as defined here: http://en.wikipedia.org/wiki/Extended_Backus-Naur_Form
    /// </summary>
    //Iso14977 = SquareBracketAsOptional | BracketComments | NumericCardinality | SemicolonTerminator | CommaSeparator | WhitespaceSeparator,
    Default = WhitespaceSeparator | CharacterSets,// | CardinalityFlags,

    // /// <summary>
    // /// W3C format as defined for the XML spec here: http://www.w3.org/TR/REC-xml/#sec-notation
    // /// </summary>
    // W3c = CharacterSets | CardinalityFlags | DoubleColonEquals,

    // /// <summary>
    // /// Enables comments using round brackets (* *), otherwise comments use C style /* */
    // /// </summary>
    // BracketComments = 1 << 0,

    /// <summary>
    /// Enables using square brackets for optional sequences.  E.g. [ (rules) ]. Mutually exclusive with <see cref="CharacterSets"/> option.
    /// </summary>
    SquareBracketAsOptional = 1 << 1,

    /// <summary>
    /// Enables using character sets and ranges in square brackets. E.g. [1-9ABCDEF], with a carat for inverse: [^1-9ABCDEF]
    /// </summary>
    CharacterSets = 1 << 2,

    /// <summary>
    /// Enables numeric cardinality to prefix a term. E.g. 10 * (myTerm)
    /// </summary>
    NumericCardinality = 1 << 3,

    // /// <summary>
    // /// Enables cardinality flags *+? after the rule, E.g. myFirstTerm* mySecondTerm+ myThirdTerm?.
    // /// </summary>
    // /// <remarks>
    // /// * = zero or more
    // /// + = one or more
    // /// ? = zero or one, only enabled when <see cref="SquareBracketAsOptional"/> is not specified.
    // /// </remarks>
    // CardinalityFlags = 1 << 4,

    // /// <summary>
    // /// Require a semicolon to terminate each rule, otherwise only whitespace is required. E.g. myTerm = Term1 , Term2;
    // /// </summary>
    // SemicolonTerminator = 1 << 5,

    // /// <summary>
    // /// Separate each term by a comma. If not specified, a term may be more than one word.
    // /// </summary>
    // CommaSeparator = 1 << 6,

    // /// <summary>
    // /// Use a double colon for the rule equals. If not specified, no colon is required.  E.g. myTerm ::= Term1  vs. myTerm = Term1
    // /// </summary>
    // DoubleColonEquals = 1 << 7,

    /// <summary>
    /// Use Terminals.WhiteSpace as a default separator between each non-terminal. Ignored when <see cref="UseWhitespaceRule"/> is specified and grammar defines the whitespace terminal.
    /// </summary>
    WhitespaceSeparator = 1 << 8,

    /// <summary>
    /// Use the rule named 'comment' for whitespace in the definition. Ignored when <see cref="UseWhitespaceRule"/> is specified and grammar defines the whitespace terminal.
    /// </summary>
    UseCommentRuleWithSeparator = 1 << 9,

    /// <summary>
    /// Use the rule named 'whitespace' as the separator between each non-terminal. Overrides <see cref="WhitespaceSeparator"/> and <see cref="UseCommentRuleWithSeparator"/>.
    /// </summary>
    UseWhitespaceRule = 1 << 10,

    /// <summary>
    /// Allows escaping in terminal strings, such as \r \n \t, \x123, etc.
    /// </summary>
    EscapeTerminalStrings = 1 << 11
}