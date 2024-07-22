﻿using DyslexiaApp.MAUI.Pages.Login;
using DyslexiaApp.MAUI.Services;

namespace DyslexiaApp.MAUI
{
    public partial class AppShell : Shell
    {
        public AppShell(AuthService authService)
        {
            InitializeComponent();

            RegisterRoutes();
            _authService = authService;
        }
        private readonly static Type[] _routablePageTypes =
            [
                typeof(LoginPage),
                typeof(Register),
                typeof(RegisterAgreement),
                typeof(HomePage),
                typeof(DiagnosticTestAgreement),
                typeof(DiagnosisLetterMatchingInformation),
                typeof(EducationalGameList),
                typeof(ProfilePage),
                typeof(LetterMatchingGame),
                typeof(PlayGame),
                typeof(DiagnosisNavigationInfo),
                typeof(NavigationSkillsGame),
                typeof(DiagnosisSymmetryInfo),
                typeof(SymmetryGameTest),
                typeof(DiagnosisResultPage),
                typeof(EducationalResultPage),
                typeof(ResetPasswordPage),
                typeof(ForgotPassword),

            ];
        private readonly AuthService _authService;
        private static void RegisterRoutes()
        {
            foreach (var pageType in _routablePageTypes)
            {
                Routing.RegisterRoute(pageType.Name, pageType);
            }
        }
        private async void SignoutMenuItem_Clicked(object sender, EventArgs e)
        {
            _authService.Signout();
            await Shell.Current.GoToAsync($"//{nameof(LoginPage)}");
        }
    }
}