using System;
using System.Net.Http;
//using System.Threading.Tasks;
using Gdk;
using Gtk;
using Pango;
using GLib;
using GApp = Gtk.Application;
using GTimeout = GLib.Timeout;
using STask = System.Threading.Tasks.Task;
using NewtonsoftJson = Newtonsoft.Json.Linq;

namespace Clockwise
{
    class Program
    {
        private static Label timeLabel;
        private static Label dateLabel;
        private static Label quoteLabel;
        private static readonly HttpClient httpClient = new HttpClient();
        
        private static readonly string[] minihoList = {
            "坚持到底，迎接光明。",
            "心中有梦，脚下有路。",
            "不怕千万人阻挡，只怕自己投降。",
            "失败是成功的影子，不失败，怎么知道自己能成功。",
            "你可以累，但不能退。",
            "只要心中有火，世界就会亮起来。",
            "做一个勇敢的人，即使脚步蹒跚。"
        };

        static void Main(string[] args)
        {
            GApp.Init();

            var window = new Gtk.Window("Clockwise – 时钟与每日励志");
            window.Fullscreen();
            window.Decorated = false;         // 去掉边框
            window.KeepAbove = true;          // 保持置顶
            window.TypeHint = WindowTypeHint.Dialog;
            window.DeleteEvent += (s, e) => { e.RetVal = true; }; // 禁止关闭按钮
            window.KeyPressEvent += OnKeyPressed;

            var vbox = new Box(Orientation.Vertical, 30);
            window.Add(vbox);

            timeLabel = new Label();
            timeLabel.SetAlignment(0.5f, 0.5f);
            timeLabel.ModifyFont(FontDescription.FromString("Segoe UI Bold 72"));
            vbox.PackStart(timeLabel, true, true, 0);

            dateLabel = new Label();
            dateLabel.SetAlignment(0.5f, 0.5f);
            dateLabel.ModifyFont(FontDescription.FromString("Segoe UI Italic 36"));
            vbox.PackStart(dateLabel, false, false, 0);

            quoteLabel = new Label();
            quoteLabel.SetAlignment(0.5f, 0.5f);
            quoteLabel.ModifyFont(FontDescription.FromString("Segoe UI Light 24"));
            // 初始显示本地励志语
            quoteLabel.Text = $"🌟 {minihoList[new Random().Next(minihoList.Length)]}";
            vbox.PackStart(quoteLabel, false, false, 0);

            // 更新时间每秒一次
            GTimeout.Add(1000, () =>
            {
                var now = System.DateTime.Now; 
                timeLabel.Text = now.ToString("HH:mm:ss"); 
                dateLabel.Text = now.ToString("dddd, yyyy-MM-dd"); 
                return true;
            });

            // 每60秒尝试更新一次励志语
            GTimeout.Add(60000, () => 
            {
                STask.Run(async () => await UpdateQuoteAsync());
                return true;
            });

            // 程序启动时尝试获取API励志语
            STask.Run(async () => await UpdateQuoteAsync());

            window.ShowAll();
            GApp.Run();
        }

        private static async STask UpdateQuoteAsync()
        {
            try
            {
                // 使用公开的励志语API
                string url = "https://v1.hitokoto.cn";
                HttpResponseMessage response = await httpClient.GetAsync(url);
                
                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    NewtonsoftJson.JObject quoteData = NewtonsoftJson.JObject.Parse(json);
                    string quote = quoteData["hitokoto"]?.ToString() ?? "";
                    string author = quoteData["from"]?.ToString() ?? "";
                    
                    if (!string.IsNullOrEmpty(quote))
                    {
                        GApp.Invoke(delegate {
                            quoteLabel.Text = $"🌟 {quote} ——{author}";
                        });
                        return; // 成功获取API数据，直接返回
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"获取API励志语失败: {ex.Message}");
            }
            
            // API请求失败或数据无效时，使用本地数组中的内容
            ShowLocalQuote();
        }

        private static void ShowLocalQuote()
        {
            GApp.Invoke(delegate {
                quoteLabel.Text = $"🌟 {minihoList[new Random().Next(minihoList.Length)]}";
            });
        }

        private static void OnKeyPressed(object o, KeyPressEventArgs args)
        {
            var key = args.Event.Key;

            // 只允许 ESC 触发退出验证，其余按键禁用
            if (key == Gdk.Key.Escape)
            {
                ShowExitChallenge();
            }
            else
            {
                args.RetVal = true;
            }
        }

        private static void ShowExitChallenge()
        {
            var dialog = new Dialog("验证退出", null, DialogFlags.Modal);
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

            var box = new Box(Orientation.Vertical, 10) { BorderWidth = 10 };
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
                    questionLabel.Text = "❌ 答案错误，请再试一次：\n" + question;
                    answerEntry.Text = "";
                }
            };

            dialog.ShowAll();
        }
    }
}