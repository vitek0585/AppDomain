using System.Collections.Generic;
using System.Linq;

namespace Program.Proxy.Extractor
{
    public class ExtractorResult
    {
        public ICollection<string> Errors { get; set; }
        public double Result { get; set; }
        public bool IsSuccess
        {
            get { return !Errors.Any(); }
        }
        public ExtractorResult()
        {
            Errors = new List<string>();
        }

    }
}