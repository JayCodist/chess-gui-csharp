using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace RoughtSheetEncore
{
    public partial class Form1 : Form
    {

        #region Declarations
        public enum TourneyFormState
        {
            Table,
            PastResults,
            Pairings,
            Home,
            ResultsEntering,
            TourneyOptionsInitial,
            TourneyOptionsFinal
        }

        TourneyInfo tourneyInfo;
        Control[] Template = new Control[5];
        Label TOtempLabel, TOLinkLabel = new Label();
        TextBox LinkTextBox = new TextBox();
        Panel TOInitOptPanel, TOFinalOptPanel, TOSummaryPanel;
        Random rnd = new Random();

        Label[] FixtureLabels, PastFixtureLabels, PastRoundLabels, PastRoundInfoLabels;
        Label OutcomeSelectionLabel;
        Form TourneyForm;
        TourneyFormState tourneyFormState;
        Button TONewTournament, TOBrowseCompletedTournaments, TOCancel, TOBack, TONext;

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label TOlabelT;
        private System.Windows.Forms.Button TONextRoundButton;
        private System.Windows.Forms.Button TOPastResultsButton;
        private System.Windows.Forms.Button TOWithdrawButton;
        private System.Windows.Forms.Button TOEnterResultButton;
        private System.Windows.Forms.Button TOReturnToTableButton;
        private System.Windows.Forms.RadioButton TOWhiteWinTick;
        private System.Windows.Forms.RadioButton TOBlackWinTick;
        private System.Windows.Forms.RadioButton TODrawTick;
        private System.Windows.Forms.Button TOResultInputButton;
        private System.Windows.Forms.Button TOResultCancelButton;
        #endregion

        public void RunTourneyUtilities()
        {
            TOLoadFromFile();
        }
        public void InitializeTourneyForm()
        {
            this.TourneyForm = new Form();
            TourneyForm.ForeColor = System.Drawing.SystemColors.ControlText;
            TourneyForm.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            TourneyForm.MaximizeBox = false;
            TourneyForm.MinimizeBox = false;
            TourneyForm.ShowIcon = false;
            TourneyForm.Text = "Lambda Tournament Pairer";
            TourneyForm.Scroll += new System.Windows.Forms.ScrollEventHandler(Form1_Scroll);
            TourneyForm.ResumeLayout(false);
            TourneyForm.StartPosition = FormStartPosition.CenterParent;
            TourneyForm.PerformLayout();
            TourneyForm.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            TourneyForm.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            TourneyForm.AutoScroll = true;
            TourneyForm.BackColor = System.Drawing.SystemColors.Control;
            TourneyForm.ClientSize = new System.Drawing.Size(800, 515);
            InitializeTourneyFormForStart();
        }
        private void InitializeTourneyFormForStart()
        {
            TONewTournament = new Button();
            TONewTournament.Click += TONewTournament_Click;
            TONewTournament.Text = "Start New Tournament";
            TONewTournament.Font = new Font("Segoe UI", 18F, FontStyle.Regular);
            TONewTournament.Location = new Point(145, 140);
            TONewTournament.AutoSize = false;
            TONewTournament.Size = new System.Drawing.Size(500, 70);
            TONewTournament.Cursor = Cursors.Hand;

            TOBrowseCompletedTournaments = new Button();
            TOBrowseCompletedTournaments.Click += TOBrowseCompletedTournaments_Click;
            TOBrowseCompletedTournaments.Text = "Browse Completed Tournaments";
            TOBrowseCompletedTournaments.Font = new Font("Segoe UI", 13F, FontStyle.Regular);
            TOBrowseCompletedTournaments.Cursor = Cursors.Hand;
            TOBrowseCompletedTournaments.Location = new Point(50, 290);
            TOBrowseCompletedTournaments.AutoSize = false;
            TOBrowseCompletedTournaments.Size = new Size(700, 50);
            tourneyFormState = TourneyFormState.Home;

            TourneyForm.Controls.Add(TONewTournament);
            TourneyForm.Controls.Add(TOBrowseCompletedTournaments);

            TourneyForm.ShowDialog(this);
        }
        void TOBrowseCompletedTournaments_Click(object sender, EventArgs e)
        {
            
        }
        void TONewTournament_Click(object sender, EventArgs e)
        {
            GoToTourneyOptionsInit();
        }
        private void GoToTourneyOptionsInit()
        {
            TourneyForm.Controls.Clear();
            tourneyFormState = TourneyFormState.TourneyOptionsInitial;
            tourneyInfo.UserParticipant = new Participant() { Name = CurrentUser.Name, FideRating = CurrentUser.Rating };
            TOInitOptPanel = new Panel();
            TOInitOptPanel.Size = TourneyForm.DisplayRectangle.Size - new System.Drawing.Size(20, 65);
            TourneyForm.Controls.Add(TOInitOptPanel);
            Color color = Color.Black;
            Font font = new System.Drawing.Font("Segoe UI", 12F, FontStyle.Regular);
            Label TourneyTypeLabel = new Label();
            ComboBox TourneyTypeCB = new ComboBox();
            TourneyTypeLabel.Text = "Tournament Type";
            TourneyTypeCB.ForeColor = color;
            TourneyTypeCB.Name = "TourneyTypeCB";
            TourneyTypeCB.FlatStyle = FlatStyle.Flat;
            TourneyTypeCB.DropDownStyle = ComboBoxStyle.DropDownList;
            TourneyTypeCB.Items.Add("Round Robin");
            TourneyTypeCB.Items.Add("Swiss");
            TourneyTypeCB.SelectedItem = tourneyInfo != null ? tourneyInfo.Type : "Round Robin";
            TourneyTypeCB.Cursor = Cursors.Hand;
            TourneyTypeLabel.ForeColor = color;
            TourneyTypeLabel.Font = font;
            TourneyTypeCB.Font = font;
            TourneyTypeCB.Width = 150;
            TourneyTypeLabel.AutoSize = true;
            TourneyTypeLabel.Location = new Point(70, 30);
            TourneyTypeCB.Location = new Point(TourneyTypeLabel.Right + 55, TourneyTypeLabel.Top - 3);
            TOInitOptPanel.Controls.Add(TourneyTypeCB);
            TOInitOptPanel.Controls.Add(TourneyTypeLabel);

            Label CycleCountLabel = new Label();
            ComboBox CycleCountCB = new ComboBox();
            CycleCountLabel.Text = "Number of " + 
                ((string)TourneyTypeCB.SelectedItem == "Round Robin" ? "cycles" : "rounds");
            CycleCountCB.ForeColor = color;
            CycleCountCB.Name = "CycleCountCB";
            CycleCountCB.FlatStyle = FlatStyle.Flat;
            CycleCountCB.DropDownStyle = ComboBoxStyle.DropDownList;
            for (int i = 0; i < ((string)TourneyTypeCB.SelectedItem == "Round Robin" ? 4 : 14); i++)
            {
                CycleCountCB.Items.Add(i + 1);
            }
            CycleCountCB.SelectedItem = tourneyInfo != null ? ((string)TourneyTypeCB.SelectedItem == "Round Robin"
                ? (tourneyInfo.TotalRounds / (tourneyInfo.Participants.Count - 1)) : tourneyInfo.TotalRounds) : 
                (string)TourneyTypeCB.SelectedItem == "Round Robin" ? 2 : 8;
            CycleCountCB.Cursor = Cursors.Hand;
            CycleCountLabel.ForeColor = color;
            CycleCountLabel.Font = font;
            CycleCountCB.Font = font;
            CycleCountCB.Width = 70;
            CycleCountLabel.AutoSize = true;
            CycleCountLabel.Location = new Point(70, TourneyTypeCB.Bottom + 20);
            CycleCountCB.Location = new Point(CycleCountLabel.Right + 55, CycleCountLabel.Top - 3);
            TOInitOptPanel.Controls.Add(CycleCountCB);
            TOInitOptPanel.Controls.Add(CycleCountLabel);

            Panel TimeControlPanel = new Panel();
            TimeControlPanel.Size = new System.Drawing.Size(370, 120);
            TimeControlPanel.BorderStyle = BorderStyle.FixedSingle;
            TimeControlPanel.Location = new Point(405, TourneyTypeCB.Top);
            TOInitOptPanel.Controls.Add(TimeControlPanel);

            Label TCLabel = new Label();
            TimeControl tc = tourneyInfo != null ? tourneyInfo.TimeControl : 
                new TimeControl()
                {
                    MainTime = new TimeSpan(1, 30, 0),
                    MainIncrement = 30,
                    TillEndOfGame = true
                };
            TCLabel.Text = "Time Control: " + tc.ToString();
            TCLabel.AutoSize = true;
            TCLabel.Font = new System.Drawing.Font("Segoe UI", 11F, FontStyle.Regular);
            TimeControlPanel.Controls.Add(TCLabel);
            TCLabel.Location = new Point(TimeControlPanel.Width / 2 - TCLabel.Width / 2, 5);

            Label TCDesc = new Label();
            TCDesc.Text = tc.GetDescription();
            TCDesc.AutoSize = false;
            TCDesc.Size = new System.Drawing.Size(350, 50);
            TCDesc.ForeColor = Color.DimGray;
            TCDesc.Font = new System.Drawing.Font("Segoe UI", 9F, FontStyle.Italic);
            TCDesc.Location = new Point(5, TCLabel.Bottom + 5);
            TimeControlPanel.Controls.Add(TCDesc);

            Button TCChange = new Button();
            TCChange.Text = "Change Time Control";
            TCChange.ForeColor = Color.Black;
            TCChange.Cursor = Cursors.Hand;
            TCChange.Font = new System.Drawing.Font("Segoe UI", 9F, FontStyle.Regular);
            TCChange.AutoSize = true;
            //TCChange.MouseLeave += TOPlayer_MouseLeave;
            //TCChange.MouseEnter += TOPlayer_MouseEnter;
            TCChange.Click += TOTCChange_Click;
            TCChange.Tag = Tuple.Create(TCLabel, TCDesc, tc, TimeControlPanel);
            TimeControlPanel.Controls.Add(TCChange);
            TCChange.Location = new Point(TimeControlPanel.Width / 2 - TCChange.Width / 2, TCLabel.Bottom + 58);

            Label PlayerCountLabel = new Label();
            ComboBox PlayerCountCB = new ComboBox();
            PlayerCountLabel.Text = "Number of players";
            PlayerCountCB.ForeColor = color;
            PlayerCountCB.Name = "PlayerCountCB";
            PlayerCountCB.FlatStyle = FlatStyle.Flat;
            PlayerCountCB.DropDownStyle = ComboBoxStyle.DropDownList;
            PlayerCountCB.Items.Add("4");
            PlayerCountCB.Items.Add("6");
            PlayerCountCB.Items.Add("8");
            PlayerCountCB.Items.Add("10");
            PlayerCountCB.SelectedItem = tourneyInfo != null ? tourneyInfo.Participants.Count.ToString() : "8";
            PlayerCountCB.Cursor = Cursors.Hand;
            PlayerCountLabel.ForeColor = color;
            PlayerCountLabel.Font = font;
            PlayerCountCB.Font = font;
            PlayerCountCB.Width = 70;
            PlayerCountLabel.AutoSize = true;
            PlayerCountLabel.Location = new Point(70, CycleCountCB.Bottom + 20);
            PlayerCountCB.Location = new Point(PlayerCountLabel.Right + 55, PlayerCountLabel.Top - 3);
            TOInitOptPanel.Controls.Add(PlayerCountCB);
            TOInitOptPanel.Controls.Add(PlayerCountLabel);

            Label label1 = new Label();
            label1.AutoSize = true;
            label1.Text = "You can drag the trackbar below to select the minimum rating for the tournament";
            label1.Location = new Point(40, PlayerCountCB.Bottom + 20);
            label1.Font = new System.Drawing.Font("Segoe UI", 10, FontStyle.Italic);

            TrackBar MinRatingTB = new TrackBar();
            MinRatingTB.Maximum = 2800;
            MinRatingTB.Minimum = 1;
            MinRatingTB.Name = "MinRatingTB";
            MinRatingTB.Width = 450;
            MinRatingTB.TickStyle = TickStyle.Both;
            MinRatingTB.TickFrequency = 1;
            MinRatingTB.Value = tourneyInfo != null ? tourneyInfo.MinRating : 
                (int)tourneyInfo.UserParticipant.FideRating - 200;
            MinRatingTB.Location = new Point(40, label1.Bottom + 10);
            MinRatingTB.ValueChanged += TOtb_ValueChanged;
            MinRatingTB.AutoSize = true;

            Label MinRatingLabel = new Label();
            MinRatingLabel.Font = new System.Drawing.Font("Segoe UI", 13, FontStyle.Bold);
            MinRatingLabel.Location = new Point(MinRatingTB.Right + 15, MinRatingTB.Top + 7);
            MinRatingLabel.AutoSize = true;
            MinRatingLabel.Text = MinRatingTB.Value.ToString() + 
                String.Format(" ({0})", GetRatingDescription(MinRatingTB.Value));
            MinRatingTB.Tag = MinRatingLabel;
            TOInitOptPanel.Controls.Add(label1);
            TOInitOptPanel.Controls.Add(MinRatingLabel);
            TOInitOptPanel.Controls.Add(MinRatingTB);

            Label label2 = new Label();
            label2.AutoSize = true;
            label2.Text = "You can drag the trackbar below to select the maximum rating for the tournament";
            label2.Location = new Point(40, MinRatingTB.Bottom + 10);
            label2.Font = new System.Drawing.Font("Segoe UI", 10, FontStyle.Italic);

            TrackBar MaxRatingTB = new TrackBar();
            MaxRatingTB.Maximum = 2800;
            MaxRatingTB.Minimum = 1;
            MaxRatingTB.Name = "MaxRatingTB";
            MaxRatingTB.Width = 450;
            MaxRatingTB.TickStyle = TickStyle.Both;
            MaxRatingTB.TickFrequency = 1;
            MaxRatingTB.Value = tourneyInfo != null ? tourneyInfo.MaxRating : 
                (int)tourneyInfo.UserParticipant.FideRating + 300;
            MaxRatingTB.Location = new Point(40, label2.Bottom + 10);
            MaxRatingTB.ValueChanged += TOtb_ValueChanged;
            MaxRatingTB.AutoSize = true;

            Label MaxRatingLabel = new Label();
            MaxRatingLabel.Font = new System.Drawing.Font("Segoe UI", 13, FontStyle.Bold);
            MaxRatingLabel.Location = new Point(MaxRatingTB.Right + 15, MaxRatingTB.Top + 7);
            MaxRatingLabel.AutoSize = true;
            MaxRatingLabel.Text = MaxRatingTB.Value.ToString() +
                String.Format(" ({0})", GetRatingDescription(MaxRatingTB.Value));
            MaxRatingTB.Tag = MaxRatingLabel;
            TOInitOptPanel.Controls.Add(label2);
            TOInitOptPanel.Controls.Add(MaxRatingLabel);
            TOInitOptPanel.Controls.Add(MaxRatingTB);

            Label label3 = new Label();
            label3.AutoSize = true;
            label3.Text = "You can drag the trackbar below to select the average rating for the tournament";
            label3.Location = new Point(40, MaxRatingTB.Bottom + 10);
            label3.Font = new System.Drawing.Font("Segoe UI", 10, FontStyle.Italic);

            TrackBar MeanRatingTB = new TrackBar();
            MeanRatingTB.Maximum = (int)tourneyInfo.UserParticipant.FideRating + 300;
            MeanRatingTB.Minimum = (int)tourneyInfo.UserParticipant.FideRating - 300;
            MeanRatingTB.Name = "MeanRatingTB";
            MeanRatingTB.Width = 450;
            MeanRatingTB.TickStyle = TickStyle.Both;
            MeanRatingTB.TickFrequency = 1;
            MeanRatingTB.Value = tourneyInfo != null ? tourneyInfo.AverageRating : 
                (int)tourneyInfo.UserParticipant.FideRating + 50;
            MeanRatingTB.Location = new Point(40, label3.Bottom + 10);
            MeanRatingTB.ValueChanged += TOtb_ValueChanged;
            MeanRatingTB.AutoSize = true;

            Label MeanRatingLabel = new Label();
            MeanRatingLabel.Font = new System.Drawing.Font("Segoe UI", 13, FontStyle.Bold);
            MeanRatingLabel.Location = new Point(MeanRatingTB.Right + 15, MeanRatingTB.Top + 7);
            MeanRatingLabel.AutoSize = true;
            MeanRatingLabel.Text = MeanRatingTB.Value.ToString() +
                String.Format(" ({0})", GetRatingDescription(MeanRatingTB.Value));
            MeanRatingTB.Tag = MeanRatingLabel;
            TOInitOptPanel.Controls.Add(label3);
            TOInitOptPanel.Controls.Add(MeanRatingLabel);
            TOInitOptPanel.Controls.Add(MeanRatingTB);

            font = new System.Drawing.Font("Segoe UI", 15F, FontStyle.Bold);
            TOCancel = new Button();
            TOBack = new Button();
            TONext = new Button();
            TOCancel.Text = "Cancel";
            TOBack.Text = "Back";
            TONext.Text = "Next";
            TOCancel.FlatStyle = FlatStyle.Flat;
            TOCancel.FlatAppearance.BorderColor = Color.DimGray;
            TOCancel.FlatAppearance.BorderSize = 3;
            TOBack.FlatStyle = FlatStyle.Flat;
            TOBack.FlatAppearance.BorderColor = Color.DimGray;
            TOBack.FlatAppearance.BorderSize = 3;
            TONext.FlatStyle = FlatStyle.Flat;
            TONext.FlatAppearance.BorderColor = Color.DimGray;
            TONext.FlatAppearance.BorderSize = 3;
            TOCancel.AutoSize = true;
            TOBack.AutoSize = true;
            TONext.AutoSize = true;
            TOCancel.Cursor = Cursors.Hand;
            TOBack.Cursor = Cursors.Hand;
            TONext.Cursor = Cursors.Hand;
            TOCancel.Font = font;
            TOBack.Font = font;
            TONext.Font = font;
            TOCancel.Location = new Point(300, TOInitOptPanel.Bottom + 10);
            TOBack.Location = new Point(500, TOInitOptPanel.Bottom + 10);
            TONext.Location = new Point(600, TOInitOptPanel.Bottom + 10);
            TONext.Tag = 
                Tuple.Create(MinRatingTB, MaxRatingTB, MeanRatingTB, PlayerCountCB, TourneyTypeCB, CycleCountCB, tc);
            TOCancel.Click += TOCancel_Click;
            TOBack.Click += TOBack_Click;
            TONext.Click += TONext_Click;
            TOBack.Enabled = false;
            TOBack.FlatAppearance.BorderColor = Color.LightGray;
            TourneyForm.Controls.Add(TONext);
            TourneyForm.Controls.Add(TOCancel);
            TourneyForm.Controls.Add(TOBack);
        }
        private void TOTCChange_Click(object sender, EventArgs e)
        {
            var tupleData = (sender as Control).Tag as Tuple<Label, Label, TimeControl, Panel>;
            var temp = ShowTimeForm(TourneyForm);
            if (temp != null)
            {
                tupleData.Item3.HumanBonusIncrement = temp.HumanBonusIncrement;
                tupleData.Item3.HumanBonusTime = temp.HumanBonusTime;
                tupleData.Item3.MainIncrement = temp.MainIncrement;
                tupleData.Item3.MainTime = temp.MainTime;
                tupleData.Item3.TCMoveNumber = temp.TCMoveNumber;
                tupleData.Item3.TCTime = temp.TCTime;
                tupleData.Item3.TillEndOfGame = temp.TillEndOfGame;

                tupleData.Item1.Text = "Time Control: " + tupleData.Item3.ToString();
                tupleData.Item1.Left = tupleData.Item4.Width / 2 - tupleData.Item1.Width / 2;
                tupleData.Item2.Text = tupleData.Item3.GetDescription();
            }
        }
        void TONext_Click(object sender, EventArgs e)
        {
            if (tourneyFormState == TourneyFormState.TourneyOptionsFinal)
            {
                tourneyInfo.IsPairingReady = false;
                tourneyInfo.CompletedFixtures = new List<Fixture>();
                tourneyInfo.CurrentRoundFixtures = new List<Fixture>();
                tourneyInfo.CurrentUserFixture = new Fixture();
                TourneyForm.Controls.Clear();
                InitializeTourneyFormForTable();
                GoToTable();
                return;
            }
            var tupleData = (sender as Control).Tag as 
                Tuple<TrackBar, TrackBar, TrackBar, ComboBox, ComboBox, ComboBox, TimeControl>;
            var str = TOOptTests(tupleData.Item1.Value, tupleData.Item2.Value, tupleData.Item3.Value);
            if (str == "")
            {
                if (tourneyInfo == null)
                    tourneyInfo = new TourneyInfo();
                tourneyInfo.Participants = new List<Participant>();
                for (int i = 0; i < int.Parse((string)tupleData.Item4.SelectedItem); i++)
                {
                    tourneyInfo.Participants.Add(new Participant());
                }
                tourneyInfo.AverageRating = tupleData.Item3.Value;
                tourneyInfo.MaxRating = tupleData.Item2.Value;
                tourneyInfo.MinRating = tupleData.Item1.Value;
                tourneyInfo.Name = tourneyInfo.UserParticipant.Name + " on " + DateTime.Now.ToShortDateString();
                tourneyInfo.Type = (string)tupleData.Item5.SelectedItem;
                tourneyInfo.TimeControl = tupleData.Item7;
                tourneyInfo.TotalRounds = tourneyInfo.Type == "Round Robin" ?
                    (tourneyInfo.Participants.Count - 1) * int.Parse(tupleData.Item6.SelectedItem.ToString())
                    : int.Parse(tupleData.Item6.SelectedItem.ToString());
                tourneyInfo.CreateBackUp();
                InitializeParticipants();
                GoToTourneyOptionsFinal();
                return;
            }
            else
            {
                tourneyInfo = new TourneyInfo();
                tourneyInfo.Participants = new List<Participant>();
                for (int i = 0; i < int.Parse((string)tupleData.Item4.SelectedItem); i++)
                {
                    tourneyInfo.Participants.Add(new Participant());
                }
                if (MessageBox.Show(str + "\nDo you want to revert to default settings and proceed?", "",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button1)
                        == System.Windows.Forms.DialogResult.Yes)
                {
                    tourneyInfo.AverageRating = tourneyInfo.BackUp.AverageRating;
                    tourneyInfo.MaxRating = tourneyInfo.BackUp.MaxRating;
                    tourneyInfo.MinRating = tourneyInfo.BackUp.MinRating;
                    tourneyInfo.Name = tourneyInfo.UserParticipant.Name + " on " + 
                        DateTime.Now.ToShortDateString();
                    tourneyInfo.Type = (string)tupleData.Item5.SelectedItem;
                    tourneyInfo.TimeControl = tupleData.Item7;
                    tourneyInfo.TotalRounds = tourneyInfo.Type == "Round Robin" ?
                        (tourneyInfo.Participants.Count - 1) * 
                        int.Parse(tupleData.Item6.SelectedItem.ToString())
                        : int.Parse(tupleData.Item6.SelectedItem.ToString());
                    InitializeParticipants();
                    GoToTourneyOptionsFinal();
                    return;
                }
                    
            }
        }
        private void GoToTourneyOptionsFinal()
        {
            TourneyForm.SuspendLayout();
            TOBack.Enabled = true;
            TOBack.FlatAppearance.BorderColor = Color.DimGray;
            TONext.Text = "START";
            tourneyFormState = TourneyFormState.TourneyOptionsFinal;
            if (TOFinalOptPanel != null)
                TourneyForm.Controls.Remove(TOFinalOptPanel);
            TOFinalOptPanel = new Panel();
            TOFinalOptPanel.Size = TourneyForm.DisplayRectangle.Size - new System.Drawing.Size(50, 65);
            TourneyForm.Controls.Add(TOFinalOptPanel);
            if (!tourneyInfo.Participants.Contains(tourneyInfo.UserParticipant))
                InitializeParticipants();
            Label infoLabel = new Label();
            infoLabel.Text = "You can click on any of the players to edit";
            infoLabel.Font = new System.Drawing.Font("Segoe UI", 11F, FontStyle.Italic);
            infoLabel.Location = new Point(230, 20);
            infoLabel.AutoSize = true;
            TOFinalOptPanel.Controls.Add(infoLabel);
            tourneyInfo.Participants = tourneyInfo.Participants.OrderByDescending(n => n.FideRating).ToList();
            Label tempLabel = null;
            for (int i = 1, j = 70; i <= tourneyInfo.Participants.Count; i++, j += 40)
            {
                if (i == (tourneyInfo.Participants.Count / 2) + 1)
                    j = 70;
                tempLabel = new Label();
                tempLabel.AutoSize = false;
                tempLabel.TextAlign = ContentAlignment.MiddleLeft;
                tempLabel.Location = new Point(i <= tourneyInfo.Participants.Count / 2 ? 50 : 450, j);
                tempLabel.Text = i.ToString() + ".   " + tourneyInfo.Participants[i - 1].Name +
                    String.Format(" ({0})", tourneyInfo.Participants[i - 1].FideRating);
                tempLabel.Font = new System.Drawing.Font("Segoe UI", 14F,
                    tourneyInfo.Participants[i - 1] != tourneyInfo.UserParticipant ? FontStyle.Regular : FontStyle.Bold);
                tempLabel.Width = 300;
                tempLabel.Height = 34;
                tempLabel.BackColor = Color.LightGray;
                tempLabel.Tag = tourneyInfo.Participants[i - 1];
                tempLabel.Cursor = tourneyInfo.Participants[i - 1] != 
                    tourneyInfo.UserParticipant ? Cursors.Hand : Cursors.Default;
                TOFinalOptPanel.Controls.Add(tempLabel);
                if (tourneyInfo.Participants[i - 1] != tourneyInfo.UserParticipant)
                {
                    tempLabel.Click += TOPlayerLabel_Click;
                    tempLabel.MouseEnter += TOPlayer_MouseEnter;
                    tempLabel.MouseLeave += TOPlayer_MouseLeave;
                }
            }
            TOSummaryPanel = new Panel();
            TOSummaryPanel.BorderStyle = BorderStyle.Fixed3D;
            TOSummaryPanel.Location = new Point(110, tempLabel.Bottom + 20);
            TOSummaryPanel.Size = new System.Drawing.Size(600, 165);
            TOSummaryPanel.BackColor = SystemColors.ControlLight;

            Label TName = new Label();
            TName.Text = "Tournament Name: " + tourneyInfo.Name;
            TName.Font = new System.Drawing.Font("Segoe UI", 10F, FontStyle.Regular);
            TName.AutoSize = true;            
            TOSummaryPanel.Controls.Add(TName);
            TName.Location = new Point(TOSummaryPanel.Width / 2 - TName.Width / 2, 7);

            Label TType = new Label();
            TType.Text = "Tournament type: " + tourneyInfo.Type;
            TType.Location = new Point(TName.Left, TName.Bottom + 3);
            TType.Font = TName.Font;
            TType.AutoSize = true;
            TOSummaryPanel.Controls.Add(TType);

            Label TCycles = new Label();
            TCycles.Text = "Number of rounds / Time Control: " + tourneyInfo.TotalRounds + " / "
                + tourneyInfo.TimeControl.ToString();
            TCycles.Location = new Point(TName.Left, TType.Bottom + 3);
            TCycles.Font = TName.Font;
            TCycles.AutoSize = true;
            TOSummaryPanel.Controls.Add(TCycles);

            Label TPlayers = new Label();
            TPlayers.Text = "Number of players: " + tourneyInfo.Participants.Count;
            TPlayers.Location = new Point(TName.Left, TCycles.Bottom + 3);
            TPlayers.Font = TName.Font;
            TPlayers.AutoSize = true;
            TOSummaryPanel.Controls.Add(TPlayers);

            Label TMean = new Label();
            TMean.Text = "Average rating: " + (int) tourneyInfo.Participants.Average(n => n.FideRating);
            TMean.Location = new Point(TName.Left, TPlayers.Bottom + 3);
            TMean.Font = TName.Font;
            TMean.AutoSize = true;
            TOSummaryPanel.Controls.Add(TMean);

            TCycles.Left = TOSummaryPanel.Width / 2 - TCycles.Width / 2;
            TName.Left = TCycles.Left;
            TMean.Left = TCycles.Left;
            TType.Left = TCycles.Left;
            TPlayers.Left = TCycles.Left;

            Button NameChange = new Button();
            NameChange.Text = "Change Tournament Name";
            NameChange.ForeColor = Color.Black;
            NameChange.Cursor = Cursors.Hand;
            NameChange.Font = TName.Font;
            NameChange.AutoSize = false;
            NameChange.MouseLeave += TOPlayer_MouseLeave;
            NameChange.MouseEnter += TOPlayer_MouseEnter;
            NameChange.Click += TONameChange_Click;
            NameChange.Size = new System.Drawing.Size(200, 30);
            TOSummaryPanel.Controls.Add(NameChange);
            NameChange.Location = new Point(TOSummaryPanel.Width / 2 - NameChange.Width / 2, TMean.Bottom + 10);

            TOFinalOptPanel.Controls.Add(TOSummaryPanel);
            TourneyForm.Controls.Remove(TOInitOptPanel);
            TourneyForm.Controls.Add(TOFinalOptPanel);
            TourneyForm.ResumeLayout();
        }
        void TONameChange_Click(object sender, EventArgs e)
        {
            Form TONameChangeForm = new Form();
            TONameChangeForm.MinimizeBox = false;
            TONameChangeForm.MaximizeBox = false;
            TONameChangeForm.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            TONameChangeForm.ShowIcon = false;
            TONameChangeForm.Size -= new System.Drawing.Size(0, 100);
            TONameChangeForm.StartPosition = FormStartPosition.CenterParent;
            TONameChangeForm.Text = "Edit Tournament Name";
            TONameChangeForm.KeyPreview = true;
            TONameChangeForm.KeyDown += TONameChangeForm_KeyDown;

            Button OK = new Button(), Cancel = new Button();
            OK.Text = "OK";
            Cancel.Text = "Cancel";
            OK.Font = new System.Drawing.Font("Segoe UI", 12F, FontStyle.Bold);
            Cancel.Font = new System.Drawing.Font("Segoe UI", 12F, FontStyle.Bold);
            OK.AutoSize = true;
            Cancel.AutoSize = true;
            OK.Location = new Point(140, 100);
            Cancel.Location = new Point(60, 100);
            OK.Click += TONameChange_Button;
            Cancel.Click += TONameChange_Button;
            Cancel.Tag = TONameChangeForm;

            var TONameChangePanel = new Panel();
            TONameChangePanel.Size = TONameChangeForm.DisplayRectangle.Size - new Size(0, 75);
            Font font = new System.Drawing.Font("Segoe UI", 12F, FontStyle.Regular);

            TextBox NameTB = new TextBox();
            NameTB.Font = font;
            NameTB.Multiline = true;
            NameTB.Size = new System.Drawing.Size(210, 50);
            NameTB.MaxLength = 25;
            NameTB.Text = tourneyInfo.Name;
            NameTB.Location = new Point(30, 30);
            NameTB.ShortcutsEnabled = true;
            TONameChangePanel.Controls.Add(NameTB);

            TONameChangeForm.Controls.Add(TONameChangePanel);
            TONameChangeForm.Controls.Add(OK);
            OK.Tag = Tuple.Create(NameTB, TONameChangeForm);
            TONameChangeForm.Tag = OK;
            TONameChangeForm.Controls.Add(Cancel);
            TONameChangeForm.ShowDialog(this);
        }
        private void TONameChange_Button(object sender, EventArgs e)
        {
            if ((sender as Control).Text == "Cancel")
                ((sender as Control).Tag as Form).Close();
            else
            {
                var tupleData = (sender as Control).Tag as Tuple<TextBox, Form>;
                String buffer = "";

                if (String.IsNullOrWhiteSpace(tupleData.Item1.Text) ||
                    tupleData.Item1.Text.Length < 3)
                    buffer = "Tournament name must be 3 or more characters. ";
                if (buffer != "")
                {
                    if (MessageBox.Show(buffer + "\nDo you want to return and discard changes?", "",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1)
                        == System.Windows.Forms.DialogResult.Yes)
                        tupleData.Item2.Close();
                }
                else
                {
                    String str = tupleData.Item1.Text.Trim();
                    tourneyInfo.Name = tupleData.Item1.Text.Substring(0, 1).ToUpper() +
                        tupleData.Item1.Text.Substring(1).Trim().ToLower();
                    tupleData.Item2.Close();
                    GoToTourneyOptionsFinal();
                }
            }
        }
        private void TONameChangeForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                (sender as Form).Close();
            else if (e.KeyCode == Keys.Enter)
                TONameChange_Button((sender as Form).Tag, new EventArgs());
        }
        void TOPlayer_MouseLeave(object sender, EventArgs e)
        {
            (sender as Control).BackColor = Color.LightGray;
        }
        void TOPlayer_MouseEnter(object sender, EventArgs e)
        {
            (sender as Control).BackColor = Color.FromArgb(100, 255, 255);
            //(sender as Control).BackColor = Color.FromArgb(255, 255, 50);
        }
        void TOPlayerLabel_Click(object sender, EventArgs e)
        {
            Form UtilDialogForm = new Form();
            UtilDialogForm.MinimizeBox = false;
            UtilDialogForm.MaximizeBox = false;
            UtilDialogForm.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            UtilDialogForm.ShowIcon = false;
            UtilDialogForm.Size -= new System.Drawing.Size(0, 20);
            UtilDialogForm.StartPosition = FormStartPosition.CenterParent;
            UtilDialogForm.Text = "Edit player";
            UtilDialogForm.KeyPreview = true;
            UtilDialogForm.KeyDown += UtilDialogForm_KeyDown;

            Button OK = new Button(), Cancel = new Button();
            OK.Text = "OK";
            Cancel.Text = "Cancel";
            OK.Font = new System.Drawing.Font("Segoe UI", 13F, FontStyle.Bold);
            Cancel.Font = new System.Drawing.Font("Segoe UI", 13F, FontStyle.Bold);
            OK.AutoSize = true;
            Cancel.AutoSize = true;
            OK.Location = new Point(180, 180);
            Cancel.Location = new Point(70, 180);
            OK.Click += TOPlayerEdit_Button;
            Cancel.Click += TOPlayerEdit_Button;
            Cancel.Tag = UtilDialogForm;

            var TOPlayerPanel = new Panel();
            TOPlayerPanel.Size = UtilDialogForm.DisplayRectangle.Size - new Size(0, 100);
            Font font = new System.Drawing.Font("Segoe UI", 12F, FontStyle.Regular);

            Label PlayerNameLabel = new Label();
            TextBox PlayerNameTB = new TextBox();
            PlayerNameLabel.Text = "Name";
            PlayerNameLabel.Font = font;
            PlayerNameLabel.AutoSize = true;
            PlayerNameTB.Font = font;
            PlayerNameTB.Width = 150;
            PlayerNameTB.MaxLength = 15;
            PlayerNameLabel.Location = new Point(30, 30);
            PlayerNameTB.Text = ((sender as Control).Tag as Participant).Name;
            PlayerNameTB.Location = new Point(PlayerNameLabel.Right - 20, PlayerNameLabel.Top - 3);
            TOPlayerPanel.Controls.Add(PlayerNameTB);
            TOPlayerPanel.Controls.Add(PlayerNameLabel);

            Label PlayerRatingLabel = new Label();
            TextBox PlayerRatingTB = new TextBox();
            PlayerRatingLabel.Text = "Rating";
            PlayerRatingLabel.Font = font;
            PlayerRatingTB.Font = font;
            PlayerRatingTB.Width = 150;
            PlayerRatingTB.MaxLength = 4;
            PlayerRatingLabel.AutoSize = true;
            PlayerRatingLabel.Location = new Point(30, PlayerNameTB.Bottom + 20);
            PlayerRatingTB.Text = ((sender as Control).Tag as Participant).FideRating.ToString();
            PlayerRatingTB.Location = new Point(PlayerRatingLabel.Right - 20, PlayerRatingLabel.Top - 3);
            TOPlayerPanel.Controls.Add(PlayerRatingTB);
            TOPlayerPanel.Controls.Add(PlayerRatingLabel);

            UtilDialogForm.Controls.Add(TOPlayerPanel);
            OK.Tag = Tuple.Create(PlayerNameTB, PlayerRatingTB, (sender as Control).Tag as Participant, UtilDialogForm);
            UtilDialogForm.Tag = OK;
            UtilDialogForm.Controls.Add(OK);
            UtilDialogForm.Controls.Add(Cancel);
            UtilDialogForm.ShowDialog(this);
        }
        void UtilDialogForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                (sender as Form).Close();
            else if (e.KeyCode == Keys.Enter)
                TOPlayerEdit_Button((sender as Form).Tag, new EventArgs());
        }
        private void TOPlayerEdit_Button(object sender, EventArgs e)
        {
            if ((sender as Control).Text == "Cancel")
                ((sender as Control).Tag as Form).Close();
            else
            {
                var tupleData = (sender as Control).Tag as Tuple<TextBox, TextBox, Participant, Form>;
                String buffer = "";
                int rating = 0;

                if (String.IsNullOrWhiteSpace(tupleData.Item1.Text))
                    buffer = "Player name cannot be empty. ";
                else if (tourneyInfo.Participants.Any(n => n != tupleData.Item3 &&
                    n.Name.CompareTo(tupleData.Item1.Text) == 0))
                    buffer = "A player with this name already exists. ";
                else if (!char.IsLetter(tupleData.Item1.Text[0]))
                    buffer = "Player name must begin with a letter. ";
                else if (tupleData.Item1.Text.Length < 3)
                    buffer = "Player name must be 3 or more characters. ";

                else if (!int.TryParse(tupleData.Item2.Text, out rating))
                    buffer = "Player rating must be a number. ";
                else if (rating < 1 || rating > 2800)
                    buffer = "Player rating must be greater than 0 and less than 2801. ";

                if (buffer != "")
                {
                    if (MessageBox.Show(buffer + "\nDo you want to return and discard changes?", "", 
                        MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1)
                        == System.Windows.Forms.DialogResult.Yes)
                        tupleData.Item4.Close();
                }
                else
                {
                    String str = tupleData.Item1.Text.Trim();
                    tupleData.Item3.Name = tupleData.Item1.Text.Substring(0, 1).ToUpper() + 
                        tupleData.Item1.Text.Substring(1).Trim().ToLower();
                    tupleData.Item3.FideRating = rating;
                    tupleData.Item4.Close();
                    GoToTourneyOptionsFinal();
                }
            }
        }
        private string TOOptTests(int min, int max, int mean)
        {
            var buffer = "";
            if (min >= max)
            {
                buffer = "Maximum rating must be greater than minimum rating";
                return buffer;
            }
            if (min + 100 > max)
            {
                buffer = "Maximum rating must exceed minimum rating by at least 100 points";
                return buffer;
            }
            if (mean <= min || mean >= max)
            {
                buffer = "Average rating must lie between minimum rating and maximum rating";
                return buffer;
            }
            if (min > mean - 100)
            {
                buffer = "Average rating should exceed minimum rating by at least 100 points";
                return buffer;
            }
            if (max < mean + 100)
            {
                buffer = "Maximum rating should exceed average rating by at least 100 points";
                return buffer;
            }
            return buffer;
        }
        void TOBack_Click(object sender, EventArgs e)
        {
            TOBack.Enabled = false;
            TOBack.FlatAppearance.BorderColor = Color.LightGray;
            TONext.Text = "Next";
            tourneyFormState = TourneyFormState.TourneyOptionsInitial;
            TourneyForm.Controls.Remove(TOFinalOptPanel);
            TourneyForm.Controls.Add(TOInitOptPanel);
        }
        void TOCancel_Click(object sender, EventArgs e)
        {
            
        }
        private string GetRatingDescription(int p)
        {
            if (p < 1200)
                return "Beginner";
            if (p < 1400)
                return "Novice";
            if (p < 1600)
                return "Intermediate";
            if (p < 1800)
                return "Club Player";
            if (p < 2000)
                return "Tournament Player";
            if (p < 2200)
                return "Expert";
            if (p <= 2300)
                return "National Master";
            if (p <= 2400)
                return "FIDE Master";
            if (p <= 2500)
                return "International Master";
            if (p <= 2600)
                return "Grandmaster";
            if (p <= 2700)
                return "Strong Grandmaster";
            return "Super Grandmaster";

        }
        private void TOtb_ValueChanged(object sender, EventArgs e)
        {
            var tb = sender as TrackBar;
            (tb.Tag as Label).Text = tb.Value.ToString() + 
                String.Format(" ({0})", GetRatingDescription(tb.Value));
        }
        public void InitializeTourneyFormForTable()
        {
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.TOlabelT = new System.Windows.Forms.Label();
            this.TONextRoundButton = new System.Windows.Forms.Button();
            this.TOPastResultsButton = new System.Windows.Forms.Button();
            this.TOWithdrawButton = new System.Windows.Forms.Button();
            this.TOEnterResultButton = new System.Windows.Forms.Button();
            this.TOReturnToTableButton = new System.Windows.Forms.Button();
            this.TOWhiteWinTick = new System.Windows.Forms.RadioButton();
            this.TOBlackWinTick = new System.Windows.Forms.RadioButton();
            this.TODrawTick = new System.Windows.Forms.RadioButton();
            this.TOResultInputButton = new System.Windows.Forms.Button();
            this.TOResultCancelButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            PrepareTable(tourneyInfo.Participants.Count);
            // 
            // labelT
            // 
            this.TOlabelT.AutoSize = true;
            this.TOlabelT.Font = new System.Drawing.Font("Maiandra GD", 15F, 
                System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TOlabelT.Location = new System.Drawing.Point(265, 12);
            this.TOlabelT.Name = "labelT";
            this.TOlabelT.Size = new System.Drawing.Size(59, 24);
            this.TOlabelT.TabIndex = 1;
            this.TOlabelT.Text = "labelT";
            this.TOlabelT.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // NextRoundButton
            // 
            this.TONextRoundButton.BackColor = System.Drawing.Color.Gray;
            this.TONextRoundButton.Font = new System.Drawing.Font("Comic Sans MS", 9.75F, 
                System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TONextRoundButton.Location = new System.Drawing.Point(710, 47);
            TONextRoundButton.Tag = TONextRoundButton.Location;
            this.TONextRoundButton.Name = "NextRoundButton";
            this.TONextRoundButton.Size = new System.Drawing.Size(74, 78);
            this.TONextRoundButton.TabIndex = 2;
            this.TONextRoundButton.Text = "Go to Next Round Pairing";
            this.TONextRoundButton.Cursor = Cursors.Hand;
            this.TONextRoundButton.UseVisualStyleBackColor = false;
            this.TONextRoundButton.Click += new System.EventHandler(this.NextRoundButton_Click);
            // 
            // PastResultsButton
            // 
            this.TOPastResultsButton.BackColor = System.Drawing.Color.Gray;
            this.TOPastResultsButton.Font = new System.Drawing.Font("Comic Sans MS", 9.75F, 
                System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TOPastResultsButton.Location = new System.Drawing.Point(710, 208);
            TOPastResultsButton.Tag = TOPastResultsButton.Location;
            this.TOPastResultsButton.Name = "PastResultsButton";
            this.TOPastResultsButton.Size = new System.Drawing.Size(74, 78);
            this.TOPastResultsButton.TabIndex = 3;
            this.TOPastResultsButton.Text = "See Past Results";
            this.TOPastResultsButton.Cursor = Cursors.Hand;
            this.TOPastResultsButton.UseVisualStyleBackColor = false;
            this.TOPastResultsButton.Click += new System.EventHandler(this.PastResultsButton_Click);
            // 
            // WithdrawButton
            // 
            this.TOWithdrawButton.BackColor = System.Drawing.Color.Gray;
            this.TOWithdrawButton.Font = new System.Drawing.Font("Comic Sans MS", 9.75F, 
                System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TOWithdrawButton.Location = new System.Drawing.Point(710, 367);
            TOWithdrawButton.Tag = TOWithdrawButton.Location;
            this.TOWithdrawButton.Name = "WithdrawButton";
            this.TOWithdrawButton.Size = new System.Drawing.Size(74, 70);
            this.TOWithdrawButton.TabIndex = 4;
            this.TOWithdrawButton.Text = "Withraw From Tourney";
            this.TOWithdrawButton.Cursor = Cursors.Hand;
            this.TOWithdrawButton.UseVisualStyleBackColor = false;
            this.TOWithdrawButton.Click += new System.EventHandler(this.WithdrawButton_Click);
            // 
            // EnterResultButton
            // 
            this.TOEnterResultButton.BackColor = System.Drawing.Color.DimGray;
            this.TOEnterResultButton.Enabled = false;
            this.TOEnterResultButton.Font = new System.Drawing.Font("Comic Sans MS", 11.25F, 
                System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TOEnterResultButton.Location = new System.Drawing.Point(0, 0);
            this.TOEnterResultButton.Name = "EnterResultButton";
            this.TOEnterResultButton.Size = new System.Drawing.Size(75, 70);
            this.TOEnterResultButton.TabIndex = 5;
            this.TOEnterResultButton.Text = "Play Now";
            this.TOEnterResultButton.Cursor = Cursors.Hand;
            this.TOEnterResultButton.UseVisualStyleBackColor = false;
            this.TOEnterResultButton.Visible = false;
            this.TOEnterResultButton.Click += new System.EventHandler(this.EnterResultButton_Click);
            // 
            // ReturnToTableButton
            // 
            this.TOReturnToTableButton.BackColor = System.Drawing.Color.DimGray;
            this.TOReturnToTableButton.Enabled = false;
            this.TOReturnToTableButton.Font = new System.Drawing.Font("Comic Sans MS", 11.25F, 
                System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TOReturnToTableButton.Location = new System.Drawing.Point(81, 0);
            this.TOReturnToTableButton.Name = "ReturnToTableButton";
            this.TOReturnToTableButton.Size = new System.Drawing.Size(75, 72);
            this.TOReturnToTableButton.TabIndex = 6;
            this.TOReturnToTableButton.Text = "Return To Table";
            this.TOReturnToTableButton.Cursor = Cursors.Hand;
            this.TOReturnToTableButton.UseVisualStyleBackColor = false;
            this.TOReturnToTableButton.Visible = false;
            this.TOReturnToTableButton.Click += new System.EventHandler(this.ReturnToTableButton_Click);
            // 
            // WhiteWinTick
            // 
            this.TOWhiteWinTick.AutoSize = true;
            this.TOWhiteWinTick.Enabled = false;
            this.TOWhiteWinTick.Location = new System.Drawing.Point(161, 16);
            this.TOWhiteWinTick.Name = "WhiteWinTick";
            this.TOWhiteWinTick.Size = new System.Drawing.Size(118, 17);
            this.TOWhiteWinTick.TabIndex = 7;
            this.TOWhiteWinTick.TabStop = true;
            this.TOWhiteWinTick.Text = "1 - 0 (White Victory)";
            this.TOWhiteWinTick.UseVisualStyleBackColor = true;
            this.TOWhiteWinTick.Visible = false;
            // 
            // BlackWinTick
            // 
            this.TOBlackWinTick.AutoSize = true;
            this.TOBlackWinTick.Enabled = false;
            this.TOBlackWinTick.Location = new System.Drawing.Point(362, 12);
            this.TOBlackWinTick.Name = "BlackWinTick";
            this.TOBlackWinTick.Size = new System.Drawing.Size(117, 17);
            this.TOBlackWinTick.TabIndex = 8;
            this.TOBlackWinTick.TabStop = true;
            this.TOBlackWinTick.Text = "0 - 1 (Black Victory)";
            this.TOBlackWinTick.UseVisualStyleBackColor = true;
            this.TOBlackWinTick.Visible = false;
            // 
            // DrawTick
            // 
            this.TODrawTick.AutoSize = true;
            this.TODrawTick.Enabled = false;
            this.TODrawTick.Location = new System.Drawing.Point(317, 29);
            this.TODrawTick.Name = "DrawTick";
            this.TODrawTick.Size = new System.Drawing.Size(50, 17);
            this.TODrawTick.TabIndex = 8;
            this.TODrawTick.TabStop = true;
            this.TODrawTick.Text = "Draw";
            this.TODrawTick.UseVisualStyleBackColor = true;
            this.TODrawTick.Visible = false;
            // 
            // ResultInputButton
            // 
            this.TOResultInputButton.BackColor = System.Drawing.Color.DimGray;
            this.TOResultInputButton.Enabled = false;
            this.TOResultInputButton.Font = new System.Drawing.Font("Comic Sans MS", 12F, 
                System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TOResultInputButton.Location = new System.Drawing.Point(581, 119);
            this.TOResultInputButton.Name = "ResultInputButton";
            this.TOResultInputButton.Size = new System.Drawing.Size(64, 43);
            this.TOResultInputButton.TabIndex = 9;
            this.TOResultInputButton.Text = "Enter";
            this.TOResultInputButton.Cursor = Cursors.Hand;
            this.TOResultInputButton.UseVisualStyleBackColor = false;
            this.TOResultInputButton.Visible = false;
            this.TOResultInputButton.Click += new System.EventHandler(this.OutcomeSelectionButton_Click);
            // 
            // ResultCancelButton
            // 
            this.TOResultCancelButton.BackColor = System.Drawing.Color.DimGray;
            this.TOResultCancelButton.Enabled = false;
            this.TOResultCancelButton.Font = new System.Drawing.Font("Comic Sans MS", 12F, 
                System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TOResultCancelButton.Location = new System.Drawing.Point(613, 119);
            this.TOResultCancelButton.Name = "ResultCancelButton";
            this.TOResultCancelButton.Size = new System.Drawing.Size(65, 43);
            this.TOResultCancelButton.TabIndex = 10;
            this.TOResultCancelButton.Text = "Cancel";
            this.TOResultCancelButton.UseVisualStyleBackColor = false;
            this.TOResultCancelButton.Visible = false;
            this.TOResultCancelButton.Click += new System.EventHandler(this.ResultCancelButton_Click);
            // 
            // TourneyForm
            // 
            TourneyForm.Controls.Add(TOBlackWinTick);
            TourneyForm.Controls.Add(TOlabelT);
            TourneyForm.Controls.Add(TOResultCancelButton);
            TourneyForm.Controls.Add(TOResultInputButton);
            TourneyForm.Controls.Add(TODrawTick);
            TourneyForm.Controls.Add(TOEnterResultButton);
            TourneyForm.Controls.Add(TOWhiteWinTick);
            TourneyForm.Controls.Add(TOReturnToTableButton);
            TourneyForm.Controls.Add(TONextRoundButton);
            TourneyForm.Controls.Add(TOWithdrawButton);
            TourneyForm.Controls.Add(TOPastResultsButton);
            TourneyForm.Controls.Add(tableLayoutPanel1);
        }
        private void PrepareTable(int playerCount)
        {
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.OutsetPartial;
            this.tableLayoutPanel1.ColumnCount = 5;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle
                (System.Windows.Forms.SizeType.Absolute, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle
                (System.Windows.Forms.SizeType.Absolute, 180F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle
                (System.Windows.Forms.SizeType.Absolute, 124F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle
                (System.Windows.Forms.SizeType.Absolute, 116F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle
                (System.Windows.Forms.SizeType.Absolute, 148F));
            this.tableLayoutPanel1.Location = new System.Drawing.Point(27, 47);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = playerCount + 1;
            for (int i = 0; i < tableLayoutPanel1.RowCount; i++)
            {
                this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle
                (System.Windows.Forms.SizeType.Absolute, 
                (tourneyInfo.Participants.Count == 4 ? 240 : 320) / tourneyInfo.Participants.Count));
            }
            this.tableLayoutPanel1.Size = new System.Drawing.Size(566, 300);
            this.tableLayoutPanel1.TabIndex = 0;
            this.tableLayoutPanel1.Visible = false;
        }
        private void ShowParticipant(Participant participant)
        {
            int x;
            Font font = new System.Drawing.Font("Candara", 13F, FontStyle.Regular);
            TOtempLabel = new Label();
            TOtempLabel.Text = participant.Ranking.ToString();
            TOtempLabel.TextAlign = ContentAlignment.MiddleCenter;
            TOtempLabel.Font = font;
            TOtempLabel.AutoSize = false;
            TOtempLabel.Size = new Size((int)tableLayoutPanel1.ColumnStyles[0].Width,
                (int)tableLayoutPanel1.RowStyles[0].Height);
            Template[0] = TOtempLabel;

            TOtempLabel = new Label();
            TOtempLabel.Text = participant.Name;
            TOtempLabel.TextAlign = ContentAlignment.MiddleCenter;
            TOtempLabel.Font = font;
            TOtempLabel.AutoSize = false;
            TOtempLabel.Size = new Size((int)tableLayoutPanel1.ColumnStyles[1].Width,
                (int)tableLayoutPanel1.RowStyles[0].Height);
            Template[1] = TOtempLabel;

            TOtempLabel = new Label();
            TOtempLabel.Text = participant.FideRating.ToString();
            TOtempLabel.TextAlign = ContentAlignment.MiddleCenter;
            TOtempLabel.Font = font;
            TOtempLabel.AutoSize = false;
            TOtempLabel.Size = new Size((int)tableLayoutPanel1.ColumnStyles[2].Width,
                (int)tableLayoutPanel1.RowStyles[0].Height);
            Template[2] = TOtempLabel;

            TOtempLabel = new Label();
            TOtempLabel.Text = participant.Score.ToString();
            TOtempLabel.TextAlign = ContentAlignment.MiddleCenter;
            TOtempLabel.Font = font;
            TOtempLabel.AutoSize = false;
            TOtempLabel.Size = new Size((int)tableLayoutPanel1.ColumnStyles[3].Width,
                (int)tableLayoutPanel1.RowStyles[0].Height);
            Template[3] = TOtempLabel;

            TOtempLabel = new Label();
            x = (int)participant.PerfRating;
            TOtempLabel.Text = x.ToString();
            TOtempLabel.TextAlign = ContentAlignment.MiddleCenter;
            TOtempLabel.Font = font;
            TOtempLabel.AutoSize = false;
            TOtempLabel.Size = new Size((int)tableLayoutPanel1.ColumnStyles[4].Width,
                (int)tableLayoutPanel1.RowStyles[0].Height);
            Template[4] = TOtempLabel;

            tableLayoutPanel1.Controls.AddRange(Template);
        }
        private void InitializeParticipants()
        {
            var ratingList = getRatings(tourneyInfo.Participants.Count, tourneyInfo.MinRating,
                tourneyInfo.MaxRating, tourneyInfo.AverageRating);

            tourneyInfo.Participants = new List<Participant>();
            tourneyInfo.Participants.Add(tourneyInfo.UserParticipant);
            foreach (var item in ratingList)
            {
                if (item != tourneyInfo.UserParticipant.FideRating)
                    tourneyInfo.Participants.Add(new Participant() { FideRating = item });
            }
            tourneyInfo.Participants = tourneyInfo.Participants;
            GetFullProfiles();
        }
        private List<int> getRatings(int playerCount, int min, int max, int average)
        {
            var baseNumbers = Enumerable.Range(0, playerCount).
                Select(x => rnd.NextDouble()).ToList();
            var baseAvg = baseNumbers.Average();
            var baseMax = baseNumbers.Max();
            var baseMin = baseNumbers.Min();

            // Scale them out to the specified values
            var results = baseNumbers.Select(num =>
                (int)(average - baseAvg + (num - baseAvg) *
                Math.Min((max - average) / (baseMax - baseAvg), (average - min) / (baseAvg - baseMin)))).ToList();
            var resultAvg = results.Average();

            // This gets rid of floating point rounding errors
            while (resultAvg < average)
            {
                for (int i = 0; i < results.Count; i++)
                {
                    if (results[i] < max)
                    {
                        results[i]++;
                        break;
                    }
                }

                resultAvg = results.Average();
            }
            while (resultAvg > average)
            {
                for (int i = 0; i < results.Count; i++)
                {
                    if (results[i] > min)
                    {
                        results[i]--;
                        break;
                    }
                }

                resultAvg = results.Average();
            }

            List<int> incList = new List<int>(), decList = new List<int>();
            foreach (var item in results)
            {
                if (item < min)
                    incList.Add(item);
                else if (item > max)
                    decList.Add(item);
            }
            results = results.Select(n => (incList.Contains(n) ? n + 1 : n)).ToList();
            results = results.Select(n => (decList.Contains(n) ? n - 1 : n)).ToList();
            results = results.Select(n => (n == results.Min() ? n + decList.Count : n)).ToList();
            results = results.Select(n => (n == results.Max() ? n - incList.Count : n)).ToList();


            var userSwap = (int)results.Select(n => Math.Abs(n - tourneyInfo.UserParticipant.FideRating)).Min();
            userSwap = results.First(n => Math.Abs(n - tourneyInfo.UserParticipant.FideRating) == userSwap);
            results = results.Select(n => (int)(n == userSwap ? tourneyInfo.UserParticipant.FideRating : n)).ToList();
            results.OrderBy(n => n);
            var userInverse = results.First(n => n != tourneyInfo.UserParticipant.FideRating &&
                (results.IndexOf(n) == results.Count / 2 || results.IndexOf(n) == (results.Count / 2 + 1)));
            results = results.Select(n => (int)(n == userInverse ? n + userSwap - 
                tourneyInfo.UserParticipant.FideRating : n)).ToList();

            return results;
        }
        private void GetFullProfiles()
        {
            var nameList = new List<String>();
            nameList.Add("Allison"); nameList.Add("Anthony"); nameList.Add("Benjamin"); nameList.Add("Buchi");
            nameList.Add("Charles"); nameList.Add("Clinton"); nameList.Add("Dorothy"); nameList.Add("David");
            nameList.Add("Emeka"); nameList.Add("Emmanuel"); nameList.Add("Francis"); nameList.Add("Felicia");
            nameList.Add("Gary"); nameList.Add("Godfrey"); nameList.Add("Harrison"); nameList.Add("Haruna");
            nameList.Add("Ikenna"); nameList.Add("Irina"); nameList.Add("Johnson"); nameList.Add("Judith");
            nameList.Add("Kelvin"); nameList.Add("Kingsley"); nameList.Add("Luke"); nameList.Add("Luwin");
            nameList.Add("Madison"); nameList.Add("Mary"); nameList.Add("Nadia"); nameList.Add("Nikita");
            nameList.Add("Okiemute"); nameList.Add("Oliver"); nameList.Add("Peter"); nameList.Add("Philip");
            nameList.Add("Roland"); nameList.Add("Richard"); nameList.Add("Susan"); nameList.Add("Stephen");
            nameList.Add("Terry"); nameList.Add("Theophilus"); nameList.Add("Uchenna"); nameList.Add("Hikaru");
            nameList.Add("Vivian"); nameList.Add("Vincent"); nameList.Add("William"); nameList.Add("Winston");
            nameList.Add("Anatoly"); nameList.Add("Wesley"); nameList.Add("Gregory"); nameList.Add("Sam");
            nameList.Add("Michael"); nameList.Add("Abiodun"); nameList.Add("Azeez"); nameList.Add("Olisa");

            foreach (var item in tourneyInfo.Participants)
            {
                if (item != tourneyInfo.UserParticipant)
                {
                    while (true)
                    {
                        var tempName = nameList[rnd.Next(0, nameList.Count - 1)];
                        if (String.Compare(tempName, tourneyInfo.UserParticipant.Name, true) != 0)
                        {
                            item.Name = tempName;
                            nameList.Remove(tempName);
                            break;
                        }
                    }
                }
            }
        }
        private void CreateTitles()
        {
            Font font = new System.Drawing.Font("Candara", 13F, FontStyle.Bold);
            TOtempLabel = new Label();
            TOtempLabel.Text = "RANKING";
            TOtempLabel.TextAlign = ContentAlignment.MiddleCenter;
            TOtempLabel.AutoSize = false;
            TOtempLabel.Size = new Size((int)tableLayoutPanel1.ColumnStyles[0].Width,
                (int)tableLayoutPanel1.RowStyles[0].Height);
            TOtempLabel.Font = font;
            Template[0] = TOtempLabel;

            TOtempLabel = new Label();
            TOtempLabel.Text = "NAME";
            TOtempLabel.TextAlign = ContentAlignment.MiddleCenter;
            TOtempLabel.Font = font;
            TOtempLabel.AutoSize = false;
            TOtempLabel.Size = new Size((int)tableLayoutPanel1.ColumnStyles[1].Width,
                (int)tableLayoutPanel1.RowStyles[0].Height);
            Template[1] = TOtempLabel;

            TOtempLabel = new Label();
            TOtempLabel.Text = "ELO RATING";
            TOtempLabel.TextAlign = ContentAlignment.MiddleCenter;
            TOtempLabel.Font = font;
            TOtempLabel.AutoSize = false;
            TOtempLabel.Size = new Size((int)tableLayoutPanel1.ColumnStyles[2].Width,
                (int)tableLayoutPanel1.RowStyles[0].Height);
            Template[2] = TOtempLabel;

            TOtempLabel = new Label();
            TOtempLabel.Text = "SCORE";
            TOtempLabel.TextAlign = ContentAlignment.MiddleCenter;
            TOtempLabel.Font = font;
            TOtempLabel.AutoSize = false;
            TOtempLabel.Size = new Size((int)tableLayoutPanel1.ColumnStyles[3].Width,
                (int)tableLayoutPanel1.RowStyles[0].Height);
            Template[3] = TOtempLabel;

            TOtempLabel = new Label();
            TOtempLabel.Text = "PERF. RATING";
            TOtempLabel.TextAlign = ContentAlignment.MiddleCenter;
            TOtempLabel.Font = font;
            TOtempLabel.AutoSize = false;
            TOtempLabel.Size = new Size((int)tableLayoutPanel1.ColumnStyles[4].Width,
                (int)tableLayoutPanel1.RowStyles[0].Height);
            Template[4] = TOtempLabel;

            tableLayoutPanel1.Controls.AddRange(Template);
        }
        private void NextRoundButton_Click(object sender, EventArgs e)
        {
            //Include support for swiss type pairing
            if (!tourneyInfo.IsPairingReady && tourneyInfo.Type == "Round Robin")
            {
                if (!tourneyInfo.IsTourneyCompleted)
                {
                    tourneyInfo.CurrentRound++;
                    if (tourneyInfo.TotalFixtureList == null)
                        GenerateFixtures(tourneyInfo.Participants.Count);
                    DoPairings();
                }
                else
                {
                    MessageBox.Show("Tourney is Over!");
                    return;
                }

            }
            DisplayPairings();
        }
        private void DisplayPairings()
        {
            tourneyFormState = TourneyFormState.Pairings;
            tableLayoutPanel1.Visible = false;
            tableLayoutPanel1.Enabled = false;
            TOWithdrawButton.Enabled = false;
            TOWithdrawButton.Visible = false;
            TONextRoundButton.Enabled = false;
            TONextRoundButton.Visible = false;
            TOPastResultsButton.Enabled = false;
            TOPastResultsButton.Visible = false;
            TOlabelT.Text = "ROUND " + tourneyInfo.CurrentRound.ToString() + " PAIRINGS";

            if (OutcomeSelectionLabel != null)
                OutcomeSelectionLabel.Visible = false;
            TOResultInputButton.Enabled = false;
            TOResultInputButton.Visible = false;
            TOResultCancelButton.Enabled = false;
            TOResultCancelButton.Visible = false;
            TOWhiteWinTick.Enabled = false;
            TOBlackWinTick.Enabled = false;
            TODrawTick.Enabled = false;
            TOWhiteWinTick.Visible = false;
            TOBlackWinTick.Visible = false;
            TODrawTick.Visible = false;

            TourneyForm.Controls.Remove(TOLinkLabel);
            TourneyForm.Controls.Remove(LinkTextBox);

            FixtureLabels = new Label[tourneyInfo.CurrentRoundFixtures.Count];
            for (int i = 0; i < tourneyInfo.CurrentRoundFixtures.Count; i++)
            {
                FixtureLabels[i] = new Label();
                FixtureLabels[i].Text = (i + 1).ToString() + ".  " + tourneyInfo.CurrentRoundFixtures[i].ToString();
                FixtureLabels[i].Font = new System.Drawing.Font("Comic Sans", 12F, FontStyle.Regular,
                                            System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                FixtureLabels[i].AutoSize = true;
                //FixtureLabels[i].BackColor = System.Drawing.Color.Gray;
                FixtureLabels[i].Padding = new System.Windows.Forms.Padding(10);
                TourneyForm.Controls.Add(FixtureLabels[i]);
            }
            for (int i = 0, j = 80; i < tourneyInfo.CurrentRoundFixtures.Count; i++, j += 50)
                FixtureLabels[i].Location = new Point(350 - FixtureLabels[i].Width / 2, j);
            TOEnterResultButton.Location = new Point(650, 70);
            TOEnterResultButton.Enabled = true;
            TOEnterResultButton.Visible = true;

            TOReturnToTableButton.Location = new Point(650, 200);
            TOReturnToTableButton.Enabled = true;
            TOReturnToTableButton.Visible = true;
        }
        private void DoPairings()
        {
            List<Fixture> temp = new List<Fixture>();
            if (tourneyInfo.CurrentRoundFixtures == null)
                tourneyInfo.CurrentRoundFixtures = new List<Fixture>();
            tourneyInfo.CurrentRoundFixtures.Clear();
            foreach (var item in tourneyInfo.TotalFixtureList)
                if (item.RoundOfFixture == tourneyInfo.CurrentRound)
                    temp.Add(item);

            foreach (var item in temp)
                tourneyInfo.TotalFixtureList.Remove(item);

            var playerList = tourneyInfo.Participants.OrderBy(n => n.Ranking).ToList();
            foreach (var item in playerList)
            {
                var fixture = temp.Find(n => n.BlackSide == item || n.WhiteSide == item);
                if (fixture != null)
                {
                    tourneyInfo.CurrentRoundFixtures.Add(fixture);
                    temp.Remove(fixture);
                }
            }

            //int j, length = temp.Count;
            //for (int i = 0; i < length; i++)
            //{
            //    j = rnd.Next(temp.Count);
            //    CurrentRoundFixtures.Add(temp[j]);
            //    temp.RemoveAt(j);
            //}

            tourneyInfo.IsPairingReady = true;
            Save();
        }
        private void Save()
        {
            if (!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                    + @"\Lambda Tourney Pairer"))
                Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                    + @"\Lambda Tourney Pairer");
            using (Stream writer = File.Create
                (Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                    + @"\Lambda Tourney Pairer\Tourney Data.dat"))
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(writer, tourneyInfo);
            }
        }
        private void TOLoadFromFile()
        {
            if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                + @"\Lambda Tourney Pairer\Tourney Data.dat"))
            {
                using (Stream reader =
                    File.OpenRead(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                    + @"\Lambda Tourney Pairer\Tourney Data.dat"))
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    tourneyInfo = (TourneyInfo)bf.Deserialize(reader);
                }
            }
            else
            {
                tourneyInfo = new TourneyInfo();
                tourneyInfo.AverageRating = CurrentUser != null ? CurrentUser.Rating + 50 : 1550;
                tourneyInfo.MinRating = CurrentUser != null ? CurrentUser.Rating - 200 : 1300;
                tourneyInfo.MaxRating = CurrentUser != null ? CurrentUser.Rating + 300 : 1800;
                tourneyInfo.TimeControl = new TimeControl()
                {
                    MainTime = new TimeSpan(1, 30, 0),
                    MainIncrement = 30,
                    TillEndOfGame = true
                };
                tourneyInfo.Participants = new List<Participant>();
                for (int i = 0; i < 8; i++)
                    tourneyInfo.Participants.Add(new Participant());
                tourneyInfo.Type = "Round Robin";
                tourneyInfo.TotalRounds = 14;
                tourneyInfo.UserParticipant = new Participant() 
                    { Name = CurrentUser != null ? CurrentUser.Name : "" };
            }
        }
        private bool CheckCompletedFixtures(Fixture fixture)
        {
            foreach (Fixture item in tourneyInfo.CompletedFixtures)
            {
                if (fixture.WhiteSide == item.BlackSide && fixture.BlackSide == item.WhiteSide)
                {
                    return true;
                }
            }
            return false;
        }
        private void GenerateFixtures(int playerCount)
        {
            tourneyInfo.TotalFixtureList = new List<Fixture>();
            int i = 1;

            #region OldProcess
            switch (playerCount)
            {
                case 4:
                    PairThese(1, 2, i); PairThese(3, 4, i); i++;
                    PairThese(3, 1, i); PairThese(2, 4, i); i++;
                    PairThese(1, 4, i); PairThese(3, 2, i); i++;
                    break;
                case 6:
                    PairThese(1, 2, i); PairThese(3, 4, i); PairThese(5, 6, i); i++;
                    PairThese(3, 1, i); PairThese(2, 5, i); PairThese(4, 6, i); i++;
                    PairThese(1, 4, i); PairThese(2, 6, i); PairThese(3, 5, i); i++;
                    PairThese(5, 1, i); PairThese(4, 2, i); PairThese(3, 6, i); i++;
                    PairThese(6, 1, i); PairThese(3, 2, i); PairThese(4, 5, i); i++;
                    break;
                case 8:
                    PairThese(1, 2, i); PairThese(3, 4, i); PairThese(5, 8, i); PairThese(6, 7, i); i++;
                    PairThese(3, 1, i); PairThese(2, 4, i); PairThese(5, 7, i); PairThese(6, 8, i); i++;
                    PairThese(1, 4, i); PairThese(2, 5, i); PairThese(3, 6, i); PairThese(7, 8, i); i++;
                    PairThese(5, 1, i); PairThese(2, 6, i); PairThese(3, 7, i); PairThese(4, 8, i); i++;
                    PairThese(1, 6, i); PairThese(2, 7, i); PairThese(3, 8, i); PairThese(4, 5, i); i++;
                    PairThese(7, 1, i); PairThese(2, 8, i); PairThese(3, 5, i); PairThese(4, 6, i); i++;
                    PairThese(1, 8, i); PairThese(2, 3, i); PairThese(4, 7, i); PairThese(5, 6, i); i++;
                    break;
                case 10:
                    PairThese(1, 2, i); PairThese(3, 4, i); PairThese(5, 6, i); PairThese(7, 8, i); PairThese(9, 10, i); i++;
                    PairThese(3, 1, i); PairThese(2, 4, i); PairThese(7, 5, i); PairThese(6, 9, i); PairThese(8, 10, i); i++;
                    PairThese(1, 4, i); PairThese(3, 2, i); PairThese(5, 8, i); PairThese(7, 9, i); PairThese(6, 10, i); i++;
                    PairThese(5, 1, i); PairThese(6, 2, i); PairThese(3, 7, i); PairThese(4, 10, i); PairThese(8, 9, i); i++;
                    PairThese(1, 6, i); PairThese(5, 2, i); PairThese(3, 8, i); PairThese(4, 9, i); PairThese(7, 10, i); i++;
                    PairThese(7, 1, i); PairThese(8, 2, i); PairThese(9, 3, i); PairThese(4, 6, i); PairThese(5, 10, i); i++;
                    PairThese(1, 8, i); PairThese(10, 2, i); PairThese(3, 6, i); PairThese(4, 7, i); PairThese(5, 9, i); i++;
                    PairThese(9, 1, i); PairThese(7, 2, i); PairThese(10, 3, i); PairThese(4, 5, i); PairThese(6, 8, i); i++;
                    PairThese(10, 1, i); PairThese(9, 2, i); PairThese(5, 3, i); PairThese(4, 8, i); PairThese(6, 7, i); i++;

                    break;
                default:
                    MessageBox.Show("Error initializing tourney!");
                    break;
            }
            #endregion

            #region New Try
            /*
             *             
            for (int x = 1; x <= playerCount; x++)
            {
                for (int y = playerCount; y > x; y--)
                {
                    PairThese(x, y, i);
                }
            }

            var tupleList = new List<Tuple<Participant, Piece.PieceSide>>();
            foreach (var item in TourneyParticipants)
                tupleList.Add(Tuple.Create(item, Piece.PieceSide.White));
            for (int round = 1; round < playerCount; round++)
            {

            }

             **/
            #endregion

            List<Fixture> tempHalf = new List<Fixture>();
            int j = 0;
            foreach (Fixture item in tourneyInfo.TotalFixtureList)
            {
                j++;
                Fixture temp = new Fixture();
                temp.WhiteSide = item.BlackSide;
                temp.BlackSide = item.WhiteSide;
                temp.RoundOfFixture = i;
                tempHalf.Add(temp);
                if (j % (playerCount / 2) == 0)
                    i++;
            }
            tourneyInfo.TotalFixtureList.AddRange(tempHalf);
        }
        private void PairThese(int p1, int p2, int round)
        {
            Fixture temp = new Fixture();
            temp.WhiteSide = tourneyInfo.Participants[p1 - 1];
            temp.BlackSide = tourneyInfo.Participants[p2 - 1];
            temp.RoundOfFixture = round;
            if (p1 == p2 || tourneyInfo.TotalFixtureList.Contains(temp))
            {
                MessageBox.Show("Error! \nCode: 067");
                return;
            }
            tourneyInfo.TotalFixtureList.Add(temp);
        }
        private void EnterResultButton_Click(object sender, EventArgs e)
        {
            EnterResults();
            //Start new game with correct settings
        }
        private void EnterResults()
        {
            tourneyFormState = TourneyFormState.ResultsEntering;
            for (int i = 0; i < FixtureLabels.Count<Label>(); i++)
            {
                FixtureLabels[i].Visible = false;
            }
            TOEnterResultButton.Enabled = false;
            TOEnterResultButton.Visible = false;
            TOReturnToTableButton.Enabled = false;
            TOReturnToTableButton.Visible = false;
            TOlabelT.Text = "";

            foreach (Fixture item in tourneyInfo.CurrentRoundFixtures)
            {
                if (item.BlackSide == tourneyInfo.UserParticipant || item.WhiteSide == tourneyInfo.UserParticipant)
                {
                    tourneyInfo.CurrentUserFixture = item;
                }
            }
            if (OutcomeSelectionLabel == null)
            {
                OutcomeSelectionLabel = new Label();
                TourneyForm.Controls.Add(OutcomeSelectionLabel);
            }
            OutcomeSelectionLabel.Text = "Please, select current round's result and click Enter";
            OutcomeSelectionLabel.AutoSize = true;
            OutcomeSelectionLabel.Visible = true;
            OutcomeSelectionLabel.Location = new Point(50, 50);

            TOLinkLabel.Text =
                @"Optionally, You can include the link to the game's pgn in the box below
            and then, click Enter. You can also paste the moves to generate the pgn";
            TOLinkLabel.Location = new Point(300, 70);
            TOLinkLabel.AutoSize = true;
            TourneyForm.Controls.Add(TOLinkLabel);
            LinkTextBox.Size = new System.Drawing.Size(250, 120);
            LinkTextBox.Multiline = true;
            LinkTextBox.Location = new Point(350, 110);
            TourneyForm.Controls.Add(LinkTextBox);

            TOWhiteWinTick.Location = new Point(80, 80);
            TOBlackWinTick.Location = new Point(80, 120);
            TODrawTick.Location = new Point(80, 160);
            TOWhiteWinTick.Text = "1 - 0  (" + tourneyInfo.CurrentUserFixture.WhiteSide.Name + " won as White)";
            TOBlackWinTick.Text = "0 - 1  (" + tourneyInfo.CurrentUserFixture.BlackSide.Name + " won as Black)";

            TOWhiteWinTick.Checked = false;
            TOWhiteWinTick.Enabled = true;
            TOWhiteWinTick.Visible = true;
            TOBlackWinTick.Enabled = true;
            TOBlackWinTick.Checked = false;
            TOBlackWinTick.Visible = true;
            TODrawTick.Enabled = true;
            TODrawTick.Visible = true;
            TODrawTick.Checked = false;

            TOResultInputButton.Location = new Point(180, 200);
            TOResultInputButton.Enabled = true;
            TOResultInputButton.Visible = true;

            TOResultCancelButton.Location = new Point(80, 200);
            TOResultCancelButton.Enabled = true;
            TOResultCancelButton.Visible = true;
        }
        private void OutcomeSelectionButton_Click(object sender, EventArgs e)
        {
            if (TOWhiteWinTick.Checked)
            {
                tourneyInfo.CurrentUserFixture.gameOutcome = Fixture.GameOutcome.WhiteWin;
                tourneyInfo.CurrentUserFixture.WhiteSide.Score++;
                tourneyInfo.CurrentUserFixture.WhiteSide.SumOfOpponentsRatings += tourneyInfo.CurrentUserFixture.BlackSide.FideRating;
                tourneyInfo.CurrentUserFixture.BlackSide.SumOfOpponentsRatings += tourneyInfo.CurrentUserFixture.WhiteSide.FideRating;
                tourneyInfo.CurrentUserFixture.WhiteSide.TotalGamesPlayed++;
                tourneyInfo.CurrentUserFixture.BlackSide.TotalGamesPlayed++;
                tourneyInfo.CurrentUserFixture.WhiteSide.FHFactor++;
                tourneyInfo.CurrentUserFixture.BlackSide.FHFactor--;
            }

            else if (TOBlackWinTick.Checked)
            {
                tourneyInfo.CurrentUserFixture.gameOutcome = Fixture.GameOutcome.BlackWin;
                tourneyInfo.CurrentUserFixture.BlackSide.Score++;
                tourneyInfo.CurrentUserFixture.BlackSide.SumOfOpponentsRatings += tourneyInfo.CurrentUserFixture.WhiteSide.FideRating;
                tourneyInfo.CurrentUserFixture.WhiteSide.SumOfOpponentsRatings += tourneyInfo.CurrentUserFixture.BlackSide.FideRating;
                tourneyInfo.CurrentUserFixture.BlackSide.TotalGamesPlayed++;
                tourneyInfo.CurrentUserFixture.WhiteSide.TotalGamesPlayed++;
                tourneyInfo.CurrentUserFixture.BlackSide.FHFactor++;
                tourneyInfo.CurrentUserFixture.WhiteSide.FHFactor--;
            }

            else if (TODrawTick.Checked)
            {
                tourneyInfo.CurrentUserFixture.gameOutcome = Fixture.GameOutcome.Draw;
                tourneyInfo.CurrentUserFixture.WhiteSide.Score += 0.5;
                tourneyInfo.CurrentUserFixture.WhiteSide.SumOfOpponentsRatings += tourneyInfo.CurrentUserFixture.BlackSide.FideRating;
                tourneyInfo.CurrentUserFixture.WhiteSide.TotalGamesPlayed++;

                tourneyInfo.CurrentUserFixture.BlackSide.Score += 0.5;
                tourneyInfo.CurrentUserFixture.BlackSide.SumOfOpponentsRatings += tourneyInfo.CurrentUserFixture.WhiteSide.FideRating;
                tourneyInfo.CurrentUserFixture.BlackSide.TotalGamesPlayed++;
            }

            else return;
            foreach (Fixture item in tourneyInfo.CurrentRoundFixtures)
                item.DateAndTimePlayed = DateTime.Now;
            if (LinkTextBox.Text != "")
            {
                SetGameLink();
            }
            tourneyInfo.IsPairingReady = false;
            tourneyInfo.CompletedFixtures.AddRange(tourneyInfo.CurrentRoundFixtures);
            GenerateAutoResults();
            GetPerformanceRatings();
            if (tourneyInfo.CurrentRound == tourneyInfo.TotalRounds)
                tourneyInfo.IsTourneyCompleted = true;
            Save();
            GoToTable();
        }
        private void SetGameLink()
        {
            String TextBuffer = LinkTextBox.Text;
            String LinkName = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                + @"\Lambda Tourney Pairer\" + tourneyInfo.Name
                + "\\" + tourneyInfo.CurrentUserFixture.WhiteSide.Name + "_"
                + tourneyInfo.CurrentUserFixture.BlackSide.Name + " "
                + Normalize(tourneyInfo.CurrentUserFixture.gameOutcome) + " "
                + tourneyInfo.CurrentUserFixture.DateAndTimePlayed.ToShortDateString() + ".pgn";
            if (File.Exists(TextBuffer))
                tourneyInfo.CurrentUserFixture.GameLink = TextBuffer;
            else
            {
                Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                    + @"\Lambda Tourney Pairer\" + tourneyInfo.Name);
                using (StreamWriter writer = new StreamWriter(LinkName))
                {
                    //  CHANGE THIS LATER!!
                    writer.WriteLine("[Event \"" + tourneyInfo.Name + " with Tourney Pairer\"]");
                    writer.WriteLine("[Site \"?\"]");
                    writer.WriteLine("[Date \"" + tourneyInfo.CurrentUserFixture.DateAndTimePlayed.Year + "."
                        + tourneyInfo.CurrentUserFixture.DateAndTimePlayed.Month + "."
                        + tourneyInfo.CurrentUserFixture.DateAndTimePlayed.Day + "\"]");
                    writer.WriteLine("[Round \"{0}\"]", tourneyInfo.UserParticipant.TotalGamesPlayed.ToString());
                    writer.WriteLine("[White \"{0}\"]", tourneyInfo.CurrentUserFixture.WhiteSide.Name);
                    writer.WriteLine("[Black \"{0}\"]", tourneyInfo.CurrentUserFixture.BlackSide.Name);
                    writer.WriteLine("[WhiteElo \"{0}\"]",
                        tourneyInfo.CurrentUserFixture.WhiteSide.FideRating.ToString());
                    writer.WriteLine("[BlackElo \"{0}\"]",
                        tourneyInfo.CurrentUserFixture.BlackSide.FideRating.ToString());
                    writer.WriteLine("[Result \"{0}\"]", Normalize(tourneyInfo.CurrentUserFixture.gameOutcome));
                    String temp;
                    temp = String.Copy(TextBuffer);
                    if (temp.Contains(']'))
                        TextBuffer = temp.Remove(0, (TextBuffer.LastIndexOf(']') + 1));
                    writer.Write(TextBuffer);
                }
                tourneyInfo.CurrentUserFixture.GameLink = LinkName;
            }
        }
        private String Normalize(Fixture.GameOutcome gameOutcome)
        {
            switch (gameOutcome)
            {
                case Fixture.GameOutcome.NotAvailable: return "?";
                case Fixture.GameOutcome.WhiteWin: return "1-0";
                case Fixture.GameOutcome.BlackWin: return "0-1";
                case Fixture.GameOutcome.Draw: return "draw";
                default: return "";
            }
        }
        private void GetPerformanceRatings()
        {
            foreach (Fixture fixture in tourneyInfo.CurrentRoundFixtures)
            {
                fixture.WhiteSide.PerfRating = (fixture.WhiteSide.SumOfOpponentsRatings
                    + (fixture.WhiteSide.FHFactor * 400)) / fixture.WhiteSide.TotalGamesPlayed;
                fixture.BlackSide.PerfRating = (fixture.BlackSide.SumOfOpponentsRatings
                    + (fixture.BlackSide.FHFactor * 400)) / fixture.BlackSide.TotalGamesPlayed;
            }
        }
        private void GenerateAutoResults()
        {
            int WBRatingOffset, seed;
            foreach (Fixture item in tourneyInfo.CurrentRoundFixtures)
            {
                if (item != tourneyInfo.CurrentUserFixture)
                {
                    WBRatingOffset = (int)(item.WhiteSide.FideRating - item.BlackSide.FideRating);

                    if (WBRatingOffset <= 50 && WBRatingOffset > -50)       // -49 to 50
                    {
                        seed = rnd.Next(100);
                        if (seed < 35)
                            item.gameOutcome = Fixture.GameOutcome.WhiteWin;
                        else if (seed < 60)
                            item.gameOutcome = Fixture.GameOutcome.BlackWin;
                        else if (seed < 100)
                            item.gameOutcome = Fixture.GameOutcome.Draw;
                    }
                    else if (WBRatingOffset <= 150 && WBRatingOffset > 50)     //  51 - 150
                    {
                        seed = rnd.Next(100);
                        if (seed < 60)
                            item.gameOutcome = Fixture.GameOutcome.WhiteWin;
                        else if (seed < 90)
                            item.gameOutcome = Fixture.GameOutcome.Draw;
                        else if (seed < 100)
                            item.gameOutcome = Fixture.GameOutcome.BlackWin;
                    }

                    else if (WBRatingOffset <= 250 && WBRatingOffset > 150)     //  151 to 250
                    {
                        seed = rnd.Next(100);
                        if (seed < 80)
                            item.gameOutcome = Fixture.GameOutcome.WhiteWin;
                        else if (seed < 95)
                            item.gameOutcome = Fixture.GameOutcome.Draw;
                        else if (seed < 100)
                            item.gameOutcome = Fixture.GameOutcome.BlackWin;
                    }
                    else if (WBRatingOffset <= 350 && WBRatingOffset > 250)     //  251 to 350
                    {
                        seed = rnd.Next(100);
                        if (seed < 90)
                            item.gameOutcome = Fixture.GameOutcome.WhiteWin;
                        else if (seed < 97)
                            item.gameOutcome = Fixture.GameOutcome.Draw;
                        else if (seed < 100)
                            item.gameOutcome = Fixture.GameOutcome.BlackWin;
                    }
                    else if (WBRatingOffset > 350)     //  351 upwards
                    {
                        seed = rnd.Next(100);
                        if (seed < 97)
                            item.gameOutcome = Fixture.GameOutcome.WhiteWin;
                        else if (seed < 98)
                            item.gameOutcome = Fixture.GameOutcome.Draw;
                        else if (seed < 100)
                            item.gameOutcome = Fixture.GameOutcome.BlackWin;
                    }
                    else if (WBRatingOffset > -150 && WBRatingOffset <= -50)     //  -149 to -50
                    {
                        seed = rnd.Next(100);
                        if (seed < 70)
                            item.gameOutcome = Fixture.GameOutcome.BlackWin;
                        else if (seed < 90)
                            item.gameOutcome = Fixture.GameOutcome.Draw;
                        else if (seed < 100)
                            item.gameOutcome = Fixture.GameOutcome.WhiteWin;
                    }
                    else if (WBRatingOffset > -250 && WBRatingOffset <= -150)     //  -249 to -150
                    {
                        seed = rnd.Next(100);
                        if (seed < 85)
                            item.gameOutcome = Fixture.GameOutcome.BlackWin;
                        else if (seed < 95)
                            item.gameOutcome = Fixture.GameOutcome.Draw;
                        else if (seed < 100)
                            item.gameOutcome = Fixture.GameOutcome.WhiteWin;
                    }
                    else if (WBRatingOffset > -350 && WBRatingOffset <= -250)     //  -349 to -250
                    {
                        seed = rnd.Next(100);
                        if (seed < 93)
                            item.gameOutcome = Fixture.GameOutcome.BlackWin;
                        else if (seed < 97)
                            item.gameOutcome = Fixture.GameOutcome.Draw;
                        else if (seed < 100)
                            item.gameOutcome = Fixture.GameOutcome.WhiteWin;
                    }
                    else if (WBRatingOffset <= -350)     //  -351 downwards
                    {
                        seed = rnd.Next(100);
                        if (seed < 97)
                            item.gameOutcome = Fixture.GameOutcome.BlackWin;
                        else if (seed < 98)
                            item.gameOutcome = Fixture.GameOutcome.Draw;
                        else if (seed < 100)
                            item.gameOutcome = Fixture.GameOutcome.WhiteWin;
                    }

                    if (item.gameOutcome == Fixture.GameOutcome.WhiteWin)
                    {
                        item.WhiteSide.Score++;
                        item.WhiteSide.FHFactor++;
                        item.BlackSide.FHFactor--;
                        item.WhiteSide.TotalGamesPlayed++;
                        item.BlackSide.TotalGamesPlayed++;
                        item.WhiteSide.SumOfOpponentsRatings += item.BlackSide.FideRating;
                        item.BlackSide.SumOfOpponentsRatings += item.WhiteSide.FideRating;
                    }
                    else if (item.gameOutcome == Fixture.GameOutcome.BlackWin)
                    {
                        item.BlackSide.Score++;
                        item.BlackSide.FHFactor++;
                        item.WhiteSide.FHFactor--;
                        item.BlackSide.TotalGamesPlayed++;
                        item.WhiteSide.TotalGamesPlayed++;
                        item.BlackSide.SumOfOpponentsRatings += item.WhiteSide.FideRating;
                        item.WhiteSide.SumOfOpponentsRatings += item.BlackSide.FideRating;
                    }
                    else if (item.gameOutcome == Fixture.GameOutcome.Draw)
                    {
                        item.WhiteSide.Score += 0.5;
                        item.BlackSide.Score += 0.5;
                        item.BlackSide.SumOfOpponentsRatings += item.WhiteSide.FideRating;
                        item.WhiteSide.SumOfOpponentsRatings += item.BlackSide.FideRating;
                        item.BlackSide.TotalGamesPlayed++;
                        item.WhiteSide.TotalGamesPlayed++;
                    }
                }
            }
        }
        private void GoToTable()
        {
            tourneyFormState = TourneyFormState.Table;
            TOPastResultsButton.Text = "See Past Results";
            if (tourneyInfo.IsTourneyCompleted)
                TOlabelT.Text = " FINAL STANDINGS";
            else if (tourneyInfo.Participants[0].TotalGamesPlayed > 0)
                TOlabelT.Text = "STANDINGS AFTER ROUND "
                    + tourneyInfo.Participants[0].TotalGamesPlayed.ToString();
            else
                TOlabelT.Text = " INITIAL STANDINGS";
            tableLayoutPanel1.Controls.Clear();
            CreateTitles();

            if (OutcomeSelectionLabel != null)
                OutcomeSelectionLabel.Visible = false;

            LinkTextBox.Text = "";
            TourneyForm.Controls.Remove(LinkTextBox);
            TourneyForm.Controls.Remove(TOLinkLabel);
            TOResultInputButton.Enabled = false;
            TOResultInputButton.Visible = false;
            TOResultCancelButton.Enabled = false;
            TOResultCancelButton.Visible = false;
            TOWhiteWinTick.Enabled = false;
            TOBlackWinTick.Enabled = false;
            TODrawTick.Enabled = false;
            TOWhiteWinTick.Visible = false;
            TOBlackWinTick.Visible = false;
            TODrawTick.Visible = false;


            if (FixtureLabels != null)
            {
                for (int i = 0; i < FixtureLabels.Count<Label>(); i++)
                {
                    FixtureLabels[i].Visible = false;
                }
            }
            TOEnterResultButton.Enabled = false;
            TOEnterResultButton.Visible = false;
            TOReturnToTableButton.Enabled = false;
            TOReturnToTableButton.Visible = false;

            int j = 1;
            List<Participant> temp = new List<Participant>();
            temp.AddRange(tourneyInfo.Participants);
            temp.Sort();
            foreach (Participant item in temp)
            {
                item.Ranking = j;
                ShowParticipant(item);
                j++;
            }

            TONextRoundButton.Location = (Point)TONextRoundButton.Tag;
            TOPastResultsButton.Location = (Point)TOPastResultsButton.Tag;
            TOWithdrawButton.Location = (Point)TOWithdrawButton.Tag;

            tableLayoutPanel1.Visible = true;
            tableLayoutPanel1.Enabled = true;
            tableLayoutPanel1.Location = new Point(15, 47);
            TOWithdrawButton.Enabled = true;
            TOWithdrawButton.Visible = true;
            TONextRoundButton.Enabled = true;
            TONextRoundButton.Visible = true;
            TOPastResultsButton.Enabled = true;
            TOPastResultsButton.Visible = true;
        }
        private void WithdrawButton_Click(object sender, EventArgs e)
        {

        }

        //  INSERT "LOADING" ANIMATION WHILE TABLE INITIALIZES
        private void ReturnToTableButton_Click(object sender, EventArgs e)
        {
            tableLayoutPanel1.SuspendLayout();
            GoToTable();
            tableLayoutPanel1.ResumeLayout();
        }
        private void ResultCancelButton_Click(object sender, EventArgs e)
        {
            DisplayPairings();
        }
        private void PastResultsButton_Click(object sender, EventArgs e)
        {
            TourneyForm.AutoScroll = true;
            if (tourneyFormState != TourneyFormState.PastResults)
            {
                if (tourneyFormState == TourneyFormState.Table)
                {
                    TOPastResultsButton.Left -= 10;
                    TOWithdrawButton.Left -= 10;
                }
                tourneyFormState = TourneyFormState.PastResults;
                tableLayoutPanel1.Enabled = false;
                tableLayoutPanel1.Visible = false;
                TONextRoundButton.Enabled = false;
                TONextRoundButton.Visible = false;
                TOlabelT.Text = "";
                TOPastResultsButton.Text = "Return To Table";

                if (tourneyInfo.CompletedFixtures.Count == 0)
                {
                    if (OutcomeSelectionLabel == null)
                        OutcomeSelectionLabel = new Label();

                    if (!TourneyForm.Controls.Contains(OutcomeSelectionLabel))
                        TourneyForm.Controls.Add(OutcomeSelectionLabel);
                    OutcomeSelectionLabel.Text =
                        "There are no results to display. No games have been played";
                    OutcomeSelectionLabel.AutoSize = true;
                    OutcomeSelectionLabel.Visible = true;
                    OutcomeSelectionLabel.Location = new Point(200, 50);
                    return;
                }
                tourneyInfo.CompletedFixtures.Reverse();
                int x, j = 0;
                PastFixtureLabels = new Label[tourneyInfo.CompletedFixtures.Count + 1];
                PastRoundLabels = new Label[tourneyInfo.CurrentRound];
                PastRoundInfoLabels = new Label[tourneyInfo.CurrentRound];
                for (int i = 0, y = 0; i < tourneyInfo.CompletedFixtures.Count + 1; i++, y += 30)
                {
                    if (i == tourneyInfo.CompletedFixtures.Count)
                    {
                        PastFixtureLabels[i] = new Label();
                        PastFixtureLabels[i].AutoSize = true;
                        PastFixtureLabels[i].Text = "    ";
                        PastFixtureLabels[i].Padding = new Padding(15);
                        x = (TourneyForm.DisplayRectangle.Width / 2) - (PastFixtureLabels[i].Width / 2);
                        PastFixtureLabels[i].Location = new Point
                            (x - 100, y);
                        PastFixtureLabels[i].Font = new Font("Candara", 10F, FontStyle.Bold);
                        TourneyForm.Controls.Add(PastFixtureLabels[i]);
                        tourneyInfo.CompletedFixtures.Reverse();
                        return;
                    }
                    if (i % (tourneyInfo.Participants.Count / 2) == 0)
                    {

                        y += 50;
                        PastRoundLabels[j] = new Label();
                        PastRoundLabels[j].Text = "ROUND " +
                            (!tourneyInfo.IsPairingReady ? (tourneyInfo.CurrentRound - j).ToString() :
                            (tourneyInfo.CurrentRound - 1 - j).ToString());
                        x = (TourneyForm.DisplayRectangle.Width / 2) - (PastRoundLabels[j].Width / 2);
                        PastRoundLabels[j].Font = new Font("Candara", 18F, FontStyle.Bold);
                        PastRoundLabels[j].AutoSize = true;
                        PastRoundLabels[j].Location = new Point(x - 10, y);
                        TourneyForm.Controls.Add(PastRoundLabels[j]);
                        y += 35;

                        PastRoundInfoLabels[j] = new Label();
                        PastRoundInfoLabels[j].Text = "Played on "
                            + tourneyInfo.CompletedFixtures[i].DateAndTimePlayed.ToLongDateString() + ". "
                            + (tourneyInfo.CompletedFixtures[i].DateAndTimePlayed.Hour < 12 ?
                            (tourneyInfo.CompletedFixtures[i].DateAndTimePlayed.Hour == 0 ?
                            "12 AM" : (tourneyInfo.CompletedFixtures[i].DateAndTimePlayed.Hour.ToString() + " AM")) :
                            (tourneyInfo.CompletedFixtures[i].DateAndTimePlayed.Hour == 12 ?
                            "12 PM" : ((tourneyInfo.CompletedFixtures[i].DateAndTimePlayed.Hour - 12).ToString()
                            + " PM")));
                        x = (TourneyForm.DisplayRectangle.Width / 2) - (PastRoundInfoLabels[j].Width / 2);
                        PastRoundInfoLabels[j].Font = new Font("Candara", 10F, FontStyle.Italic);
                        PastRoundInfoLabels[j].AutoSize = true;
                        PastRoundInfoLabels[j].Location = new Point(x - 65, y);
                        TourneyForm.Controls.Add(PastRoundInfoLabels[j]);
                        j++;
                        y += 30;
                    }
                    PastFixtureLabels[i] = new Label();
                    PastFixtureLabels[i].AutoSize = true;
                    PastFixtureLabels[i].Text = tourneyInfo.CompletedFixtures[i].ToString();
                    PastFixtureLabels[i].Padding = new Padding(15);
                    x = (TourneyForm.DisplayRectangle.Width / 2) - (PastFixtureLabels[i].Width / 2);
                    PastFixtureLabels[i].Location = new Point
                        (x - 100, y);
                    PastFixtureLabels[i].Font = new Font("Candara", 10F, FontStyle.Bold);
                    PastFixtureLabels[i].Click += new EventHandler(Results_Click);
                    TourneyForm.Controls.Add(PastFixtureLabels[i]);
                }
                TourneyForm.HorizontalScroll.Visible = false;
                TourneyForm.HorizontalScroll.Enabled = false;
                tourneyInfo.CompletedFixtures.Reverse();
            }
            else
            {
                if (PastFixtureLabels != null)
                    for (int i = 0; i < tourneyInfo.CompletedFixtures.Count + 1; i++)
                    {
                        TourneyForm.Controls.Remove(PastFixtureLabels[i]);
                        PastFixtureLabels[i].Dispose();
                    }
                if (PastRoundLabels != null)
                    for (int i = 0; i < tourneyInfo.Participants[0].TotalGamesPlayed; i++)
                    {
                        TourneyForm.Controls.Remove(PastRoundLabels[i]);
                        PastRoundLabels[i].Dispose();
                    }
                if (PastRoundInfoLabels != null)
                    for (int i = 0; i < tourneyInfo.Participants[0].TotalGamesPlayed; i++)
                    {
                        TourneyForm.Controls.Remove(PastRoundInfoLabels[i]);
                        PastRoundInfoLabels[i].Dispose();
                    }
                TourneyForm.AutoScroll = false;
                TOPastResultsButton.Location = new Point(588, 168);
                TOWithdrawButton.Location = new Point(588, 277);
                GoToTable();
            }
        }
        private void Results_Click(object sender, EventArgs e)
        {
            Label temp = sender as Label;
            foreach (var item in tourneyInfo.CompletedFixtures)
            {
                if ((item.WhiteSide == tourneyInfo.UserParticipant ||
                    item.BlackSide == tourneyInfo.UserParticipant) &&
                    item.ToString() == temp.Text)
                {
                    if (File.Exists(item.GameLink))
                    {
                        //if (File.Exists
                        //    (@"C:\Program Files (x86)\ShredderChess\Shredder Classic 4\Shredder.exe"))
                        //    System.Diagnostics.Process.Start
                        //     (@"C:\Program Files (x86)\ShredderChess\Shredder Classic 4\Shredder.exe",
                        //     item.GameLink);
                    }
                }
            }
        }
        private void Form1_Scroll(object sender, ScrollEventArgs e)
        {
            int y1 = TOPastResultsButton.Location.Y + (e.NewValue - e.OldValue);
            int y2 = TOWithdrawButton.Location.Y + (e.NewValue - e.OldValue);
            TOPastResultsButton.Location = new Point(TOPastResultsButton.Location.X, y1);
            TOWithdrawButton.Location = new Point(TOWithdrawButton.Location.X, y2);
        }
        [Serializable]
        public class Participant : IComparable<Participant>
        {
            public String Name { get; set; }
            public double FideRating { get; set; }
            public double Score { get; set; }
            public int Ranking { get; set; }
            public double PerfRating { get; set; }
            public double SumOfOpponentsRatings { get; set; }
            public int TotalGamesPlayed { get; set; }
            public int FHFactor { get; set; }
            public int CompareTo(Participant other)
            {
                if (this.Score < other.Score)
                    return 1;
                else if (this.Score > other.Score)
                    return -1;
                else
                {
                    if (this.PerfRating < other.PerfRating)
                        return 1;
                    else if (this.PerfRating > other.PerfRating)
                        return -1;
                    else
                    {
                        if (this.FideRating < other.FideRating)
                            return 1;
                        else if (this.FideRating > other.FideRating)
                            return -1;
                        else
                            return 0;
                    }

                }
            }

            public override string ToString()
            {
                return this.Name + " (" + this.FideRating + ")";
            }
        }
        [Serializable]
        public class Fixture: IEquatable<Fixture>
        {
            public enum GameOutcome
            {
                NotAvailable,       // CHECK IF ENUM IS NULLABLE
                WhiteWin,
                BlackWin,
                Draw,
            }

            public bool Equals(Fixture other)
            {
                return (this.WhiteSide == other.WhiteSide && this.BlackSide == other.BlackSide);
            }

            public override bool Equals(object obj)
            {
                if (obj is Fixture)
                {
                    return Equals((Fixture)obj);
                }
                return false;
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();          // LOOK UP GETTING HASHCODES FOR PROPERTIES
            }

            public override string ToString()
            {
                if (gameOutcome == GameOutcome.NotAvailable)
                    return WhiteSide.Name + " (" + WhiteSide.FideRating + ")"
                        + "        vs        " + BlackSide.Name + " (" + BlackSide.FideRating + ")";
                else if (gameOutcome == GameOutcome.BlackWin)
                    return WhiteSide.Name + " (" + WhiteSide.FideRating + ")"
                        + "       0 - 1       " + BlackSide.Name + " (" + BlackSide.FideRating + ")";
                else if (gameOutcome == GameOutcome.WhiteWin)
                    return WhiteSide.Name + " (" + WhiteSide.FideRating + ")"
                        + "       1 - 0       " + BlackSide.Name + " (" + BlackSide.FideRating + ")";
                else
                    return WhiteSide.Name + " (" + WhiteSide.FideRating + ")"
                        + "   1/2 - 1/2    " + BlackSide.Name + " (" + BlackSide.FideRating + ")";
            }

            public Participant WhiteSide { get; set; }
            public Participant BlackSide { get; set; }
            public bool HasBeenPlayed { get; set; }
            public GameOutcome gameOutcome { get; set; }
            public int RoundOfFixture { get; set; }
            public DateTime DateAndTimePlayed { get; set; }
            public String GameLink { get; set; }
        }
        [Serializable]
        public class TourneyInfo
        {
            public TourneyInfo()
            {
                BackUp = new TourneyInfo("hack");
                BackUp.AverageRating = CurrentUser != null ? CurrentUser.Rating + 50 : 1550;
                BackUp.MinRating = CurrentUser != null ? CurrentUser.Rating - 200 : 1300;
                BackUp.MaxRating = CurrentUser != null ? CurrentUser.Rating + 300 : 1800;
            }
            public TourneyInfo(object hack)
            {

            }
            public int CurrentRound { get; set; }
            public List<Participant> Participants { get; set; }
            public Participant UserParticipant { get; set; }
            public List<Fixture> TotalFixtureList { get; set; }
            public List<Fixture> CurrentRoundFixtures { get; set; }
            public List<Fixture> CompletedFixtures { get; set; }
            public bool IsPairingReady { get; set; }
            public bool IsTourneyCompleted { get; set; }
            public String Name { get; set; }
            public String Type { get; set; }
            public int TotalRounds { get; set; }
            public Fixture CurrentUserFixture { get; set; }
            public int MinRating { get; set; }
            public int MaxRating { get; set; }
            public int AverageRating { get; set; }
            public TimeControl TimeControl { get; set; }
            public TourneyInfo BackUp { get; set; }
            public void CreateBackUp()
            {
                BackUp = new TourneyInfo();
                BackUp.AverageRating = AverageRating;
                BackUp.MaxRating = MaxRating;
                BackUp.MinRating = MinRating;
            }
        }
    }
}