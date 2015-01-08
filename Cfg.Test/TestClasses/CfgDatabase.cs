using Transformalize.Libs.Cfg.Net;

namespace Cfg.Test.TestClasses {
    public class CfgDatabase : CfgNode {
        [Cfg(required = true, unique = true)]
        public string Name { get; set; }
        [Cfg(value = @"\\san\sql-backups")]
        public string BackupFolder { get; set; }
        [Cfg(value = 4)]
        public int BackupsToKeep { get; set; }
    }
}