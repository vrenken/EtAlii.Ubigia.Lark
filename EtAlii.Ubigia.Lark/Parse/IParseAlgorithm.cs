// TODO: Apologies, generated using an LLM, probably not the best approach.

namespace EtAlii.Ubigia.Lark;

/// <summary>
/// Pluggable algorithm surface (Earley/LALR).
/// </summary>
public interface IParseAlgorithm
{
    ParseResult Parse(string input);
}
