﻿using DyslexiaApp.API.Data.Entities;
using DyslexiaAppMAUI.Shared;
using DyslexiaAppMAUI.Shared.Dtos;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Linq;

namespace DyslexiaApp.API.Services
{
    public class DyslexiaDiagnosisService
    {
        private readonly AppDbContext _context;

        public DyslexiaDiagnosisService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<DyslexiaDiagnosisDto[]> GetDyslexiaDiagnosesAsync()
        {
            return await _context.DyslexiaDiagnosis
                .AsNoTracking()
                .Select(diagnosis => new DyslexiaDiagnosisDto(
                    diagnosis.Id,
                    diagnosis.TestResults,
                    diagnosis.FeedBack,
                    diagnosis.Description,
                    diagnosis.MatchingGames.Select(matchingGame => new MatchingGameDto(
                        matchingGame.Id,
                        new EducationalDto(
                            matchingGame.EducationalGame.Id,
                            matchingGame.EducationalGame.Name,
                            matchingGame.EducationalGame.Description,
                            matchingGame.EducationalGame.GameSessions.Select(gs => new GameSessionDto(gs.Id, gs.SessionScore)).ToArray(),
                            new List<MatchingGameDto>() // Bu liste ilişkili oyunlar için, gerekirse doldurulabilir.
                        ),
                        matchingGame.GameSessions.Select(gs => new GameSessionDto(gs.Id, gs.SessionScore)).ToArray(),
                        matchingGame.Questions.Select(q => new QuestionDto
                        {
                            Id = q.Id,
                            QuestionText = q.QuestionText,
                            MainImage = q.MainImage != null ? new ImageDto { Id = q.MainImage.Id, Url = q.MainImage.Url, Description = q.MainImage.Description } : null,
                            ImageOptions = q.ImageOptions.Select(io => new ImageDto { Id = io.Id, Url = io.Url, Description = io.Description }).ToList(),
                            CorrectAnswerIndex = q.CorrectAnswerIndex
                        }).ToList()
                    )).ToArray(),
                    diagnosis.NavigationGames.Select(navGame => new NavigationGameDto(
                        navGame.Id,
                        navGame.Questions.Select(q => new NavigationGameQuestionDto
                        {
                            Id = q.Id,
                            BaloonPosition = q.BaloonPosition,
                        }).ToList(),
                        navGame.BalloonPosition
                    )).ToArray()
                ))
                .ToArrayAsync();
        }




        public async Task<DyslexiaDiagnosisDto> AddDyslexiaDiagnosisAsync(DyslexiaDiagnosisDto newDiagnosisDto)
        {
            var newDiagnosis = new DyslexiaDiagnosis
            {
                Id = Guid.NewGuid(),
                TestResults = newDiagnosisDto.TestResults,
                FeedBack = newDiagnosisDto.FeedBack,
                Description = newDiagnosisDto.Description,
            };

            _context.DyslexiaDiagnosis.Add(newDiagnosis);
            await _context.SaveChangesAsync();

            return newDiagnosisDto; // Ideal olarak, oluşturulan varlık ile doldurulmuş yeni bir DTO döndürülmelidir.
        }

        public async Task<double> CalculateAccuracyRateAsync(UserAnswersDto userAnswersDto)
        {
            int totalQuestions = userAnswersDto.AnswerResults.Count;
            int correctAnswers = 0;
            int wrongAnswers = 0;

            foreach (var userAnswer in userAnswersDto.AnswerResults)
            {
                var question = await _context.Questions.FindAsync(userAnswer.QuestionId);
                if (question == null) continue;

                if (userAnswer.SelectedAnswerIndex == question.CorrectAnswerIndex)
                {
                    correctAnswers++;
                }
                else
                {
                    wrongAnswers++;
                }
            }

            double accuracyRate = (double)correctAnswers / totalQuestions;
            return accuracyRate;
        }

        public string DetermineDyslexiaRate(double accuracyRate)
        {
            if (accuracyRate > 0.8)
            {
                return "Low risk of dyslexia";
            }
            else if (accuracyRate > 0.5)
            {
                return "Moderate risk of dyslexia";
            }
            else
            {
                return "High risk of dyslexia";
            }
        }

        public async Task<DyslexiaResultDto> SubmitAnswersAsync(UserAnswersDto userAnswersDto,string email)
        {
            double accuracyRate = await CalculateAccuracyRateAsync(userAnswersDto);
            string dyslexiaRate = DetermineDyslexiaRate(accuracyRate);

            Debug.WriteLine($"Accuracy : {accuracyRate}");
            Debug.WriteLine($"Dyslexia : {dyslexiaRate}");

            // userId ile kullanıcıyı bulup doğruluk oranını güncelle
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user != null)
            {
                user.Accuracy = (int)(accuracyRate * 100); // Accuracy oranını yüzde olarak saklamak için
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
            }

            return new DyslexiaResultDto
            {
                AccuracyRate = accuracyRate,
                DyslexiaRate = dyslexiaRate
            };
        }



    }
}