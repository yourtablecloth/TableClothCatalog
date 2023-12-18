﻿using System.ComponentModel;
using System.Runtime.CompilerServices;
using TableCloth.Components;

namespace TableCloth.ViewModels
{
    public class InputPasswordWindowViewModel : ViewModelBase
    {
        public InputPasswordWindowViewModel(
            X509CertPairScanner certPairScanner,
            AppMessageBox appMessageBox)
        {
            _certPairScanner = certPairScanner;
            _appMessageBox = appMessageBox;
        }

        private readonly X509CertPairScanner _certPairScanner;
        private readonly AppMessageBox _appMessageBox;

        public X509CertPairScanner CertPairScanner
            => _certPairScanner;

        public AppMessageBox AppMessageBox
            => _appMessageBox;
    }
}
