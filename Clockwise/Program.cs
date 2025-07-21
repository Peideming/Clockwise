using System;
using Gtk;
using GLib;
using Pango;
using Gdk;
using GApp = Gtk.Application;
using GTimeout = GLib.Timeout;

namespace Clockwise
{
    class Program
    {
        private static Label timeLabel;
        private static Label dateLabel;
        private static Label quoteLabel;

        private static readonly string[] minihoList =
        {
            "每天进步一点点。",
            "不怕慢，只怕停。",
            "成功属于坚持不懈的人。",
            "相信自己，你能做到！",
            "保持好奇，持续探索。",
            "热爱生活，创造价值。"
        };

        static void Main(string[] args)
        {
            GApp.Init();

            var window = new Gtk.Window("Clockwise – 时钟与每日励志");
            window.Fullscreen();
            window.DeleteEvent += (s, e) => { GApp.Quit(); };
            window.KeyPressEvent += OnKeyPressed;

            var vbox = new VBox(false, 30);
            window.Add(vbox);

            // 时间标签
            timeLabel = new Label();
            timeLabel.SetAlignment(0.5f, 0.5f);
            timeLabel.ModifyFont(FontDescription.FromString("Segoe UI Bold 72"));
            timeLabel.Text = "";
            vbox.PackStart(timeLabel, true, true, 0);

            // 日期标签
            dateLabel = new Label();
            dateLabel.SetAlignment(0.5f, 0.5f);
            dateLabel.ModifyFont(FontDescription.FromString("Segoe UI Italic 36"));
            dateLabel.Text = "";
            vbox.PackStart(dateLabel, false, false, 0);

            // 励志标签
            var quote = minihoList[new Random().Next(minihoList.Length)];
            quoteLabel = new Label();
            quoteLabel.SetAlignment(0.5f, 0.5f);
            quoteLabel.ModifyFont(FontDescription.FromString("Segoe UI Light 24"));
            quoteLabel.Text = $"🌟 {quote}";
            vbox.PackStart(quoteLabel, false, false, 0);

            // 每秒更新
            GTimeout.Add(1000, () =>
            {
                var now = System.DateTime.Now;
                timeLabel.Text = now.ToString("HH:mm:ss");
                dateLabel.Text = now.ToString("dddd, yyyy-MM-dd");
                return true;
            });

            window.ShowAll();
            GApp.Run();
        }

        private static void OnKeyPressed(object o, KeyPressEventArgs args)
        {
            if (args.Event.Key == Gdk.Key.Escape)
            {
                ShowExitChallenge();
            }
        }

        private static void ShowExitChallenge()
        {
            var dialog = new Dialog("退出验证", null, DialogFlags.Modal);
            dialog.SetDefaultSize(320, 160);

            var rand = new Random();
            int a = rand.Next(1, 20);
            int b = rand.Next(1, 20);
            char[] ops = { '+', '-', '*', '/' };
            char op = ops[rand.Next(ops.Length)];
            double correct = op switch
            {
                '+' => a + b,
                '-' => a - b,
                '*' => a * b,
                '/' => Math.Round((double)a / b, 2),
                _ => 0
            };

            string question = $"{a} {op} {b} = ?";
            var questionLabel = new Label(question);
            var answerEntry = new Entry();
            var submitButton = new Button("提交");

            var box = new VBox(false, 10) { BorderWidth = 10 };
            box.PackStart(questionLabel, false, false, 5);
            box.PackStart(answerEntry, false, false, 5);
            box.PackStart(submitButton, false, false, 5);
            dialog.ContentArea.PackStart(box, false, false, 0);


            submitButton.Clicked += (s, e) =>
            {
                if (double.TryParse(answerEntry.Text, out double answer) &&
                    Math.Abs(answer - correct) < 0.01)
                {
                    GApp.Quit();
                }
                else
                {
                    questionLabel.Text = "❌ 错误答案，请再试一次：\n" + question;
                    answerEntry.Text = "";
                }
            };

            dialog.ShowAll();
        }
    }
}
