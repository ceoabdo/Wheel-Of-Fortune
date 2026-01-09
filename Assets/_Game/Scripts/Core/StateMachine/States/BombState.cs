using WheelOfFortune.Core.Models;
using WheelOfFortune.Infrastructure.Interfaces;
using WheelOfFortune.Infrastructure.DependencyInjection;

namespace WheelOfFortune.Core.StateMachine.States
{
    public sealed class BombState : IWheelGameState
    {
        private readonly WheelGameModel _model;
        private readonly WheelGameStateMachine _stateMachine;
        private readonly PlayingState _playingState;
        
        private IScreenService _screenService;
        private IWheelGameService _gameService;
        private IRevivalService _revivalService;

        public BombState(WheelGameModel model, WheelGameStateMachine stateMachine, PlayingState playingState)
        {
            _model = model;
            _stateMachine = stateMachine;
            _playingState = playingState;
        }

        public void Enter()
        {
            ResolveAllServices();
            _model.SetPendingBomb(true);
            _screenService?.ShowBombScreen();
        }

        private void ResolveAllServices()
        {
            ServiceResolver.Resolve(ref _screenService);
            ServiceResolver.Resolve(ref _gameService);
            ServiceResolver.Resolve(ref _revivalService);
        }

        public void Exit()
        {
        }

        public void HandleSpinRequested()
        {
        }

        public void HandleLeaveRequested()
        {
        }

        public void HandleContinueRequested()
        {
            ResolveRevivalService();
            ResolveGameService();

            if (_revivalService == null || _gameService == null)
            {
                return;
            }

            int availableCurrency = _gameService.GetPendingCurrency();
            int reviveCost = _model.GetContinueCost();

            if (_revivalService.TryRevive(reviveCost, availableCurrency))
            {
                _gameService.SpendCurrency(reviveCost);
            }
        }

        public void HandleGiveUpRequested()
        {
            _model.ClearPendingRewards();
            _model.ResetGame();
            
            ResolveGameService();
            _gameService?.ClearCollectedRewards();
            
            _stateMachine.ChangeState(_playingState);
        }

        private void ResolveScreenService()
        {
            ServiceResolver.Resolve(ref _screenService);
        }

        private void ResolveGameService()
        {
            ServiceResolver.Resolve(ref _gameService);
        }

        private void ResolveRevivalService()
        {
            ServiceResolver.Resolve(ref _revivalService);
        }
    }
}
