namespace Transformalize.Libs.Cfg.Net.Parsers {
    public interface IParser {
        INode Parse(string cfg);
    }
}