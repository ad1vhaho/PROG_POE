using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace PROG_POE
{
    public partial class MainWindow : Window
    {
        private List<CyberTask> tasks = new();
        private DispatcherTimer reminderTimer = new();
        private List<QuizQuestion> quizQuestions;
        private int currentQuestionIndex = 0;
        private int quizScore = 0;
        private string userName = "";
        private bool isUserNameSet = false;
        private List<string> activityLog = new();
        private bool showingAllActivities = false;
        private Random rand = new();

        private Dictionary<string, List<string>> keywordResponses = new()
        {
            { "password", new() { "Use strong, unique passwords.", "Never reuse passwords across different accounts." } },
            { "phishing", new() { "Beware of phishing emails.", "Don't click unknown links." } },
            { "privacy", new() { "Check your social media privacy settings.", "Be careful sharing personal info." } },
            { "2fa", new() { "Enable two-factor authentication.", "2FA adds security to your accounts." } },
            { "malware", new() { "Avoid downloading unknown files.", "Keep your antivirus updated." } },
            { "ransomware", new() { "Backup data regularly.", "Don't click suspicious links or ads." } },
            { "quiz", new() { "Try the quiz tab to test your knowledge!" } },
            { "task", new() { "Use task buttons to add and manage your tasks." } },
            { "activity", new() { "Type 'show activity log' to review your recent actions." } }
        };

        public MainWindow()
        {
            InitializeComponent();
            DisableChatControls();
            SendBtn.IsEnabled = true;
            UserInput.IsEnabled = false;
            PlayVoiceGreeting("Resources/ProgGreet.wav");
            AsciiArt.Text = GetAsciiArt();
            InitQuiz();
            InitReminderTimer();
            AddChatMessage("👋 Please enter your full name to get started.");
        }

        private void PlayVoiceGreeting(string path)
        {
            try
            {
                new SoundPlayer(@"C:\Users\adivh\source\repos\PROG_POE\PROG_POE\ProgGreet.wav").Play();
            }
            catch
            {
                MessageBox.Show("Unable to play greeting sound.");
            }
        }

        private string GetAsciiArt()
        {
            return "  C   R   Y   P   T   O   B   O   T";
        }

        private void Send_Click(object sender, RoutedEventArgs e)
        {
            if (!isUserNameSet)
            {
                string nameInput = UserNameBox.Text.Trim();

                if (string.IsNullOrWhiteSpace(nameInput) || !nameInput.Contains(" "))
                {
                    AddChatMessage("⚠️ Please enter your full name (first and last).");
                    return;
                }

                userName = nameInput;
                isUserNameSet = true;
                EnableChatControls();
                UserInput.Focus();
                UserInput.Clear();
                AddChatMessage($"🤖 Hello {userName}, I'm here to help you. Ask me about cybersecurity topics!");
                return;
            }

            string input = UserInput.Text.Trim();

            if (string.IsNullOrWhiteSpace(input))
            {
                AddChatMessage("🤖 Please type something before sending.");
                return;
            }

            if (input.Equals("exit", StringComparison.OrdinalIgnoreCase))
            {
                AddChatMessage("👋 Goodbye! Stay safe online.");
                Application.Current.Shutdown();
                return; 
            }

            AddChatMessage($"You: {input}");

            if (input.ToLower().Contains("show activity log"))
            {
                DisplayActivityLog();
            }
            else
            {
                AddChatMessage($"🤖 {GenResponse(input.ToLower())}");
            }

            UserInput.Clear();
        }

        private void DisplayActivityLog()
        {
            var logToShow = showingAllActivities ? activityLog : activityLog.TakeLast(10);
            AddChatMessage("📋 Here's your recent activity:");
            int count = 1;
            foreach (var log in logToShow)
                AddChatMessage($"{count++}. {log}");

            if (!showingAllActivities && activityLog.Count > 10)
            {
                var result = MessageBox.Show("Would you like to see all activities?", "More Log", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    showingAllActivities = true;
                    DisplayActivityLog();
                    showingAllActivities = false;
                }
            }
        }

        private void AddChatMessage(string message)
        {
            ChatDisplay.Items.Add(message);
            ChatDisplay.ScrollIntoView(message);
        }

        private void EnableChatControls()
        {
            UserInput.IsEnabled = true;
            AddTaskBtn.IsEnabled = true;
            DeleteTaskBtn.IsEnabled = true;
            CompleteTaskBtn.IsEnabled = true;
            TaskList.IsEnabled = true;
            SendBtn.IsEnabled = true;
            UserNameBox.IsEnabled = false;
        }

        private void DisableChatControls()
        {
            UserInput.IsEnabled = false;
            AddTaskBtn.IsEnabled = false;
            DeleteTaskBtn.IsEnabled = false;
            CompleteTaskBtn.IsEnabled = false;
            TaskList.IsEnabled = false;
        }

        private string GenResponse(string input)
        {
            foreach (var pair in keywordResponses)
            {
                if (input.Contains(pair.Key))
                {
                    var list = pair.Value;
                    return list[rand.Next(list.Count)];
                }
            }
            return "I'm not sure I understand. Try asking something else about cybersecurity.";
        }
         
        private void InitQuiz()
        {
            quizQuestions = new List<QuizQuestion>()
            {
                new QuizQuestion("What is phishing?", new[]{"Scam email to steal data", "Fishing sport", "A website"}, 0),
                new QuizQuestion("What is a strong password?", new[]{"123456", "Your name", "A mix of symbols & letters"}, 2),
                new QuizQuestion("Why use 2FA?", new[]{"Extra security", "Slows login", "For fun"}, 0),
                new QuizQuestion("What is malware?", new[]{"Antivirus", "Harmful software", "Password"}, 1),
                new QuizQuestion("Why avoid public Wi-Fi?", new[]{"It's too fast", "It's insecure", "It's illegal"}, 1)
            };
            quizQuestions = quizQuestions.OrderBy(q => rand.Next()).ToList();
            DisplayCurrentQuiz();
        }

        private void DisplayCurrentQuiz()
        {
            if (currentQuestionIndex >= quizQuestions.Count)
            {
                QuizQuestion.Text = $"Quiz complete! Score: {quizScore}/{quizQuestions.Count}";
                QuizOptions.Items.Clear();
                QuizFeedback.Text = "";
                activityLog.Add($"Completed quiz with score {quizScore}/{quizQuestions.Count}");
                return;
            }

            var q = quizQuestions[currentQuestionIndex];
            QuizQuestion.Text = q.Question;
            QuizOptions.Items.Clear();
            foreach (var opt in q.Options)
                QuizOptions.Items.Add(opt);
            QuizOptions.SelectedIndex = -1;
            QuizFeedback.Text = "";
        }

        private void SubmitQuizAnswer_Click(object sender, RoutedEventArgs e)
        {
            if (QuizOptions.SelectedIndex == -1)
            {
                MessageBox.Show("Select an answer before submitting.");
                return;
            }

            var correct = quizQuestions[currentQuestionIndex].AnswerIndex;
            if (QuizOptions.SelectedIndex == correct)
            {
                QuizFeedback.Text = "✅ Correct!";
                quizScore++;
            }
            else
            {
                QuizFeedback.Text = $"❌ Incorrect. Correct: {quizQuestions[currentQuestionIndex].Options[correct]}";
            }

            currentQuestionIndex++;
            DisplayCurrentQuiz();
        }

        private void InitReminderTimer()
        {
            reminderTimer.Interval = TimeSpan.FromSeconds(30);
            reminderTimer.Tick += (s, e) =>
            {
                foreach (var task in tasks)
                {
                    if (!task.IsDone)
                        AddChatMessage($"🔔 Reminder: {task.Title} - {task.Description}");
                }
            };
            reminderTimer.Start();
        }

        private void AddTask_Click(object sender, RoutedEventArgs e)
        {
            var title = Microsoft.VisualBasic.Interaction.InputBox("Enter task name:", "New Task");
            var desc = Microsoft.VisualBasic.Interaction.InputBox("Enter task description:", "New Task");

            if (!string.IsNullOrWhiteSpace(title))
            {
                var newTask = new CyberTask(title, desc);
                tasks.Add(newTask);
                TaskList.Items.Add(newTask);
                activityLog.Add($"Added task: {title}");
            }
        }

        private void DeleteTask_Click(object sender, RoutedEventArgs e)
        {
            if (TaskList.SelectedItem is CyberTask selected)
            {
                tasks.Remove(selected);
                TaskList.Items.Remove(selected);
                activityLog.Add($"Deleted task: {selected.Title}");
            }
            else
            {
                MessageBox.Show("Select a task to delete.");
            }
        }

        private void CompleteTask_Click(object sender, RoutedEventArgs e)
        {
            if (TaskList.SelectedItem is CyberTask selected)
            {
                selected.IsDone = true;
                TaskList.Items.Refresh();
                activityLog.Add($"Completed task: {selected.Title}");
            }
            else
            {
                MessageBox.Show("Select a task to complete.");
            }
        }

        private void Home_Click(object sender, RoutedEventArgs e)
        {
            currentQuestionIndex = 0;
            quizScore = 0;
            InitQuiz();
            AddChatMessage("🏠 You are back on Home. Quiz restarted.");
        }

        private void Quiz_Click(object sender, RoutedEventArgs e)
        {
            AddChatMessage("🧠 You can now attempt the Quiz.");
        }

        private void Tasks_Click(object sender, RoutedEventArgs e)
        {
            AddChatMessage("📌 Manage your tasks using the buttons below.");
        }
    }

    public class CyberTask
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsDone { get; set; }

        public CyberTask(string title, string description)
        {
            Title = title;
            Description = description;
        }

        public override string ToString() => $"{Title} - {(IsDone ? "✔️ Done" : "⏳ Pending")}";
    }

    public class QuizQuestion
    {
        public string Question { get; set; }
        public string[] Options { get; set; }
        public int AnswerIndex { get; set; }

        public QuizQuestion(string question, string[] options, int answerIndex)
        {
            Question = question;
            Options = options;
            AnswerIndex = answerIndex;
        }
    }
}
