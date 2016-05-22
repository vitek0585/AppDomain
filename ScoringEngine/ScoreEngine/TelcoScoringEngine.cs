using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using CredoLab.ExtractorContract;
using Newtonsoft.Json;
using ScoringEngine.Extractor;
using ScoringEngine.Extractor.Common;
using ScoringEngine.Interfaces;
using ScoringEngine.Rsa;
using ScoringEngine.ScoreModel;
using ScoringEngine.Status;

namespace ScoringEngine.ScoreEngine
{
    public class TelcoScoringEngine : IScoreEngine
    {
        private ScoreCard _scoreCard;
        private IScoreCalculate _scoreCalculate;
        private IExtractorsContainer _extractorsContainer;
        private RsaEncryption _rsaEncryption;
        private ManualResetEventSlim _initializeProcessScoreCard;

        private WaitHandle _waitHandle;
        private object _sharedObject = new object();

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
            var calculateResult = _scoreCalculate.Calculate(data, _scoreCard, extractors);
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
                lock (_sharedObject)
                {
                    if (_scoreCard != null)
                    {
                        return new OperationResult(OperationStatus.ScoreCardAlreadyInitialized);
                    }
                    //stop thread

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
            lock (_sharedObject)
            {
                return UpdatingScoreCard(scoreCardEncrypt);
            }
        }

        private OperationResult UpdatingScoreCard(string scoreCardEncrypt)
        {
            var messageBuilder = new StringBuilder();
            var scoreCardDecrypt = _rsaEncryption.DecryptData(scoreCardEncrypt);
            _scoreCard = JsonConvert.DeserializeObject<ScoreCard>(scoreCardDecrypt);
            var extractors = GetExtractors();
            foreach (var feature in _scoreCard.Features)
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