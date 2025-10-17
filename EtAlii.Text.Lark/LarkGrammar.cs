#nullable disable

using Eto.Parse.Parsers;
using Eto.Parse.Scanners;
using Eto.Parse.Writers;
using System.CodeDom.Compiler;
using Eto.Parse;

namespace EtAlii.Text.Lark;

/// <summary>
/// Grammar to build a parser grammar using Lark modified Extended Backus-Naur Form
/// </summary>
public class LarkGrammar : Grammar
{
    private Dictionary<string, Parser> _parserLookup;
    private readonly string _startParserName = "start";
    // private Grammar _startGrammar;
    // private Parser _separator;
    
    //public bool DefineCommonNonTerminals { get; set; }

    //public IDictionary<string, Parser> SpecialParsers => _specialLookup;
    // private readonly Dictionary<string, Parser> _specialLookup = new(StringComparer.OrdinalIgnoreCase);
    
    
    // private IEnumerable<Tuple<string, Parser>> GetTerminals()
    // {
    //     foreach (var declaredProperty in typeof (Terminals).GetTypeInfo().DeclaredProperties)
    //     {
    //         if (typeof (Parser).GetTypeInfo().IsAssignableFrom(declaredProperty.PropertyType.GetTypeInfo()))
    //         {
    //             var parser = declaredProperty.GetValue(null, null) as Parser;
    //             yield return new Tuple<string, Parser>(declaredProperty.Name, parser.WithName(declaredProperty.Name));
    //         }
    //     }
    // }

    public LarkGrammar()
        : base("lark")
    {
        // ReSharper disable InconsistentNaming

        //Separator = Terminals.SingleLineWhiteSpace;
        
        var commaDelimiter = new RepeatCharTerminal(new RepeatCharItem(char.IsWhiteSpace), ',', new RepeatCharItem(char.IsWhiteSpace));
        //var jnull = new LiteralTerminal { Value = "null", Name = "null", CaseSensitive = false };

        // COLON    : ':';
        // LC       : '{';
        // RC       : '}';
        // LP       : '(';
        // RP       : ')';
        // LB       : '[';
        // RB       : ']';
        // COMMA    : ',';
        // DOT      : '.';
        // ARROW    : '->';
        // IGNORE   : '%ignore';
        // IMPORT   : '%import';
        // OVERRIDE : '%override';
        // DECLARE  : '%declare';
        // DD       : '..';
        // SQ       : '~';
        // WS_INLINE: (' ' | '\t')+ -> channel(OFF_CHANNEL);
        // COMMENT: '//' ~[\n\r]* -> channel(OFF_CHANNEL);
        // fragment FSTRING        : '"' (~["\\\r\n] | EscapeSequence)* '"';
        var FSTRING = new StringParser { QuoteCharacters = ['\"'], Name = "terminal string" };
        // STRING : FSTRING 'i'?;
        var STRING = FSTRING.Optional();
        
        // fragment EscapeSequence : '\\' [btnfr"'\\];

        // fragment DIGIT    : '0' .. '9';
        // fragment HEXDIGIT : 'a' .. 'f' | 'A' .. 'F' | DIGIT;
        
        // fragment INT      : DIGIT+;
        // NUMBER            : ('+' | '-')? INT;
        var NUMBER = new NumberParser { AllowExponent = false, AllowSign = true, AllowDecimal = false, Name = "number" };

        // REGEXP : '/' ('\\' '/' | '\\' '\\' | ~'/') ('\\' '/' | '\\' '\\' | ~'/')*? '/' [imslux]*;
        var REGEXP = new StringParser { QuoteCharacters = ['/'], Name = "regexp" };
        // NL     : ('\r'? '\n')+ (' ' | '\t' | '\n' | '\r' | '\f' | 'u2B7F')* -> channel(OFF_CHANNEL);
        // VBAR   : '|';
        // OP     : [+*] | '?';
        var OP = new AlternativeParser('+', '*', '?').WithName("operator");

        // WS_INLINE: (' ' | '\t')+ -> channel(OFF_CHANNEL);
        // COMMENT: '//' ~[\n\r]* -> channel(OFF_CHANNEL);
        // var comment = new GroupParser("//", new AlternativeParser("\r", "\n", "\r\n"));
        // var ows = -(Terminals.WhiteSpace | comment);
        // var rws = +(Terminals.WhiteSpace | comment);
        var ows = -(Terminals.WhiteSpace);
        var rws = +(Terminals.WhiteSpace);

        // RULE   : '!'? [_?]? [a-z] [_a-z0-9]*;
        var RULE = new OptionalParser('!') & new OptionalParser('_') & Terminals.Letter & -(Terminals.LetterOrDigit | '_');
        // TOKEN  : '_'? [A-Z] [_A-Z0-9]*;
        var TOKEN = new OptionalParser('_') & Terminals.Letter & -(Terminals.LetterOrDigit | '_');

        
        // name
        //     : RULE
        //     | TOKEN
        var name = RULE | TOKEN;

        // priority
        //     : '.' NUMBER
        var priority = "." & new NumberParser().WithName("priority");

        // rule_params
        //     : ('{' RULE (',' RULE)* '}')?
        var rule_params = ("{" & RULE.SeparatedBy(commaDelimiter).Named("rule parameters") & "}").Optional();
        //
        // token_params
        //     : ('{' TOKEN (',' TOKEN)* '}')?
        var token_params = ("{" & TOKEN.SeparatedBy(commaDelimiter).Named("token parameters") & "}").Optional();

        // value
        //     : STRING '..' STRING
        //     | name
        //     | (REGEXP | STRING)
        //     | name '{' value (',' value)* '}'
        var value = new AlternativeParser { Name = "value" };
        value.Add(
            (STRING & ".." & STRING).Named("range"),
            name,
            REGEXP | STRING//,
//            name & '{' & value.SeparatedBy(commaDelimiter).Named("value list") & "}"
        );

        var atom = new AlternativeParser();
        
        // expr
        //     : atom (OP | '~' NUMBER ('..' NUMBER)?)?
        var expr = atom & (OP | "~" & NUMBER & (".." & NUMBER).Optional()).Optional();

        // expansion
        //     : expr*
        var expansion = expr.Repeat();


        // alias
        //     : expansion ('->' RULE)?
        var alias = expansion & ("->" & RULE).Optional();

        // expansions
        //     : alias (VBAR alias)*
        var expansions = alias.SeparatedBy('|').Named("expansions");

        // atom
        //     : '(' expansions ')'
        //     | '[' expansions ']'
        //     | value
        atom.Add( 
            ('(' & expansions & ")") |
            ('[' & expansions & "]") |
            value);


        // rule_
        //     : RULE rule_params priority? ':' expansions
        var rule_ = RULE & rule_params & priority.Optional() & ":" & expansions;
        
        //
        // statement
        //     : '%ignore' expansions
        //     | '%import' import_path ('->' name)?
        //     | '%import' import_path name_list
        //     | '%override' rule_
        //     | '%declare' name+
        var statement = 
            ("%ignore" & expansions) |
            ("%override" & rule_) |
            ("%declare" & name.Repeat());

        // token
        //     : TOKEN token_params priority? ':' expansions
        var token = TOKEN & token_params & priority.Optional() & ':' & expansions;
        
        // import_path
        //     : '.'? name ('.' name)*
        //     ;
        //
        // name_list
        //     : '(' name (',' name)* ')'
        //     ;
        //
        // item
        //     : rule_
        //     | token
        //     | statement
        var item = rule_ | token | statement;

        // start_
        //     : item* EOF
        //     ;
        var start = +item;
        start.Separator = rws;
        Inner = ows & start & ows;

        // AttachEvents();
        
        //return;
        
        /*
        // ReSharper restore InconsistentNaming

        // Style = style;

        // special sequences for each terminal
        foreach (var terminal in GetTerminals()) //  was Terminals.GetTerminals();
        {
            var name = "Terminals." + terminal.Item1;
            _specialLookup[name] = terminal.Item2;
        }

        // terminals
        var hexCharacter = "#x" & +Terminals.HexDigit;
        var character = (("\\" & Terminals.AnyChar) | hexCharacter | Terminals.AnyChar.Except("]")).WithName("character");
        var characterRange = (character & "-" & character).WithName("character range");
        var characterSet = ("[" & ~(Parser)"^" & +(characterRange | character) & "]").WithName("character set");
        var terminalString = new StringParser { QuoteCharacters = ['\"', '\'', '’'], Name = "terminal string" };
        var specialSequence = ("?" & (+Terminals.AnyChar).Until("?").WithName("name") & "?").WithName("special sequence");
        var metaIdentifierTerminal = new OptionalParser("?", "inline") & (Terminals.Letter | '_') & -(Terminals.LetterOrDigit | '_');
        //var integer = new NumberParser().WithName("integer");

        // nonterminals
        var definitionList = new RepeatParser(0).WithName("definition list");
        var singleDefinition = new RepeatParser(1).WithName("single definition");
        var term = new SequenceParser().WithName("term");
        var primary = new AlternativeParser().WithName("primary");
        var exception = new UnaryParser("exception");
        var factor = new SequenceParser().WithName("factor");
        var metaIdentifier = new RepeatParser(1).WithName("meta identifier");
        var syntaxRule = new SequenceParser().WithName("syntax rule");
        var ruleEquals = new AlternativeParser().WithName("equals");
        Parser metaReference = metaIdentifier;

        Parser groupedSequence = ("(" & ows & definitionList & ows & ")").WithName("grouped sequence");

        //if (style.HasFlag(LarkStyle.EscapeTerminalStrings))
        //{
            terminalString.AllowEscapeCharacters = true;
        //}

        // if (style.HasFlag(LarkStyle.SquareBracketAsOptional))
        // {
        //     primary.Add(("[" & ows & definitionList & ows & "]").WithName("optional sequence"));
        // }

        // if (!style.HasFlag(LarkStyle.CardinalityFlags))
        // {
        //     //var repeatedSequence = ("{" & ows & definitionList & ows & "}").WithName("repeated sequence");
        //     var repeatedSequence = (ows & definitionList & ows).WithName("repeated sequence");
        //     primary.Add(repeatedSequence);
        // }

        // rules
        metaIdentifier.Inner = metaIdentifierTerminal;
        metaIdentifier.Separator = +Terminals.SingleLineWhiteSpace;
        // if (!style.HasFlag(LarkStyle.CommaSeparator))
        // {
            // w3c identifiers must be a single word
            metaIdentifier.Maximum = 1;
            metaReference = metaReference.NotFollowedBy(ows & ruleEquals);
        // }
        primary.Add(groupedSequence, metaReference, terminalString, specialSequence);
        //if (style.HasFlag(LarkStyle.CharacterSets) && !style.HasFlag(LarkStyle.SquareBracketAsOptional))
        //{
            // w3c supports character sets
            primary.Add(hexCharacter.Named("hex character"));
            primary.Add(characterSet);
        //}
        // if (style.HasFlag(LarkStyle.NumericCardinality))
        // {
        //     factor.Add(~(integer & ows & "*" & ows));
        // }

        factor.Add(primary);
        
        // Enables cardinality flags *+? after the rule, E.g. myFirstTerm* mySecondTerm+ myThirdTerm?.
        factor.Add(~(ows & Terminals.Set("?*+").WithName("cardinality")));

        term.Add(factor, ~(ows & "-" & ows & exception));
        exception.Inner = term;
        singleDefinition.Inner = term;
        singleDefinition.Separator = ows; //singleDefinition.Separator = style.HasFlag(LarkStyle.CommaSeparator) ? ows & "," & ows : ows;
        definitionList.Inner = singleDefinition;
        definitionList.Separator = ows & "|" & ows;
        ruleEquals.Add(":"); //ruleEquals.Add(style.HasFlag(LarkStyle.DoubleColonEquals) ? "::=" : "=", ":=");
        syntaxRule.Add(metaIdentifier, ows, ruleEquals, ows, definitionList);
        // if (style.HasFlag(LarkStyle.SemicolonTerminator))
        // {
        //     syntaxRule.Add(ows, ";"); // iso rules are terminated by a semicolon
        // }

        // var syntaxRules = +syntaxRule;
        // syntaxRules.Separator = rws; //syntaxRules.Separator = style.HasFlag(LarkStyle.SemicolonTerminator) ? ows : rws;
        //
        // Inner = ows & syntaxRules & ows;
        //
        // AttachEvents();
        */
    }

    // protected override void OnPreMatch(Match match)
    // {
    //     base.OnPreMatch(match);
    //     // _separator = Terminals.WhiteSpace;
    // }

    // private void AttachEvents()
    // {
    //     var syntaxRule = this["syntax rule"];
    //     syntaxRule.Matched += m =>
    //     {
    //         var name = m["meta identifier"].Text;
    //         // var isTerminal = m["equals"].Text == ":=";
    //         var isTerminal = m["equals"].Text == ":";
    //         var parser = m.Tag as UnaryParser;
    //         var inner = DefinitionList(m["definition list"], isTerminal);
    //         if (_separator != null && name == _startParserName)
    //         {
    //             parser.Inner = _separator & inner & _separator;
    //         }
    //         else
    //         {
    //             parser.Inner = inner;
    //         }
    //     };
    //     syntaxRule.PreMatch += m =>
    //     {
    //         var name = m["meta identifier"].Text;
    //         var parser = name == _startParserName ? _startGrammar ?? new Grammar(name) : new UnaryParser(name);
    //         m.Tag = _parserLookup[name] = parser;
    //     };
    // }

    // private Parser DefinitionList(Match match, bool isTerminal)
    // {
    //     var definitions = match.Find("single definition").ToList();
    //     return definitions.Count switch
    //     {
    //         1 => SingleDefinition(definitions[0], isTerminal),
    //         0 => null,
    //         _ => new AlternativeParser(definitions.Select(r => SingleDefinition(r, isTerminal)))
    //     };
    // }

    // private Parser SingleDefinition(Match match, bool isTerminal)
    // {
    //     var terms = match.Find("term").ToArray();
    //     if (terms.Length == 1)
    //     {
    //         return Term(terms[0], isTerminal);
    //     }
    //
    //     var sequence = new SequenceParser(terms.Select(r => Term(r, isTerminal)));
    //     if (!isTerminal)
    //     {
    //         sequence.Separator = _separator;
    //     }
    //
    //     return sequence;
    // }
    //
    // private Parser Term(Match match, bool isTerminal)
    // {
    //     var factor = Factor(match["factor"], isTerminal);
    //     var exception = match["exception"];
    //     if (exception)
    //     {
    //         return new ExceptParser(factor, Term(exception["term"], isTerminal));
    //     }
    //
    //     if (factor == null)
    //     {
    //         throw new Exception("woo");
    //     }
    //
    //     return factor;
    // }
    //
    // private Parser Factor(Match match, bool isTerminal)
    // {
    //     var primary = Primary(match["primary"], isTerminal);
    //     var cardinality = match["cardinality"];
    //     if (cardinality)
    //     {
    //         switch (cardinality.Text)
    //         {
    //             case "?":
    //                 primary = new OptionalParser(primary);
    //                 break;
    //             case "*":
    //                 primary = new RepeatParser(primary, 0);
    //                 break;
    //             case "+":
    //                 primary = new RepeatParser(primary, 1);
    //                 break;
    //             default:
    //                 throw new FormatException($"Cardinality '{cardinality.Text}' is unknown");
    //         }
    //     }
    //     var integer = match["integer"];
    //     if (integer)
    //     {
    //         return new RepeatParser(primary, Int32.Parse(integer.Text));
    //     }
    //
    //     return primary;
    // }
    //
    // private Parser Primary(Match match, bool isTerminal)
    // {
    //     var child = match.Matches.FirstOrDefault(r => r.Name != null);
    //     if (child == null)
    //     {
    //         throw new FormatException($"Primary must have a child. Text: '{match.Text}'");
    //     }
    //
    //     switch (child.Name)
    //     {
    //         case "grouped sequence":
    //             return new UnaryParser(DefinitionList(child["definition list"], isTerminal));
    //         case "optional sequence":
    //             return new OptionalParser(DefinitionList(child["definition list"], isTerminal));
    //         case "repeated sequence":
    //             var repeat = new RepeatParser(DefinitionList(child["definition list"], isTerminal), 0);
    //             if (!isTerminal)
    //             {
    //                 repeat.Separator = _separator;
    //             }
    //
    //             return repeat;
    //         case "meta identifier":
    //             if (!_parserLookup.TryGetValue(child.Text, out var parser))
    //             {
    //                 parser = _parserLookup[child.Text] = Terminals.LetterOrDigit.Repeat().Named(child.Text);
    //             }
    //             return parser;
    //         case "terminal string":
    //             return new LiteralTerminal(child.StringValue);
    //         case "hex character":
    //             return new SingleCharTerminal((char)int.Parse(child.Text.Substring(2), NumberStyles.HexNumber));
    //         case "character set":
    //             return CharacterSet(child);
    //         case "special sequence":
    //             var name = child["name"].Text.Trim();
    //             if (_specialLookup.TryGetValue(name, out parser))
    //             {
    //                 return parser;
    //             }
    //
    //             return null;
    //         default:
    //             throw new FormatException($"Could not parse child with name '{child.Name}'");
    //     }
    // }
    //
    // private char? Character(Match match)
    // {
    //     var text = match.Text;
    //     if (text.StartsWith("#x", StringComparison.OrdinalIgnoreCase))
    //     {
    //         var val = int.Parse(text.Substring(2), NumberStyles.HexNumber);
    //         if (val <= 0xFFFF && val >= 0)
    //         {
    //             return (char)val;
    //         }
    //
    //         return null;
    //     }
    //     if (text.Length == 2 && text[0] == '\\')
    //     {
    //         return text[1];
    //     }
    //     if (text.Length != 1)
    //     {
    //         throw new FormatException($"Character should only match one character '{text}'");
    //     }
    //
    //     return text[0];
    // }

    // private Parser CharacterSet(Match match)
    // {
    //     var alt = new AlternativeParser();
    //     var inverse = match.Text.StartsWith("[^", StringComparison.Ordinal);
    //     var characters = new List<char>();
    //     foreach (var child in match.Matches)
    //     {
    //         if (child.Name == null)
    //         {
    //             continue;
    //         }
    //
    //         switch (child.Name)
    //         {
    //             case "character range":
    //                 var first = Character(child.Matches.First(r => r.Name == "character"));
    //                 var last = Character(child.Matches.Last(r => r.Name == "character"));
    //                 if (first != null && last != null)
    //                 {
    //                     alt.Add(new CharRangeTerminal(first.Value, last.Value) { Inverse = inverse });
    //                 }
    //
    //                 break;
    //             case "character":
    //                 var character = Character(child);
    //                 if (character != null)
    //                 {
    //                     characters.Add(character.Value);
    //                 }
    //
    //                 break;
    //             default:
    //                 throw new FormatException($"Invalid character set child for text '{child.Text}'");
    //         }
    //     }
    //     if (characters.Count > 0)
    //     {
    //         alt.Add(new CharSetTerminal(characters.ToArray()) { Inverse = inverse });
    //     }
    //     if (alt.Items.Count > 1)
    //     {
    //         return alt;
    //     }
    //
    //     if (alt.Items.Count > 0)
    //     {
    //         return alt.Items[0];
    //     }
    //
    //     return new UnaryParser();
    //     //throw new FormatException(string.Format("Character set has no characters '{0}'", match.Text));
    // }

    protected override int InnerParse(ParseArgs args)
    {
        _parserLookup = new Dictionary<string, Parser>(StringComparer.OrdinalIgnoreCase);

        _parserLookup["letter or digit"] = Terminals.LetterOrDigit;
        _parserLookup["letter"] = Terminals.Letter;
        _parserLookup["decimal digit"] = Terminals.Digit;
        _parserLookup["character"] = Terminals.AnyChar;

        return base.InnerParse(args);
    }

    //public Grammar Build(string bnf, string startParserName, Grammar grammar = null)
    public Grammar Build(string bnf)
    {
        // _startParserName = startParserName;
        // _startGrammar = grammar;
        // _startGrammar = null;
        var match = Match(new StringScanner(bnf));

        if (!match.Success)
        {
            throw new FormatException($"Error parsing Lark grammar: \n{match.ErrorMessage}");
        }
        if (!_parserLookup.TryGetValue(_startParserName, out var parser))
        {
            throw new ArgumentException("The Lark grammar does not contain a start");
        }

        return parser as Grammar;
    }

    //public string ToCode(string bnf, string startParserName, string className = "GeneratedGrammar")
    public string ToCode(string bnf, string className = "GeneratedGrammar")
    {
        using var writer = new StringWriter();
        ToCode(bnf, writer, className);
        return writer.ToString();
    }

    //public void ToCode(string bnf, string startParserName, TextWriter writer, string className = "GeneratedGrammar")
    public void ToCode(string bnf, TextWriter writer, string className = "GeneratedGrammar")
    {
        var parser = Build(bnf);//, startParserName);
        var iw = new IndentedTextWriter(writer, "    ");

        iw.WriteLine("/* Date Created: {0}, Source EBNF:", DateTime.Now);
        iw.Indent++;
        foreach (var line in bnf.Split('\n'))
            iw.WriteLine(line);
        iw.Indent--;
        iw.WriteLine("*/");

        var parserWriter = new CodeParserWriter
        {
            ClassName = className
        };
        parserWriter.Write(parser, writer);
    }
}