using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using Program.Proxy.Extractor;
using Program.Proxy.Rsa;
using Program.Proxy.ScoreModel;
using Program.Proxy.Status;


namespace Program.Proxy
{
    public class TelcoScoringEngine : IScoringEngine
    {
        private string _scoreCard;
        private IScoreCalculate _scoreCalculate;
        private IExtractorsContainer _extractorsContainer;
        private RsaEncryption _rsaEncryption;
        private ManualResetEventSlim _initializeProcessScoreCard;

        private object _rootSync = new object();

        public TelcoScoringEngine()
        {
            _extractorsContainer = new ExtractorsContainer(String.Empty);
            _scoreCalculate = new ScoreCalculator();
            _rsaEncryption = new RsaEncryption();
            _initializeProcessScoreCard = new ManualResetEventSlim(true);
        }

        public OperationResult TryCalculate(string data)
        {
            _initializeProcessScoreCard.Wait();
            if (_scoreCard == null)
            {
                var keyForEncrypt = _rsaEncryption.GetKeyForEncrypt();
                return new OperationResult(OperationStatus.NotInitializeScoreCard, keyForEncrypt);
            }
            var extractors = GetExtractors();
            var scoreCard = JsonConvert.DeserializeObject<ScoreCard>(_scoreCard);
            var calculateResult = _scoreCalculate.Calculate(data, scoreCard, extractors);
            return new OperationResult(OperationStatus.CompleteCalculate, score: calculateResult.Score);
        }

        public OperationResult InitializeScoreCard(string scoreCardEncrypt)
        {
            try
            {
                if (_scoreCard != null)
                {
                    return new OperationResult(OperationStatus.ScoreCardAlreadyInitialized);
                }
                _initializeProcessScoreCard.Reset();
                lock (_rootSync)
                {
                    if (_scoreCard != null)
                    {
                        return new OperationResult(OperationStatus.ScoreCardAlreadyInitialized);
                    }
                    return UpdatingScoreCard(scoreCardEncrypt);
                }
            }
            catch (Exception e)
            {
                return new OperationResult(OperationStatus.FailedInitializeScoreCard, e.Message);
            }
        }

        public OperationResult UpdateScoreCard(string scoreCardEncrypt)
        {
            _initializeProcessScoreCard.Reset();
            lock (_rootSync)
            {
                return UpdatingScoreCard(scoreCardEncrypt);
            }
        }

        private OperationResult UpdatingScoreCard(string scoreCardEncrypt)
        {
            var messageBuilder = new StringBuilder();
            var scoreCardDecrypt = _rsaEncryption.DecryptData(scoreCardEncrypt);
            var scoreCard = JsonConvert.DeserializeObject<ScoreCard>(scoreCardDecrypt);
            var extractors = GetExtractors();
            foreach (var feature in scoreCard.Features)
            {
                var ifExistsExtractor = extractors.Any(extractor => string.Equals(feature.ExtractorId, extractor.Identifier));
                if (ifExistsExtractor)
                {
                    messageBuilder.AppendLine($"Not found extractor for score card feature - {feature.ExtractorId}");
                }
            }
            _initializeProcessScoreCard.Set();
            var messages = messageBuilder.ToString();
            if (string.IsNullOrEmpty(messages))
            {
                //todo: do need refresh keys?
                _scoreCard = scoreCardDecrypt;
                _rsaEncryption.RefreshKeys();
                return new OperationResult(OperationStatus.SuccessInitializedScoreCard);
            }
            _scoreCard = null;
            return new OperationResult(OperationStatus.FailedInitializeScoreCard, messages);
        }

        public string GetKeyForUpdate()
        {
            return _rsaEncryption.GetKeyForEncrypt();
        }

        private IEnumerable<IExtractor> GetExtractors()
        {
            _extractorsContainer.Load();
            return _extractorsContainer.GetAllExtractors();
        }
    }
}