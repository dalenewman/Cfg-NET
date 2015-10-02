namespace Cfg.Net.Contracts {
    public interface ISerializer : IDependency {
        string Serialize(CfgNode node);
    }
}
