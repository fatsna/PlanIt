using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using Newtonsoft.Json.Linq;
using RunNow.Core;
using RunNow.Models;
using RunNow.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;


namespace RunNow.ViewModels
{
    public partial class EmotionViewModel : ObservableObject
    {
        private readonly TcpClientService _tcpService;

        // Toolkit이 자동으로 public 프로퍼티를 생성함 (AllQuestions, CurrentQuestions, ... )
        [ObservableProperty] private ObservableCollection<QuestionModel> allQuestions = new();
        [ObservableProperty] private ObservableCollection<QuestionModel> currentQuestions = new();

        // (유지) 단계 표시용
        [ObservableProperty] private bool isStep1Visible = true;
        [ObservableProperty] private bool isStep2Visible = false;
        [ObservableProperty] private bool isStep3Visible = false;

        public EmotionViewModel(TcpClientService tcpService)
        {
            _tcpService = tcpService;

            // 문항 로드 & 표시
            LoadQuestions();
            UpdateCurrentQuestions();

            // 버튼 활성 갱신을 위해 변경 알림 구독
            AllQuestions.CollectionChanged += (_, __) => OnPropertyChanged(nameof(IsAllAnswered));
            OnPropertyChanged(nameof(IsAllAnswered));
        }

        /// 모든 문항이 응답되었는지 계산 속성 (버튼 활성화에 사용)
        public bool IsAllAnswered =>
            AllQuestions != null
            && AllQuestions.Count > 0
            && AllQuestions.All(q => q.SelectedAnswerIndex >= 0);

        ///  카테고리별 2문항씩 랜덤 선별 → 총 14문항(7카테고리 가정)
        private void LoadQuestions()
        {
            AllQuestions.Clear();

            var categoryGroups = Constants.Questions
                .GroupBy(q => q.Category!)
                .ToDictionary(g => g.Key, g => g.ToList());

            const int questionsPerCategory = 2;
            var selected = new List<QuestionModel>();
            var rnd = new Random();

            // 카테고리별 2문항 랜덤 추출
            foreach (var kv in categoryGroups)
            {
                var pool = kv.Value.OrderBy(_ => rnd.Next()).ToList();
                int take = Math.Min(questionsPerCategory, pool.Count);

                foreach (var item in pool.Take(take))
                {
                    var q = new QuestionModel
                    {
                        QuestionText = item.QuestionText!,
                        Category = item.Category!,
                        IsPositive = item.IsPositive
                    };
                    q.Answered += OnQuestionAnswered; // 라디오 변경 시 호출
                    selected.Add(q);
                }
            }

            // 목표 미달 시 남은 질문으로 채우기
            int targetCount = categoryGroups.Count * questionsPerCategory;
            if (selected.Count < targetCount)
            {
                string KeyOf(Constants.QuestionItem it) => $"{it.Category}|{it.QuestionText}|{it.IsPositive}";
                var selectedKeys = new HashSet<string>(
                    selected.Select(s => $"{s.Category}|{s.QuestionText}|{s.IsPositive}")
                );

                var remainPool = Constants.Questions
                    .Where(x => !selectedKeys.Contains(KeyOf(x)))
                    .OrderBy(_ => rnd.Next());

                foreach (var item in remainPool)
                {
                    if (selected.Count >= targetCount) break;
                    var q = new QuestionModel
                    {
                        QuestionText = item.QuestionText!,
                        Category = item.Category!,
                        IsPositive = item.IsPositive
                    };
                    q.Answered += OnQuestionAnswered;
                    selected.Add(q);
                }
            }

            // 보기 좋게 Q1.~ 번호 부여
            for (int i = 0; i < selected.Count; i++)
                selected[i].QuestionText = $"Q{i + 1}. {selected[i].QuestionText}";

            foreach (var q in selected)
                AllQuestions.Add(q);
        }

        private void UpdateCurrentQuestions()
        {
            CurrentQuestions.Clear();
            foreach (var q in AllQuestions)
                CurrentQuestions.Add(q);

            OnPropertyChanged(nameof(IsAllAnswered));
        }

        // 모든 문항 응답되면 자동 전송
        private void OnQuestionAnswered()
        {
            // 버튼 활성/비활성 즉시 반영
            OnPropertyChanged(nameof(IsAllAnswered));

            if (IsAllAnswered)
            {
                // 결과 데이터 생성 후, 토큰을 지정해 메시지 발송
                var data = BuildEmotionResultData();
                WeakReferenceMessenger.Default.Send<ValueChangedMessage<JObject>, string>(
                    new ValueChangedMessage<JObject>(data),
                    "EmotionSurveyCompleted"); // 토큰
            }
        }

        /// 카테고리별 평균 점수 계산
        private JObject CalculateCategoryAverages()
        {
            var grouped = AllQuestions.GroupBy(q => q.Category);
            var obj = new JObject();

            foreach (var g in grouped)
            {
                // 🔁 CHANGED: 새 매핑 적용
                var scores = g.Select(q =>
                {
                    int idx = q.SelectedAnswerIndex; // 0~4
                    return q.IsPositive ? (idx + 1) : (5 - idx);
                });

                double avg = scores.Any() ? scores.Average() : 0.0;
                obj[g.Key] = Math.Round(avg, 2);
            }

            // 전체 평균
            if (AllQuestions.Any())
            {
                // 🔁 CHANGED: 새 매핑 적용
                var total = AllQuestions
                    .Select(q => q.IsPositive ? (q.SelectedAnswerIndex + 1) : (5 - q.SelectedAnswerIndex))
                    .Average();
                obj["전체 평균"] = Math.Round(total, 2);
            }

            return obj;
        }

        // (선택) 결과 메시지 확인용 — 페이지 이동은 MainWindow의 EmotionResultCommand 사용
        [RelayCommand]
        private void ShowResults()
        {
            var result = CalculateCategoryAverages();
            var lines = result.Properties().Select(p => $"{p.Name}: {p.Value}점");
            MessageBox.Show(string.Join("\n", lines), "감정 분석 결과");
        }

        //  결과 데이터 구성 (Emotion_result_model과 맞는 필드명)
        public JObject BuildEmotionResultData()
        {
            var categories = AllQuestions
                .GroupBy(q => q.Category)
                .Select(g => g.Key)
                .ToList();

            var scores100 = new List<int>();
            var answers = new List<string>();

            foreach (var cat in categories)
            {
                var qs = AllQuestions.Where(q => q.Category == cat).ToList();
                if (qs.Count == 0)
                {
                    scores100.Add(0);
                    answers.Add($"{cat}: 데이터 없음");
                    continue;
                }

                // ★ 점수 매핑: 긍정=idx+1, 부정=5-idx (idx: 0~4)
                double avg5 = qs.Select(q =>
                {
                    int idx = Math.Clamp(q.SelectedAnswerIndex, 0, 4);
                    return q.IsPositive ? (idx + 1) : (5 - idx);   // 1~5
                }).Average();

                int score100 = (int)Math.Round((avg5 / 5.0) * 100.0);
                scores100.Add(score100);

                string msg = score100 switch
                {
                    >= 80 => $"{cat}: 전반적으로 양호합니다.",
                    >= 60 => $"{cat}: 보통 수준, 개선 권장.",
                    >= 40 => $"{cat}: 주의 필요, 습관 조정 권장.",
                    _ => $"{cat}: 위험 신호, 적극 개선 필요."
                };
                answers.Add(msg);
            }

            var dto = new { Categorys = categories, Scores100 = scores100, answer = answers };
            return JObject.FromObject(dto);
        }

        [RelayCommand]
        private void Submit()
        {
            if (!IsAllAnswered) return;

            var data = BuildEmotionResultData();
            WeakReferenceMessenger.Default.Send<ValueChangedMessage<JObject>, string>(
                new ValueChangedMessage<JObject>(data),
                "EmotionSurveyCompleted");
        }
    }
}