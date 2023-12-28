﻿using System;
using System.Collections.Generic;
using TableCloth.Commands.SplashScreen;
using TableCloth.Contracts;
using TableCloth.Events;

namespace TableCloth.ViewModels;

public sealed class SplashScreenViewModel : ViewModelBase
{
    [Obsolete("This constructor should be used only in design time context.")]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public SplashScreenViewModel() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    public SplashScreenViewModel(
        SplashScreenLoadedCommand splashScreenLoadedCommand)
    {
        _splashScreenLoadedCommand = splashScreenLoadedCommand;
    }

    public event EventHandler<DialogRequestEventArgs>? InitializeDone;

    public void NotifyInitialized(object sender, DialogRequestEventArgs e)
        => this.InitializeDone?.Invoke(sender, e);

    private readonly SplashScreenLoadedCommand _splashScreenLoadedCommand;

    public SplashScreenLoadedCommand SplashScreenLoadedCommand
        => _splashScreenLoadedCommand;

    private bool _appStartupSucceed = false;
    private bool _v2UIOptedIn = true;
    private IList<string> _passedArguments = new string[] { };
    private ITableClothArgumentModel? _parsedArgument;
    private IList<string> _warnings = new List<string>();

    public bool AppStartupSucceed
    {
        get => _appStartupSucceed;
        set => SetProperty(ref _appStartupSucceed, value);
    }

    public bool V2UIOptedIn
    {
        get => _v2UIOptedIn;
        set => SetProperty(ref _v2UIOptedIn, value);
    }

    public IList<string> PassedArguments
    {
        get => _passedArguments;
        set => SetProperty(ref _passedArguments, value);
    }

    public ITableClothArgumentModel? ParsedArgument
    {
        get => _parsedArgument;
        set => SetProperty(ref _parsedArgument, value);
    }

    public IList<string> Warnings
    {
        get => _warnings;
        set => SetProperty(ref _warnings, value);
    }
}