using NewWorldCompanion.Entities;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewWorldCompanion.ViewModels.Dialogs
{
    public class CooldownConfigViewModel : BindableBase
    {
        private CooldownTimer _cooldownTimer = new CooldownTimer();

        // Start of Constructor region

        #region Constructor

        public CooldownConfigViewModel(Action<CooldownConfigViewModel> closeHandler, CooldownTimer cooldownTimer)
        {
            // Init View commands
            CooldownConfigDoneCommand = new DelegateCommand(CooldownConfigDoneExecute);
            CloseCommand = new DelegateCommand<CooldownConfigViewModel>(closeHandler);

            _cooldownTimer = cooldownTimer;
        }

        #endregion

        // Start of Properties region

        #region Properties

        public DelegateCommand CooldownConfigDoneCommand { get; }
        public DelegateCommand<CooldownConfigViewModel> CloseCommand { get; }

        public CooldownTimer CooldownTimer { get => _cooldownTimer; set => _cooldownTimer = value; }

        #endregion

        // Start of Events region

        #region Events

        #endregion

        // Start of Methods region

        #region Methods

        private void CooldownConfigDoneExecute()
        {
            CloseCommand.Execute(this);
        }

        #endregion
    }
}