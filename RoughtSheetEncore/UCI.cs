using System;
using System.Diagnostics;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System.IO;
using System.Drawing.Drawing2D;
using System.Drawing;
using System.Runtime.Serialization.Formatters.Binary;

namespace RoughtSheetEncore
{
    public partial class Form1: Form
    {

        #region Declarations

        static List<Engine> InstalledEngines, LoadedEngines;
        System.Windows.Forms.Timer PVTimer;
        System.Windows.Forms.Timer ArrowTimer;
        static Engine FirstEngine;
        Engine SecondEngine;
        Arrow BestMoveArrow;
        static readonly object uciLocker = new object();
        bool isInfiniteSearch = false, isEngineMatchInProgress = false, ShouldHighlightCheckedKing;
        String FENforStartPos, DefaultENGPath;
        Form OptionsENGForm, ENGRegForm;
        #endregion

        private void SetupUCI()
        {
            AnalyzeButton2.Click += AnalyzeButton_Click;
            GoButton2.Click += GoButton_Click;
            NameButton2.Click += NameButton_Click;

            RunOtherUtilities();

            DefaultENGPath = "stockfish_8_x64.exe";

            try
            {
                BinaryFormatter bf = new BinaryFormatter();
                if (File.Exists(@"Engines\Installed Engines.dat"))
                    using (Stream input = File.OpenRead(@"Engines\Installed Engines.dat"))
                        InstalledEngines = bf.Deserialize(input) as List<Engine>;
            }
            catch (Exception)
            {
                InstalledEngines = null;
            }
            
            if (InstalledEngines == null)
                InstalledEngines = new List<Engine>();

            LoadedEngines = new List<Engine>();

            if (!InstalledEngines.Any(n => n.Path == DefaultENGPath))
            {
                InstallEngine(DefaultENGPath);
                InstallEngine("komodo-9-64bit.exe");    // remove later
            }

            InstalledEngines = InstalledEngines.Where(n => File.Exists(n.Path)).OrderBy(n => n.Name).ToList();
            if (FirstEngine != null && !InstalledEngines.Contains(FirstEngine))
            {
                FirstEngine = InstalledEngines.Find(n => n.Path == DefaultENGPath);
                if (FirstEngine != null && FirstEngine.Equals(SecondEngine))
                {
                    SecondEngine = null;
                    Remove2ndENGPanel();
                }
            }
            if (SecondEngine != null && !InstalledEngines.Contains(SecondEngine))
            { 
                SecondEngine = null;
                Remove2ndENGPanel();
            }

            if (FirstEngine == null && InstalledEngines.Any(n => n.Path == DefaultENGPath))
            {
                LoadEngine(InstalledEngines.Find(item => item.Path == DefaultENGPath), true);
            }
            else if (InstalledEngines.Contains(FirstEngine))
            {
                LoadEngine(FirstEngine, true);
                if (SecondEngine != null && InstalledEngines.Contains(SecondEngine))
                    LoadEngine(SecondEngine, false);
            }
            BestMoveArrow = new Arrow();
            PVTimer = new System.Windows.Forms.Timer();
            ArrowTimer = new System.Windows.Forms.Timer();
            ArrowTimer.Tick += ArrowTimer_Tick;
            ArrowTimer.Interval = 50;
            PVTimer.Interval = 1000;
            PVTimer.Enabled = false;
            PVTimer.Tick += PVTimer_Tick;

            if (SecondEngine == null)
                Remove2ndENGPanel();
        }
        private void Remove2ndENGPanel()
        {
            EnginePanel1.Height = EnginePanel.Height;
            EnginePanel2.Visible = false;
        }
        private void Add2ndENGPanel()
        {
            EnginePanel1.Height = EnginePanel.Height - EnginePanel2.Height - 8;
            EnginePanel2.Visible = true;
        }
        void ArrowTimer_Tick(object sender, EventArgs e)
        {
            ArrowTimer.Stop();
            BestMoveArrow.Enabled = true;
            if (isInfiniteSearch && !BestMoveArrow.IsInvalid)
                UpdateArrow(false);
        }
        void BGWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            EngineLine line;
            if (e.UserState is Tuple<Engine, Move>)
            {
                var tupleData = e.UserState as Tuple<Engine, Move>;
                if (tupleData.Item1.ShouldIgnore || ModeOfPlay == PlayMode.EditPosition)
                    return;
                UpdateArrow(true);
                MakeMove(tupleData.Item2, panel1.CreateGraphics(), false, this);
            }
            #region Engine Initialization
            if (e.UserState is Engine)
            {
                Engine engine = e.UserState as Engine;
                engine.WaitOutTimer.Stop();
                engine.WaitOutTimer.Enabled = false;
                engine.isUciEngine = true;
                Cursor = Cursors.Default;

                if (!InstalledEngines.Contains(engine) && engine.State == EngineState.Installing)
                {
                    if (MessageBox.Show("Proceed with Installing\n" + engine.Name + "\nby " + engine.Author,
                        "Confirm Installation", MessageBoxButtons.OKCancel, MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button1) == System.Windows.Forms.DialogResult.OK)
                    {
                        InstalledEngines.Add(engine);

                        if (LoadedEngines.Count < 1)        //rethink
                            LoadEngine(engine, true);
                    }
                    if (engine != FirstEngine && engine != SecondEngine)
                        try
                        {
                            engine.Process.Kill();
                            engine.State = EngineState.Idle;
                        }
                        catch (Exception)
                        {

                        }
                    return;
                }
                else if (engine.State == EngineState.Installing)
                {
                    String activeName = "", realName = "";
                    int index = 1, x, y;
                    activeName = engine.Name.Trim();
                    x = activeName.IndexOf("(");
                    y = activeName.IndexOf(")");
                    if (x > 0 && y > 0)
                        realName = activeName.Substring(0, x);
                    else
                        realName = activeName;

                    while (true)
                    {
                        activeName = realName + String.Format(" ({0})", index);
                        if (!InstalledEngines.Any(n => n.Name == activeName))
                            break;
                        else
                            index++;
                    }
                    if (MessageBox.Show("The engine, \"" + engine.Name.Trim() + "\" is already installed. Click \"OK\" to"
                        + " install it as \"" + activeName + "\"", "Engine already exists", MessageBoxButtons.OKCancel,
                        MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button1) == System.Windows.Forms.DialogResult.OK)
                    {
                        engine.Name = activeName;
                        InstalledEngines.Add(engine);
                    }
                    return;
                }
                engine.NameButton.Text = engine.Name;
                toolTip1.SetToolTip(engine.NameButton, engine.Name + " is currently loaded. \nClick to change");
                engine.ComboBox.Visible = false;
                engine.NameButton.Visible = true;
                if (!LoadedEngines.Contains(engine))
                    LoadedEngines.Add(engine);
                engine.State = EngineState.Idle;

                #region setoption support
                foreach (var item in engine.Options)
                {
                    if (item.Name == "Hash" && item.Type == EngineOption.OptionType.Spin)
                    {
                        engine._Hash = true;
                        item.ShouldDisplay = false;
                    }
                    else if (item.Name == "Ponder" && item.Type == EngineOption.OptionType.Check)
                    {
                        item.ShouldDisplay = false;
                        engine._Ponder = true;
                    }
                    else if (item.Name == "OwnBook" && item.Type == EngineOption.OptionType.Check)
                    {
                        item.ShouldDisplay = false;
                        engine._OwnBook = true;
                    }
                    else if (item.Name == "MultiPV" && item.Type == EngineOption.OptionType.Spin)
                    {
                        item.ShouldDisplay = false;
                        engine._MultiPV = true;
                    }
                    else if (item.Name == "UCI_LimitStrength" && item.Type == EngineOption.OptionType.Check)
                    {
                        item.ShouldDisplay = false;
                        engine._UCI_LimitStrength = true;
                    }
                    else if (item.Name == "UCI_Elo" && item.Type == EngineOption.OptionType.Spin)
                    {
                        item.ShouldDisplay = false;
                        engine._UCI_Elo = true;
                    }
                    else if (item.Name == "UCI_AnalyseMode" && item.Type == EngineOption.OptionType.Check)
                    {
                        item.ShouldDisplay = false;
                        engine._UCI_AnalyseMode = true;
                    }
                    else if (item.Name == "UCI_EngineAbout" && item.Type == EngineOption.OptionType.String)
                    {
                        item.ShouldDisplay = false;
                        engine._UCI_EngineAbout = true;
                    }
                    else if (item.Name == "Skill Level" && item.Type == EngineOption.OptionType.Spin)
                    {
                        item.ShouldDisplay = false;
                        engine._SkillLevel = true;
                    }
                    else if (item.Name == "Threads" && item.Type == EngineOption.OptionType.Spin)
                    {
                        item.ShouldDisplay = false;
                        engine._Threads = true;
                    }
                    else if (item.Name.Contains("UCI_"))
                        item.ShouldDisplay = false;
                }
                #endregion

                engine.Process.StandardInput.WriteLine("ucinewgame");
                if (engine._UCI_AnalyseMode)
                    engine.Process.StandardInput.WriteLine("setoption name UCI_AnalyseMode value true");
                if (gameDetails == null && FirstEngine.isUciEngine && (SecondEngine == null || SecondEngine.isUciEngine))
                {
                    if (gameDetails == null)
                        SetGameDetails(CurrentUser, FirstEngine, new GameDetails());
                }
                ShowGameDetails();
            }
            #endregion
            else if (e.UserState is Tuple<Engine, String>)
            {
                var tupleData = e.UserState as Tuple<Engine, String>;
                if (tupleData.Item2 == "arrow" && e.ProgressPercentage == 1 && (tupleData.Item1 == FirstEngine ||
                    !FirstEngine.isAnalyzing || isEngineMatchInProgress))
                {
                    if (!IsClicked)
                        BestMoveArrow.Enabled = true;
                    UpdateArrow(false);
                }
                else if (tupleData.Item2 == "registerok")
                {
                    if (ENGRegForm.Visible)
                    {
                        ENGRegForm.Close();
                        MessageBox.Show(tupleData.Item1.Name + " is successfully registered"); 
                    }
                }
                else if (tupleData.Item2 == "register")
                {
                    if (ENGRegForm.Visible)
                    {
                        MessageBox.Show("Registration Error! Username or password is incorrect");
                        return;
                    }
                    if (tupleData.Item1.shouldNotReg)
                    {
                        tupleData.Item1.Process.StandardInput.WriteLine("register later");
                    }
                    else
                    {
                        ShowENGRegForm(tupleData.Item1);
                        return;
                    }
                }
                else if (tupleData.Item2 == "copyprotection")
                {
                    InstalledEngines.Remove(tupleData.Item1);
                    if (LoadedEngines.Contains(tupleData.Item1))
                    {
                        MessageBox.Show("Copy protection test has failed for the engine, " + tupleData.Item1.Name
                                        + ". It will now exit");
                        CloseEngine(tupleData.Item1); 
                    }
                }
            }
            else if (e.UserState is Tuple<Engine, String, decimal>)
            {
                var tupleData = e.UserState as Tuple<Engine, String, decimal>;

                #region CentiPawn
                if (tupleData.Item2 == "cp")
                {
                    decimal d;
                    if (e.ProgressPercentage != 1)
                    {
                        if (tupleData.Item1.OtherLines == null || 
                            e.ProgressPercentage > tupleData.Item1.OtherLines.Count + 1)
                            return;
                        line = tupleData.Item1.OtherLines[e.ProgressPercentage - 2];
                        line.Evaluation = "";
                        d = tupleData.Item3 / 100;
                        if (d > 0 && sideToPlay == Piece.PieceSide.White ||
                            d < 0 && sideToPlay == Piece.PieceSide.Black)
                            line.Evaluation = "+";
                        if (sideToPlay == Piece.PieceSide.White)
                            line.Evaluation += String.Format("{0:0.00}", d);
                        else
                            line.Evaluation += String.Format("{0:0.00}", -d);
                        line.Label.Text = line.Depth + "      " + line.AnalysisTime + "      " + line.Evaluation
                            + "      " + line.PV;
                        return;
                    }
                    line = tupleData.Item1.MainLine;
                    line.Evaluation = "";
                    d = tupleData.Item3 / 100;
                    if (d > 0 && sideToPlay == Piece.PieceSide.White || 
                        d < 0 && sideToPlay == Piece.PieceSide.Black)
                        line.Evaluation = "+";
                    if (sideToPlay == Piece.PieceSide.White)
                        line.Evaluation += String.Format("{0:0.00}", d);
                    else
                        line.Evaluation += String.Format("{0:0.00}", -d);
                    tupleData.Item1.EvalLabel.Text = line.Evaluation;
                    line.Label.Text = line.Depth + "      " + line.AnalysisTime + "      " + line.Evaluation
                            + "      " + line.PV;

                    double x = double.Parse(tupleData.Item1.EvalLabel.Text);
                    tupleData.Item1.EvalDescriptLabel.Text = tupleData.Item1.EvalLabel.Text 
                        + " means that ";
                    if (x == 0)
                    {
                        tupleData.Item1.EvalDescriptLabel.Text += "the position is even";
                    }
                    else if (Math.Abs(x) > 0.2)
                    {
                        tupleData.Item1.EvalDescriptLabel.Text += (x > 0 ? "White" : "Black")
                            +" has ";
                        if (Math.Abs(x) < 0.6)
                            tupleData.Item1.EvalDescriptLabel.Text += "a slight edge";
                        else if (Math.Abs(x) < 2)
                            tupleData.Item1.EvalDescriptLabel.Text += "the upper hand";
                        else if (Math.Abs(x) < 5)
                            tupleData.Item1.EvalDescriptLabel.Text += "a decisive avantage";
                        else
                            tupleData.Item1.EvalDescriptLabel.Text += "a winning position. " 
                                +(x < 0 ? "White" : "Black") +" can resign";
                    }
                    else
                    {
                        tupleData.Item1.EvalDescriptLabel.Text += "the position is roughly even";
                    }
                }
            #endregion

                else if (tupleData.Item2 == "mate")
                {
                    decimal d;
                    if (tupleData.Item3 == 0)
                    {
                        tupleData.Item1.EvalDescriptLabel.Text = "Check Mate";
                        tupleData.Item1.EvalLabel.Text = "";
                        tupleData.Item1.DepthLabel.Text = "";
                        tupleData.Item1.PVLabel.Text = "";
                        UpdateArrow(true);
                        return;
                    }
                    if (e.ProgressPercentage != 1)
                    {
                        if (tupleData.Item1.OtherLines == null ||
                            e.ProgressPercentage > tupleData.Item1.OtherLines.Count + 1)
                            return;
                        line = tupleData.Item1.OtherLines[e.ProgressPercentage - 2];
                        d = (sideToPlay == Piece.PieceSide.White ?
                        tupleData.Item3 : -1 * tupleData.Item3);
                        line.Evaluation = "#" + d;
                        line.Label.Text = line.Depth + "      " + line.AnalysisTime + "      " + line.Evaluation
                            + "      " + line.PV;
                        return;
                    }

                    d = (sideToPlay == Piece.PieceSide.White ?
                        tupleData.Item3 : -1 * tupleData.Item3);

                    line = tupleData.Item1.MainLine;
                    tupleData.Item1.EvalLabel.Text = "#" +d;
                    line.Evaluation = "#" + d;
                    line.Label.Text = line.Depth + "      " + line.AnalysisTime + "      " + line.Evaluation
                            + "      " + line.PV;

                    if (d < 0)
                        tupleData.Item1.EvalDescriptLabel.Text = tupleData.Item1.EvalLabel.Text 
                        + " means that Black can Mate in " + Math.Abs(d);
                    else
                        tupleData.Item1.EvalDescriptLabel.Text = tupleData.Item1.EvalLabel.Text
                        + " means that White can Mate in " + Math.Abs(d);
                }
            }
            else if (e.UserState is Tuple<Engine, String, int>)
            {
                var tupleData = e.UserState as Tuple<Engine, String, int>;
                if (tupleData.Item2 == "depth")
                {
                    if (e.ProgressPercentage != 1)
                    {
                        if (tupleData.Item1.OtherLines == null ||
                            e.ProgressPercentage > tupleData.Item1.OtherLines.Count + 1)
                            return;
                        line = tupleData.Item1.OtherLines[e.ProgressPercentage - 2];
                        line.Depth = tupleData.Item3.ToString();
                        line.Label.Text = line.Depth + "      " + line.AnalysisTime + "      " + line.Evaluation
                            + "      " + line.PV;
                        return;
                    }
                    line = tupleData.Item1.MainLine;
                    tupleData.Item1.DepthLabel.Text = "Depth " + tupleData.Item3.ToString();
                    line.Depth = tupleData.Item3.ToString();
                    line.Label.Text = line.Depth + "      " + line.AnalysisTime + "      " + line.Evaluation
                            + "      " + line.PV;
                }
                else if (tupleData.Item2 == "time")
                {
                    TimeSpan ts = new TimeSpan(0, 0, 0, 0, tupleData.Item3);
                    if (e.ProgressPercentage != 1)
                    {
                        if (tupleData.Item1.OtherLines == null ||
                            e.ProgressPercentage > tupleData.Item1.OtherLines.Count + 1)
                            return;
                        line = tupleData.Item1.OtherLines[e.ProgressPercentage - 2];
                        line.AnalysisTime = String.Format("{0:00}", ts.Minutes + ts.Hours * 60 + ts.Days * 24 * 60);
                        line.AnalysisTime += ":" + String.Format("{0:00}", ts.Seconds);
                        line.Label.Text = line.Depth + "      " + line.AnalysisTime + "      " + line.Evaluation
                            + "      " + line.PV;
                        return;
                    }
                    line = tupleData.Item1.MainLine;
                    line.AnalysisTime = String.Format("{0:00}", ts.Minutes + ts.Hours * 60 + ts.Days * 24 * 60);
                    line.AnalysisTime += ":" +String.Format("{0:00}", ts.Seconds);
                    line.Label.Text = line.Depth + "      " + line.AnalysisTime + "      " + line.Evaluation
                            + "      " + line.PV;
                }
            }
            else if (e.UserState is Tuple<EngineLine, String, String>)
            {
                var tupleData = e.UserState as Tuple<EngineLine, String, String>;
                if (tupleData.Item2 == "pv")
                {
                    if (e.ProgressPercentage != 1)
                    {
                        if (tupleData.Item1.Engine.OtherLines == null ||
                            e.ProgressPercentage > tupleData.Item1.Engine.OtherLines.Count + 1)
                            return;
                        line = tupleData.Item1;
                        line.PV = tupleData.Item3;
                        line.Label.Text = line.Depth + "      " + line.AnalysisTime + "      " + line.Evaluation
                            + "      " + line.PV;
                        toolTip1.SetToolTip(line.Label, line.PV);
                        PVTimer.Enabled = true;
                        PVTimer.Start();
                        return;
                    }
                    line = tupleData.Item1;
                    tupleData.Item1.Engine.PVLabel.Text = tupleData.Item3;
                    line.PV = tupleData.Item3;
                    line.Label.Text = line.Depth + "      " + line.AnalysisTime + "      " + line.Evaluation
                            + "      " + line.PV;
                    toolTip1.SetToolTip(line.Label, line.PV);
                    PVTimer.Enabled = true;
                    PVTimer.Start();
                }
            }
        }
        private void ShowENGRegForm(Engine engine)
        {
            ENGRegForm = new Form();
            ENGRegForm.Text = "Engine Registration";
            ENGRegForm.MaximizeBox = false;
            ENGRegForm.MinimizeBox = false;
            ENGRegForm.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            ENGRegForm.KeyDown += ENGRegForm_KeyDown;
            ENGRegForm.Size = new System.Drawing.Size(350, 320);
            ENGRegForm.KeyPreview = true;
            ENGRegForm.StartPosition = FormStartPosition.CenterParent;
            Font font = new System.Drawing.Font("Calibri", 11F, FontStyle.Regular);

            Label label = new Label();
            label.AutoSize = true;
            label.Font = font;
            label.Text = engine.Name + " requires username and password\n to function with all the features";
            label.Location = new Point(10, 10);

            Label userLabel = new Label(), passLabel = new Label();
            userLabel.AutoSize = true;
            userLabel.Font = font;
            passLabel.Font = font;
            passLabel.AutoSize = true;
            userLabel.Text = "Username";
            passLabel.Text = "Password";
            userLabel.Location = new Point(30, label.Bottom + 30);
            passLabel.Location = new Point(30, userLabel.Bottom + 10);

            TextBox userTB = new TextBox(), passTB = new TextBox();
            userTB.Font = font;
            passTB.Font = font;
            userTB.Width = 170;
            passTB.Width = 170;
            userTB.Location = new Point(userLabel.Right - 15, userLabel.Top - 3);
            passTB.Location = new Point(passLabel.Right - 15, passLabel.Top - 3);

            CheckBox cb = new CheckBox();
            cb.Font = new System.Drawing.Font("Calibri", 10, FontStyle.Bold);
            cb.Text = "Remember my choice";
            cb.AutoSize = true;
            cb.Checked = false;
            cb.Location = new Point(80, passLabel.Bottom + 40);

            Button Enter = new Button(), NotNow = new Button();
            Enter.Text = "Register";
            NotNow.Text = "Not now";
            Enter.AutoSize = true;
            NotNow.AutoSize = true;
            Enter.Font = new System.Drawing.Font("Calibri", 12, FontStyle.Bold);
            NotNow.Font = new System.Drawing.Font("Calibri", 12, FontStyle.Bold);
            NotNow.Location = new Point(80, cb.Bottom + 30);
            Enter.Location = new Point(180, cb.Bottom + 30);
            Enter.Tag = true;
            NotNow.Tag = false;
            Enter.Click += ENGRegForm_Click;
            NotNow.Click += ENGRegForm_Click;

            ENGRegForm.Controls.Add(label);
            ENGRegForm.Controls.Add(userLabel);
            ENGRegForm.Controls.Add(passLabel);
            ENGRegForm.Controls.Add(userTB);
            ENGRegForm.Controls.Add(passTB);
            ENGRegForm.Controls.Add(cb);
            ENGRegForm.Controls.Add(Enter);
            ENGRegForm.Controls.Add(NotNow);
            ENGRegForm.Tag = Tuple.Create(engine, userTB, passTB, cb);
            ENGRegForm.ShowDialog(this);
        }
        void ENGRegForm_KeyDown(object sender, KeyEventArgs e)
        {
            Button b = new Button();
            if (e.KeyCode == Keys.Escape)
            {
                b.Tag = false;
                ENGRegForm_Click(b, new EventArgs());
            }
            else if (e.KeyCode == Keys.Enter)
            {
                b.Tag = true;
                ENGRegForm_Click(b, new EventArgs());
            }
        }
        void ENGRegForm_Click(object sender, EventArgs e)
        {
            bool t1 = (bool)(sender as Control).Tag;
            var t2 = ENGRegForm.Tag as Tuple<Engine, TextBox, TextBox, CheckBox>;
            if (t2.Item4.Checked)
                t2.Item1.shouldNotReg = true;

            if (!t1)
            {
                t2.Item1.Process.StandardInput.WriteLine("register later");
                ENGRegForm.Close();
                return;
            }
            else
            {
                t2.Item1.Process.StandardInput.WriteLine("register name " + t2.Item2.Text
                    + " code " + t2.Item3.Text);
            }
        }
        private void UpdateArrow(bool ShouldClose)
        {
            UpdateArrow(BestMoveArrow, ShouldClose);
        }
        private void UpdateArrow(Arrow arrow, bool ShouldClose)
        {
            if (ShouldClose)
                arrow.Enabled = false;
            if (arrow.isShowing)
            {
                using (Graphics g = panel1.CreateGraphics())
                    RefreshSquares(arrow.Squares, RedrawPerspective.Arrow, null);
                arrow.isShowing = false;
            }

            if (arrow == BestMoveArrow && !settings.appearance.ShouldShowBMA)
                return;

            if (arrow.Enabled && !arrow.IsInvalid && arrow.StartingSquare != null && arrow.StoppingSquare != null
                && !IsAnimating)
            {
                arrow.Update();
                if (arrow == BestMoveArrow && (arrow.StartingSquare.Piece == null || 
                    arrow.StartingSquare.Piece.Side != sideToPlay))
                    return;
                if (arrow.StartingSquare == arrow.StoppingSquare && arrow != BestMoveArrow)
                {
                    if (settings.appearance.UserArrowSquareHLType == "Shade")
                        panel1.CreateGraphics().FillRectangle(arrow.Pen.Brush, arrow.StartingSquare.Rectangle);
                    else if (settings.appearance.UserArrowSquareHLType == "Circle")
                        graphics.DrawEllipse(new Pen(arrow.Color, 3), 
                            new Rectangle(arrow.StartingSquare.Rectangle.Location + new Size(1, 1), 
                                arrow.StartingSquare.Rectangle.Size - new Size(3, 3)));
                    if (arrow.StartingSquare.Piece != null)
                        PlacePiece(arrow.StartingSquare.Piece, arrow.StartingSquare);
                    return;
                }
                Point x = new Point(arrow.StartingSquare.Rectangle.Height / 2,
                        arrow.StartingSquare.Rectangle.Width / 2)
                        + new Size(arrow.StartingSquare.Rectangle.Location);
                Point y = new Point(arrow.StoppingSquare.Rectangle.Height / 2,
                    arrow.StoppingSquare.Rectangle.Width / 2)
                    + new Size(arrow.StoppingSquare.Rectangle.Location);
                panel1.CreateGraphics().DrawLine(arrow.Pen, x, y);
                if (arrow == BestMoveArrow)
                    arrow.isShowing = true;
            }
        }
        private void OutputScanner(Engine engine, BackgroundWorker BGWorker)
        {
            if (engine.Output == "readyok")
            {
                engine.isBusy = false;
                return;
            }

            if (engine.ShouldIgnore && engine.Side != sideToPlay)
                return;
            String str = "";
            if (engine.isUciEngine && (ModeOfPlay == PlayMode.SinglePlayer || 
                ModeOfPlay == PlayMode.EngineVsEngine))
            {
                if (engine.Output.IndexOf("bestmove") == 0)
                {
                    String buffer = engine.Output.Substring(9, 4);
                    Square Sq1 = GetSquare(buffer.Substring(0, 2));
                    Square Sq2 = GetSquare(buffer.Substring(2, 2));

                    if (Sq1 == null || Sq1.Piece == null || Sq1.Piece.Side != engine.Side)
                    {
                        return;
                    }

                    Move move = new Move(Sq1, Sq1.Piece, Sq2);
                    if (Sq1.Piece.Type == Piece.PieceType.Pawn &&
                        (buffer[3] == '1' || buffer[3] == '8'))
                    {
                        buffer = engine.Output.Substring(9, 5);
                        switch (buffer[4])
                        {
                            case 'q': move.PromoteType = Move.ItsPromoteType.Queen; break;
                            case 'r': move.PromoteType = Move.ItsPromoteType.Rook; break;
                            case 'b': move.PromoteType = Move.ItsPromoteType.Bishop; break;
                            case 'n': move.PromoteType = Move.ItsPromoteType.Knight; break;
                        }
                    }
                    int x = engine.Output.IndexOf("ponder");
                    engine.PonderString = engine.Output.Substring(x + 7, 4);
                    engine.Time.AnimationCompCount = 0;
                    while (IsAnimating)
                    {
                        Thread.Sleep(10);
                        engine.Time.AnimationCompCount++;
                    }
                    BGWorker.ReportProgress(0, Tuple.Create(engine, move));

                    return;
                }
            }

            if (engine.isUciEngine && engine.isAnalyzing)
            {
                int MultiPV = engine.OtherLines != null ? engine.Output.IndexOf("multipv") : 1;
                if (MultiPV < 0)
                    return;
                if (engine.OtherLines != null)
                {
                    int start = MultiPV + 8;
                    MultiPV = engine.Output.IndexOf(" ", start);
                    str = engine.Output.Substring(start, MultiPV - start); 
                    if (!int.TryParse(str, out MultiPV) || (MultiPV > engine.OtherLines.Count + 1))
                        return;
                }
                if (engine.Output.Contains("score cp"))
                {
                    int x = engine.Output.IndexOf("score cp");
                    int j = -1;
                    bool START = false;
                    str = "";
                    foreach (var item in engine.Output)
                    {
                        j++;
                        if (j < x)
                            continue;
                        if (char.IsNumber(item))
                        {
                            START = true;
                            str += item;
                        }
                        else if (item == '-' && !START)
                        {
                            START = true;
                            str += item;
                        }
                        else if (START)
                            break;
                    }
                    decimal d;
                    if (!decimal.TryParse(str, out d))
                        return;
                    else
                        BGWorker.ReportProgress(MultiPV, Tuple.Create(engine, "cp", d));
                }

                if (engine.Output.Contains("time"))
                {
                    int x = engine.Output.IndexOf("time");
                    int j = -1;
                    bool START = false;
                    str = "";
                    foreach (var item in engine.Output)
                    {
                        j++;
                        if (j < x)
                            continue;
                        if (char.IsNumber(item))
                        {
                            START = true;
                            str += item;
                        }
                        else if (START)
                            break;
                    }
                    if (!int.TryParse(str, out x))
                        return;
                    BGWorker.ReportProgress(MultiPV, Tuple.Create(engine, "time", x));
                }

                if (engine.Output.Contains(" depth"))
                {
                    int x = engine.Output.IndexOf(" depth") + 1;
                    int j = -1;
                    bool START = false;
                    str = "";
                    foreach (var item in engine.Output)
                    {
                        j++;
                        if (j < x)
                            continue;
                        if (char.IsNumber(item))
                        {
                            START = true;
                            str += item;
                        }
                        else if (START)
                            break;
                    }
                    int d;
                    if (!int.TryParse(str, out d))
                        return;
                    else
                        BGWorker.ReportProgress(MultiPV, Tuple.Create(engine, "depth", d));
                }
                if (engine.Output.Contains("score mate"))
                {
                    int x = engine.Output.IndexOf("score mate");
                    int j = -1;
                    bool START = false;
                    str = "";
                    foreach (var item in engine.Output)
                    {
                        j++;
                        if (j < x)
                            continue;
                        if (char.IsNumber(item))
                        {
                            START = true;
                            str += item;
                        }
                        else if (item == '-' && !START)
                        {
                            START = true;
                            str += item;
                        }
                        else if (START)
                            break;
                    }
                    decimal d;
                    if (!decimal.TryParse(str, out d))
                        return;
                    else
                        BGWorker.ReportProgress(MultiPV, Tuple.Create(engine, "mate", d));
                    engine.MainLine.isPVActive = true;
                }
                EngineLine line = MultiPV == 1 ? engine.MainLine : engine.OtherLines[MultiPV - 2];
                if (engine.Output.Contains(" pv"))
                {
                    int x = engine.Output.IndexOf(" pv");
                    String buffer = engine.Output.Substring(x + 4);

                    if (MultiPV == 1)
                    {
                        Square Sq;
                        Arrow tempArrow = new Arrow();
                        TryGetSquare(buffer.Substring(0, 2), out Sq);
                        tempArrow.StartingSquare = Sq;
                        TryGetSquare(buffer.Substring(2, 2), out Sq);
                        tempArrow.StoppingSquare = Sq;
                        if (!tempArrow.Equals(BestMoveArrow))
                        {
                            BestMoveArrow.IsInvalid = false;
                            BestMoveArrow.StartingSquare = tempArrow.StartingSquare;
                            BestMoveArrow.StoppingSquare = tempArrow.StoppingSquare;
                            BGWorker.ReportProgress(MultiPV, Tuple.Create(engine, "arrow"));
                        }
                        else if (!BestMoveArrow.isShowing && !BestMoveArrow.IsInvalid)
                        {
                            BGWorker.ReportProgress(MultiPV, Tuple.Create(engine, "arrow"));
                        }
                    }

                    try
                    {
                        if (line.LastPVString != buffer)
                        {
                            line.LastPVString = buffer;
                            str = LongNotationToShort(line, buffer, BGWorker);
                            if (str != "")
                            {
                                line.isPVActive = false;
                                BGWorker.ReportProgress(MultiPV,
                                Tuple.Create(line, "pv", str));
                            }
                        }
                    }
                    catch (Exception)
                    {
                        return;
                    }
                }
                
            }
            else if (engine.Output == "uciok")
            {
                BGWorker.ReportProgress(0, engine);
                //MessageBox.Show(engine.ToString());
                return;
            }
            else if (engine.Output == "registration error")
            {
                BGWorker.ReportProgress(0, Tuple.Create(engine, "register"));
            }
            else if (engine.Output == "registration ok")
            {
                BGWorker.ReportProgress(0, Tuple.Create(engine, "registerok"));
            }
            else if (engine.Output == "copyprotection error")
            {
                BGWorker.ReportProgress(0, Tuple.Create(engine, "copyprotection"));
            }
            else if (!InstalledEngines.Contains(engine) && engine.Output.Contains("option name"))
            {
                EngineOption opt = new EngineOption();
                int x = engine.Output.IndexOf("name");
                int y = engine.Output.IndexOf("type");
                if (x < 0 || y < 0)
                    return;
                opt.Name = engine.Output.Substring(x + 5, y - x - 6);

                engine.Output = engine.Output.Substring(y + 5);
                if (engine.Output == "button")
                {
                    opt.Type = EngineOption.OptionType.Button;
                    engine.Options.Add(opt);
                    return;
                }
                x = engine.Output.IndexOf(" ");
                if (x < 0)
                    x = engine.Output.IndexOf("\n");
                if (x < 0)
                    return;
                String buffer = engine.Output.Substring(0, x);
                switch (buffer)
                {
                    case "spin": opt.Type = EngineOption.OptionType.Spin;
                        break;
                    case "check": opt.Type = EngineOption.OptionType.Check;
                        break;
                    case "button": opt.Type = EngineOption.OptionType.Button;
                        engine.Options.Add(opt);
                        return;
                    case "string": opt.Type = EngineOption.OptionType.String;
                        break;
                    case "combo": opt.Type = EngineOption.OptionType.Combo;
                        break;
                    default:
                        return;
                }

                x = engine.Output.IndexOf("default");
                if (x < 0)
                {
                    if (opt.Type == EngineOption.OptionType.String)
                    {
                        engine.Options.Add(opt);
                        return;
                    }
                    else
                        return;
                }
                if (opt.Type == EngineOption.OptionType.Combo)
                {
                    if (x < engine.Output.IndexOf("var"))
                    {
                        x += 8;    //Default value
                        y = engine.Output.IndexOf(' ', x);
                        opt.DefaultValue = engine.Output.Substring(x, y - x);
                        opt.CurrentValue = opt.DefaultValue;

                        opt.ComboValues = new List<string>();
                        while (true)
                        {
                            x = engine.Output.IndexOf("var", y);
                            if (x < 0)
                                break;
                            x += 4;
                            y = engine.Output.IndexOf(' ', x);
                            if (y < 0)
                            {
                                opt.ComboValues.Add(engine.Output.Substring(x));
                                break;
                            }
                            else
                                opt.ComboValues.Add(engine.Output.Substring(x, y - x));
                        }
                    }
                    else
                    {
                        opt.ComboValues = new List<string>();
                        y = 0;
                        while (true)
                        {
                            x = engine.Output.IndexOf("var", y);
                            if (x < 0)
                                break;
                            x += 4;
                            y = engine.Output.IndexOf(' ', x);
                            if (y < 0)
                            {
                                opt.ComboValues.Add(engine.Output.Substring(x));
                                break;
                            }
                            else
                                opt.ComboValues.Add(engine.Output.Substring(x, y - x));
                        }


                        x = engine.Output.IndexOf("default") + 8;    //Default value
                        opt.DefaultValue = engine.Output.Substring(x);
                        opt.CurrentValue = opt.DefaultValue;
                    }
                    engine.Options.Add(opt);
                    return;
                }
                if (opt.Type == EngineOption.OptionType.Check)
                {
                    if (engine.Output.Contains("true"))
                    {
                        opt.DefaultValue = true;
                        opt.CurrentValue = opt.DefaultValue;
                        engine.Options.Add(opt);
                        return;
                    }
                    else if (engine.Output.Contains("false"))
                    {
                        opt.DefaultValue = false;
                        opt.CurrentValue = opt.DefaultValue;
                        engine.Options.Add(opt);
                        return;
                    }
                    else
                        return;
                }
                else if (opt.Type == EngineOption.OptionType.String)
                {
                    opt.DefaultValue = engine.Output.Substring(x + 8);
                    opt.CurrentValue = opt.DefaultValue;
                    engine.Options.Add(opt);
                    return;
                }
                else if (opt.Type == EngineOption.OptionType.Spin)
                {
                    x = engine.Output.IndexOf("min");
                    y = engine.Output.IndexOf("default");
                    int z = engine.Output.IndexOf("max");
                    if (x < 0 || y < 0 || z < 0)
                        return;
                    int j = -1;
                    bool START = false;
                    str = "";
                    foreach (var item in engine.Output)
                    {
                        j++;
                        if (j < x)
                            continue;
                        if (char.IsNumber(item))
                        {
                            START = true;
                            str += item;
                        }
                        else if (item == '-' && !START)
                        {
                            START = true;
                            str += item;
                        }
                        else if (START)
                            break;
                    }
                    if (!int.TryParse(str, out j))
                        return;
                    else
                        opt.MinValue = j;

                    j = -1;
                    START = false;
                    str = "";
                    foreach (var item in engine.Output)
                    {
                        j++;
                        if (j < y)
                            continue;
                        if (char.IsNumber(item))
                        {
                            START = true;
                            str += item;
                        }
                        else if (item == '-' && !START)
                        {
                            START = true;
                            str += item;
                        }
                        else if (START)
                            break;
                    }
                    if (!int.TryParse(str, out j))
                        return;
                    else
                        opt.DefaultValue = j;

                    j = -1;
                    START = false;
                    str = "";
                    foreach (var item in engine.Output)
                    {
                        j++;
                        if (j < z)
                            continue;
                        if (char.IsNumber(item))
                        {
                            START = true;
                            str += item;
                        }
                        else if (item == '-' && !START)
                        {
                            START = true;
                            str += item;
                        }
                        else if (START)
                            break;
                    }
                    if (!int.TryParse(str, out j))
                        return;
                    else
                        opt.MaxValue = j;
                    opt.CurrentValue = opt.DefaultValue;
                    engine.Options.Add(opt);
                    return;
                }
            }

            if (engine.Output.Contains("id name"))
            {
                int x = engine.Output.IndexOf("id name");
                if (String.IsNullOrWhiteSpace(engine.Name))
                    engine.Name = engine.Output.Substring(x + 8);
                return;
            }
            if (engine.Output.Contains("id author"))
            {
                int x = engine.Output.IndexOf("id author");
                engine.Author = engine.Output.Substring(x + 10);
                return;
            }
        }
        private void ClearEngineOutput(Engine engine)
        {
            engine.PVLabel.Text = "";
            engine.EvalLabel.Text = "";
            engine.EvalDescriptLabel.Text = "";
            engine.DepthLabel.Text = "";
        }
        private string AddTimeInfo()
        {
            String buffer = "wtime ";
            buffer += Math.Round(WhiteTime.TimeLeft.TotalMilliseconds, 0);
            buffer += " btime ";
            buffer += Math.Round(BlackTime.TimeLeft.TotalMilliseconds, 0);
            buffer += " winc ";
            buffer += WhiteTime.Increment;
            buffer += " binc ";
            buffer += BlackTime.Increment;
            return buffer;
        }
        private void AnalyzeButton_Click(object sender, EventArgs e)
        {
            Button button = sender as Button;
            Engine engine = button.Tag as Engine;
            if (!engine.isUciEngine)
                return;
            BestMoveArrow.IsInvalid = false;

            ModeOfPlay = PlayMode.EditPosition;
            if (isEngineMatchInProgress)
            {
                FirstEngine.isAnalyzing = false;
                SecondEngine.isAnalyzing = false;
                engine.Process.StandardInput.WriteLine("stop");
                (engine.Opponent as Engine).Process.StandardInput.WriteLine("stop");
                OnGameEnding();
            }
            isEngineMatchInProgress = false;

            if (!engine.isAnalyzing)
            {
                DoAnalysis(engine);

                if (ShouldDrawUserArrows && CurrentPosition.Lines.Count > 0)
                {
                    List<Square> list = new List<Square>();
                    foreach (var item in CurrentPosition.Lines)
                    {
                        item.Enabled = false;
                        list.AddRange(item.Squares);
                    }
                    if (settings.appearance.SaveUserArrows != "Save with position")
                        CurrentPosition.Lines.Clear();
                    RefreshSquares(list, RedrawPerspective.UserArrows, null);
                }
            }
            else if (engine.isAnalyzing)
            {
                engine.Process.StandardInput.WriteLine("stop");
                engine.isAnalyzing = false;
                if (!FirstEngine.isAnalyzing && (SecondEngine == null || !SecondEngine.isAnalyzing))
                {
                    isInfiniteSearch = false;
                    UpdateArrow(true);
                    if (settings.appearance.SaveUserArrows == "Save with position")
                        if (ShouldDrawUserArrows && CurrentPosition.Lines.Count > 0)
                        {
                            List<Square> list = new List<Square>();
                            foreach (var item in CurrentPosition.Lines)
                            {
                                item.Enabled = true;
                                list.AddRange(item.Squares);
                            }
                            RefreshSquares(list, RedrawPerspective.Arrow, null);
                        }
                }
                button.Text = "Analyze";
            }
        }
        private void GoButton_Click(object sender, EventArgs e)
        {
            var str = "test";
            MessageBox.Show(str.GetType().ToString());

            //InitializeTourneyForm();

            #region Opening book code
            //UserParticipant = new Participant();
            //UserParticipant.FideRating = 2069;
            //var temp = getRatings(10, 1840, 2350, 2000);
            //MessageBox.Show(string.Join(", ", temp) + "\n" + temp.Average());

            //List<OpeningNode> TabierListNew = new List<OpeningNode>();
            //foreach (var item in TabierList)
            //{
            //    bool isWhite = true, isDot = false, isPreWhite = true;
            //    if (true)
            //    {
            //        if (String.IsNullOrWhiteSpace(item.OpeningLine) ||
            //            item.OpeningLine.IndexOfAny(new char[] { '(', ')', '[', ']', '{', '}' }) >= 0)
            //        {

            //        }

            //        foreach (var CHAR in item.OpeningLine.Trim())
            //        {
            //            if (Char.IsLetter(CHAR) && CHAR != item.OpeningLine.Trim().First())
            //            {
            //                if (isPreWhite)
            //                    isWhite = true;
            //                else
            //                    isWhite = false;
            //                continue;
            //            }

            //            if (CHAR == ' ')
            //            {
            //                if (isDot)
            //                    isPreWhite = false;
            //                else
            //                    isPreWhite = true;
            //            }

            //            if (CHAR == '.')
            //                isDot = true;
            //            else
            //                isDot = false;
            //        }
            //        if (isWhite)
            //            item.SideToPlay = Piece.PieceSide.White;
            //        else
            //            item.SideToPlay = Piece.PieceSide.Black;
            //        TabierListNew.Add(item);
            //    }
            //}

            //BinaryFormatter bf = new BinaryFormatter();
            //using (Stream output = File.Create("OpeningBookModified.dat"))
            //{
            //    bf.Serialize(output, TabierListNew);
            //}
            //MessageBox.Show("Test");


            //String Str = "", Str2;
            //using (StreamReader sr = new StreamReader
            //    (@"C:\Users\Johnson\Downloads\Downloads\[Sleigh_Ride]_Dick_Francis__-_Sleigh_Ride(b-ok.org).htm"))
            //{
            //    Str = sr.ReadToEnd();
            //    Str2 = Str.Replace("\n\r", "<p>");
            //    Str2 = Str.Replace("\r\n", "<p>");
            //    Str2 = Str.Replace("\n", "<p>");
            //}
            //using (StreamWriter sw = new StreamWriter
            //    (@"C:\Users\Johnson\Downloads\Downloads\[Sleigh_Ride]_Dick_Francis__-_Sleigh_Ride(b-ok.org).htm", false))
            //{
            //    sw.Write(Str2);
            //}
            #endregion

        }
        private bool CheckPlayerName(Game game, List<string> list)
        {
            foreach (var item in list)
            {
                if (game.GameDetails.BlackPlayer.Name.Contains(item) ||
                    game.GameDetails.WhitePlayer.Name.Contains(item))
                    return true;
            }
            return false;
        }
        void UnitTest(int x)
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            var tupleList = new List<Tuple<Label, String>>();
            List<String> TempList = new List<String>();
            List<String> DBColumns = new List<String>();
            DBColumns.Add("White");
            DBColumns.Add("Black");
            DBColumns.Add("Event");
            DBColumns.Add("Result");
            DBColumns.Add("ECO");
            DBColumns.Add("Date");
            for (int i = 0; i < x; i++)
            {
                TempList.Add(DateTime.Now.ToString());
            }
            DBEPanel.SuspendLayout();
            foreach (var item in TempList)
            {
                foreach (var column in DBColumns)
                {
                    Label tempLabel = new Label();
                    tempLabel.AutoSize = false;
                    tempLabel.BackColor = Color.White;
                    tempLabel.ForeColor = Color.Black;
                    tempLabel.TextAlign = ContentAlignment.MiddleLeft;
                    tempLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F,
                        System.Drawing.FontStyle.Regular);
                    tempLabel.MouseEnter += DBERecordLabel_MouseEnter;
                    tempLabel.MouseLeave += DBERecordLabel_MouseLeave;
                    tempLabel.AutoEllipsis = true;
                    tempLabel.MouseDown += DBERecordLabel_MouseDown;
                    tempLabel.Tag = item;
                    switch (column)
                    {
                        case "White":
                            tempLabel.Text = item;
                            break;
                        case "Black":
                            tempLabel.Text = item;
                            break;
                        case "Event":
                            tempLabel.Text = item;
                            break;
                        case "ECO":
                            tempLabel.Text = item;
                            break;
                        case "Date":
                            tempLabel.Text = item;
                            break;
                        case "Result":
                            tempLabel.Text = item;
                            break;
                    }
                    tempLabel.Size = new Size(0, 3) + new Size(3, 0);
                    tempLabel.Location = new Point(0,
                        (TempList.IndexOf(item) * (tempLabel.Size.Height - 4) + 40));
                    tupleList.Add(Tuple.Create(tempLabel, column));
                    DBEPanel.Controls.Add(tempLabel);
                }
            }
            DBEPanel.ResumeLayout();
            sw.Stop();
        }
        private void NameButton_Click(object sender, EventArgs e)
        {
            Engine engine = (sender as Control).Tag as Engine;
            if (engine == null || !engine.isUciEngine)
                return;
            engine.ComboBox.Items.Clear();
            engine.ComboBox.Items.AddRange(InstalledEngines.ToArray());
            engine.ComboBox.Location = engine.NameButton.Location;
            engine.ComboBox.SelectedIndex = InstalledEngines.IndexOf(engine);
            engine.NameButton.Visible = false;
            engine.ComboBox.Visible = true;
            engine.ComboBox.DroppedDown = true;
            
        }
        void ComboBox_DropDownClosed(object sender, EventArgs e)
        {
            Engine engine = (sender as Control).Tag as Engine;
            if (engine.Equals(engine.ComboBox.SelectedItem as Engine))
            {
                engine.ComboBox.Visible = false;
                engine.NameButton.Visible = true;
            }
            else
            {
                LoadEngine(engine.ComboBox.SelectedItem as Engine, engine == FirstEngine);
            }
        }
        void bgw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            
        }
        void bgw_DoWork(object sender, DoWorkEventArgs e)
        {
            int PlyCount;
            if (File.Exists(@"C:\Users\JAY JAY\Documents\Chess Database\Grandmaster\zPly.dat"))
            {
                BinaryFormatter bf = new BinaryFormatter();
                using (Stream input = File.OpenRead
                    (@"C:\Users\JAY JAY\Documents\Chess Database\Grandmaster\zPly.dat"))
                {
                    PlyCount = (int)bf.Deserialize(input) + 1;
                }
                bf = new BinaryFormatter();
                using (Stream output = File.Create
                    (@"C:\Users\JAY JAY\Documents\Chess Database\Grandmaster\zPly.dat"))
                {
                    bf.Serialize(output, PlyCount);
                }
            }
            else
            {
                PlyCount = 0;
                BinaryFormatter bf = new BinaryFormatter();
                using (Stream output = File.Create
                    (@"C:\Users\JAY JAY\Documents\Chess Database\Grandmaster\zPly.dat"))
                {
                    bf.Serialize(output, PlyCount);
                }
            }

            int i = 0;
            while (File.Exists
                    (@"C:\Users\JAY JAY\Documents\Chess Database\Grandmaster\zzz\z" +i +".dat"))
            {
                List<Game> GList;
                BinaryFormatter bf = new BinaryFormatter();
                using (Stream input = File.OpenRead
                    (@"C:\Users\JAY JAY\Documents\Chess Database\Grandmaster\zzz\z" +i +".dat"))
                {
                    GList = bf.Deserialize(input) as List<Game>;

                }

                i++;
            }
        }
        private void SendPositionToEngine(Engine item, String str)
        {
            item.ShouldIgnore = true;
            if (item.isAnalyzing)
                item.Process.StandardInput.WriteLine("stop");
            String buffer = "";
            if (MainLine.MovesList[0].Compare(StartingPosition))
                buffer = "position startpos moves ";
            else
                buffer = "position fen " + FENforStartPos + " moves ";
            List<String> tempList = new List<string>();
            int i = CurrentVariation.MovesList.IndexOf(CurrentPosition);
            for (; i >= 0; i--)
            {
                if (CurrentVariation.MovesList[i] != MainLine.MovesList[0])
                    tempList.Add(CurrentVariation.MovesList[i].LastMovePlayed.UCINotation);
            }
            if (CurrentVariation != MainLine)
            {
                VariationHolder CurrentNode = CurrentVariation;
                while (true)
                {
                    for (i = CurrentNode.ParentIndex; i >= 0; i--)
                    {
                        if (CurrentNode.ParentLine.MovesList[i] != MainLine.MovesList[0])
                            tempList.Add(CurrentNode.ParentLine.MovesList[i].LastMovePlayed.UCINotation);
                    }
                    if (CurrentNode.ParentLine.ParentLine != null)
                        CurrentNode = CurrentNode.ParentLine;
                    else
                        break;
                }
            }

            tempList.Reverse();
            foreach (var notation in tempList)
            {
                buffer += notation + " ";
            }
            buffer += str;
            item.Process.StandardInput.WriteLine(buffer);
        }
        void BGWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            Engine engine = e.Argument as Engine;
            BackgroundWorker BGWorker = sender as BackgroundWorker;
            String buffer = "";
            while (true)
            {
                try
                {
                    buffer = engine.Process.StandardOutput.ReadLine();
                }
                catch (Exception)
                {
                    
                }
                if (buffer == null || buffer == "")
                {
                    Thread.Sleep(5);
                }
                else
                {
                    engine.Output = String.Copy(buffer);

                    try
                    {
                        OutputScanner(engine, sender as BackgroundWorker);
                    }
                    catch (Exception)
                    {
                        
                    }
                    buffer = "";
                }
            }
        }
        private string LongNotationToShort(EngineLine line, string p, BackgroundWorker BGWorker)
        {
            Engine engine = line.Engine;
            engine.PVSquares = new List<Square>();
            engine.PVMoveCount = MoveCount;
            foreach (var item in Squares)
            {
                engine.PVSquares.Add(item.GetClone());
            }
            String buffer = "", str = "", str2 = "";
            while (p != "")
            {
                str = "";
                int i = 0;
                foreach (var item in p)
                {
                    i++;
                    if (!char.IsWhiteSpace(item))
                        str += item;
                    else
                        break;
                }
                p = p.Substring(i);
                try
                {
                    str2 = MakeMove(str, engine);
                }
                catch (Exception)
                {
                    return buffer;
                }

                if (str2 == "")
                    return buffer;

                if (sideToPlay == Piece.PieceSide.Black && buffer == "")
                {
                    int mc = 0;
                    if (CurrentPosition.LastMovePlayed != null)
                        mc = CurrentPosition.LastMovePlayed.MoveNo;
                    buffer += (sideToPlay == Piece.PieceSide.White ?
                        (mc + 1).ToString() + ". " : mc.ToString() + "... "); 
                }
                buffer += str2 + " ";
            }
            return buffer;
        }
        void PVTimer_Tick(object sender, EventArgs e)
        {
            PVTimer.Stop();
            PVTimer.Enabled = false;
            foreach (var item in LoadedEngines)
                if (item.OtherLines != null)
                    foreach (var line in item.OtherLines)
                        line.isPVActive = true;
        }
        String MakeMove(String buffer, Engine engine)
        {
            Move move = new Move();
            Square OriginalSq, DestSq, Sq1, Sq2;
            if (!TryGetSquare(engine, buffer.Substring(0, 2), out OriginalSq))
                return "";
            if (!TryGetSquare(engine, buffer.Substring(2, 2), out DestSq))
                return "";
            move.PieceMoving = OriginalSq.Piece;
            move.OriginalSquare = OriginalSq;
            move.DestSquare = DestSq;
            if (DestSq.Piece != null)
                move.IsCapture = true;

            if (buffer.Length == 5)
            {
                OriginalSq.Piece = null;
                if (DestSq.Piece != null)
                    move.IsCapture = true;
                switch (buffer[4])
                {
                    case 'q': move.PromoteType = Move.ItsPromoteType.Queen;
                        DestSq.Piece = new Piece();
                        DestSq.Piece.Type = Piece.PieceType.Queen;
                        DestSq.Piece.Side = move.PieceMoving.Side;
                        break;
                    case 'r': move.PromoteType = Move.ItsPromoteType.Rook;
                        DestSq.Piece = new Piece();
                        DestSq.Piece.Type = Piece.PieceType.Rook;
                        DestSq.Piece.Side = move.PieceMoving.Side;
                        break;
                    case 'b': move.PromoteType = Move.ItsPromoteType.Bishop;
                        DestSq.Piece = new Piece();
                        DestSq.Piece.Type = Piece.PieceType.Bishop;
                        DestSq.Piece.Side = move.PieceMoving.Side;
                        break;
                    case 'n': move.PromoteType = Move.ItsPromoteType.Knight;
                        DestSq.Piece = new Piece();
                        DestSq.Piece.Type = Piece.PieceType.Knight;
                        DestSq.Piece.Side = move.PieceMoving.Side;
                        break;
                    default:
                        return "";
                }
                move.GetShortNotation(engine);
            }
            else if (OriginalSq.Piece == WhiteKing && buffer == "e1g1")
            {
                move.GetShortNotation(engine);
                DestSq.Piece = WhiteKing;
                TryGetSquare(engine, "h1", out Sq1);
                TryGetSquare(engine, "f1", out Sq2);
                Sq2.Piece = Sq1.Piece;
                Sq1.Piece = null;
                OriginalSq.Piece = null;
            }
            else if (OriginalSq.Piece == WhiteKing && buffer == "e1c1")
            {
                move.GetShortNotation(engine);
                DestSq.Piece = WhiteKing;
                TryGetSquare(engine, "a1", out Sq1);
                TryGetSquare(engine, "d1", out Sq2);
                Sq2.Piece = Sq1.Piece;
                Sq1.Piece = null;
                OriginalSq.Piece = null;
            }
            else if (OriginalSq.Piece == BlackKing && buffer == "e8g8")
            {
                move.GetShortNotation(engine);
                DestSq.Piece = BlackKing;
                TryGetSquare(engine, "h8", out Sq1);
                TryGetSquare(engine, "f8", out Sq2);
                Sq2.Piece = Sq1.Piece;
                Sq1.Piece = null;
                OriginalSq.Piece = null;
            }
            else if (OriginalSq.Piece == BlackKing && buffer == "e8c8")
            {
                move.GetShortNotation(engine);
                DestSq.Piece = WhiteKing;
                TryGetSquare(engine, "a8", out Sq1);
                TryGetSquare(engine, "d8", out Sq2);
                Sq2.Piece = Sq1.Piece;
                Sq1.Piece = null;
                OriginalSq.Piece = null;
            }

            else if (OriginalSq.Piece.Type == Piece.PieceType.Pawn &&
                OriginalSq.Name[0] != DestSq.Name[0] && DestSq.Piece == null)
            {
                move.GetShortNotation(engine);
                move.IsCapture = true;
                DestSq.Piece = OriginalSq.Piece;
                OriginalSq.Piece = null;
                TryGetSquare(engine, (buffer[2].ToString() + buffer[1]), out Sq1);
                Sq1.Piece = null;
            }

            else
            {
                move.GetShortNotation(engine);
                DestSq.Piece = OriginalSq.Piece;
                OriginalSq.Piece = null;
            }
            engine.PVMoveCount++;
            move.MoveNo = (engine.PVMoveCount + 1) / 2;

            //bool isCheck = false;
            return (move.PieceMoving.Side == Piece.PieceSide.White ? 
                move.MoveNo.ToString() +". " : "") +move.ShortNotation;
        }
        void ShowEngineOptionsForm(Engine engine)
        {
            if (!engine.isUciEngine)
                return;
            if (engine.isAnalyzing)
            {
                MessageBox.Show("You must stop analysis before you can change Engine options");
                return;
            }
            var indexList = new List<EngineOption>();
            foreach (var item in engine.Options)
                if (item.Type == EngineOption.OptionType.Button)
                    indexList.Add(item);
            foreach (var item in indexList)
            {
                engine.Options.Remove(item);
                engine.Options.Add(item);
            }

            OptionsENGForm = new Form();
            Font font = new System.Drawing.Font("Calibri", 12F, FontStyle.Regular);
            OptionsENGForm.Text = "Settings for " + engine.Name;
            OptionsENGForm.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            OptionsENGForm.StartPosition = FormStartPosition.CenterParent;
            OptionsENGForm.KeyDown += OptionsForm_KeyDown;
            OptionsENGForm.KeyPreview = true;
            OptionsENGForm.Tag = engine;
            OptionsENGForm.MaximizeBox = false;
            OptionsENGForm.MinimizeBox = false;
            ToolTip tt = new ToolTip();
            tt.Active = true;
            tt.AutoPopDelay = 3000;
            tt.InitialDelay = 200;
            tt.ReshowDelay = 500;
            tt.ShowAlways = false;

            OptionsENGForm.AutoScroll = true;
            Control LastControl = null;
            bool shouldGoLeft = true;

            foreach (var item in engine.Options)
            {
                if (!item.ShouldDisplay)
                    continue;
                Label label = null;
                if (item.Type != EngineOption.OptionType.Button)
                {
                    label = new Label();
                    label.Text = item.Name;
                    label.Font = font;
                    label.AutoSize = true;
                    label.Location = new Point(shouldGoLeft ? 20 : 350,
                        LastControl == null ? 40 : (shouldGoLeft ? LastControl.Bottom + 10 : LastControl.Location.Y));
                    OptionsENGForm.Controls.Add(label);
                }
                switch (item.Type)
                {
                    case EngineOption.OptionType.Spin:
                        TextBox tb = new TextBox();
                        item.Control = tb;
                        tb.Size = new System.Drawing.Size(40, 15);
                        tb.Text = ((int)item.CurrentValue).ToString();
                        tb.Font = font;
                        tb.Location = new Point(label.Location.X + 200, label.Location.Y);
                        break;
                    case EngineOption.OptionType.String:
                        TextBox tb2 = new TextBox();
                        item.Control = tb2;
                        tb2.Text = (String)item.CurrentValue;
                        tb2.Size = new System.Drawing.Size(100, 15);
                        tb2.Font = font;
                        tb2.Location = new Point(label.Location.X + 200, label.Location.Y);
                        break;
                    case EngineOption.OptionType.Button:
                        Button bt = new Button();
                        item.Control = bt;
                        bt.Text = item.Name;
                        bt.Tag = Tuple.Create(engine, item);
                        bt.Click += OptButtons_Click;
                        bt.AutoSize = true;
                        if (!(LastControl is Button))
                        {
                            shouldGoLeft = true;
                        }
                        bt.Font = new System.Drawing.Font(font.OriginalFontName, font.Size, FontStyle.Bold);
                        bt.Location = new Point(shouldGoLeft ? 20 : 350, LastControl == null ? 10 : 
                            (shouldGoLeft ? LastControl.Bottom + 20 : LastControl.Top));
                        break;
                    case EngineOption.OptionType.Combo:
                        ComboBox combo = new ComboBox();
                        item.Control = combo;
                        combo.Items.AddRange(item.ComboValues.ToArray());
                        combo.Font = font;
                        combo.DropDownStyle = ComboBoxStyle.DropDownList;
                        combo.FlatStyle = FlatStyle.Flat;
                        combo.SelectedItem = item.CurrentValue;
                        combo.Location = new Point(label.Location.X + 200, label.Location.Y);
                        break;
                    case EngineOption.OptionType.Check:
                        CheckBox cb = new CheckBox();
                        item.Control = cb;
                        cb.Checked = (bool)item.CurrentValue;
                        cb.Location = new Point(label.Location.X + 200, label.Location.Y);
                        cb.Text = "";
                        break;
                }
                OptionsENGForm.Controls.Add(item.Control);
                if (item.Type != EngineOption.OptionType.Button)
                    tt.SetToolTip(item.Control, "Default value: " + item.DefaultValue
                        +(item.Type == EngineOption.OptionType.Spin ? "\nMin value: " + 
                        item.MinValue +"\nMax value: " +item.MaxValue : ""));
                LastControl = item.Control;
                shouldGoLeft = !shouldGoLeft;
            }

            Button DefaultSettings = new Button();
            Button SaveButton = new Button();
            Button CancelButton = new Button();
            Button NewEngine = new Button();
            DefaultSettings.Font = new System.Drawing.Font("Calibri", 12F, FontStyle.Bold);
            SaveButton.Font = new System.Drawing.Font("Calibri", 16F, FontStyle.Bold);
            CancelButton.Font = new System.Drawing.Font("Calibri", 16F, FontStyle.Bold);
            NewEngine.Font = new System.Drawing.Font("Calibri", 12F, FontStyle.Bold);
            DefaultSettings.Tag = engine;
            SaveButton.Tag = engine;
            CancelButton.Tag = engine;
            NewEngine.Tag = engine;
            DefaultSettings.Text = "&Default setting";
            SaveButton.Text = "Save";
            CancelButton.Text = "Cancel";
            NewEngine.Text = "Create &new engine";
            DefaultSettings.Click += DefaultSettings_Click;
            SaveButton.Click += SaveButton_Click;
            CancelButton.Click += CancelButton_Click;
            NewEngine.Click += NewEngine_Click;
            DefaultSettings.AutoSize = true;
            SaveButton.AutoSize = true;
            CancelButton.AutoSize = true;
            NewEngine.AutoSize = true;
            DefaultSettings.Location = new Point(30, LastControl.Bottom + 50);
            NewEngine.Location = new Point(200, LastControl.Bottom + 50);
            CancelButton.Location = new Point(500, LastControl.Bottom + 50);
            SaveButton.Location = new Point(600, LastControl.Bottom + 50);
            OptionsENGForm.Controls.Add(DefaultSettings);
            OptionsENGForm.Controls.Add(SaveButton);
            OptionsENGForm.Controls.Add(CancelButton);
            OptionsENGForm.Controls.Add(NewEngine);

            OptionsENGForm.Size = new System.Drawing.Size(750, LastControl.Bottom < 500 ? LastControl.Bottom + 150 : 650);

            OptionsENGForm.ShowDialog(this);
        }
        void OptButtons_Click(object sender, EventArgs e)
        {
            var TupleData = (sender as Button).Tag as Tuple<Engine, EngineOption>;
            TupleData.Item1.Process.StandardInput.WriteLine("setoption name " + TupleData.Item2.Name);
        }
        void OptionsForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                CancelButton_Click(OptionsENGForm, new EventArgs());
            else if (e.KeyCode == Keys.Enter)
                SaveButton_Click(OptionsENGForm, new EventArgs());
        }
        void NewEngine_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }
        void CancelButton_Click(object sender, EventArgs e)
        {
            OptionsENGForm.Close();
        }
        void SaveButton_Click(object sender, EventArgs e)
        {
            OptionsENGForm.Close();
            Engine engine = (sender as Control).Tag as Engine;
            int x = 0;
            foreach (var item in engine.Options)
            {
                if (item.Type == EngineOption.OptionType.Button || !item.ShouldDisplay)
                    continue;
                if (item.Type == EngineOption.OptionType.Spin)
                {
                    if (int.TryParse(item.Control.Text, out x) && x <= item.MaxValue && x >= item.MinValue)
                        item.CurrentValue = x;
                    continue;
                }
                else if (item.Type == EngineOption.OptionType.Check)
                    item.CurrentValue = (item.Control as CheckBox).Checked;
                else
                    item.CurrentValue = item.Control.Text;
            }
            SetEngineParameters(engine);
        }
        void DefaultSettings_Click(object sender, EventArgs e)
        {
            Engine engine = (sender as Button).Tag as Engine;
            foreach (var item in engine.Options)
            {
                if (!item.ShouldDisplay)
                    continue;
                switch (item.Type)
                {
                    case EngineOption.OptionType.Spin:
                        item.Control.Text = ((int)item.DefaultValue).ToString();
                        break;
                    case EngineOption.OptionType.String:
                        item.Control.Text = item.DefaultValue as String;
                        break;
                    case EngineOption.OptionType.Button:
                        continue;
                    case EngineOption.OptionType.Combo:
                        (item.Control as ComboBox).SelectedItem = item.DefaultValue;
                        break;
                    case EngineOption.OptionType.Check:
                        (item.Control as CheckBox).Checked = (bool)item.DefaultValue;
                        break;
                }
                item.CurrentValue = item.DefaultValue;
            }
            SetEngineParameters(engine);
        }
        private void ShowInstallEngineDialog()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.CheckFileExists = true;
            ofd.CheckPathExists = true;
            ofd.Title = "Install UCI Chess Engine";
            ofd.Multiselect = true;
            ofd.Filter = "Application types (*.exe) |*.exe| All Files (*.*) |*.*";
            ofd.ShowDialog(this);
            foreach (var item in ofd.FileNames)
            {
                InstallEngine(item);
            }
        }
        private void InstallEngine(string path)
        {
            Engine engine = new Engine();
            engine.Path = path;
            engine.Process = new Process();
            engine.Process.StartInfo.FileName = engine.Path;
            engine.Process.StartInfo.CreateNoWindow = true;
            engine.Process.StartInfo.UseShellExecute = false;
            engine.Process.StartInfo.RedirectStandardInput = true;
            engine.Process.StartInfo.RedirectStandardOutput = true;
            engine.BGWorker = new BackgroundWorker();
            engine.BGWorker.WorkerReportsProgress = true;
            engine.BGWorker.DoWork += BGWorker_DoWork;
            engine.BGWorker.ProgressChanged += BGWorker_ProgressChanged;
            engine.State = EngineState.Installing;
            engine.Process.Start();
            engine.Process.StandardInput.WriteLine("uci");
            engine.BGWorker.RunWorkerAsync(engine);
            WaitOutEngine(engine, 10, true);
        }
        private void LoadEngine(Engine _engine, bool isFirstEngine)
        {
            if (!File.Exists(_engine.Path))
            {
                if (LoadedEngines.Count == 0)
                    EnginePanel1.Visible = false;
                return;
            }
            Engine engine = CloneEngine(_engine);
            if ((isFirstEngine ? FirstEngine : SecondEngine) != null
                && LoadedEngines.Contains(isFirstEngine ? FirstEngine : SecondEngine))
            {
                try
                {
                    if (isFirstEngine && FirstEngine.Process.HasExited == false)
                        FirstEngine.Process.Kill();
                    else if (!isFirstEngine && !SecondEngine.Process.HasExited)
                        SecondEngine.Process.Kill();
                }
                catch (Exception)
                {
                    
                }
                LoadedEngines.Remove(isFirstEngine ? FirstEngine : SecondEngine);
            }

            if (!isFirstEngine && SecondEngine == null)
                Add2ndENGPanel();

            engine.Process = new Process();

            if (isFirstEngine)
                FirstEngine = engine;
            else
                SecondEngine = engine;
            FirstEngine.AnalyzeButton = AnalyzeButton1;
            FirstEngine.NameButton = NameButton1;
            FirstEngine.EvalLabel = EvaluationLabel1;
            FirstEngine.DepthLabel = DepthLabel1;
            FirstEngine.GoButton = GoButton1;
            FirstEngine.PVLabel = PVLabel1;
            FirstEngine.EvalDescriptLabel = EvalDescriptLabel1;
            FirstEngine.MainLine.Label = PVLabel1;
            FirstEngine.Panel = EnginePanel1;
            FirstEngine.ComboBox = comboBox1;
            AnalyzeButton1.Tag = FirstEngine;
            NameButton1.Tag = FirstEngine;
            EvaluationLabel1.Tag = FirstEngine;
            DepthLabel1.Tag = FirstEngine;
            GoButton1.Tag = FirstEngine;
            PVLabel1.Tag = FirstEngine;
            EnginePanel1.Tag = FirstEngine;
            EvalDescriptLabel1.Tag = FirstEngine;
            comboBox1.Tag = FirstEngine;
            EnginePanel1.Tag = FirstEngine;

            if (SecondEngine != null && (!isFirstEngine || LoadedEngines.Contains(SecondEngine)))
            {
                SecondEngine.AnalyzeButton = AnalyzeButton2;
                SecondEngine.NameButton = NameButton2;
                SecondEngine.EvalLabel = EvaluationLabel2;
                SecondEngine.EvalDescriptLabel = EvalDescriptLabel2;
                SecondEngine.DepthLabel = DepthLabel2;
                SecondEngine.GoButton = GoButton2;
                SecondEngine.ComboBox = comboBox2;
                SecondEngine.PVLabel = PVLabel2;
                SecondEngine.MainLine.Label = PVLabel2;
                SecondEngine.Panel = EnginePanel2;
                AnalyzeButton2.Tag = SecondEngine;
                NameButton2.Tag = SecondEngine;
                EvaluationLabel2.Tag = SecondEngine;
                DepthLabel2.Tag = SecondEngine;
                GoButton2.Tag = SecondEngine;
                PVLabel2.Tag = SecondEngine;
                EnginePanel2.Tag = SecondEngine;
                EvalDescriptLabel2.Tag = SecondEngine;
                comboBox2.Tag = SecondEngine;
            }

            engine.Process.StartInfo.FileName = engine.Path;
            engine.Process.StartInfo.CreateNoWindow = true;
            engine.Process.StartInfo.UseShellExecute = false;
            engine.Process.StartInfo.RedirectStandardInput = true;
            engine.Process.StartInfo.RedirectStandardOutput = true;
            engine.BGWorker = new BackgroundWorker();
            engine.BGWorker.WorkerReportsProgress = true;
            engine.BGWorker.DoWork += BGWorker_DoWork;
            engine.BGWorker.ProgressChanged += BGWorker_ProgressChanged;
            engine.State = EngineState.Loading;
            engine.Process.Start();
            engine.Process.StandardInput.WriteLine("uci");
            engine.BGWorker.RunWorkerAsync(engine);

            EnginePanel1.Visible = true;

            WaitOutEngine(engine, 10, true);
        }
        private void DoAnalysis(Engine engine)
        {
            if (engine._UCI_AnalyseMode)
                engine.Process.StandardInput.WriteLine("setoption name UCI_AnalyseMode value true");
            ModeOfPlay = PlayMode.EditPosition;
            SendPositionToEngine(engine, "");
            engine.isAnalyzing = true;
            engine.State = EngineState.Analysing;
            engine.Process.StandardInput.WriteLine("go infinite");
            engine.ShouldIgnore = false;
            BestMoveArrow.Enabled = true;
            isInfiniteSearch = true;
            engine.AnalyzeButton.Text = "Stop";
            
        }
        private void SetEngineParameters(Engine engine)
        {
            engine.Process.StandardInput.WriteLine("stop");
            engine.Process.StandardInput.WriteLine("isready");
            engine.isBusy = true;
            if (!WaitOutEngine(engine, 5, false))
                return;
            String str = "";
            foreach (var item in engine.Options)
            {
                if (item.Type == EngineOption.OptionType.Button || !item.ShouldDisplay)
                    continue;
                str = "setoption name " + item.Name
                    + " value " + (item.Type == EngineOption.OptionType.Check ?
                    item.CurrentValue.ToString().ToLower() : item.CurrentValue.ToString());
                engine.Process.StandardInput.WriteLine(str);
            }
            engine.Process.StandardInput.WriteLine("isready");
            engine.isBusy = true;
            if (!WaitOutEngine(engine, 5,false))
                return;
        }
        private void engineContextMenuStrip_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (engineContextMenuStrip.Tag == null || !(engineContextMenuStrip.Tag as Engine).isUciEngine)
            {
                e.Cancel = true;
                return;
            }
            foreach (var item in engineContextMenuStrip.Items)
            {
                (item as ToolStripItem).Enabled = true;
            }
            Engine engine = engineContextMenuStrip.Tag as Engine;
            if (String.IsNullOrEmpty(engine.PVLabel.Text))
                ENGCopyOutput.Enabled = false;
            if (!engine._Hash)
                ENGHashTables.Enabled = false;

            if (SecondEngine == null)
                ENGCloseEngine.Enabled = false;

            if (FirstEngine != null && SecondEngine != null)
            {
                ENGAddEngine.Enabled = false;
                addEngineToolStripMenuItem.Enabled = false;
            }

            ENGChangeEngine.DropDownItems.Clear();
            foreach (var item in InstalledEngines)
                ENGChangeEngine.DropDownItems.Add(item.Name);
            foreach (var item in ENGChangeEngine.DropDownItems)
                (item as ToolStripItem).Click += ChangeEngine_Click;

            if (ENGAddEngine.Enabled)
            {
                ENGAddEngine.DropDownItems.Clear();
                foreach (var item in InstalledEngines)
                    ENGAddEngine.DropDownItems.Add(item.Name);
                foreach ( var item in ENGAddEngine.DropDownItems)
                    (item as ToolStripItem).Click += AddEngine_Click;
            }

            if (engine.OtherLines == null || engine.OtherLines.Count == 0)
                ENGAnalyzeLessMoves.Enabled = false;
        }
        private void ChangeEngine_Click(object sender, EventArgs e)
        {
            Engine engine = null;
            foreach (var item in InstalledEngines)
                if (item.Name == sender.ToString())
                    engine = item;
            if (engine == null)
                return;
            LoadEngine(engine, engineContextMenuStrip.Tag == FirstEngine);
        }
        void AddEngine_Click(object sender, EventArgs e)
        {
            Engine engine = null;
            foreach ( var item in InstalledEngines)
                if (item.Name == sender.ToString())
                    engine = item;
            if (engine == null)
                return;
            if (LoadedEngines.Count == 0)
                LoadEngine(engine, true);
            else
                LoadEngine(engine, false);
        }
        void CloseEngine(Engine engine)
        {
            if (!engine.isUciEngine)
                return;
            engine.Process.StandardInput.WriteLine("stop");
            if (engine == FirstEngine)
                Move2ndENGUp();

            try
            {
                engine.Process.Kill();
            }
            catch (Exception)
            {
                
            }
            LoadedEngines.Remove(engine);
            SecondEngine = null;
            Remove2ndENGPanel();

            if (FirstEngine == null)
            {
                LoadedEngines.Clear();
                foreach (var item in InstalledEngines)
                    if (item.Path == DefaultENGPath)
                    {
                        FirstEngine = item;
                        break;
                    }
                if (FirstEngine != null)
                    LoadEngine(FirstEngine, true);
                else
                    EnginePanel1.Visible = false;
            }
        }
        private void Move2ndENGUp()
        {
            FirstEngine = SecondEngine;
            SecondEngine = null;
            if (FirstEngine != null)
            {
                FirstEngine.AnalyzeButton = AnalyzeButton1;
                FirstEngine.NameButton = NameButton1;
                FirstEngine.EvalLabel = EvaluationLabel1;
                FirstEngine.DepthLabel = DepthLabel1;
                FirstEngine.GoButton = GoButton1;
                FirstEngine.PVLabel = PVLabel1;
                FirstEngine.EvalDescriptLabel = EvalDescriptLabel1;
                FirstEngine.MainLine.Label = PVLabel1;
                FirstEngine.Panel = EnginePanel1;
                FirstEngine.ComboBox = comboBox1;
                AnalyzeButton1.Tag = FirstEngine;
                NameButton1.Tag = FirstEngine;
                EvaluationLabel1.Tag = FirstEngine;
                DepthLabel1.Tag = FirstEngine;
                GoButton1.Tag = FirstEngine;
                PVLabel1.Tag = FirstEngine;
                EnginePanel1.Tag = FirstEngine;
                EvalDescriptLabel1.Tag = FirstEngine;
                comboBox1.Tag = FirstEngine;
                EnginePanel1.Tag = FirstEngine;
                FirstEngine.NameButton.Text = FirstEngine.Name;

                FirstEngine.NameButton.Visible = true;
                FirstEngine.ComboBox.Visible = false;
            }
        }
        private void EnginePanel_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                engineContextMenuStrip.Tag = (sender as Control).Tag;
            }
        }
        private void ENGAnalyzeMoreMoves_Click(object sender, EventArgs e)
        {
            // Ensure other ways to enable Multi PV provide for the next line
            Engine engine = engineContextMenuStrip.Tag as Engine;
            if (engine.OtherLines == null)
                engine.OtherLines = new List<EngineLine>();
            engine.Process.StandardInput.WriteLine("stop");
            engine.Process.StandardInput.WriteLine("setoption name MultiPV value "
                + (engine.OtherLines.Count + 2).ToString());
            engine.isBusy = true;
            engine.Process.StandardInput.WriteLine("isready");
            engine.OtherLines.Add(new EngineLine(engine));
            Label lastLabel = engine.PVLabel;
            foreach (var item in engine.OtherLines)
            {
                if (item != null && item.Label != null)
                {
                    item.Label.Text = "";
                    lastLabel = item.Label;
                }
                else
                {
                    item.Label = new Label();
                    item.Label.AutoSize = lastLabel.AutoSize;
                    item.Label.Location = new Point(lastLabel.Location.X, lastLabel.Bottom + 3);
                    item.Label.Font = lastLabel.Font;
                    item.Label.Size = lastLabel.Size;
                    item.Label.AutoEllipsis = true;
                    item.Label.Tag = engine;
                    item.Label.BackColor = lastLabel.BackColor;
                    engine.Panel.Controls.Add(item.Label);

                    engine.EvalDescriptLabel.Location = new Point(item.Label.Location.X, item.Label.Bottom + 10);
                }
            }

            if (!WaitOutEngine(engine, 5, false))
                return;

            DoAnalysis(engine);
            
            if (ShouldDrawUserArrows && CurrentPosition.Lines.Count > 0)
            {
                List<Square> list = new List<Square>();
                foreach (var item in CurrentPosition.Lines)
                {
                    item.Enabled = false;
                    list.AddRange(item.Squares);
                }
                RefreshSquares(list, RedrawPerspective.UserArrows, null); 
            }
        }
        private void ENGAnalyzeLessMoves_Click(object sender, EventArgs e)
        {
            // Ensure other ways to enable Multi PV provide for the next line
            Engine engine = engineContextMenuStrip.Tag as Engine;
            if (engine.OtherLines == null || engine.OtherLines.Count < 1)
                return;
            engine.ShouldIgnore = true;
            engine.isAnalyzing = false;
            engine.Process.StandardInput.WriteLine("stop");
            engine.Process.StandardInput.WriteLine("setoption name MultiPV value "
                + (engine.OtherLines.Count).ToString());
            engine.isBusy = true;
            engine.Process.StandardInput.WriteLine("isready");
            Label label = engine.OtherLines[engine.OtherLines.Count - 1].Label;
            engine.EvalDescriptLabel.Location = label.Location;
            engine.Panel.Controls.Remove(label);
            engine.OtherLines.RemoveAt(engine.OtherLines.Count - 1);
            if (!WaitOutEngine(engine, 5, false))
                return;

            DoAnalysis(engine);

            if (ShouldDrawUserArrows && CurrentPosition.Lines.Count > 0)
            {
                List<Square> list = new List<Square>();
                foreach (var item in CurrentPosition.Lines)
                {
                    item.Enabled = false;
                    list.AddRange(item.Squares);
                }
                RefreshSquares(list, RedrawPerspective.UserArrows, null);
            }
        }
        private void ENGEngineOptions_Click(object sender, EventArgs e)
        {
            ShowEngineOptionsForm(engineContextMenuStrip.Tag as Engine);
        }
        private void ENGHashTables_Click(object sender, EventArgs e)
        {
            Engine engine = engineContextMenuStrip.Tag as Engine;
            ShowHashForm(engine);
        }
        private void ShowHashForm(Engine engine)
        {
            if (!engine._Hash || !engine.isUciEngine)
                return;
            if (engine.isAnalyzing)
            {
                MessageBox.Show("You must stop engine analysis before changing Hash size");
                return;
            }
            EngineOption opt = null;
            foreach (var item in engine.Options)
                if (!item.ShouldDisplay && item.Name == "Hash" && item.Type == EngineOption.OptionType.Spin)
                    opt = item;
            if (opt == null)
                return;

            Form HashForm = new Form();
            HashForm.MinimizeBox = false;
            HashForm.MaximizeBox = false;
            HashForm.StartPosition = FormStartPosition.CenterParent;
            HashForm.KeyPreview = true;
            HashForm.Text = "Hash Size for " + engine.Name;
            HashForm.KeyDown += HashForm_KeyDown;
            HashForm.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            TextBox HashTB = new TextBox();
            Label mbLabel = new Label();
            
            HashTB.Text = (opt.CurrentValue).ToString();
            HashTB.Font = new System.Drawing.Font("Calibri", 13F, FontStyle.Regular);
            HashTB.Location = new Point(100, 30);
            HashTB.Width = 60;
            HashTB.TextAlign = HorizontalAlignment.Right;

            Label label = new Label();
            label.Font = new System.Drawing.Font("Calibri", 12F, FontStyle.Regular);
            label.AutoSize = true;
            label.Location = new Point(30, HashTB.Bottom + 20);
            PerformanceCounter pf = new PerformanceCounter("Memory", "Available MBytes");
            label.Text = "You have " + pf.RawValue.ToString() + "MB of RAM available";
            mbLabel.Font = new System.Drawing.Font("Calibri", 13F, FontStyle.Regular);
            mbLabel.AutoSize = true;
            mbLabel.Text = "MB";
            mbLabel.Location = new Point(HashTB.Right - 1, HashTB.Top);
            
            CheckBox currentEngine = new CheckBox();
            CheckBox allEngines = new CheckBox();
            currentEngine.Text = "Always use this value for this engine";
            allEngines.Text = "Always use this value for all engines";

            int a;
            allEngines.Checked = int.TryParse(settings.engine.HashSizeHandling, out a);
            currentEngine.Checked = settings.engine.HashSizeHandling == "Specify for each engine. . ."
                || allEngines.Checked;
            allEngines.Tag = currentEngine;
            allEngines.CheckedChanged += allEngines_CheckedChanged;

            currentEngine.AutoSize = true;
            allEngines.AutoSize = true;
            currentEngine.Font = new System.Drawing.Font("Calibri", 10F, FontStyle.Bold);
            allEngines.Font = new System.Drawing.Font("Calibri", 10F, FontStyle.Bold);
            currentEngine.Location = new Point(35, label.Bottom + 70);
            allEngines.Location = new Point(35, currentEngine.Bottom + 1);

            Button OKButton = new Button();
            Button CancelButton = new Button();
            OKButton.Text = "OK";
            CancelButton.Text = "Cancel";
            OKButton.AutoSize = true;
            CancelButton.AutoSize = true;
            OKButton.Font = new System.Drawing.Font("Calibri", 13F, FontStyle.Bold);
            CancelButton.Font = new System.Drawing.Font("Calibri", 13F, FontStyle.Bold);
            OKButton.Tag = Tuple.Create(HashForm, true, HashTB, opt, currentEngine, allEngines);
            CancelButton.Tag = Tuple.Create(HashForm, false, HashTB, opt, currentEngine, allEngines);
            OKButton.Location = new Point(170, allEngines.Bottom + 30);
            CancelButton.Location = new Point(50, allEngines.Bottom + 30);
            OKButton.Click += HashButtons_Click;
            CancelButton.Click += HashButtons_Click;

            HashForm.Controls.Add(HashTB);
            HashForm.Controls.Add(mbLabel);
            HashForm.Controls.Add(label);
            HashForm.Controls.Add(OKButton);
            HashForm.Controls.Add(allEngines);
            HashForm.Controls.Add(currentEngine);
            HashForm.Controls.Add(CancelButton);
            HashForm.Size = new System.Drawing.Size(320, 350);
            HashForm.Tag = Tuple.Create(engine, OKButton, CancelButton);
            HashForm.ShowDialog(this);

            if (HashForm.DialogResult != System.Windows.Forms.DialogResult.OK)
                return;
            if (currentEngine.Checked)
            {
                if (settings.engine.HashSizeHandling == "Specify for each engine. . .")
                {
                    Tuple<Engine, String> temp = null;
                    foreach (var item in settings.engine.HashOptionList)
                        if (item.Item1.Equals(engine))
                        {
                            temp = item;
                            continue;
                        }
                    if (temp == null)
                        settings.engine.HashOptionList.Add(Tuple.Create(engine, HashTB.Text));
                    else
                        settings.engine.HashOptionList[settings.engine.HashOptionList.IndexOf(temp)] =
                            Tuple.Create(engine, HashTB.Text);
                }
                else
                {
                    settings.engine.HashOptionList.Clear();
                    if (settings.engine.HashSizeHandling == "Always ask")
                        foreach (var item in InstalledEngines)
                            settings.engine.HashOptionList.Add(Tuple.Create(item, "Always ask"));
                    else if (settings.engine.HashSizeHandling == "Always use engine default")
                        foreach (var item in InstalledEngines)
                            settings.engine.HashOptionList.Add(Tuple.Create(item, "Use engine default"));
                    else if (int.TryParse(settings.engine.HashSizeHandling, out a))
                        foreach (var item in InstalledEngines)
                            settings.engine.HashOptionList.Add(Tuple.Create(item, a.ToString()));
                    settings.engine.HashSizeHandling = "Specify for each engine. . .";

                    Tuple<Engine, String> temp = null;
                    foreach (var item in settings.engine.HashOptionList)
                        if (item.Item1.Equals(engine))
                        {
                            temp = item;
                            continue;
                        }
                    settings.engine.HashOptionList[settings.engine.HashOptionList.IndexOf(temp)] =
                        Tuple.Create(engine, HashTB.Text);
                }
            }

            if (allEngines.Checked)
            {
                settings.engine.HashSizeHandling = HashTB.Text;
            }
        }
        void allEngines_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            if (cb.Checked)
                (cb.Tag as CheckBox).Checked = true;
        }
        void HashForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                HashButtons_Click(((sender as Form).Tag as Tuple<Engine, Button, Button>).Item2, new EventArgs());
            }
            else if (e.KeyCode == Keys.Enter)
            {
                HashButtons_Click(((sender as Form).Tag as Tuple<Engine, Button, Button>).Item2, new EventArgs());
            }
        }
        void HashButtons_Click(object sender, EventArgs e)
        {
            var tupleData = (sender as Control).Tag as Tuple<Form, bool, TextBox, EngineOption, CheckBox, CheckBox>;
            if (tupleData.Item2)
            {
                int value;
                if (!int.TryParse(tupleData.Item3.Text, out value))
                {
                    MessageBox.Show("Error! You must enter a number between "
                        +tupleData.Item4.MinValue +" and " +tupleData.Item4.MaxValue
                        +".\nDefault value is " + (int)tupleData.Item4.DefaultValue);
                    return;
                }
                if (value > tupleData.Item4.MaxValue || value < tupleData.Item4.MinValue)
                {
                    MessageBox.Show("Error! You must enter a number between "
                        + tupleData.Item4.MinValue + " and " + tupleData.Item4.MaxValue
                        + ".\nDefault value is " + (int)tupleData.Item4.DefaultValue);
                    return;
                }
                Engine engine = (tupleData.Item1.Tag as Tuple<Engine, Button, Button>).Item1 as Engine;
                engine.isBusy = true;
                engine.Process.StandardInput.WriteLine("setoption name Hash value " + tupleData.Item3.Text);
                engine.Process.StandardInput.WriteLine("isready");
                if (!WaitOutEngine(engine, 5, false))
                {
                    tupleData.Item1.Close();
                    return;
                }
                tupleData.Item4.CurrentValue = tupleData.Item3.Text;
                tupleData.Item1.DialogResult = System.Windows.Forms.DialogResult.OK;
                tupleData.Item1.Close();
            }
            else
                tupleData.Item1.Close();
        }
        private void installEngineToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowInstallEngineDialog();
        }
        private void enginesToolStripMenuItem_DropDownOpened(object sender, EventArgs e)
        {
            uninstallEngineToolStripMenuItem.DropDownItems.Clear();
            addEngineToolStripMenuItem.DropDownItems.Clear();
            foreach (var item in InstalledEngines)
            {
                uninstallEngineToolStripMenuItem.DropDownItems.Add(item.Name);
                addEngineToolStripMenuItem.DropDownItems.Add(item.Name);
            }
            foreach (var item in uninstallEngineToolStripMenuItem.DropDownItems)
                (item as ToolStripItem).Click += UninstallEngine_Click;
            foreach (var item in addEngineToolStripMenuItem.DropDownItems)
                (item as ToolStripItem).Click += AddEngine_Click;

            addEngineToolStripMenuItem.Enabled = SecondEngine == null;
        }
        void UninstallEngine_Click(object sender, EventArgs e)
        {
            Engine engine = null;
            foreach (var item in InstalledEngines)
                if (item.Name == sender.ToString())
                    engine = item;
            if (engine == null)
                return;
            if (LoadedEngines.Contains(engine))
                MessageBox.Show("You cannot uninstall an actively loaded Engine."
                    + "\nYou must close or change the Engine");
            else
            {
                if (MessageBox.Show("Do you want to uninstall\n" + engine.Name + "\nby " + engine.Author,
                        "Confirm Uninstallation", MessageBoxButtons.OKCancel, MessageBoxIcon.Asterisk,
                        MessageBoxDefaultButton.Button1) == System.Windows.Forms.DialogResult.OK)
                    InstalledEngines.Remove(engine);
            }
        }
        private void ENGClearOuput_Click(object sender, EventArgs e)
        {
            Engine engine = engineContextMenuStrip.Tag as Engine;
            if (!engine.isUciEngine)
                return;
            engine.DepthLabel.Text = "";
            engine.PVLabel.Text = "";
            engine.EvalDescriptLabel.Text = "";
            engine.EvalLabel.Text = "";
            if (engine.OtherLines != null)
                foreach (var item in engine.OtherLines)
                    item.Label.Text = "";
        }
        private void ENGCopyOutput_Click(object sender, EventArgs e)
        {
            Engine engine = engineContextMenuStrip.Tag as Engine;
            String buffer = engine.PVLabel.Text + Environment.NewLine + Environment.NewLine;
            if (engine.OtherLines != null)
                foreach (var item in engine.OtherLines)
                    buffer += item.Label.Text + Environment.NewLine + Environment.NewLine;
            try
            {
                if (!String.IsNullOrEmpty(buffer))
                    Clipboard.SetText(buffer);
                else
                    Clipboard.Clear();
            }
            catch (System.Runtime.InteropServices.ExternalException)
            {
                String str = "There was an error in pasting the game. Try again later"
                    + "\n Error Message: The Clipboard could not be cleared. "
                    + "\n This typically occurs when the Clipboard"
                    + "is being used by another process.";
                MessageBox.Show(str);
            }
        }
        private void ENGCloseEngine_Click(object sender, EventArgs e)
        {
            CloseEngine(engineContextMenuStrip.Tag as Engine);
        }
        private Engine CloneEngine(Engine _engine)
        {
            Engine engine = new Engine();
            engine.Name = _engine.Name;
            engine.Author = _engine.Author;
            engine.Path = _engine.Path;
            engine.Opponent = _engine.Opponent;
            foreach (var item in _engine.Options)
            {
                EngineOption opt = new EngineOption();
                opt.ComboValues = item.ComboValues;
                opt.CurrentValue = item.CurrentValue;
                opt.DefaultValue = item.DefaultValue;
                opt.MaxValue = item.MaxValue;
                opt.MinValue = item.MinValue;
                opt.Name = item.Name;
                opt.Type = item.Type;
                engine.Options.Add(opt);
            }
            engine.shouldNotReg = _engine.shouldNotReg;
            engine.Time = _engine.Time;

            if (CurrentUser.Opponent == _engine)
                CurrentUser.Opponent = engine;
            if (CurrentPlayer == _engine)
                CurrentPlayer = engine;
            if (CurrentPlayer != null && CurrentPlayer.Opponent == _engine)
                CurrentPlayer.Opponent = engine;
            return engine;
        }
        private bool WaitOutEngine(Engine engine, int seconds, bool isWaitingForUci)
        {
            if (isWaitingForUci)
            {
                if (!engine.isUciEngine)
                {
                    Cursor = Cursors.AppStarting;
                    engine.WaitOutTimer = new System.Windows.Forms.Timer();
                    engine.WaitOutTimer.Interval = 1000 * seconds;
                    engine.WaitOutTimer.Tag = engine;
                    engine.WaitOutTimer.Tick += WaitOutTimer_Tick;
                    engine.WaitOutTimer.Enabled = true;
                    engine.WaitOutTimer.Start();
                }
                return true;
            }

            engine.Process.StandardInput.WriteLine("isready");
            engine.WaitOutWatch = new Stopwatch();
            engine.WaitOutWatch.Start();
            Cursor = Cursors.AppStarting;
            while (engine.isBusy && engine.WaitOutWatch.ElapsedMilliseconds <= seconds * 1000)
            {
                Thread.Sleep(5);
            }
            if (engine.isBusy)
            {
                try
                {
                    if (!engine.Process.HasExited)
                        engine.Process.Kill();
                }
                catch (Exception)
                {
                    
                }
                LoadEngine(engine, engine == FirstEngine);
                Cursor = Cursors.Default;
                MessageBox.Show(engine.Name + " has stopped responding and will be restarted"); 
                return false;
            }
            Cursor = Cursors.Default;
            return true;
        }
        void WaitOutTimer_Tick(object sender, EventArgs e)
        {
            (sender as System.Windows.Forms.Timer).Stop();
            (sender as System.Windows.Forms.Timer).Enabled = false;
            Engine engine = (sender as System.Windows.Forms.Timer).Tag as Engine;
            if (!engine.isUciEngine)
            {
                InstalledEngines.Remove(engine);
                try
                {
                    if (!engine.Process.HasExited)
                        engine.Process.Kill();
                }
                catch (Exception)
                {
                    
                }
                if (engine == SecondEngine)
                {
                    SecondEngine = null;
                    Remove2ndENGPanel();
                }
                else if (engine == FirstEngine && SecondEngine != null)
                {
                    Move2ndENGUp();
                    LoadedEngines.Remove(engine);
                    SecondEngine = null;
                    Remove2ndENGPanel();
                }
                else if (engine == FirstEngine)
                {
                    foreach (var item in InstalledEngines)
                        if (item.Path == DefaultENGPath)
                        {
                            LoadEngine(item, true);
                        }
                }
                Cursor = Cursors.Default;
                MessageBox.Show("Error! " + engine.Path + " is not a UCI engine and has been closed");
            }
        }
        [Serializable]
        public class Engine : IPlayer, IEquatable<Engine>
        {
            public Engine()
            {
                Output = "";
                Options = new List<EngineOption>();
                MainLine = new EngineLine(this);
            }
            [NonSerialized]
            private readonly object dlocker = new object();
            [NonSerialized]
            System.Windows.Forms.Timer waitOutTimer;
            public System.Windows.Forms.Timer WaitOutTimer { get { return waitOutTimer; } set { waitOutTimer = value; } }
            [NonSerialized]
            Stopwatch waitOutWatch;
            public Stopwatch WaitOutWatch { get { return waitOutWatch; } set { waitOutWatch = value; } }
            [NonSerialized]
            ComboBox comboBox;
            public ComboBox ComboBox { get { return comboBox; } set { comboBox = value; } }
            public bool _Hash { get; set; }
            public bool _Ponder { get; set; }
            public bool _OwnBook { get; set; }
            public bool _MultiPV { get; set; }
            public bool _UCI_LimitStrength { get; set; }
            public bool _UCI_Elo { get; set; }
            public bool _UCI_AnalyseMode { get; set; }
            public bool _UCI_EngineAbout { get; set; }
            public bool _SkillLevel { get; set; }
            public bool _Threads { get; set; }

            public bool shouldNotReg { get; set; }
            [NonSerialized]
            Piece.PieceSide side;
            public Piece.PieceSide Side { get { return side; } set { side = value; } }
            public EngineState State { get; set; }
            [NonSerialized]
            EngineLine mainLine;
            public EngineLine MainLine { get { return mainLine; } set { mainLine = value; } }
            [NonSerialized]
            List<EngineLine> otherLines;
            public List<EngineLine> OtherLines { get { return otherLines; } set { otherLines = value; } }
            public TimeSpan AnalysisTime { get; set; }
            [NonSerialized]
            bool isbusy;
            public bool isBusy { get { return isbusy; } set { isbusy = value; } }
            Clock time;
            public Clock Time { get { return time; } set { time = value; } }
            public bool isTempRating { get; set; }
            List<EngineOption> options;
            public List<EngineOption> Options { get { return options; } set { options = value; } }
            public String PonderString { get; set; }
            [NonSerialized]
            bool shouldIgnore;
            public bool ShouldIgnore { get { return shouldIgnore; } set { shouldIgnore = value; } }
            public int Rating { get; set; }
            [NonSerialized]
            Label pvLabel;
            public Label PVLabel { get { return pvLabel; } set { pvLabel = value; } }
            IPlayer opponent;
            public IPlayer Opponent { get { return opponent; } set { opponent = value; } }
            [NonSerialized]
            Panel panel;
            public Panel Panel { get { return panel; } set { panel = value; } }
            [NonSerialized]
            Button nameButton;
            public Button NameButton { get { return nameButton; } set { nameButton = value; } }
            [NonSerialized]
            Button analyzeButton;
            public Button AnalyzeButton { get { return analyzeButton; } set { analyzeButton = value; } }
            [NonSerialized]
            Button goButton;
            public Button GoButton { get { return goButton; } set { goButton = value; } }
            [NonSerialized]
            Label depthLabel;
            public Label DepthLabel { get { return depthLabel; } set { depthLabel = value; } }
            [NonSerialized]
            Label evalLabel;
            public Label EvalLabel { get { return evalLabel; } set { evalLabel = value; } }
            [NonSerialized]
            Label evalDescriptLabel;
            public Label EvalDescriptLabel { get { return evalDescriptLabel; } set { evalDescriptLabel = value; } }
            [NonSerialized]
            bool isanalyzing;
            public bool isAnalyzing { get { return isanalyzing; } set { isanalyzing = value; } }
            public int PVMoveCount { get; set; }
            public List<Square> PVSquares { get; set; }
            [NonSerialized]
            bool isuciengine;
            public bool isUciEngine { get { return isuciengine; } set { isuciengine = value; } }
            public String Path { get; set; }
            public String Name { get; set; }
            public String Author { get; set; }
            [NonSerialized]
            Process process;
            public Process Process { get { return process; } set { process = value; } }
            [NonSerialized]
            BackgroundWorker bgw;
            public BackgroundWorker BGWorker { get { return bgw; } set { bgw = value; } }

            String engineOutput;
            public String Output
            {
                get { return engineOutput; }
                set { engineOutput = value; }
            }
            public override string ToString()
            {
                return Name;
            }
            public bool Equals(Engine eng)
            {
                if (eng is Engine && Name != "" && Name != null)
                {
                    return Name == eng.Name;
                }
                return false;
            }
        }
        [Serializable]
        public class EngineOption
        {
            public enum OptionType
            {
                Spin,
                String,
                Button,
                Check,
                Combo
            }
            public EngineOption()
            {
                ShouldDisplay = true;
            }
            public String Name { get; set; }
            public bool ShouldDisplay { get; set; }
            public OptionType Type { get; set; }
            public Object DefaultValue { get; set; }
            public List<String> ComboValues { get; set; }
            public int MinValue { get; set; }
            public int MaxValue { get; set; }
            public Object CurrentValue { get; set; }
            [NonSerialized]
            Control control;
            public Control Control { get { return control; } set { control = value; } }
        }
        public class EngineLine
        {
            public EngineLine(Engine eng)
            {
                PV = "";
                AnalysisTime = "";
                Depth = "";
                Evaluation = "";
                isPVActive = true;
                LastPVString = "";
                Engine = eng;
            }
            public bool isPVActive { get; set; }
            [NonSerialized]
            Label label;
            public Label Label { get { return label; } set { label = value; } }
            public String PV { get; set; }
            public Engine Engine { get; set; }
            public String AnalysisTime { get; set; }
            public String Depth { get; set; }
            public String Evaluation { get; set; }
            public String LastPVString { get; set; }
        }
        public enum EngineState
        {
            Installing,
            Loading,
            Idle,
            Thinking,
            Analysing,
            Pondering
        }
    }
}