using System;
using System.Collections.Generic;
using System.Drawing;
using System.Media;
using System.Windows.Forms;

namespace PokerDivinationGame
{
    // ★★★ 關鍵修正：把 Form1 移到檔案的最上方，讓 Visual Studio 設計工具第一個讀到它 ★★★
    public partial class Form1 : Form
    {
        private List<Card> deck = new List<Card>();
        private Random random = new Random();

        private Card[] drawnCards = new Card[3];
        private int drawCount = 0;

        // UI 元件
        private Button btnShuffle;
        private Button btnDraw;
        private Label lblStatus;
        private RichTextBox rtbMeaning;
        private Panel[] pnlCards = new Panel[3];
        private Label[] lblCardTitles = new Label[3];

        private Timer shuffleTimer;
        private int shuffleTicks = 0;

        public Form1()
        {
            this.Text = "🔮 皇家聖三角 · 命運誠實占卜";
            this.Size = new Size(780, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(245, 242, 235);

            InitializeGameComponents();
            InitDeck();
            InitTimer();
        }

        private void InitializeGameComponents()
        {
            lblStatus = new Label { Text = "【 命運之輪 】請先點擊開始洗牌...", Location = new Point(50, 20), Size = new Size(680, 35), Font = new Font("微軟正黑體", 14, FontStyle.Bold), TextAlign = ContentAlignment.MiddleCenter, ForeColor = Color.FromArgb(60, 40, 20) };
            this.Controls.Add(lblStatus);

            string[] titles = { "❶ 過去 (顯示根源)", "❷ 現在 (當前處境)", "❸ 未來 (命運走向)" };
            int startX = 60;

            for (int i = 0; i < 3; i++)
            {
                lblCardTitles[i] = new Label { Text = titles[i], Location = new Point(startX + (i * 230), 75), Size = new Size(160, 25), Font = new Font("微軟正黑體", 10, FontStyle.Bold), TextAlign = ContentAlignment.MiddleCenter, ForeColor = Color.Gray };
                this.Controls.Add(lblCardTitles[i]);

                pnlCards[i] = new Panel { Location = new Point(startX + (i * 230), 105), Size = new Size(160, 240), BorderStyle = BorderStyle.None, BackColor = Color.White, Tag = i };
                pnlCards[i].Paint += PnlCard_Paint;
                this.Controls.Add(pnlCards[i]);
            }

            btnShuffle = new Button { Text = "🃏 開始洗牌儀式", Location = new Point(230, 370), Size = new Size(150, 45), Font = new Font("微軟正黑體", 11, FontStyle.Bold), BackColor = Color.FromArgb(70, 50, 40), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnShuffle.Click += BtnShuffle_Click;
            this.Controls.Add(btnShuffle);

            btnDraw = new Button { Text = "🔮 翻開下一張", Location = new Point(400, 370), Size = new Size(150, 45), Font = new Font("微軟正黑體", 11, FontStyle.Bold), BackColor = Color.DarkGoldenrod, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Enabled = false };
            btnDraw.Click += BtnDraw_Click;
            this.Controls.Add(btnDraw);

            rtbMeaning = new RichTextBox { Location = new Point(50, 440), Size = new Size(660, 200), Font = new Font("微軟正黑體", 11), ReadOnly = true, BorderStyle = BorderStyle.FixedSingle, BackColor = Color.FromArgb(253, 251, 247) };
            this.Controls.Add(rtbMeaning);
        }

        private void InitTimer()
        {
            shuffleTimer = new Timer();
            shuffleTimer.Interval = 50;
            shuffleTimer.Tick += ShuffleTimer_Tick;
        }

        private void InitDeck()
        {
            deck.Clear();
            foreach (Suit suit in Enum.GetValues(typeof(Suit)))
            {
                for (int val = 1; val <= 13; val++) { deck.Add(new Card(suit, val)); }
            }
        }

        private void BtnShuffle_Click(object sender, EventArgs e)
        {
            btnShuffle.Enabled = false;
            btnDraw.Enabled = false;
            shuffleTicks = 0;
            lblStatus.Text = "🔮 命運能量調和中，正在洗牌...";

            SystemSounds.Question.Play();
            shuffleTimer.Start();
        }

        private void ShuffleTimer_Tick(object sender, EventArgs e)
        {
            shuffleTicks++;
            foreach (var pnl in pnlCards) { pnl.Invalidate(); }

            if (shuffleTicks >= 20)
            {
                shuffleTimer.Stop();
                RealShuffle();
            }
        }

        private void RealShuffle()
        {
            InitDeck();
            int n = deck.Count;
            while (n > 1)
            {
                n--;
                int k = random.Next(n + 1);
                Card value = deck[k];
                deck[k] = deck[n];
                deck[n] = value;
            }

            drawCount = 0;
            for (int i = 0; i < 3; i++) { drawnCards[i] = null; lblCardTitles[i].ForeColor = Color.DimGray; }

            foreach (var pnl in pnlCards) { pnl.Invalidate(); }

            lblStatus.Text = "✨ 聖三角牌陣已就緒。請在心中默念問題，點擊『翻開下一張』";
            rtbMeaning.Text = "【 聖三角占卜法 】\n依序翻牌，解讀問題的「過去」、「現在」與「未來」。";
            btnShuffle.Enabled = true;
            btnDraw.Enabled = true;
        }

        private void BtnDraw_Click(object sender, EventArgs e)
        {
            if (drawCount < 3 && deck.Count > 0)
            {
                drawnCards[drawCount] = deck[0];
                deck.RemoveAt(0);

                lblCardTitles[drawCount].ForeColor = Color.DarkGoldenrod;
                pnlCards[drawCount].Invalidate();

                UpdateDivinationText();

                if (drawCount < 2)
                {
                    SystemSounds.Asterisk.Play();
                }

                drawCount++;

                if (drawCount == 3)
                {
                    btnDraw.Enabled = false;
                    lblStatus.Text = "🎉 占卜完整解讀完成！命運已然揭曉。";
                    PlayDestinySound(drawnCards[2]);
                }
            }
        }

        private void PlayDestinySound(Card futureCard)
        {
            try
            {
                if (futureCard.Value == 1 || futureCard.Value >= 11 ||
                    (futureCard.Value > 8 && (futureCard.CardSuit == Suit.Hearts || futureCard.CardSuit == Suit.Diamonds)))
                {
                    using (SoundPlayer player = new SoundPlayer(@"C:\Windows\Media\Windows Logon.wav")) { player.Play(); }
                }
                else if (futureCard.CardSuit == Suit.Spades && futureCard.Value >= 2 && futureCard.Value <= 7)
                {
                    using (SoundPlayer player = new SoundPlayer(@"C:\Windows\Media\Windows Hardware Remove.wav")) { player.Play(); }
                }
                else
                {
                    SystemSounds.Hand.Play();
                }
            }
            catch (Exception)
            {
                SystemSounds.Beep.Play();
            }
        }

        private void PnlCard_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            Panel pnl = (Panel)sender;
            int idx = (int)pnl.Tag;
            Rectangle rect = new Rectangle(0, 0, pnl.Width - 1, pnl.Height - 1);

            if (shuffleTimer.Enabled)
            {
                g.Clear(Color.FromArgb(230, 220, 240));
                using (Pen magicPen = new Pen(Color.MediumPurple, 3)) { g.DrawRectangle(magicPen, 8, 8, pnl.Width - 16, pnl.Height - 16); }
                using (Font font = new Font("Webdings", 24)) { g.DrawString("v", font, Brushes.MediumPurple, new PointF(65, 100)); }
            }
            else if (drawnCards[idx] == null)
            {
                g.Clear(Color.FromArgb(250, 248, 242));
                using (Pen dashedPen = new Pen(Color.LightGray, 2))
                {
                    dashedPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                    g.DrawRectangle(dashedPen, 5, 5, pnl.Width - 11, pnl.Height - 11);
                }
                using (Font font = new Font("微軟正黑體", 10, FontStyle.Italic)) { g.DrawString("等待賜牌", font, Brushes.Silver, new PointF(50, 110)); }
            }
            else
            {
                Card card = drawnCards[idx];
                g.Clear(Color.White);
                using (Pen borderPen = new Pen(Color.FromArgb(200, 180, 150), 2)) { g.DrawRectangle(borderPen, rect); }

                Brush brush = new SolidBrush(card.GetColor());
                Font cardFont = new Font("Georgia", 20, FontStyle.Bold);
                Font centerFont = new Font("Arial", 52, FontStyle.Bold);

                g.DrawString(card.GetRankString(), cardFont, brush, new PointF(10, 10));
                g.DrawString(card.GetSuitSymbol(), cardFont, brush, new PointF(10, 40));
                g.DrawString(card.GetRankString(), cardFont, brush, new PointF(120, 195));
                g.DrawString(card.GetSuitSymbol(), cardFont, brush, new PointF(120, 165));
                g.DrawString(card.GetSuitSymbol(), centerFont, brush, new PointF(45, 80));
            }
        }

        private void UpdateDivinationText()
        {
            rtbMeaning.Clear();
            string[] positionNames = { "【 💡 過去的根源 】", "【 🧭 現在的處境 】", "【 🎯 未來的走向 】" };

            for (int i = 0; i <= drawCount; i++)
            {
                if (i >= 3 || drawnCards[i] == null) continue;

                Card card = drawnCards[i];

                rtbMeaning.SelectionFont = new Font("微軟正黑體", 11, FontStyle.Bold);
                rtbMeaning.SelectionColor = Color.DarkGoldenrod;
                rtbMeaning.AppendText(positionNames[i] + "\n");

                rtbMeaning.SelectionFont = new Font("微軟正黑體", 11, FontStyle.Bold);
                rtbMeaning.SelectionColor = card.GetColor();
                rtbMeaning.AppendText("牌面：" + card.GetSuitSymbol() + " " + card.GetRankString() + " -> ");

                if (i == 2 && card.CardSuit == Suit.Spades && card.Value >= 2 && card.Value <= 7)
                {
                    string alertLevel = "";
                    string dangerDesc = "";
                    Color alertColor = Color.Red;

                    if (card.Value == 2 || card.Value == 3)
                    {
                        alertLevel = "⚠️【嚴重程度：烈火級大凶 — 腹背受敵】\n";
                        dangerDesc = "實話實說：未來的局勢極度惡劣。你可能會遭遇親近之人的背叛、或是計劃被外力攔腰折斷。此時任何盲目的掙扎都是徒勞，繼續固執推進只會全盤皆輸。請立刻止損、徹底放棄目前的執念，退回安全的防線才是唯一的活路。";
                        alertColor = Color.DarkRed;
                    }
                    else if (card.Value == 4 || card.Value == 5)
                    {
                        alertLevel = "⚠️【嚴重程度：重度級中凶 — 陷入泥淖】\n";
                        dangerDesc = "實話實說：你將面臨極大的孤立無援感。接下來這段路，你的努力將得不到回報，周圍的人不僅不理解你，甚至可能落井下石。這是一個強烈的停損信號，代表你的方向徹頭徹尾錯了，需要大刀闊斧地改弦易轍。";
                        alertColor = Color.Crimson;
                    }
                    else
                    {
                        alertLevel = "⚠️【嚴重程度：輕中度小凶 — 暗流洶湧】\n";
                        dangerDesc = "實話實說：表面看似平靜，但隱患已經在暗中滋生。未來的發展會不斷出現莫名其妙的小意外阻撓你，耗盡你的耐心。這是命運在提醒你，你目前忽略了某個致命的細節，請收起僥倖心態，重新做最壞的打算。";
                        alertColor = Color.OrangeRed;
                    }

                    rtbMeaning.SelectionFont = new Font("微軟正黑體", 11, FontStyle.Bold);
                    rtbMeaning.SelectionColor = alertColor;
                    rtbMeaning.AppendText(alertLevel);

                    rtbMeaning.SelectionFont = new Font("微軟正黑體", 11, FontStyle.Regular);
                    rtbMeaning.SelectionColor = Color.Black;
                    rtbMeaning.AppendText(dangerDesc + "\n\n");
                }
                else
                {
                    string meaning = "";
                    if (i == 0)
                    {
                        switch (card.Value)
                        {
                            case 1: case 11: case 12: case 13: meaning = "這件事的起因源於某個強大的新機會，或者曾有一位關鍵人物對你產生了深遠的影響。"; break;
                            default: meaning = "這件事在過去已經累積了一段時間（潛在能量指數：" + card.Value + "/13），過去的基礎打得很穩固。"; break;
                        }
                    }
                    else if (i == 1)
                    {
                        switch (card.Value)
                        {
                            case 1: meaning = "當下正處於一個巨大的轉折點與黃金新開端，此時不往前衝更待何時？"; break;
                            case 11: case 12: case 13: meaning = "現在局勢需要你發揮智慧與人際手腕，身邊可能正有人在暗中觀察、考核你。"; break;
                            default:
                                if (card.CardSuit == Suit.Spades) meaning = "注意！目前面臨較大的精神壓力（壓力值：" + card.Value + "），建議先讓自己停看聽，別盲目下決定。";
                                else meaning = "目前局勢相對平穩，正處於穩步發展的階段，不需要過度焦慮。";
                                break;
                        }
                    }
                    else
                    {
                        switch (card.Value)
                        {
                            case 1: meaning = "✨【天降大吉】未來將迎來完美的突破，你的願望極有可能以令人驚喜的形式實現！"; break;
                            case 11: case 12: case 13: meaning = "👑【貴人相助】未來你將會坐上主導者的位置，或者會有一位非常博學、權威的貴人前來相助。"; break;
                            default:
                                if (card.CardSuit == Suit.Hearts || card.CardSuit == Suit.Diamonds)
                                    meaning = "♥️【順遂如意】前景一片光明！未來的回報相當豐厚，無論是情感還是物質上都將獲得實質的豐收。";
                                else
                                    meaning = "🐾【行穩致遠】未來需要你付出對應的行動與努力（考驗指數：" + card.Value + "），只要堅持下去，就能開花結果。";
                                break;
                        }
                    }
                    rtbMeaning.SelectionFont = new Font("微軟正黑體", 11, FontStyle.Regular);
                    rtbMeaning.SelectionColor = Color.Black;
                    rtbMeaning.AppendText(meaning + "\n\n");
                }
            }

            rtbMeaning.SelectionStart = rtbMeaning.Text.Length;
            rtbMeaning.ScrollToCaret();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }
    }

    // ★★★ 搬家後的資料類別（放在最底端，不影響設計工具運作） ★★★
    public enum Suit { Hearts, Diamonds, Clubs, Spades }

    public class Card
    {
        public Suit CardSuit { get; set; }
        public int Value { get; set; }

        public Card(Suit suit, int value)
        {
            CardSuit = suit;
            Value = value;
        }

        public string GetRankString()
        {
            switch (Value) { case 1: return "A"; case 11: return "J"; case 12: return "Q"; case 13: return "K"; default: return Value.ToString(); }
        }

        public string GetSuitSymbol()
        {
            switch (CardSuit) { case Suit.Hearts: return "♥"; case Suit.Diamonds: return "♦"; case Suit.Clubs: return "♣"; case Suit.Spades: return "♠"; default: return ""; }
        }

        public Color GetColor()
        {
            return (CardSuit == Suit.Hearts || CardSuit == Suit.Diamonds) ? Color.Crimson : Color.FromArgb(30, 30, 30);
        }
    }
}