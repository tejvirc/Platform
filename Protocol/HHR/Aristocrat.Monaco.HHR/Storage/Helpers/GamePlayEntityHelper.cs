namespace Aristocrat.Monaco.Hhr.Storage.Helpers
{
    using System;
    using System.Data;
    using System.Linq;
    using Client.Messages;
    using Models;
    using Newtonsoft.Json;
    using Protocol.Common.Storage.Entity;

    public class GamePlayEntityHelper : IGamePlayEntityHelper
    {
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private GamePlayRequest _gamePlayRequest;
        private RaceStartRequest _raceStartRequest;
        private GamePlayResponse _gamePlayResponse;
        private bool _prizeCalculationError;
        private bool _gamePlayRequestFailed;
        private bool _horseAnimationFinished;
        private bool _manualHandicapWin;

        public GamePlayEntityHelper(IUnitOfWorkFactory unitOfWorkFactory)
        {
            _unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));

            using (var unitOfWork = _unitOfWorkFactory.Create())
            {
                var entity = unitOfWork.Repository<GamePlayEntity>().Queryable().FirstOrDefault();

                if (entity is null)
                {
                    entity = new GamePlayEntity
                    {
                        GamePlayRequest = string.Empty,
                        GamePlayResponse = string.Empty,
                        RaceStartRequest = string.Empty,
                        PrizeCalculationError = false
                    };

                    unitOfWork.Repository<GamePlayEntity>().Add(entity);
                    unitOfWork.SaveChanges();
                }

                _gamePlayRequest = JsonConvert.DeserializeObject<GamePlayRequest>(entity.GamePlayRequest);
                _raceStartRequest = JsonConvert.DeserializeObject<RaceStartRequest>(entity.RaceStartRequest);
                _gamePlayResponse = JsonConvert.DeserializeObject<GamePlayResponse>(entity.GamePlayResponse);
                _prizeCalculationError = entity.PrizeCalculationError;
                _gamePlayRequestFailed = entity.GamePlayRequestFailed;
                _horseAnimationFinished = entity.HorseAnimationFinished;
                _manualHandicapWin = entity.ManualHandicapWin;
            }
        }

        public GamePlayRequest GamePlayRequest
        {
            get => _gamePlayRequest;
            set
            {
                _gamePlayRequest = value;
                using (var unitOfWork = _unitOfWorkFactory.Create())
                {
                    unitOfWork.BeginTransaction(IsolationLevel.Serializable);
                    var repo = unitOfWork.Repository<GamePlayEntity>();
                    var entity = repo.Queryable().First();
                    entity.GamePlayRequest = JsonConvert.SerializeObject(value);
                    repo.AddOrUpdate(entity);
                    unitOfWork.Commit();
                }
            }
        }

        public RaceStartRequest RaceStartRequest
        {
            get => _raceStartRequest;
            set
            {
                _raceStartRequest = value;
                using (var unitOfWork = _unitOfWorkFactory.Create())
                {
                    unitOfWork.BeginTransaction(IsolationLevel.Serializable);
                    var repo = unitOfWork.Repository<GamePlayEntity>();
                    var entity = repo.Queryable().First();
                    entity.RaceStartRequest = JsonConvert.SerializeObject(value);
                    repo.AddOrUpdate(entity);
                    unitOfWork.Commit();
                }
            }
        }

        public GamePlayResponse GamePlayResponse
        {
            get => _gamePlayResponse;
            set
            {
                _gamePlayResponse = value;
                using (var unitOfWork = _unitOfWorkFactory.Create())
                {
                    unitOfWork.BeginTransaction(IsolationLevel.Serializable);
                    var repo = unitOfWork.Repository<GamePlayEntity>();
                    var entity = repo.Queryable().First();
                    entity.GamePlayResponse = JsonConvert.SerializeObject(value);
                    repo.AddOrUpdate(entity);
                    unitOfWork.Commit();
                }
            }
        }

        public bool PrizeCalculationError
        {
            get => _prizeCalculationError;
            set
            {
                _prizeCalculationError = value;
                using (var unitOfWork = _unitOfWorkFactory.Create())
                {
                    unitOfWork.BeginTransaction(IsolationLevel.Serializable);
                    var repo = unitOfWork.Repository<GamePlayEntity>();
                    var entity = repo.Queryable().First();
                    entity.PrizeCalculationError = value;
                    repo.AddOrUpdate(entity);
                    unitOfWork.Commit();
                }
            }
        }

        public bool GamePlayRequestFailed
        {
            get => _gamePlayRequestFailed;
            set
            {
                _gamePlayRequestFailed = value;
                using (var unitOfWork = _unitOfWorkFactory.Create())
                {
                    unitOfWork.BeginTransaction(IsolationLevel.Serializable);
                    var repo = unitOfWork.Repository<GamePlayEntity>();
                    var entity = repo.Queryable().First();
                    entity.GamePlayRequestFailed = value;
                    repo.AddOrUpdate(entity);
                    unitOfWork.Commit();
                }
            }
        }

        public bool HorseAnimationFinished
        {
            get => _horseAnimationFinished;
            set
            {
                _horseAnimationFinished = value;
                using (var unitOfWork = _unitOfWorkFactory.Create())
                {
                    unitOfWork.BeginTransaction(IsolationLevel.Serializable);
                    var repo = unitOfWork.Repository<GamePlayEntity>();
                    var entity = repo.Queryable().First();
                    entity.HorseAnimationFinished = value;
                    repo.AddOrUpdate(entity);
                    unitOfWork.Commit();
                }
            }
        }

        public bool ManualHandicapWin
        {
            get => _manualHandicapWin;
            set
            {
                _manualHandicapWin = value;
                using (var unitOfWork = _unitOfWorkFactory.Create())
                {
                    unitOfWork.BeginTransaction(IsolationLevel.Serializable);
                    var repo = unitOfWork.Repository<GamePlayEntity>();
                    var entity = repo.Queryable().First();
                    entity.ManualHandicapWin = value;
                    repo.AddOrUpdate(entity);
                    unitOfWork.Commit();
                }
            }
        }
    }
}