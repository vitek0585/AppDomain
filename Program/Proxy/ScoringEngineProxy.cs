using System;
using System.IO;
using System.Reflection;
using System.Security;
using System.Security.Permissions;

namespace Program.Proxy
{
    public class ScoringEngineProxy
    {
        static readonly string PathToAssembly = Path.Combine(Environment.CurrentDirectory, "plugins");
        private const string DomainName = "domain.scoringEngine";

        private static IScoringEngine _scoringEngine;
        private static object _sharedObject = new object();

        private AppDomainManager _appDomainManager;
        public IScoringEngine ScoringEngine
        {
            get
            {
                if (_scoringEngine == null)
                    _scoringEngine = CreateScoringEngine();
                return _scoringEngine;
            }
        }

        private IScoringEngine CreateScoringEngine()
        {
            lock (_sharedObject)
            {
                if (!_appDomainManager.IsExistDomain(DomainName))
                    _appDomainManager.CreateNonTrustDomain(DomainName, PathToAssembly);
                var typeOfScoreEngine = _appDomainManager.FindType(PathToAssembly, typeof(IScoringEngine));
                _scoringEngine = _appDomainManager.CreateInstanceInsideDomain<IScoringEngine>(DomainName, typeOfScoreEngine);
            }
            return null;
        }

        private ScoringEngineProxy()
        {

        }

        public OperationResult TryCalculate(string data)
        {
            return _scoringEngine.TryCalculate(data);
        }

        public OperationResult InitializeScoreCard(string scoreCard)
        {
            return null;
        }

        public OperationResult UpdateScoreCard(string scoreCard)
        {
            return null;
        }

        public string GetKeyForUpdate()
        {
            return null;
        }
    }
}