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

        private static IScoreEngine _scoreEngine;
        private static object _sharedObject = new object();

        private AppDomainManager _appDomainManager;
        public IScoreEngine ScoringEngine
        {
            get
            {
                if (_scoreEngine == null)
                    _scoreEngine = CreateScoringEngine();
                return _scoreEngine;
            }
        }

        private IScoreEngine CreateScoringEngine()
        {
            lock (_sharedObject)
            {
                if (!_appDomainManager.IsExistDomain(DomainName))
                    _appDomainManager.CreateNonTrustDomain(DomainName, PathToAssembly);
                var typeOfScoreEngine = _appDomainManager.FindType(PathToAssembly, typeof(IScoreEngine));
                _scoreEngine = _appDomainManager.CreateInstanceInsideDomain<IScoreEngine>(DomainName, typeOfScoreEngine);
            }
            return null;
        }

        private ScoringEngineProxy()
        {

        }

        public OperationResult TryCalculate(string data)
        {
            return null;
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