namespace Aristocrat.Monaco.Gaming.UI.ViewModels.OperatorMenu
{
    using Application.UI.OperatorMenu;
    using Contracts;
    using Kernel;
    using Models;
    using System.Collections.Generic;

    public class SingleGameSetupViewModel : OperatorMenuSaveViewModelBase
    {
        private readonly ReadOnlyGameConfiguration _game;

        private string _gameName;
        private List<string> _betOptions;
        private List<string> _denomOptions;
        private List<string> _paytableOptions;
        private List<string> _playwayOptions;
        private string _maxBet;
        private string _rtp;
        private string _selectedBet;
        private string _selectedDenom;
        private string _selectedPaytable;
        private string _selectedPlayway;
        private long _topAward;

        private readonly string _originalBet;
        private readonly string _originalDenom;
        private readonly string _originalPaytable;
        private readonly string _originalPlayway;

        public SingleGameSetupViewModel(ReadOnlyGameConfiguration game)
        {
            // TODO Setup available options
            _game = game;
            GameName = game.ThemeName;

            PaytableOptions = new List<string>{game.PaytableId};
            SelectedPaytable = game.PaytableId;
            _originalPaytable = game.PaytableId;

            DenomOptions = new List<string>(game.AvailableDenominations);
            SelectedDenom = game.Denomination;
            _originalDenom = game.Denomination;

            PlaywayOptions = new List<string>();
            _originalPlayway = null;

            BetOptions = new List<string>();
            _originalBet = null;

            RTP = game.BaseGameRTP;
            MaxBet = game.MaximumWagerCredits;
            TopAward = game.TopAward;
        }

        public string GameName
        {
            get => _gameName;
            set
            {
                _gameName = value;
                RaisePropertyChanged(nameof(GameName));
            }
        }

        public List<string> PaytableOptions
        {
            get => _paytableOptions;
            set
            {
                _paytableOptions = value;
                RaisePropertyChanged(nameof(PaytableOptions));
            }
        }

        public string SelectedPaytable
        {
            get => _selectedPaytable;
            set
            {
                _selectedPaytable = value;
                RaisePropertyChanged(nameof(SelectedPaytable));
            }
        }

        public List<string> DenomOptions
        {
            get => _denomOptions;
            set
            {
                _denomOptions = value;
                RaisePropertyChanged(nameof(DenomOptions));
            }
        }

        public string SelectedDenom
        {
            get => _selectedDenom;
            set
            {
                _selectedDenom = value;
                RaisePropertyChanged(nameof(SelectedDenom));
            }
        }

        public List<string> PlaywayOptions
        {
            get => _playwayOptions;
            set
            {
                _playwayOptions = value;
                RaisePropertyChanged(nameof(PlaywayOptions));
            }
        }

        public string SelectedPlayway
        {
            get => _selectedPlayway;
            set
            {
                _selectedPlayway = value;
                RaisePropertyChanged(nameof(SelectedPlayway));
            }
        }

        public List<string> BetOptions
        {
            get => _betOptions;
            set
            {
                _betOptions = value;
                RaisePropertyChanged(nameof(BetOptions));
            }
        }

        public string SelectedBet
        {
            get => _selectedBet;
            set
            {
                _selectedBet = value;
                RaisePropertyChanged(nameof(SelectedBet));
            }
        }

        public string RTP
        {
            get => _rtp;
            set
            {
                _rtp = value;
                RaisePropertyChanged(nameof(RTP));
            }
        }

        public string MaxBet
        {
            get => _maxBet;
            set
            {
                _maxBet = value;
                RaisePropertyChanged(nameof(MaxBet));
            }
        }

        public long TopAward
        {
            get => _topAward;
            set
            {
                _topAward = value;
                RaisePropertyChanged(nameof(TopAward));
            }
        }

        public override bool HasChanges()
        {
            return SelectedBet != _originalBet && SelectedDenom != _originalDenom &&
                   SelectedPaytable != _originalPaytable && SelectedPlayway != _originalPlayway;
        }

        public override void Save()
        {
            // TODO Apply changes to game configurations on save
            var gameProvider = ServiceManager.GetInstance().GetService<IGameProvider>();
            var game = gameProvider.GetGame(_game.Id);
        }
    }
}
