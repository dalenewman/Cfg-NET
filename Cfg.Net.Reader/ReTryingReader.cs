using Cfg.Net.Contracts;

namespace Cfg.Net.Reader {
    public class ReTryingReader : IReader {
        private readonly IReader _reader;
        private readonly int _attempts;

        public ReTryingReader(IReader reader, int attempts) {
            _reader = reader;
            _attempts = attempts;
        }

        public ReaderResult Read(string resource, ILogger logger) {
            var result = new ReaderResult { Source = Source.Url };
            for (var i = 0; i < _attempts; i++) {
                result = _reader.Read(resource, logger);
                if (result.Source != Source.Error)
                    break;
            }
            return result;
        }
    }
}