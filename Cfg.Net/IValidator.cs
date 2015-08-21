namespace Transformalize.Libs.Cfg.Net {
    public interface IValidator {
        CfgValidatorResult Validate(string parent, string name, object value);
    }
}
